using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit.NoPia;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Emit;

internal abstract class PEModuleBuilder<TCompilation, TSourceModuleSymbol, TAssemblySymbol, TTypeSymbol, TNamedTypeSymbol, TMethodSymbol, TSyntaxNode, TEmbeddedTypesManager, TModuleCompilationState> : CommonPEModuleBuilder, ITokenDeferral where TCompilation : Compilation where TSourceModuleSymbol : class, IModuleSymbol where TAssemblySymbol : class, IAssemblySymbol where TTypeSymbol : class where TNamedTypeSymbol : class, TTypeSymbol, INamespaceTypeDefinition where TMethodSymbol : class, IMethodDefinition where TSyntaxNode : SyntaxNode where TEmbeddedTypesManager : CommonEmbeddedTypesManager where TModuleCompilationState : ModuleCompilationState<TNamedTypeSymbol, TMethodSymbol>
{
	private sealed class SynthesizedDefinitions
	{
		public ConcurrentQueue<INestedTypeDefinition> NestedTypes;

		public ConcurrentQueue<IMethodDefinition> Methods;

		public ConcurrentQueue<IPropertyDefinition> Properties;

		public ConcurrentQueue<IFieldDefinition> Fields;

		public ImmutableArray<ITypeDefinitionMember> GetAllMembers()
		{
			ArrayBuilder<ITypeDefinitionMember> instance = ArrayBuilder<ITypeDefinitionMember>.GetInstance();
			if (Fields != null)
			{
				foreach (IFieldDefinition field in Fields)
				{
					instance.Add(field);
				}
			}
			if (Methods != null)
			{
				foreach (IMethodDefinition method in Methods)
				{
					instance.Add(method);
				}
			}
			if (Properties != null)
			{
				foreach (IPropertyDefinition property in Properties)
				{
					instance.Add(property);
				}
			}
			if (NestedTypes != null)
			{
				foreach (INestedTypeDefinition nestedType in NestedTypes)
				{
					instance.Add(nestedType);
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	private readonly RootModuleType _rootModuleType = new RootModuleType();

	internal readonly TSourceModuleSymbol SourceModule;

	internal readonly TCompilation Compilation;

	private PrivateImplementationDetails _privateImplementationDetails;

	private ArrayMethods _lazyArrayMethods;

	private HashSet<string> _namesOfTopLevelTypes;

	internal readonly TModuleCompilationState CompilationState;

	private readonly ConcurrentDictionary<TNamedTypeSymbol, SynthesizedDefinitions> _synthesizedDefs = new ConcurrentDictionary<TNamedTypeSymbol, SynthesizedDefinitions>();

	public abstract TEmbeddedTypesManager EmbeddedTypesManagerOpt { get; }

	internal override IAssemblySymbol CommonCorLibrary => CorLibrary;

	internal abstract TAssemblySymbol CorLibrary { get; }

	protected bool HaveDeterminedTopLevelTypes => _namesOfTopLevelTypes != null;

	internal sealed override IModuleSymbol CommonSourceModule => SourceModule;

	internal sealed override Compilation CommonCompilation => Compilation;

	internal sealed override CommonModuleCompilationState CommonModuleCompilationState => CompilationState;

	internal sealed override CommonEmbeddedTypesManager CommonEmbeddedTypesManagerOpt => EmbeddedTypesManagerOpt;

	public ArrayMethods ArrayMethods
	{
		get
		{
			ArrayMethods arrayMethods = _lazyArrayMethods;
			if (arrayMethods == null)
			{
				arrayMethods = new ArrayMethods();
				if (Interlocked.CompareExchange(ref _lazyArrayMethods, arrayMethods, null) != null)
				{
					arrayMethods = _lazyArrayMethods;
				}
			}
			return arrayMethods;
		}
	}

	internal PrivateImplementationDetails PrivateImplClass => _privateImplementationDetails;

	internal override bool SupportsPrivateImplClass => true;

	protected PEModuleBuilder(TCompilation compilation, TSourceModuleSymbol sourceModule, ModulePropertiesForSerialization serializationProperties, IEnumerable<ResourceDescription> manifestResources, OutputKind outputKind, EmitOptions emitOptions, TModuleCompilationState compilationState)
		: base(manifestResources, emitOptions, outputKind, serializationProperties, compilation)
	{
		Compilation = compilation;
		SourceModule = sourceModule;
		CompilationState = compilationState;
	}

	internal sealed override void CompilationFinished()
	{
		CompilationState.Freeze();
	}

	internal abstract INamedTypeReference GetSystemType(TSyntaxNode syntaxOpt, DiagnosticBag diagnostics);

	internal abstract INamedTypeReference GetSpecialType(SpecialType specialType, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

	internal sealed override ITypeReference EncTranslateType(ITypeSymbol type, DiagnosticBag diagnostics)
	{
		return EncTranslateLocalVariableType((TTypeSymbol)type, diagnostics);
	}

	internal virtual ITypeReference EncTranslateLocalVariableType(TTypeSymbol type, DiagnosticBag diagnostics)
	{
		return Translate(type, null, diagnostics);
	}

	protected bool ContainsTopLevelType(string fullEmittedName)
	{
		return _namesOfTopLevelTypes.Contains(fullEmittedName);
	}

	internal abstract IEnumerable<INamespaceTypeDefinition> GetTopLevelTypesCore(EmitContext context);

	public override IEnumerable<INamespaceTypeDefinition> GetTopLevelTypes(EmitContext context)
	{
		NoPiaReferenceIndexer noPiaIndexer = null;
		HashSet<string> names = ((_namesOfTopLevelTypes != null) ? null : new HashSet<string>());
		if (EmbeddedTypesManagerOpt != null && !EmbeddedTypesManagerOpt.IsFrozen)
		{
			noPiaIndexer = new NoPiaReferenceIndexer(context);
			Dispatch(noPiaIndexer);
		}
		AddTopLevelType(names, _rootModuleType);
		VisitTopLevelType(noPiaIndexer, _rootModuleType);
		yield return _rootModuleType;
		foreach (INamespaceTypeDefinition anonymousType in GetAnonymousTypes(context))
		{
			AddTopLevelType(names, anonymousType);
			VisitTopLevelType(noPiaIndexer, anonymousType);
			yield return anonymousType;
		}
		foreach (INamespaceTypeDefinition item in GetTopLevelTypesCore(context))
		{
			AddTopLevelType(names, item);
			VisitTopLevelType(noPiaIndexer, item);
			yield return item;
		}
		PrivateImplementationDetails privateImplClass = PrivateImplClass;
		if (privateImplClass != null)
		{
			AddTopLevelType(names, privateImplClass);
			VisitTopLevelType(noPiaIndexer, privateImplClass);
			yield return privateImplClass;
		}
		if (EmbeddedTypesManagerOpt != null)
		{
			foreach (INamespaceTypeDefinition type in EmbeddedTypesManagerOpt.GetTypes(context.Diagnostics, names))
			{
				AddTopLevelType(names, type);
				yield return type;
			}
		}
		if (names != null)
		{
			_namesOfTopLevelTypes = names;
		}
	}

	internal abstract IAssemblyReference Translate(TAssemblySymbol symbol, DiagnosticBag diagnostics);

	internal abstract ITypeReference Translate(TTypeSymbol symbol, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

	internal abstract IMethodReference Translate(TMethodSymbol symbol, DiagnosticBag diagnostics, bool needDeclaration);

	internal sealed override IAssemblyReference Translate(IAssemblySymbol symbol, DiagnosticBag diagnostics)
	{
		return Translate((TAssemblySymbol)symbol, diagnostics);
	}

	internal sealed override ITypeReference Translate(ITypeSymbol symbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		return Translate((TTypeSymbol)symbol, (TSyntaxNode)syntaxNodeOpt, diagnostics);
	}

	internal sealed override IMethodReference Translate(IMethodSymbol symbol, DiagnosticBag diagnostics, bool needDeclaration)
	{
		return Translate((TMethodSymbol)symbol, diagnostics, needDeclaration);
	}

	internal MetadataConstant CreateConstant(TTypeSymbol type, object value, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		return new MetadataConstant(Translate(type, syntaxNodeOpt, diagnostics), value);
	}

	private static void AddTopLevelType(HashSet<string> names, INamespaceTypeDefinition type)
	{
		names?.Add(MetadataHelpers.BuildQualifiedName(type.NamespaceName, MetadataWriter.GetMangledName(type)));
	}

	private static void VisitTopLevelType(NoPiaReferenceIndexer noPiaIndexer, INamespaceTypeDefinition type)
	{
		noPiaIndexer?.Visit((ITypeDefinition)type);
	}

	internal IFieldReference GetModuleVersionId(ITypeReference mvidType, TSyntaxNode syntaxOpt, DiagnosticBag diagnostics)
	{
		PrivateImplementationDetails privateImplClass = GetPrivateImplClass(syntaxOpt, diagnostics);
		EnsurePrivateImplementationDetailsStaticConstructor(privateImplClass, syntaxOpt, diagnostics);
		return privateImplClass.GetModuleVersionId(mvidType);
	}

	internal IFieldReference GetInstrumentationPayloadRoot(int analysisKind, ITypeReference payloadType, TSyntaxNode syntaxOpt, DiagnosticBag diagnostics)
	{
		PrivateImplementationDetails privateImplClass = GetPrivateImplClass(syntaxOpt, diagnostics);
		EnsurePrivateImplementationDetailsStaticConstructor(privateImplClass, syntaxOpt, diagnostics);
		return privateImplClass.GetOrAddInstrumentationPayloadRoot(analysisKind, payloadType);
	}

	private void EnsurePrivateImplementationDetailsStaticConstructor(PrivateImplementationDetails details, TSyntaxNode syntaxOpt, DiagnosticBag diagnostics)
	{
		if (details.GetMethod(".cctor") == null)
		{
			details.TryAddSynthesizedMethod(CreatePrivateImplementationDetailsStaticConstructor(details, syntaxOpt, diagnostics));
		}
	}

	protected abstract IMethodDefinition CreatePrivateImplementationDetailsStaticConstructor(PrivateImplementationDetails details, TSyntaxNode syntaxOpt, DiagnosticBag diagnostics);

	public void AddSynthesizedDefinition(TNamedTypeSymbol container, INestedTypeDefinition nestedType)
	{
		SynthesizedDefinitions cacheOfSynthesizedDefinitions = GetCacheOfSynthesizedDefinitions(container);
		if (cacheOfSynthesizedDefinitions.NestedTypes == null)
		{
			Interlocked.CompareExchange(ref cacheOfSynthesizedDefinitions.NestedTypes, new ConcurrentQueue<INestedTypeDefinition>(), null);
		}
		cacheOfSynthesizedDefinitions.NestedTypes.Enqueue(nestedType);
	}

	internal abstract IEnumerable<INestedTypeDefinition> GetSynthesizedNestedTypes(TNamedTypeSymbol container);

	public IEnumerable<INestedTypeDefinition> GetSynthesizedTypes(TNamedTypeSymbol container)
	{
		IEnumerable<INestedTypeDefinition> synthesizedNestedTypes = GetSynthesizedNestedTypes(container);
		IEnumerable<INestedTypeDefinition> enumerable = null;
		SynthesizedDefinitions cacheOfSynthesizedDefinitions = GetCacheOfSynthesizedDefinitions(container, addIfNotFound: false);
		if (cacheOfSynthesizedDefinitions != null)
		{
			enumerable = cacheOfSynthesizedDefinitions.NestedTypes;
		}
		if (synthesizedNestedTypes == null)
		{
			return enumerable;
		}
		if (enumerable == null)
		{
			return synthesizedNestedTypes;
		}
		return synthesizedNestedTypes.Concat(enumerable);
	}

	private SynthesizedDefinitions GetCacheOfSynthesizedDefinitions(TNamedTypeSymbol container, bool addIfNotFound = true)
	{
		if (addIfNotFound)
		{
			return _synthesizedDefs.GetOrAdd(container, (TNamedTypeSymbol _) => new SynthesizedDefinitions());
		}
		_synthesizedDefs.TryGetValue(container, out var value);
		return value;
	}

	public void AddSynthesizedDefinition(TNamedTypeSymbol container, IMethodDefinition method)
	{
		SynthesizedDefinitions cacheOfSynthesizedDefinitions = GetCacheOfSynthesizedDefinitions(container);
		if (cacheOfSynthesizedDefinitions.Methods == null)
		{
			Interlocked.CompareExchange(ref cacheOfSynthesizedDefinitions.Methods, new ConcurrentQueue<IMethodDefinition>(), null);
		}
		cacheOfSynthesizedDefinitions.Methods.Enqueue(method);
	}

	public IEnumerable<IMethodDefinition> GetSynthesizedMethods(TNamedTypeSymbol container)
	{
		return GetCacheOfSynthesizedDefinitions(container, addIfNotFound: false)?.Methods;
	}

	public void AddSynthesizedDefinition(TNamedTypeSymbol container, IPropertyDefinition property)
	{
		SynthesizedDefinitions cacheOfSynthesizedDefinitions = GetCacheOfSynthesizedDefinitions(container);
		if (cacheOfSynthesizedDefinitions.Properties == null)
		{
			Interlocked.CompareExchange(ref cacheOfSynthesizedDefinitions.Properties, new ConcurrentQueue<IPropertyDefinition>(), null);
		}
		cacheOfSynthesizedDefinitions.Properties.Enqueue(property);
	}

	public IEnumerable<IPropertyDefinition> GetSynthesizedProperties(TNamedTypeSymbol container)
	{
		return GetCacheOfSynthesizedDefinitions(container, addIfNotFound: false)?.Properties;
	}

	public void AddSynthesizedDefinition(TNamedTypeSymbol container, IFieldDefinition field)
	{
		SynthesizedDefinitions cacheOfSynthesizedDefinitions = GetCacheOfSynthesizedDefinitions(container);
		if (cacheOfSynthesizedDefinitions.Fields == null)
		{
			Interlocked.CompareExchange(ref cacheOfSynthesizedDefinitions.Fields, new ConcurrentQueue<IFieldDefinition>(), null);
		}
		cacheOfSynthesizedDefinitions.Fields.Enqueue(field);
	}

	public IEnumerable<IFieldDefinition> GetSynthesizedFields(TNamedTypeSymbol container)
	{
		return GetCacheOfSynthesizedDefinitions(container, addIfNotFound: false)?.Fields;
	}

	internal override ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> GetSynthesizedMembers()
	{
		ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>>.Builder builder = ImmutableDictionary.CreateBuilder<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>>();
		foreach (KeyValuePair<TNamedTypeSymbol, SynthesizedDefinitions> synthesizedDef in _synthesizedDefs)
		{
			builder.Add(synthesizedDef.Key, synthesizedDef.Value.GetAllMembers());
		}
		return builder.ToImmutable();
	}

	public ImmutableArray<ITypeDefinitionMember> GetSynthesizedMembers(ITypeDefinition container)
	{
		return GetCacheOfSynthesizedDefinitions((TNamedTypeSymbol)container, addIfNotFound: false)?.GetAllMembers() ?? ImmutableArray<ITypeDefinitionMember>.Empty;
	}

	IFieldReference ITokenDeferral.GetFieldForData(ImmutableArray<byte> data, SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		return GetPrivateImplClass((TSyntaxNode)syntaxNode, diagnostics).CreateDataField(data);
	}

	public abstract IMethodReference GetInitArrayHelper();

	internal PrivateImplementationDetails GetPrivateImplClass(TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		PrivateImplementationDetails privateImplementationDetails = _privateImplementationDetails;
		if (privateImplementationDetails == null && SupportsPrivateImplClass)
		{
			privateImplementationDetails = new PrivateImplementationDetails(this, SourceModule.Name, Compilation.GetSubmissionSlotIndex(), GetSpecialType(SpecialType.System_Object, syntaxNodeOpt, diagnostics), GetSpecialType(SpecialType.System_ValueType, syntaxNodeOpt, diagnostics), GetSpecialType(SpecialType.System_Byte, syntaxNodeOpt, diagnostics), GetSpecialType(SpecialType.System_Int16, syntaxNodeOpt, diagnostics), GetSpecialType(SpecialType.System_Int32, syntaxNodeOpt, diagnostics), GetSpecialType(SpecialType.System_Int64, syntaxNodeOpt, diagnostics), SynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
			if (Interlocked.CompareExchange(ref _privateImplementationDetails, privateImplementationDetails, null) != null)
			{
				privateImplementationDetails = _privateImplementationDetails;
			}
		}
		return privateImplementationDetails;
	}

	public sealed override ITypeReference GetPlatformType(PlatformType platformType, EmitContext context)
	{
		if (platformType == PlatformType.SystemType)
		{
			return GetSystemType((TSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
		}
		return GetSpecialType((SpecialType)platformType, (TSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
	}
}
