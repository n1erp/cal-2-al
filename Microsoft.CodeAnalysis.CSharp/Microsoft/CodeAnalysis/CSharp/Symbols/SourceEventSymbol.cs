using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceEventSymbol : EventSymbol, IAttributeTargetSymbol
{
	private readonly Location _location;

	private readonly SyntaxReference _syntaxRef;

	private readonly DeclarationModifiers _modifiers;

	internal readonly SourceMemberContainerTypeSymbol containingType;

	protected SymbolCompletionState state;

	private CustomAttributesBag<CSharpAttributeData> _lazyCustomAttributesBag;

	private string _lazyDocComment;

	private OverriddenOrHiddenMembersResult _lazyOverriddenOrHiddenMembers;

	private ThreeState _lazyIsWindowsRuntimeEvent;

	internal sealed override bool RequiresCompletion => true;

	public abstract override string Name { get; }

	public abstract override MethodSymbol AddMethod { get; }

	public abstract override MethodSymbol RemoveMethod { get; }

	public abstract override ImmutableArray<EventSymbol> ExplicitInterfaceImplementations { get; }

	public abstract override TypeSymbol Type { get; }

	public sealed override Symbol ContainingSymbol => containingType;

	public override NamedTypeSymbol ContainingType => containingType;

	public sealed override ImmutableArray<Location> Locations => ImmutableArray.Create(_location);

	public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(_syntaxRef);

	internal SyntaxList<AttributeListSyntax> AttributeDeclarationSyntaxList
	{
		get
		{
			if (containingType.AnyMemberHasAttributes)
			{
				CSharpSyntaxNode cSharpSyntaxNode = CSharpSyntaxNode;
				if (cSharpSyntaxNode != null)
				{
					return cSharpSyntaxNode.Kind() switch
					{
						SyntaxKind.EventDeclaration => ((EventDeclarationSyntax)cSharpSyntaxNode).AttributeLists, 
						SyntaxKind.VariableDeclarator => ((EventFieldDeclarationSyntax)cSharpSyntaxNode.Parent.Parent).AttributeLists, 
						_ => throw ExceptionUtilities.UnexpectedValue(cSharpSyntaxNode.Kind()), 
					};
				}
			}
			return default;
		}
	}

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => this;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.Event;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations => AllowedAttributeLocations;

	protected abstract AttributeLocation AllowedAttributeLocations { get; }

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			if (!containingType.AnyMemberHasAttributes)
			{
				return null;
			}
			CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
			if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
			{
				return ((CommonEventEarlyWellKnownAttributeData)lazyCustomAttributesBag.EarlyDecodedWellKnownAttributeData)?.ObsoleteAttributeData;
			}
			return ObsoleteAttributeData.Uninitialized;
		}
	}

	internal sealed override bool IsDirectlyExcludedFromCodeCoverage => GetDecodedWellKnownAttributeData()?.HasExcludeFromCodeCoverageAttribute ?? false;

	internal sealed override bool HasSpecialName => GetDecodedWellKnownAttributeData()?.HasSpecialNameAttribute ?? false;

	public sealed override bool IsAbstract => (_modifiers & DeclarationModifiers.Abstract) != 0;

	public sealed override bool IsExtern => (_modifiers & DeclarationModifiers.Extern) != 0;

	public sealed override bool IsStatic => (_modifiers & DeclarationModifiers.Static) != 0;

	public sealed override bool IsOverride => (_modifiers & DeclarationModifiers.Override) != 0;

	public sealed override bool IsSealed => (_modifiers & DeclarationModifiers.Sealed) != 0;

	public sealed override bool IsVirtual => (_modifiers & DeclarationModifiers.Virtual) != 0;

	public sealed override Accessibility DeclaredAccessibility => ModifierUtils.EffectiveAccessibility(_modifiers);

	internal sealed override bool MustCallMethodsDirectly => false;

	internal SyntaxReference SyntaxReference => _syntaxRef;

	internal CSharpSyntaxNode CSharpSyntaxNode => (CSharpSyntaxNode)_syntaxRef.GetSyntax();

	internal SyntaxTree SyntaxTree => _syntaxRef.SyntaxTree;

	internal bool IsNew => (_modifiers & DeclarationModifiers.New) != 0;

	internal DeclarationModifiers Modifiers => _modifiers;

	internal override OverriddenOrHiddenMembersResult OverriddenOrHiddenMembers
	{
		get
		{
			if (_lazyOverriddenOrHiddenMembers == null)
			{
				Interlocked.CompareExchange(ref _lazyOverriddenOrHiddenMembers, this.MakeOverriddenOrHiddenMembers(), null);
			}
			return _lazyOverriddenOrHiddenMembers;
		}
	}

	public sealed override bool IsWindowsRuntimeEvent
	{
		get
		{
			if (!_lazyIsWindowsRuntimeEvent.HasValue())
			{
				_lazyIsWindowsRuntimeEvent = ComputeIsWindowsRuntimeEvent().ToThreeState();
			}
			return _lazyIsWindowsRuntimeEvent.Value();
		}
	}

	internal SourceEventSymbol(SourceMemberContainerTypeSymbol containingType, CSharpSyntaxNode syntax, SyntaxTokenList modifiers, ExplicitInterfaceSpecifierSyntax interfaceSpecifierSyntaxOpt, SyntaxToken nameTokenSyntax, DiagnosticBag diagnostics)
	{
		_location = nameTokenSyntax.GetLocation();
		this.containingType = containingType;
		_syntaxRef = syntax.GetReference();
		bool explicitInterfaceImplementation = interfaceSpecifierSyntaxOpt != null;
		_modifiers = MakeModifiers(modifiers, explicitInterfaceImplementation, _location, diagnostics, out var _);
		CheckAccessibility(_location, diagnostics);
	}

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		state.DefaultForceComplete(this);
	}

	internal override LexicalSortKey GetLexicalSortKey()
	{
		return new LexicalSortKey(_location, DeclaringCompilation);
	}

	private CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		if ((_lazyCustomAttributesBag == null || !_lazyCustomAttributesBag.IsSealed) && LoadAndValidateAttributes(OneOrMany.Create(AttributeDeclarationSyntaxList), ref _lazyCustomAttributesBag))
		{
			DeclaringCompilation.SymbolDeclaredEvent(this);
			state.NotePartComplete(CompletionPart.Attributes);
		}
		return _lazyCustomAttributesBag;
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return GetAttributesBag().Attributes;
	}

	protected CommonEventWellKnownAttributeData GetDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (CommonEventWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	internal CommonEventEarlyWellKnownAttributeData GetEarlyDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (CommonEventEarlyWellKnownAttributeData)customAttributesBag.EarlyDecodedWellKnownAttributeData;
	}

	internal override CSharpAttributeData EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		if (Symbol.EarlyDecodeDeprecatedOrExperimentalOrObsoleteAttribute(ref arguments, out var attributeData, out var obsoleteData))
		{
			if (obsoleteData != null)
			{
				arguments.GetOrCreateData<CommonEventEarlyWellKnownAttributeData>().ObsoleteAttributeData = obsoleteData;
			}
			return attributeData;
		}
		return base.EarlyDecodeWellKnownAttribute(ref arguments);
	}

	internal sealed override void DecodeWellKnownAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		if (attribute.IsTargetAttribute(this, AttributeDescription.SpecialNameAttribute))
		{
			arguments.GetOrCreateData<CommonEventWellKnownAttributeData>().HasSpecialNameAttribute = true;
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.ExcludeFromCodeCoverageAttribute))
		{
			arguments.GetOrCreateData<CommonEventWellKnownAttributeData>().HasExcludeFromCodeCoverageAttribute = true;
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.TupleElementNamesAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitTupleElementNamesAttribute, arguments.AttributeSyntaxOpt.Location);
		}
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		if (Type.ContainsDynamic())
		{
			CSharpCompilation declaringCompilation = DeclaringCompilation;
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDynamicAttribute(Type, 0));
		}
		if (Type.ContainsTupleNames())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, DeclaringCompilation.SynthesizeTupleNamesAttribute(Type));
		}
	}

	private void CheckAccessibility(Location location, DiagnosticBag diagnostics)
	{
		CSDiagnosticInfo cSDiagnosticInfo = ModifierUtils.CheckAccessibility(_modifiers);
		if (cSDiagnosticInfo != null)
		{
			diagnostics.Add(new CSDiagnostic(cSDiagnosticInfo, location));
		}
	}

	private DeclarationModifiers MakeModifiers(SyntaxTokenList modifiers, bool explicitInterfaceImplementation, Location location, DiagnosticBag diagnostics, out bool modifierErrors)
	{
		bool isInterface = ContainingType.IsInterface;
		DeclarationModifiers defaultAccess = (isInterface ? DeclarationModifiers.Public : DeclarationModifiers.Private);
		DeclarationModifiers declarationModifiers = DeclarationModifiers.Unsafe;
		if (!explicitInterfaceImplementation)
		{
			declarationModifiers |= DeclarationModifiers.New;
			if (!isInterface)
			{
				declarationModifiers |= DeclarationModifiers.AccessibilityMask | DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.Static | DeclarationModifiers.Virtual | DeclarationModifiers.Override;
			}
		}
		if (!isInterface)
		{
			declarationModifiers |= DeclarationModifiers.Extern;
		}
		DeclarationModifiers declarationModifiers2 = ModifierUtils.MakeAndCheckNontypeMemberModifiers(modifiers, defaultAccess, declarationModifiers, location, diagnostics, out modifierErrors);
		this.CheckUnsafeModifier(declarationModifiers2, diagnostics);
		if (isInterface)
		{
			declarationModifiers2 = (declarationModifiers2 & ~DeclarationModifiers.AccessibilityMask) | DeclarationModifiers.Abstract | DeclarationModifiers.Public;
		}
		return declarationModifiers2;
	}

	protected void CheckModifiersAndType(DiagnosticBag diagnostics)
	{
		Location location = Locations[0];
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (DeclaredAccessibility == Accessibility.Private && (IsVirtual || IsAbstract || IsOverride))
		{
			diagnostics.Add(ErrorCode.ERR_VirtualPrivate, location, this);
		}
		else if (IsStatic && (IsOverride || IsVirtual || IsAbstract))
		{
			diagnostics.Add(ErrorCode.ERR_StaticNotVirtual, location, this);
		}
		else if (IsOverride && (IsNew || IsVirtual))
		{
			diagnostics.Add(ErrorCode.ERR_OverrideNotNew, location, this);
		}
		else if (IsSealed && !IsOverride)
		{
			diagnostics.Add(ErrorCode.ERR_SealedNonOverride, location, this);
		}
		else if (IsAbstract && ContainingType.TypeKind == TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, location, SyntaxFacts.GetText(SyntaxKind.AbstractKeyword));
		}
		else if (IsVirtual && ContainingType.TypeKind == TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, location, SyntaxFacts.GetText(SyntaxKind.VirtualKeyword));
		}
		else if (IsAbstract && IsExtern)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractAndExtern, location, this);
		}
		else if (IsAbstract && IsSealed)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractAndSealed, location, this);
		}
		else if (IsAbstract && IsVirtual)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractNotVirtual, location, Kind.Localize(), this);
		}
		else if (ContainingType.IsSealed && DeclaredAccessibility.HasProtected() && !IsOverride)
		{
			diagnostics.Add(AccessCheck.GetProtectedMemberInSealedTypeError(ContainingType), location, this);
		}
		else if (ContainingType.IsStatic && !IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_InstanceMemberInStaticClass, location, Name);
		}
		else if (Type.SpecialType != SpecialType.System_Void)
		{
			if (!this.IsNoMoreVisibleThan(Type, ref useSiteDiagnostics))
			{
				diagnostics.Add(ErrorCode.ERR_BadVisEventType, location, this, Type);
			}
			else if (!Type.IsDelegateType() && !Type.IsErrorType())
			{
				diagnostics.Add(ErrorCode.ERR_EventNotDelegate, location, this);
			}
			else if (IsAbstract && !ContainingType.IsAbstract && (ContainingType.TypeKind == TypeKind.Class || ContainingType.TypeKind == TypeKind.Submission))
			{
				diagnostics.Add(ErrorCode.ERR_AbstractInConcreteClass, location, this, ContainingType);
			}
			else if (IsVirtual && ContainingType.IsSealed)
			{
				diagnostics.Add(ErrorCode.ERR_NewVirtualInSealed, location, this, ContainingType);
			}
		}
		diagnostics.Add(location, useSiteDiagnostics);
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return SourceDocumentationCommentUtils.GetAndCacheDocumentationComment(this, expandIncludes, ref _lazyDocComment);
	}

	protected static void CopyEventCustomModifiers(EventSymbol eventWithCustomModifiers, ref TypeSymbol type, AssemblySymbol containingAssembly)
	{
		TypeSymbol type2 = eventWithCustomModifiers.Type;
		if (type.Equals(type2, TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds | TypeCompareKind.IgnoreDynamic))
		{
			type = CustomModifierUtils.CopyTypeCustomModifiers(type2, type, containingAssembly);
		}
	}

	private bool ComputeIsWindowsRuntimeEvent()
	{
		if (containingType.IsInterfaceType())
		{
			return this.IsCompilationOutputWinMdObj();
		}
		ImmutableArray<EventSymbol> explicitInterfaceImplementations = ExplicitInterfaceImplementations;
		if (!explicitInterfaceImplementations.IsEmpty)
		{
			return explicitInterfaceImplementations[0].IsWindowsRuntimeEvent;
		}
		EventSymbol overriddenEvent = OverriddenEvent;
		if ((object)overriddenEvent != null)
		{
			return overriddenEvent.IsWindowsRuntimeEvent;
		}
		bool flag = false;
		foreach (NamedTypeSymbol interfacesAndTheirBaseInterfacesNoUseSiteDiagnostic in containingType.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics)
		{
			foreach (Symbol member in interfacesAndTheirBaseInterfacesNoUseSiteDiagnostic.GetMembers(Name))
			{
				if (member.Kind == SymbolKind.Event && this == containingType.FindImplementationForInterfaceMember(member))
				{
					flag = true;
					if (((EventSymbol)member).IsWindowsRuntimeEvent)
					{
						return true;
					}
				}
			}
		}
		if (flag)
		{
			return false;
		}
		return this.IsCompilationOutputWinMdObj();
	}

	internal static string GetAccessorName(string eventName, bool isAdder)
	{
		return (isAdder ? "add_" : "remove_") + eventName;
	}

	protected TypeSymbol BindEventType(Binder binder, TypeSyntax typeSyntax, DiagnosticBag diagnostics)
	{
		binder = binder.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks | BinderFlags.SuppressUnsafeDiagnostics, this);
		return binder.BindType(typeSyntax, diagnostics);
	}

	internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, DiagnosticBag diagnostics)
	{
		CheckModifiersAndType(diagnostics);
		Type.CheckAllConstraints(conversions, Locations[0], diagnostics);
	}
}
