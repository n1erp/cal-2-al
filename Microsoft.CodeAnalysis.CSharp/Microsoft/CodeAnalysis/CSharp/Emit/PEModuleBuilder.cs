using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Emit.NoPia;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal abstract class PEModuleBuilder : PEModuleBuilder<CSharpCompilation, SourceModuleSymbol, AssemblySymbol, TypeSymbol, NamedTypeSymbol, MethodSymbol, SyntaxNode, EmbeddedTypesManager, ModuleCompilationState>
{
	protected readonly ConcurrentDictionary<Symbol, IModuleReference> AssemblyOrModuleSymbolToModuleRefMap = new ConcurrentDictionary<Symbol, IModuleReference>();

	private readonly ConcurrentDictionary<Symbol, object> _genericInstanceMap = new ConcurrentDictionary<Symbol, object>();

	private readonly ConcurrentSet<TypeSymbol> _reportedErrorTypesMap = new ConcurrentSet<TypeSymbol>();

	private readonly EmbeddedTypesManager _embeddedTypesManagerOpt;

	private readonly string _metadataName;

	private ImmutableArray<ExportedType> _lazyExportedTypes;

	private Dictionary<FieldSymbol, NamedTypeSymbol> _fixedImplementationTypes;

	private bool _needsGeneratedIsReadOnlyAttribute_Value;

	private bool _needsGeneratedIsUnmanagedAttribute_Value;

	private bool _needsGeneratedAttributes_IsFrozen;

	public override EmbeddedTypesManager EmbeddedTypesManagerOpt => _embeddedTypesManagerOpt;

	internal bool NeedsGeneratedIsReadOnlyAttribute
	{
		get
		{
			_needsGeneratedAttributes_IsFrozen = true;
			if (!Compilation.NeedsGeneratedIsReadOnlyAttribute)
			{
				return _needsGeneratedIsReadOnlyAttribute_Value;
			}
			return true;
		}
	}

	internal bool NeedsGeneratedIsByRefLikeAttribute => Compilation.NeedsGeneratedIsByRefLikeAttribute;

	internal bool NeedsGeneratedIsUnmanagedAttribute
	{
		get
		{
			_needsGeneratedAttributes_IsFrozen = true;
			if (!Compilation.NeedsGeneratedIsUnmanagedAttribute)
			{
				return _needsGeneratedIsUnmanagedAttribute_Value;
			}
			return true;
		}
	}

	public override string Name => _metadataName;

	internal sealed override string ModuleName => _metadataName;

	internal sealed override AssemblySymbol CorLibrary => SourceModule.ContainingSourceAssembly.CorLibrary;

	public sealed override bool GenerateVisualBasicStylePdb => false;

	public sealed override IEnumerable<string> LinkedAssembliesDebugInfo => SpecializedCollections.EmptyEnumerable<string>();

	public sealed override string DefaultNamespace => null;

	internal virtual bool IgnoreAccessibility => false;

	internal virtual bool IsEncDelta => false;

	internal PEModuleBuilder(SourceModuleSymbol sourceModule, EmitOptions emitOptions, OutputKind outputKind, ModulePropertiesForSerialization serializationProperties, IEnumerable<ResourceDescription> manifestResources)
		: base(sourceModule.ContainingSourceAssembly.DeclaringCompilation, sourceModule, serializationProperties, manifestResources, outputKind, emitOptions, new ModuleCompilationState())
	{
		string metadataName = sourceModule.MetadataName;
		_metadataName = ((metadataName != "?") ? metadataName : (emitOptions.OutputNameOverride ?? metadataName));
		AssemblyOrModuleSymbolToModuleRefMap.Add(sourceModule, this);
		if (sourceModule.AnyReferencedAssembliesAreLinked)
		{
			_embeddedTypesManagerOpt = new EmbeddedTypesManager(this);
		}
	}

	internal sealed override ICustomAttribute SynthesizeAttribute(WellKnownMember attributeConstructor)
	{
		return Compilation.TrySynthesizeAttribute(attributeConstructor);
	}

	public sealed override IEnumerable<ICustomAttribute> GetSourceAssemblyAttributes(bool isRefAssembly)
	{
		return SourceModule.ContainingSourceAssembly.GetCustomAttributesToEmit(this, isRefAssembly, OutputKind.IsNetModule());
	}

	public sealed override IEnumerable<SecurityAttribute> GetSourceAssemblySecurityAttributes()
	{
		return SourceModule.ContainingSourceAssembly.GetSecurityAttributes();
	}

	public sealed override IEnumerable<ICustomAttribute> GetSourceModuleAttributes()
	{
		return SourceModule.GetCustomAttributesToEmit(this);
	}

	public sealed override ImmutableArray<UsedNamespaceOrType> GetImports()
	{
		return ImmutableArray<UsedNamespaceOrType>.Empty;
	}

	protected sealed override IEnumerable<IAssemblyReference> GetAssemblyReferencesFromAddedModules(DiagnosticBag diagnostics)
	{
		ImmutableArray<ModuleSymbol> modules = SourceModule.ContainingAssembly.Modules;
		for (int i = 1; i < modules.Length; i++)
		{
			foreach (AssemblySymbol referencedAssemblySymbol in modules[i].GetReferencedAssemblySymbols())
			{
				yield return Translate(referencedAssemblySymbol, diagnostics);
			}
		}
	}

	private void ValidateReferencedAssembly(AssemblySymbol assembly, AssemblyReference asmRef, DiagnosticBag diagnostics)
	{
		AssemblyIdentity identity = SourceModule.ContainingAssembly.Identity;
		AssemblyIdentity identity2 = asmRef.Identity;
		if (identity.IsStrongName && !identity2.IsStrongName && asmRef.Identity.ContentType != AssemblyContentType.WindowsRuntime)
		{
			diagnostics.Add(new CSDiagnosticInfo(ErrorCode.WRN_ReferencedAssemblyDoesNotHaveStrongName, assembly), NoLocation.Singleton);
		}
		if (OutputKind != OutputKind.NetModule && !string.IsNullOrEmpty(identity2.CultureName) && !string.Equals(identity2.CultureName, identity.CultureName, StringComparison.OrdinalIgnoreCase))
		{
			diagnostics.Add(new CSDiagnosticInfo(ErrorCode.WRN_RefCultureMismatch, assembly, identity2.CultureName), NoLocation.Singleton);
		}
		Machine machine = assembly.Machine;
		if ((object)assembly != assembly.CorLibrary && (machine != Machine.I386 || assembly.Bit32Required))
		{
			Machine machine2 = SourceModule.Machine;
			if ((machine2 != Machine.I386 || SourceModule.Bit32Required) && machine2 != machine)
			{
				diagnostics.Add(new CSDiagnosticInfo(ErrorCode.WRN_ConflictingMachineAssembly, assembly), NoLocation.Singleton);
			}
		}
		if (_embeddedTypesManagerOpt != null && _embeddedTypesManagerOpt.IsFrozen)
		{
			_embeddedTypesManagerOpt.ReportIndirectReferencesToLinkedAssemblies(assembly, diagnostics);
		}
	}

	internal sealed override IEnumerable<INestedTypeDefinition> GetSynthesizedNestedTypes(NamedTypeSymbol container)
	{
		return null;
	}

	public sealed override MultiDictionary<DebugSourceDocument, DefinitionWithLocation> GetSymbolToLocationMap()
	{
		MultiDictionary<DebugSourceDocument, DefinitionWithLocation> result = new MultiDictionary<DebugSourceDocument, DefinitionWithLocation>();
		Stack<NamespaceOrTypeSymbol> stack = new Stack<NamespaceOrTypeSymbol>();
		stack.Push(SourceModule.GlobalNamespace);
		while (stack.Count > 0)
		{
			NamespaceOrTypeSymbol namespaceOrTypeSymbol = stack.Pop();
			switch (namespaceOrTypeSymbol.Kind)
			{
			case SymbolKind.Namespace:
			{
				Location smallestSourceLocationOrNull = GetSmallestSourceLocationOrNull(namespaceOrTypeSymbol);
				if (!(smallestSourceLocationOrNull != null))
				{
					break;
				}
				foreach (Symbol member in namespaceOrTypeSymbol.GetMembers())
				{
					SymbolKind kind = member.Kind;
					if ((uint)(kind - 11) <= 1u)
					{
						stack.Push((NamespaceOrTypeSymbol)member);
						continue;
					}
					throw ExceptionUtilities.UnexpectedValue(member.Kind);
				}
				break;
			}
			case SymbolKind.NamedType:
			{
				Location smallestSourceLocationOrNull = GetSmallestSourceLocationOrNull(namespaceOrTypeSymbol);
				if (!(smallestSourceLocationOrNull != null))
				{
					break;
				}
				AddSymbolLocation(result, smallestSourceLocationOrNull, (IDefinition)namespaceOrTypeSymbol);
				foreach (Symbol member2 in namespaceOrTypeSymbol.GetMembers())
				{
					switch (member2.Kind)
					{
					case SymbolKind.NamedType:
						stack.Push((NamespaceOrTypeSymbol)member2);
						break;
					case SymbolKind.Method:
					{
						MethodSymbol methodSymbol = (MethodSymbol)member2;
						if (!methodSymbol.IsDefaultValueTypeConstructor() && (!methodSymbol.IsPartialMethod() || (object)methodSymbol.PartialImplementationPart != null))
						{
							AddSymbolLocation(result, member2);
						}
						break;
					}
					case SymbolKind.Field:
					case SymbolKind.Property:
						AddSymbolLocation(result, member2);
						break;
					case SymbolKind.Event:
					{
						AddSymbolLocation(result, member2);
						FieldSymbol associatedField = ((EventSymbol)member2).AssociatedField;
						if ((object)associatedField != null)
						{
							AddSymbolLocation(result, associatedField);
						}
						break;
					}
					default:
						throw ExceptionUtilities.UnexpectedValue(member2.Kind);
					}
				}
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(namespaceOrTypeSymbol.Kind);
			}
		}
		return result;
	}

	private void AddSymbolLocation(MultiDictionary<DebugSourceDocument, DefinitionWithLocation> result, Symbol symbol)
	{
		Location smallestSourceLocationOrNull = GetSmallestSourceLocationOrNull(symbol);
		if (smallestSourceLocationOrNull != null)
		{
			AddSymbolLocation(result, smallestSourceLocationOrNull, (IDefinition)symbol);
		}
	}

	private void AddSymbolLocation(MultiDictionary<DebugSourceDocument, DefinitionWithLocation> result, Location location, IDefinition definition)
	{
		FileLinePositionSpan lineSpan = location.GetLineSpan();
		DebugSourceDocument debugSourceDocument = DebugDocumentsBuilder.TryGetDebugDocument(lineSpan.Path, location.SourceTree.FilePath);
		if (debugSourceDocument != null)
		{
			result.Add(debugSourceDocument, new DefinitionWithLocation(definition, lineSpan.StartLinePosition.Line, lineSpan.StartLinePosition.Character, lineSpan.EndLinePosition.Line, lineSpan.EndLinePosition.Character));
		}
	}

	private Location GetSmallestSourceLocationOrNull(Symbol symbol)
	{
		CSharpCompilation declaringCompilation = symbol.DeclaringCompilation;
		Location location = null;
		foreach (Location location2 in symbol.Locations)
		{
			if (location2.IsInSource && (location == null || declaringCompilation.CompareSourceLocations(location, location2) > 0))
			{
				location = location2;
			}
		}
		return location;
	}

	internal virtual NamedTypeSymbol GetDynamicOperationContextType(NamedTypeSymbol contextType)
	{
		return contextType;
	}

	internal virtual VariableSlotAllocator TryCreateVariableSlotAllocator(MethodSymbol method, MethodSymbol topLevelMethod, DiagnosticBag diagnostics)
	{
		return null;
	}

	internal virtual ImmutableArray<AnonymousTypeKey> GetPreviousAnonymousTypes()
	{
		return ImmutableArray<AnonymousTypeKey>.Empty;
	}

	internal virtual int GetNextAnonymousTypeIndex()
	{
		return 0;
	}

	internal virtual bool TryGetAnonymousTypeName(NamedTypeSymbol template, out string name, out int index)
	{
		name = null;
		index = -1;
		return false;
	}

	internal sealed override ImmutableArray<INamespaceTypeDefinition> GetAnonymousTypes(EmitContext context)
	{
		if (context.MetadataOnly)
		{
			return ImmutableArray<INamespaceTypeDefinition>.Empty;
		}
		return StaticCast<INamespaceTypeDefinition>.From(Compilation.AnonymousTypeManager.GetAllCreatedTemplates());
	}

	internal override IEnumerable<INamespaceTypeDefinition> GetTopLevelTypesCore(EmitContext context)
	{
		foreach (NamedTypeSymbol additionalTopLevelType in GetAdditionalTopLevelTypes(context.Diagnostics))
		{
			yield return additionalTopLevelType;
		}
		foreach (NamedTypeSymbol embeddedType in GetEmbeddedTypes(context.Diagnostics))
		{
			yield return embeddedType;
		}
		Stack<NamespaceSymbol> namespacesToProcess = new Stack<NamespaceSymbol>();
		namespacesToProcess.Push(SourceModule.GlobalNamespace);
		while (namespacesToProcess.Count > 0)
		{
			NamespaceSymbol namespaceSymbol = namespacesToProcess.Pop();
			foreach (Symbol member in namespaceSymbol.GetMembers())
			{
				if (member is NamespaceSymbol item)
				{
					namespacesToProcess.Push(item);
				}
				else
				{
					yield return (NamedTypeSymbol)member;
				}
			}
		}
	}

	internal virtual ImmutableArray<NamedTypeSymbol> GetAdditionalTopLevelTypes(DiagnosticBag diagnostics)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal virtual ImmutableArray<NamedTypeSymbol> GetEmbeddedTypes(DiagnosticBag diagnostics)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	private static void GetExportedTypes(NamespaceOrTypeSymbol symbol, int parentIndex, ArrayBuilder<ExportedType> builder)
	{
		int parentIndex2;
		if (symbol.Kind == SymbolKind.NamedType)
		{
			if (symbol.DeclaredAccessibility != Accessibility.Public)
			{
				return;
			}
			parentIndex2 = builder.Count;
			builder.Add(new ExportedType((ITypeReference)symbol, parentIndex, isForwarder: false));
		}
		else
		{
			parentIndex2 = -1;
		}
		foreach (Symbol member in symbol.GetMembers())
		{
			if (member is NamespaceOrTypeSymbol symbol2)
			{
				GetExportedTypes(symbol2, parentIndex2, builder);
			}
		}
	}

	public sealed override ImmutableArray<ExportedType> GetExportedTypes(DiagnosticBag diagnostics)
	{
		if (_lazyExportedTypes.IsDefault)
		{
			_lazyExportedTypes = CalculateExportedTypes();
			if (_lazyExportedTypes.Length > 0)
			{
				ReportExportedTypeNameCollisions(_lazyExportedTypes, diagnostics);
			}
		}
		return _lazyExportedTypes;
	}

	private ImmutableArray<ExportedType> CalculateExportedTypes()
	{
		SourceAssemblySymbol containingSourceAssembly = SourceModule.ContainingSourceAssembly;
		ArrayBuilder<ExportedType> instance = ArrayBuilder<ExportedType>.GetInstance();
		if (!OutputKind.IsNetModule())
		{
			ImmutableArray<ModuleSymbol> modules = containingSourceAssembly.Modules;
			for (int i = 1; i < modules.Length; i++)
			{
				GetExportedTypes(modules[i].GlobalNamespace, -1, instance);
			}
		}
		HashSet<NamedTypeSymbol> seenTopLevelTypes = new HashSet<NamedTypeSymbol>();
		GetForwardedTypes(seenTopLevelTypes, containingSourceAssembly.GetSourceDecodedWellKnownAttributeData(), instance);
		if (!OutputKind.IsNetModule())
		{
			GetForwardedTypes(seenTopLevelTypes, containingSourceAssembly.GetNetModuleDecodedWellKnownAttributeData(), instance);
		}
		return instance.ToImmutableAndFree();
	}

	private void ReportExportedTypeNameCollisions(ImmutableArray<ExportedType> exportedTypes, DiagnosticBag diagnostics)
	{
		SourceAssemblySymbol containingSourceAssembly = SourceModule.ContainingSourceAssembly;
		Dictionary<string, NamedTypeSymbol> dictionary = new Dictionary<string, NamedTypeSymbol>(StringOrdinalComparer.Instance);
		foreach (ExportedType item in exportedTypes)
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)item.Type;
			if (!namedTypeSymbol.IsTopLevelType())
			{
				continue;
			}
			string text = MetadataHelpers.BuildQualifiedName(((INamespaceTypeReference)namedTypeSymbol).NamespaceName, MetadataWriter.GetMangledName(namedTypeSymbol));
			NamedTypeSymbol value;
			if (ContainsTopLevelType(text))
			{
				if ((object)namedTypeSymbol.ContainingAssembly == containingSourceAssembly)
				{
					diagnostics.Add(ErrorCode.ERR_ExportedTypeConflictsWithDeclaration, NoLocation.Singleton, namedTypeSymbol, namedTypeSymbol.ContainingModule);
				}
				else
				{
					diagnostics.Add(ErrorCode.ERR_ForwardedTypeConflictsWithDeclaration, NoLocation.Singleton, namedTypeSymbol);
				}
			}
			else if (dictionary.TryGetValue(text, out value))
			{
				if ((object)namedTypeSymbol.ContainingAssembly == containingSourceAssembly)
				{
					diagnostics.Add(ErrorCode.ERR_ExportedTypesConflict, NoLocation.Singleton, namedTypeSymbol, namedTypeSymbol.ContainingModule, value, value.ContainingModule);
				}
				else if ((object)value.ContainingAssembly == containingSourceAssembly)
				{
					diagnostics.Add(ErrorCode.ERR_ForwardedTypeConflictsWithExportedType, NoLocation.Singleton, namedTypeSymbol, namedTypeSymbol.ContainingAssembly, value, value.ContainingModule);
				}
				else
				{
					diagnostics.Add(ErrorCode.ERR_ForwardedTypesConflict, NoLocation.Singleton, namedTypeSymbol, namedTypeSymbol.ContainingAssembly, value, value.ContainingAssembly);
				}
			}
			else
			{
				dictionary.Add(text, namedTypeSymbol);
			}
		}
	}

	private static void GetForwardedTypes(HashSet<NamedTypeSymbol> seenTopLevelTypes, CommonAssemblyWellKnownAttributeData<NamedTypeSymbol> wellKnownAttributeData, ArrayBuilder<ExportedType> builder)
	{
		if (wellKnownAttributeData == null || !(wellKnownAttributeData.ForwardedTypes?.Count > 0))
		{
			return;
		}
		ArrayBuilder<(NamedTypeSymbol, int)> instance = ArrayBuilder<(NamedTypeSymbol, int)>.GetInstance();
		foreach (NamedTypeSymbol item in wellKnownAttributeData.ForwardedTypes.OrderBy((NamedTypeSymbol t) => t.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.QualifiedNameArityFormat)))
		{
			NamedTypeSymbol originalDefinition = item.OriginalDefinition;
			if (!seenTopLevelTypes.Add(originalDefinition))
			{
				continue;
			}
			instance.Push((originalDefinition, -1));
			while (instance.Count > 0)
			{
				var (namedTypeSymbol, parentIndex) = instance.Pop();
				if (namedTypeSymbol.DeclaredAccessibility != Accessibility.Private)
				{
					int count = builder.Count;
					builder.Add(new ExportedType(namedTypeSymbol, parentIndex, isForwarder: true));
					ImmutableArray<NamedTypeSymbol> typeMembers = namedTypeSymbol.GetTypeMembers();
					for (int num = typeMembers.Length - 1; num >= 0; num--)
					{
						instance.Push((typeMembers[num], count));
					}
				}
			}
		}
		instance.Free();
	}

	internal IEnumerable<AssemblySymbol> GetReferencedAssembliesUsedSoFar()
	{
		foreach (AssemblySymbol referencedAssemblySymbol in SourceModule.GetReferencedAssemblySymbols())
		{
			if (!referencedAssemblySymbol.IsLinked && !referencedAssemblySymbol.IsMissing && AssemblyOrModuleSymbolToModuleRefMap.ContainsKey(referencedAssemblySymbol))
			{
				yield return referencedAssemblySymbol;
			}
		}
	}

	private NamedTypeSymbol GetUntranslatedSpecialType(SpecialType specialType, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol specialType2 = SourceModule.ContainingAssembly.GetSpecialType(specialType);
		DiagnosticInfo useSiteDiagnostic = specialType2.GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null)
		{
			Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, diagnostics, (syntaxNodeOpt != null) ? syntaxNodeOpt.Location : NoLocation.Singleton);
		}
		return specialType2;
	}

	internal sealed override INamedTypeReference GetSpecialType(SpecialType specialType, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		return Translate(GetUntranslatedSpecialType(specialType, syntaxNodeOpt, diagnostics), syntaxNodeOpt, diagnostics, fromImplements: false, needDeclaration: true);
	}

	internal sealed override INamedTypeReference GetSystemType(SyntaxNode syntaxOpt, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol wellKnownType = Compilation.GetWellKnownType(WellKnownType.System_Type);
		DiagnosticInfo useSiteDiagnostic = wellKnownType.GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null)
		{
			Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, diagnostics, (syntaxOpt != null) ? syntaxOpt.Location : NoLocation.Singleton);
		}
		return Translate(wellKnownType, syntaxOpt, diagnostics, fromImplements: false, needDeclaration: true);
	}

	public sealed override IMethodReference GetInitArrayHelper()
	{
		return (MethodSymbol)Compilation.GetWellKnownTypeMember(WellKnownMember.System_Runtime_CompilerServices_RuntimeHelpers__InitializeArrayArrayRuntimeFieldHandle);
	}

	public sealed override bool IsPlatformType(ITypeReference typeRef, PlatformType platformType)
	{
		if (typeRef is NamedTypeSymbol namedTypeSymbol)
		{
			if (platformType == PlatformType.SystemType)
			{
				return (object)namedTypeSymbol == Compilation.GetWellKnownType(WellKnownType.System_Type);
			}
			return (int)namedTypeSymbol.SpecialType == (sbyte)platformType;
		}
		return false;
	}

	protected sealed override IAssemblyReference GetCorLibraryReferenceToEmit(EmitContext context)
	{
		AssemblySymbol corLibrary = CorLibrary;
		if (!corLibrary.IsMissing && !corLibrary.IsLinked && (object)corLibrary != SourceModule.ContainingAssembly)
		{
			return Translate(corLibrary, context.Diagnostics);
		}
		return null;
	}

	internal sealed override IAssemblyReference Translate(AssemblySymbol assembly, DiagnosticBag diagnostics)
	{
		if ((object)SourceModule.ContainingAssembly == assembly)
		{
			return (IAssemblyReference)this;
		}
		if (AssemblyOrModuleSymbolToModuleRefMap.TryGetValue(assembly, out var value))
		{
			return (IAssemblyReference)value;
		}
		AssemblyReference assemblyReference = new AssemblyReference(assembly);
		AssemblyReference assemblyReference2 = (AssemblyReference)AssemblyOrModuleSymbolToModuleRefMap.GetOrAdd(assembly, assemblyReference);
		if (assemblyReference2 == assemblyReference)
		{
			ValidateReferencedAssembly(assembly, assemblyReference2, diagnostics);
		}
		AssemblyOrModuleSymbolToModuleRefMap.TryAdd(assembly.Modules[0], assemblyReference2);
		return assemblyReference2;
	}

	internal IModuleReference Translate(ModuleSymbol module, DiagnosticBag diagnostics)
	{
		if ((object)SourceModule == module)
		{
			return this;
		}
		if ((object)module == null)
		{
			return null;
		}
		if (AssemblyOrModuleSymbolToModuleRefMap.TryGetValue(module, out var value))
		{
			return value;
		}
		value = TranslateModule(module, diagnostics);
		return AssemblyOrModuleSymbolToModuleRefMap.GetOrAdd(module, value);
	}

	protected virtual IModuleReference TranslateModule(ModuleSymbol module, DiagnosticBag diagnostics)
	{
		AssemblySymbol containingAssembly = module.ContainingAssembly;
		if ((object)containingAssembly != null && (object)containingAssembly.Modules[0] == module)
		{
			IModuleReference moduleReference = new AssemblyReference(containingAssembly);
			IModuleReference orAdd = AssemblyOrModuleSymbolToModuleRefMap.GetOrAdd(containingAssembly, moduleReference);
			if (orAdd == moduleReference)
			{
				ValidateReferencedAssembly(containingAssembly, (AssemblyReference)moduleReference, diagnostics);
			}
			else
			{
				moduleReference = orAdd;
			}
			return moduleReference;
		}
		return new ModuleReference(this, module);
	}

	internal INamedTypeReference Translate(NamedTypeSymbol namedTypeSymbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool fromImplements = false, bool needDeclaration = false)
	{
		if (namedTypeSymbol.IsAnonymousType)
		{
			namedTypeSymbol = AnonymousTypeManager.TranslateAnonymousTypeSymbol(namedTypeSymbol);
		}
		else if (namedTypeSymbol.IsTupleType)
		{
			namedTypeSymbol = namedTypeSymbol.TupleUnderlyingType;
			CheckTupleUnderlyingType(namedTypeSymbol, syntaxNodeOpt, diagnostics);
		}
		if (namedTypeSymbol.OriginalDefinition.Kind == SymbolKind.ErrorType)
		{
			ErrorTypeSymbol errorTypeSymbol = (ErrorTypeSymbol)namedTypeSymbol.OriginalDefinition;
			DiagnosticInfo diagnosticInfo = errorTypeSymbol.GetUseSiteDiagnostic() ?? errorTypeSymbol.ErrorInfo;
			if (diagnosticInfo == null && namedTypeSymbol.Kind == SymbolKind.ErrorType)
			{
				errorTypeSymbol = (ErrorTypeSymbol)namedTypeSymbol;
				diagnosticInfo = errorTypeSymbol.GetUseSiteDiagnostic() ?? errorTypeSymbol.ErrorInfo;
			}
			if (_reportedErrorTypesMap.Add(errorTypeSymbol))
			{
				diagnostics.Add(new CSDiagnostic(diagnosticInfo ?? new CSDiagnosticInfo(ErrorCode.ERR_BogusType, string.Empty), (syntaxNodeOpt == null) ? NoLocation.Singleton : syntaxNodeOpt.Location));
			}
			return ErrorType.Singleton;
		}
		if (!namedTypeSymbol.IsDefinition)
		{
			if (!namedTypeSymbol.IsUnboundGenericType)
			{
				return namedTypeSymbol;
			}
			namedTypeSymbol = namedTypeSymbol.OriginalDefinition;
		}
		else if (!needDeclaration)
		{
			NamedTypeSymbol containingType = namedTypeSymbol.ContainingType;
			object value;
			if (namedTypeSymbol.Arity > 0)
			{
				if (_genericInstanceMap.TryGetValue(namedTypeSymbol, out value))
				{
					return (INamedTypeReference)value;
				}
				INamedTypeReference value2;
				if ((object)containingType == null)
				{
					value2 = new GenericNamespaceTypeInstanceReference(namedTypeSymbol);
				}
				else
				{
					value2 = ((!IsGenericType(containingType)) ? ((INamedTypeReference)new GenericNestedTypeInstanceReference(namedTypeSymbol)) : ((INamedTypeReference)new SpecializedGenericNestedTypeInstanceReference(namedTypeSymbol)));
				}
				return (INamedTypeReference)_genericInstanceMap.GetOrAdd(namedTypeSymbol, value2);
			}
			if (IsGenericType(containingType))
			{
				if (_genericInstanceMap.TryGetValue(namedTypeSymbol, out value))
				{
					return (INamedTypeReference)value;
				}
				INamedTypeReference value2 = new SpecializedNestedTypeReference(namedTypeSymbol);
				return (INamedTypeReference)_genericInstanceMap.GetOrAdd(namedTypeSymbol, value2);
			}
		}
		if (_embeddedTypesManagerOpt != null)
		{
			return _embeddedTypesManagerOpt.EmbedTypeIfNeedTo(namedTypeSymbol, fromImplements, syntaxNodeOpt, diagnostics);
		}
		return namedTypeSymbol;
	}

	private void CheckTupleUnderlyingType(NamedTypeSymbol namedTypeSymbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = namedTypeSymbol.BaseTypeNoUseSiteDiagnostics;
		if (((object)baseTypeNoUseSiteDiagnostics != null && baseTypeNoUseSiteDiagnostics.SpecialType == SpecialType.System_ValueType) || !_reportedErrorTypesMap.Add(namedTypeSymbol))
		{
			return;
		}
		Location location = ((syntaxNodeOpt == null) ? NoLocation.Singleton : syntaxNodeOpt.Location);
		if ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			DiagnosticInfo useSiteDiagnostic = baseTypeNoUseSiteDiagnostics.GetUseSiteDiagnostic();
			if (useSiteDiagnostic != null && useSiteDiagnostic.Severity == DiagnosticSeverity.Error)
			{
				diagnostics.Add(useSiteDiagnostic, location);
				return;
			}
		}
		diagnostics.Add(new CSDiagnostic(new CSDiagnosticInfo(ErrorCode.ERR_PredefinedValueTupleTypeMustBeStruct, namedTypeSymbol.MetadataName), location));
	}

	public static bool IsGenericType(NamedTypeSymbol toCheck)
	{
		while ((object)toCheck != null)
		{
			if (toCheck.Arity > 0)
			{
				return true;
			}
			toCheck = toCheck.ContainingType;
		}
		return false;
	}

	internal static IGenericParameterReference Translate(TypeParameterSymbol param)
	{
		if (!param.IsDefinition)
		{
			throw new InvalidOperationException(string.Format(CSharpResources.GenericParameterDefinition, param.Name));
		}
		return param;
	}

	internal sealed override ITypeReference Translate(TypeSymbol typeSymbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		switch (typeSymbol.Kind)
		{
		case SymbolKind.DynamicType:
			return Translate((DynamicTypeSymbol)typeSymbol, syntaxNodeOpt, diagnostics);
		case SymbolKind.ArrayType:
			return Translate((ArrayTypeSymbol)typeSymbol);
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
			return Translate((NamedTypeSymbol)typeSymbol, syntaxNodeOpt, diagnostics);
		case SymbolKind.PointerType:
			return Translate((PointerTypeSymbol)typeSymbol);
		case SymbolKind.TypeParameter:
			return Translate((TypeParameterSymbol)typeSymbol);
		default:
			throw ExceptionUtilities.UnexpectedValue(typeSymbol.Kind);
		}
	}

	internal IFieldReference Translate(FieldSymbol fieldSymbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool needDeclaration = false)
	{
		if (!fieldSymbol.IsDefinition)
		{
			return fieldSymbol;
		}
		if (!needDeclaration && IsGenericType(fieldSymbol.ContainingType))
		{
			if (_genericInstanceMap.TryGetValue(fieldSymbol, out var value))
			{
				return (IFieldReference)value;
			}
			IFieldReference value2 = new SpecializedFieldReference(fieldSymbol);
			return (IFieldReference)_genericInstanceMap.GetOrAdd(fieldSymbol, value2);
		}
		if (_embeddedTypesManagerOpt != null)
		{
			return _embeddedTypesManagerOpt.EmbedFieldIfNeedTo(fieldSymbol, syntaxNodeOpt, diagnostics);
		}
		return fieldSymbol;
	}

	public static TypeMemberVisibility MemberVisibility(Symbol symbol)
	{
		switch (symbol.DeclaredAccessibility)
		{
		case Accessibility.Public:
			return TypeMemberVisibility.Public;
		case Accessibility.Private:
		{
			NamedTypeSymbol containingType = symbol.ContainingType;
			if ((object)containingType != null && containingType.TypeKind == TypeKind.Submission)
			{
				return TypeMemberVisibility.Public;
			}
			return TypeMemberVisibility.Private;
		}
		case Accessibility.Internal:
			if (symbol.ContainingAssembly.IsInteractive)
			{
				return TypeMemberVisibility.Public;
			}
			return TypeMemberVisibility.Assembly;
		case Accessibility.Protected:
			if (symbol.ContainingType.TypeKind == TypeKind.Submission)
			{
				return TypeMemberVisibility.Public;
			}
			return TypeMemberVisibility.Family;
		case Accessibility.ProtectedAndInternal:
			return TypeMemberVisibility.FamilyAndAssembly;
		case Accessibility.ProtectedOrInternal:
			if (symbol.ContainingAssembly.IsInteractive)
			{
				return TypeMemberVisibility.Public;
			}
			return TypeMemberVisibility.FamilyOrAssembly;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.DeclaredAccessibility);
		}
	}

	internal sealed override IMethodReference Translate(MethodSymbol symbol, DiagnosticBag diagnostics, bool needDeclaration)
	{
		return Translate(symbol, null, diagnostics, null, needDeclaration);
	}

	internal IMethodReference Translate(MethodSymbol methodSymbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, BoundArgListOperator optArgList = null, bool needDeclaration = false)
	{
		IMethodReference methodReference = Translate(methodSymbol, syntaxNodeOpt, diagnostics, needDeclaration);
		if (optArgList != null && optArgList.Arguments.Length > 0)
		{
			IParameterTypeInformation[] array = new IParameterTypeInformation[optArgList.Arguments.Length];
			int num = methodSymbol.ParameterCount;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ArgListParameterTypeInformation(num, !optArgList.ArgumentRefKindsOpt.IsDefaultOrEmpty && optArgList.ArgumentRefKindsOpt[i] != RefKind.None, Translate(optArgList.Arguments[i].Type, syntaxNodeOpt, diagnostics));
				num++;
			}
			return new ExpandedVarargsMethodReference(methodReference, array.AsImmutableOrNull());
		}
		return methodReference;
	}

	private IMethodReference Translate(MethodSymbol methodSymbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool needDeclaration)
	{
		NamedTypeSymbol namedTypeSymbol = methodSymbol.ContainingType;
		if (namedTypeSymbol.IsAnonymousType)
		{
			methodSymbol = AnonymousTypeManager.TranslateAnonymousTypeMethodSymbol(methodSymbol);
		}
		else if (methodSymbol.IsTupleMethod)
		{
			namedTypeSymbol = namedTypeSymbol.TupleUnderlyingType;
			methodSymbol = methodSymbol.TupleUnderlyingMethod;
		}
		if (!methodSymbol.IsDefinition)
		{
			return methodSymbol;
		}
		if (!needDeclaration)
		{
			bool isGenericMethod = methodSymbol.IsGenericMethod;
			bool flag = IsGenericType(namedTypeSymbol);
			if (isGenericMethod | flag)
			{
				if (_genericInstanceMap.TryGetValue(methodSymbol, out var value))
				{
					return (IMethodReference)value;
				}
				IMethodReference value2;
				if (isGenericMethod)
				{
					value2 = ((!flag) ? ((IMethodReference)new GenericMethodInstanceReference(methodSymbol)) : ((IMethodReference)new SpecializedGenericMethodInstanceReference(methodSymbol)));
				}
				else
				{
					value2 = new SpecializedMethodReference(methodSymbol);
				}
				return (IMethodReference)_genericInstanceMap.GetOrAdd(methodSymbol, value2);
			}
		}
		if (_embeddedTypesManagerOpt != null)
		{
			return _embeddedTypesManagerOpt.EmbedMethodIfNeedTo(methodSymbol, syntaxNodeOpt, diagnostics);
		}
		return methodSymbol;
	}

	internal IMethodReference TranslateOverriddenMethodReference(MethodSymbol methodSymbol, CSharpSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		if (IsGenericType(methodSymbol.ContainingType))
		{
			if (methodSymbol.IsDefinition)
			{
				if (_genericInstanceMap.TryGetValue(methodSymbol, out var value))
				{
					return (IMethodReference)value;
				}
				IMethodReference value2 = new SpecializedMethodReference(methodSymbol);
				return (IMethodReference)_genericInstanceMap.GetOrAdd(methodSymbol, value2);
			}
			return new SpecializedMethodReference(methodSymbol);
		}
		if (_embeddedTypesManagerOpt != null)
		{
			return _embeddedTypesManagerOpt.EmbedMethodIfNeedTo(methodSymbol, syntaxNodeOpt, diagnostics);
		}
		return methodSymbol;
	}

	internal ImmutableArray<IParameterTypeInformation> Translate(ImmutableArray<ParameterSymbol> @params)
	{
		if (!@params.Any() || !MustBeWrapped(@params.First()))
		{
			return StaticCast<IParameterTypeInformation>.From(@params);
		}
		return TranslateAll(@params);
	}

	private static bool MustBeWrapped(ParameterSymbol param)
	{
		if (param.IsDefinition && ContainerIsGeneric(param.ContainingSymbol))
		{
			return true;
		}
		return false;
	}

	private ImmutableArray<IParameterTypeInformation> TranslateAll(ImmutableArray<ParameterSymbol> @params)
	{
		ArrayBuilder<IParameterTypeInformation> instance = ArrayBuilder<IParameterTypeInformation>.GetInstance();
		foreach (ParameterSymbol item in @params)
		{
			instance.Add(CreateParameterTypeInformationWrapper(item));
		}
		return instance.ToImmutableAndFree();
	}

	private IParameterTypeInformation CreateParameterTypeInformationWrapper(ParameterSymbol param)
	{
		if (_genericInstanceMap.TryGetValue(param, out var value))
		{
			return (IParameterTypeInformation)value;
		}
		IParameterTypeInformation value2 = new ParameterTypeInformation(param);
		return (IParameterTypeInformation)_genericInstanceMap.GetOrAdd(param, value2);
	}

	private static bool ContainerIsGeneric(Symbol container)
	{
		if (container.Kind != SymbolKind.Method || !((MethodSymbol)container).IsGenericMethod)
		{
			return IsGenericType(container.ContainingType);
		}
		return true;
	}

	internal ITypeReference Translate(DynamicTypeSymbol symbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		return GetSpecialType(SpecialType.System_Object, syntaxNodeOpt, diagnostics);
	}

	internal static IArrayTypeReference Translate(ArrayTypeSymbol symbol)
	{
		return symbol;
	}

	internal static IPointerTypeReference Translate(PointerTypeSymbol symbol)
	{
		return symbol;
	}

	public NamedTypeSymbol SetFixedImplementationType(SourceMemberFieldSymbol field)
	{
		if (_fixedImplementationTypes == null)
		{
			Interlocked.CompareExchange(ref _fixedImplementationTypes, new Dictionary<FieldSymbol, NamedTypeSymbol>(), null);
		}
		lock (_fixedImplementationTypes)
		{
			if (_fixedImplementationTypes.TryGetValue(field, out var value))
			{
				return value;
			}
			value = new FixedFieldImplementationType(field);
			_fixedImplementationTypes.Add(field, value);
			AddSynthesizedDefinition(value.ContainingType, value);
			return value;
		}
	}

	internal NamedTypeSymbol GetFixedImplementationType(FieldSymbol field)
	{
		_fixedImplementationTypes.TryGetValue(field, out var value);
		return value;
	}

	protected override IMethodDefinition CreatePrivateImplementationDetailsStaticConstructor(PrivateImplementationDetails details, SyntaxNode syntaxOpt, DiagnosticBag diagnostics)
	{
		return new SynthesizedPrivateImplementationDetailsStaticConstructor(SourceModule, details, GetUntranslatedSpecialType(SpecialType.System_Void, syntaxOpt, diagnostics));
	}

	internal abstract SynthesizedAttributeData SynthesizeEmbeddedAttribute();

	internal SynthesizedAttributeData SynthesizeIsReadOnlyAttribute(Symbol symbol)
	{
		if ((object)Compilation.SourceModule != symbol.ContainingModule)
		{
			return null;
		}
		return TrySynthesizeIsReadOnlyAttribute();
	}

	internal SynthesizedAttributeData SynthesizeIsUnmanagedAttribute(Symbol symbol)
	{
		if ((object)Compilation.SourceModule != symbol.ContainingModule)
		{
			return null;
		}
		return TrySynthesizeIsUnmanagedAttribute();
	}

	internal SynthesizedAttributeData SynthesizeIsByRefLikeAttribute(Symbol symbol)
	{
		if ((object)Compilation.SourceModule != symbol.ContainingModule)
		{
			return null;
		}
		return TrySynthesizeIsByRefLikeAttribute();
	}

	protected virtual SynthesizedAttributeData TrySynthesizeIsReadOnlyAttribute()
	{
		return Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_IsReadOnlyAttribute__ctor);
	}

	protected virtual SynthesizedAttributeData TrySynthesizeIsUnmanagedAttribute()
	{
		return Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_IsUnmanagedAttribute__ctor);
	}

	protected virtual SynthesizedAttributeData TrySynthesizeIsByRefLikeAttribute()
	{
		return Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_IsByRefLikeAttribute__ctor);
	}

	internal void EnsureIsReadOnlyAttributeExists()
	{
		if (!_needsGeneratedIsReadOnlyAttribute_Value && !Compilation.NeedsGeneratedIsReadOnlyAttribute && Compilation.CheckIfIsReadOnlyAttributeShouldBeEmbedded(null, null))
		{
			_needsGeneratedIsReadOnlyAttribute_Value = true;
		}
	}

	internal void EnsureIsUnmanagedAttributeExists()
	{
		if (!_needsGeneratedIsUnmanagedAttribute_Value && !Compilation.NeedsGeneratedIsUnmanagedAttribute && Compilation.CheckIfIsUnmanagedAttributeShouldBeEmbedded(null, null))
		{
			_needsGeneratedIsUnmanagedAttribute_Value = true;
		}
	}
}
