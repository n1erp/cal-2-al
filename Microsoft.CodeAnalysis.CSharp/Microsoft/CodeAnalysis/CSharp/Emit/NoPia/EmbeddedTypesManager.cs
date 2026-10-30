using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Emit.NoPia;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Emit.NoPia;

internal sealed class EmbeddedTypesManager : EmbeddedTypesManager<PEModuleBuilder, ModuleCompilationState, EmbeddedTypesManager, SyntaxNode, CSharpAttributeData, Symbol, AssemblySymbol, NamedTypeSymbol, FieldSymbol, MethodSymbol, EventSymbol, PropertySymbol, ParameterSymbol, TypeParameterSymbol, EmbeddedType, EmbeddedField, EmbeddedMethod, EmbeddedEvent, EmbeddedProperty, EmbeddedParameter, EmbeddedTypeParameter>
{
	private readonly ConcurrentDictionary<AssemblySymbol, string> _assemblyGuidMap = new ConcurrentDictionary<AssemblySymbol, string>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);

	private readonly ConcurrentDictionary<Symbol, bool> _reportedSymbolsMap = new ConcurrentDictionary<Symbol, bool>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);

	private NamedTypeSymbol _lazySystemStringType = ErrorTypeSymbol.UnknownResultType;

	private readonly MethodSymbol[] _lazyWellKnownTypeMethods;

	public EmbeddedTypesManager(PEModuleBuilder moduleBeingBuilt)
		: base(moduleBeingBuilt)
	{
		_lazyWellKnownTypeMethods = new MethodSymbol[362];
		for (int i = 0; i < _lazyWellKnownTypeMethods.Length; i++)
		{
			_lazyWellKnownTypeMethods[i] = ErrorMethodSymbol.UnknownMethod;
		}
	}

	public NamedTypeSymbol GetSystemStringType(SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		if ((object)_lazySystemStringType == ErrorTypeSymbol.UnknownResultType)
		{
			NamedTypeSymbol namedTypeSymbol = ModuleBeingBuilt.Compilation.GetSpecialType(SpecialType.System_String);
			DiagnosticInfo useSiteDiagnostic = namedTypeSymbol.GetUseSiteDiagnostic();
			if (namedTypeSymbol.IsErrorType())
			{
				namedTypeSymbol = null;
			}
			if (Interlocked.CompareExchange(ref _lazySystemStringType, namedTypeSymbol, ErrorTypeSymbol.UnknownResultType) == ErrorTypeSymbol.UnknownResultType && useSiteDiagnostic != null)
			{
				Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, diagnostics, (syntaxNodeOpt != null) ? syntaxNodeOpt.Location : NoLocation.Singleton);
			}
		}
		return _lazySystemStringType;
	}

	public MethodSymbol GetWellKnownMethod(WellKnownMember method, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		return LazyGetWellKnownTypeMethod(ref _lazyWellKnownTypeMethods[(int)method], method, syntaxNodeOpt, diagnostics);
	}

	private MethodSymbol LazyGetWellKnownTypeMethod(ref MethodSymbol lazyMethod, WellKnownMember member, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		if ((object)lazyMethod == ErrorMethodSymbol.UnknownMethod)
		{
			MethodSymbol value = (MethodSymbol)Binder.GetWellKnownTypeMember(ModuleBeingBuilt.Compilation, member, out var diagnosticInfo);
			if (diagnosticInfo != null && diagnosticInfo.Severity == DiagnosticSeverity.Error)
			{
				value = null;
			}
			if (Interlocked.CompareExchange(ref lazyMethod, value, ErrorMethodSymbol.UnknownMethod) == ErrorMethodSymbol.UnknownMethod && diagnosticInfo != null)
			{
				Symbol.ReportUseSiteDiagnostic(diagnosticInfo, diagnostics, (syntaxNodeOpt != null) ? syntaxNodeOpt.Location : NoLocation.Singleton);
			}
		}
		return lazyMethod;
	}

	internal override int GetTargetAttributeSignatureIndex(Symbol underlyingSymbol, CSharpAttributeData attrData, AttributeDescription description)
	{
		return attrData.GetTargetAttributeSignatureIndex(underlyingSymbol, description);
	}

	internal override CSharpAttributeData CreateSynthesizedAttribute(WellKnownMember constructor, CSharpAttributeData attrData, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		MethodSymbol wellKnownMethod = GetWellKnownMethod(constructor, syntaxNodeOpt, diagnostics);
		if ((object)wellKnownMethod == null)
		{
			return null;
		}
		return constructor switch
		{
			WellKnownMember.System_Runtime_InteropServices_ComEventInterfaceAttribute__ctor => (CSharpAttributeData)new SynthesizedAttributeData(wellKnownMethod, ImmutableArray.Create(attrData.CommonConstructorArguments[0], attrData.CommonConstructorArguments[0]), ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty), 
			WellKnownMember.System_Runtime_InteropServices_CoClassAttribute__ctor => new SynthesizedAttributeData(wellKnownMethod, ImmutableArray.Create(new TypedConstant(wellKnownMethod.Parameters[0].Type, TypedConstantKind.Type, wellKnownMethod.ContainingAssembly.GetSpecialType(SpecialType.System_Object))), ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty), 
			_ => new SynthesizedAttributeData(wellKnownMethod, attrData.CommonConstructorArguments, attrData.CommonNamedArguments), 
		};
	}

	internal string GetAssemblyGuidString(AssemblySymbol assembly)
	{
		if (_assemblyGuidMap.TryGetValue(assembly, out var value))
		{
			return value;
		}
		assembly.GetGuidString(out value);
		return _assemblyGuidMap.GetOrAdd(assembly, value);
	}

	protected override void OnGetTypesCompleted(ImmutableArray<EmbeddedType> types, DiagnosticBag diagnostics)
	{
		foreach (EmbeddedType item in types)
		{
			_assemblyGuidMap.TryAdd(item.UnderlyingNamedType.ContainingAssembly, null);
		}
		foreach (AssemblySymbol item2 in ModuleBeingBuilt.GetReferencedAssembliesUsedSoFar())
		{
			ReportIndirectReferencesToLinkedAssemblies(item2, diagnostics);
		}
	}

	protected override void ReportNameCollisionBetweenEmbeddedTypes(EmbeddedType typeA, EmbeddedType typeB, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol underlyingNamedType = typeA.UnderlyingNamedType;
		NamedTypeSymbol underlyingNamedType2 = typeB.UnderlyingNamedType;
		Error(diagnostics, ErrorCode.ERR_InteropTypesWithSameNameAndGuid, null, underlyingNamedType, underlyingNamedType.ContainingAssembly, underlyingNamedType2.ContainingAssembly);
	}

	protected override void ReportNameCollisionWithAlreadyDeclaredType(EmbeddedType type, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol underlyingNamedType = type.UnderlyingNamedType;
		Error(diagnostics, ErrorCode.ERR_LocalTypeNameClash, null, underlyingNamedType, underlyingNamedType.ContainingAssembly);
	}

	internal override void ReportIndirectReferencesToLinkedAssemblies(AssemblySymbol a, DiagnosticBag diagnostics)
	{
		foreach (ModuleSymbol module in a.Modules)
		{
			foreach (AssemblySymbol referencedAssemblySymbol in module.GetReferencedAssemblySymbols())
			{
				if (!referencedAssemblySymbol.IsMissing && referencedAssemblySymbol.IsLinked && _assemblyGuidMap.ContainsKey(referencedAssemblySymbol))
				{
					Error(diagnostics, ErrorCode.WRN_ReferencedAssemblyReferencesLinkedPIA, null, referencedAssemblySymbol, a);
				}
			}
		}
	}

	internal static bool IsValidEmbeddableType(NamedTypeSymbol namedType, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, EmbeddedTypesManager optTypeManager = null)
	{
		if (namedType.SpecialType != SpecialType.None || namedType.IsErrorType() || !namedType.ContainingAssembly.IsLinked)
		{
			return false;
		}
		ErrorCode errorCode = ErrorCode.Unknown;
		switch (namedType.TypeKind)
		{
		case TypeKind.Delegate:
		case TypeKind.Enum:
		case TypeKind.Interface:
		case TypeKind.Struct:
			if ((object)namedType.ContainingType != null)
			{
				errorCode = ErrorCode.ERR_NoPIANestedType;
			}
			else if (namedType.IsGenericType)
			{
				errorCode = ErrorCode.ERR_GenericsUsedInNoPIAType;
			}
			break;
		default:
			errorCode = ErrorCode.ERR_NewCoClassOnLink;
			break;
		}
		if (errorCode != ErrorCode.Unknown)
		{
			ReportNotEmbeddableSymbol(errorCode, namedType, syntaxNodeOpt, diagnostics, optTypeManager);
			return false;
		}
		return true;
	}

	private static void ReportNotEmbeddableSymbol(ErrorCode error, Symbol symbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, EmbeddedTypesManager optTypeManager)
	{
		if (optTypeManager == null || optTypeManager._reportedSymbolsMap.TryAdd(symbol.OriginalDefinition, value: true))
		{
			Error(diagnostics, error, syntaxNodeOpt, symbol.OriginalDefinition);
		}
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, SyntaxNode syntaxOpt, params object[] args)
	{
		Error(diagnostics, syntaxOpt, new CSDiagnosticInfo(code, args));
	}

	private static void Error(DiagnosticBag diagnostics, SyntaxNode syntaxOpt, DiagnosticInfo info)
	{
		diagnostics.Add(new CSDiagnostic(info, (syntaxOpt == null) ? NoLocation.Singleton : syntaxOpt.Location));
	}

	internal INamedTypeReference EmbedTypeIfNeedTo(NamedTypeSymbol namedType, bool fromImplements, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		if (IsValidEmbeddableType(namedType, syntaxNodeOpt, diagnostics, this))
		{
			return EmbedType(namedType, fromImplements, syntaxNodeOpt, diagnostics);
		}
		return namedType;
	}

	private EmbeddedType EmbedType(NamedTypeSymbol namedType, bool fromImplements, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		EmbeddedType embeddedType = new EmbeddedType(this, namedType);
		EmbeddedType orAdd = EmbeddedTypesMap.GetOrAdd(namedType, embeddedType);
		bool isInterface = namedType.IsInterface;
		if (isInterface & fromImplements)
		{
			orAdd.EmbedAllMembersOfImplementedInterface(syntaxNodeOpt, diagnostics);
		}
		if (embeddedType != orAdd)
		{
			return orAdd;
		}
		new NoPiaReferenceIndexer(new EmitContext(ModuleBeingBuilt, syntaxNodeOpt, diagnostics, metadataOnly: false, includePrivateMembers: true)).VisitTypeDefinitionNoMembers(embeddedType);
		if (!isInterface)
		{
			if (namedType.TypeKind != TypeKind.Struct)
			{
				_ = namedType.TypeKind;
			}
			foreach (FieldSymbol item in namedType.GetFieldsToEmit())
			{
				EmbedField(embeddedType, item, syntaxNodeOpt, diagnostics);
			}
			foreach (MethodSymbol item2 in namedType.GetMethodsToEmit())
			{
				if ((object)item2 != null)
				{
					EmbedMethod(embeddedType, item2, syntaxNodeOpt, diagnostics);
				}
			}
		}
		return embeddedType;
	}

	internal override EmbeddedField EmbedField(EmbeddedType type, FieldSymbol field, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		EmbeddedField embeddedField = new EmbeddedField(type, field);
		EmbeddedField orAdd = EmbeddedFieldsMap.GetOrAdd(field, embeddedField);
		if (embeddedField != orAdd)
		{
			return orAdd;
		}
		EmbedReferences(embeddedField, syntaxNodeOpt, diagnostics);
		TypeKind typeKind = field.ContainingType.TypeKind;
		if (typeKind == TypeKind.Interface || typeKind == TypeKind.Delegate || (typeKind == TypeKind.Struct && (field.IsStatic || field.DeclaredAccessibility != Accessibility.Public)))
		{
			ReportNotEmbeddableSymbol(ErrorCode.ERR_InteropStructContainsMethods, field.ContainingType, syntaxNodeOpt, diagnostics, this);
		}
		return embeddedField;
	}

	internal override EmbeddedMethod EmbedMethod(EmbeddedType type, MethodSymbol method, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		EmbeddedMethod embeddedMethod = new EmbeddedMethod(type, method);
		EmbeddedMethod orAdd = EmbeddedMethodsMap.GetOrAdd(method, embeddedMethod);
		if (embeddedMethod != orAdd)
		{
			return orAdd;
		}
		EmbedReferences(embeddedMethod, syntaxNodeOpt, diagnostics);
		TypeKind typeKind = type.UnderlyingNamedType.TypeKind;
		if (typeKind == TypeKind.Enum || typeKind == TypeKind.Struct)
		{
			ReportNotEmbeddableSymbol(ErrorCode.ERR_InteropStructContainsMethods, type.UnderlyingNamedType, syntaxNodeOpt, diagnostics, this);
		}
		else if (embeddedMethod.HasBody())
		{
			Error(diagnostics, ErrorCode.ERR_InteropMethodWithBody, syntaxNodeOpt, method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
		}
		Symbol associatedSymbol = method.AssociatedSymbol;
		if ((object)associatedSymbol != null)
		{
			switch (associatedSymbol.Kind)
			{
			case SymbolKind.Property:
				EmbedProperty(type, (PropertySymbol)associatedSymbol, syntaxNodeOpt, diagnostics);
				break;
			case SymbolKind.Event:
				EmbedEvent(type, (EventSymbol)associatedSymbol, syntaxNodeOpt, diagnostics, isUsedForComAwareEventBinding: false);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(associatedSymbol.Kind);
			}
		}
		return embeddedMethod;
	}

	internal override EmbeddedProperty EmbedProperty(EmbeddedType type, PropertySymbol property, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		MethodSymbol getMethod = property.GetMethod;
		MethodSymbol setMethod = property.SetMethod;
		EmbeddedMethod getter = (((object)getMethod != null) ? EmbedMethod(type, getMethod, syntaxNodeOpt, diagnostics) : null);
		EmbeddedMethod setter = (((object)setMethod != null) ? EmbedMethod(type, setMethod, syntaxNodeOpt, diagnostics) : null);
		EmbeddedProperty embeddedProperty = new EmbeddedProperty(property, getter, setter);
		EmbeddedProperty orAdd = EmbeddedPropertiesMap.GetOrAdd(property, embeddedProperty);
		if (embeddedProperty != orAdd)
		{
			return orAdd;
		}
		EmbedReferences(embeddedProperty, syntaxNodeOpt, diagnostics);
		return embeddedProperty;
	}

	internal override EmbeddedEvent EmbedEvent(EmbeddedType type, EventSymbol @event, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool isUsedForComAwareEventBinding)
	{
		MethodSymbol addMethod = @event.AddMethod;
		MethodSymbol removeMethod = @event.RemoveMethod;
		EmbeddedMethod adder = (((object)addMethod != null) ? EmbedMethod(type, addMethod, syntaxNodeOpt, diagnostics) : null);
		EmbeddedMethod remover = (((object)removeMethod != null) ? EmbedMethod(type, removeMethod, syntaxNodeOpt, diagnostics) : null);
		EmbeddedEvent embeddedEvent = new EmbeddedEvent(@event, adder, remover);
		EmbeddedEvent orAdd = EmbeddedEventsMap.GetOrAdd(@event, embeddedEvent);
		if (embeddedEvent != orAdd)
		{
			if (isUsedForComAwareEventBinding)
			{
				orAdd.EmbedCorrespondingComEventInterfaceMethod(syntaxNodeOpt, diagnostics, isUsedForComAwareEventBinding);
			}
			return orAdd;
		}
		EmbedReferences(embeddedEvent, syntaxNodeOpt, diagnostics);
		embeddedEvent.EmbedCorrespondingComEventInterfaceMethod(syntaxNodeOpt, diagnostics, isUsedForComAwareEventBinding);
		return embeddedEvent;
	}

	protected override EmbeddedType GetEmbeddedTypeForMember(Symbol member, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol containingType = member.ContainingType;
		if (IsValidEmbeddableType(containingType, syntaxNodeOpt, diagnostics, this))
		{
			return EmbedType(containingType, fromImplements: false, syntaxNodeOpt, diagnostics);
		}
		return null;
	}

	internal static ImmutableArray<EmbeddedParameter> EmbedParameters(CommonEmbeddedMember containingPropertyOrMethod, ImmutableArray<ParameterSymbol> underlyingParameters)
	{
		return underlyingParameters.SelectAsArray((ParameterSymbol p, CommonEmbeddedMember c) => new EmbeddedParameter(c, p), containingPropertyOrMethod);
	}

	protected override CSharpAttributeData CreateCompilerGeneratedAttribute()
	{
		return ModuleBeingBuilt.Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor);
	}
}
