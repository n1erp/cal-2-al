using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourcePropertySymbol : PropertySymbol, IAttributeTargetSymbol
{
	private const string DefaultIndexerName = "Item";

	private readonly SourceMemberContainerTypeSymbol _containingType;

	private readonly string _name;

	private readonly SyntaxReference _syntaxRef;

	private readonly Location _location;

	private readonly DeclarationModifiers _modifiers;

	private readonly CustomModifiersTuple _customModifiers;

	private readonly SourcePropertyAccessorSymbol _getMethod;

	private readonly SourcePropertyAccessorSymbol _setMethod;

	private readonly SynthesizedBackingFieldSymbol _backingField;

	private readonly TypeSymbol _explicitInterfaceType;

	private readonly ImmutableArray<PropertySymbol> _explicitInterfaceImplementations;

	private readonly bool _isExpressionBodied;

	private readonly bool _isAutoProperty;

	private readonly RefKind _refKind;

	private SymbolCompletionState _state;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private TypeSymbol _lazyType;

	private readonly string _sourceName;

	private string _lazyDocComment;

	private OverriddenOrHiddenMembersResult _lazyOverriddenOrHiddenMembers;

	private SynthesizedSealedPropertyAccessor _lazySynthesizedSealedAccessor;

	private CustomAttributesBag<CSharpAttributeData> _lazyCustomAttributesBag;

	internal bool IsExpressionBodied => _isExpressionBodied;

	public override RefKind RefKind => _refKind;

	public override TypeSymbol Type
	{
		get
		{
			if ((object)_lazyType == null)
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				Binder binder = CreateBinderForTypeAndParameters();
				BasePropertyDeclarationSyntax syntax = (BasePropertyDeclarationSyntax)_syntaxRef.GetSyntax();
				TypeSymbol value = ComputeType(binder, syntax, instance);
				if ((object)Interlocked.CompareExchange(ref _lazyType, value, null) == null)
				{
					AddDeclarationDiagnostics(instance);
				}
				instance.Free();
			}
			return _lazyType;
		}
	}

	internal bool HasPointerType
	{
		get
		{
			if ((object)_lazyType != null)
			{
				return _lazyType.IsPointerType();
			}
			RefKind refKind;
			return ((BasePropertyDeclarationSyntax)_syntaxRef.GetSyntax()).Type.SkipRef(out refKind).Kind() == SyntaxKind.PointerType;
		}
	}

	public override string Name => _name;

	public override string MetadataName => _sourceName.Replace(" ", "");

	public override Symbol ContainingSymbol => _containingType;

	public override NamedTypeSymbol ContainingType => _containingType;

	public override ImmutableArray<Location> Locations => ImmutableArray.Create(_location);

	internal Location Location => _location;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(_syntaxRef);

	public override bool IsAbstract => (_modifiers & DeclarationModifiers.Abstract) != 0;

	public override bool IsExtern => (_modifiers & DeclarationModifiers.Extern) != 0;

	public override bool IsStatic => (_modifiers & DeclarationModifiers.Static) != 0;

	internal bool IsFixed => false;

	public override bool IsIndexer => (_modifiers & DeclarationModifiers.Indexer) != 0;

	public override bool IsOverride => (_modifiers & DeclarationModifiers.Override) != 0;

	public override bool IsSealed => (_modifiers & DeclarationModifiers.Sealed) != 0;

	public override bool IsVirtual => (_modifiers & DeclarationModifiers.Virtual) != 0;

	internal bool IsNew => (_modifiers & DeclarationModifiers.New) != 0;

	public override MethodSymbol GetMethod => _getMethod;

	public override MethodSymbol SetMethod => _setMethod;

	internal override CallingConvention CallingConvention
	{
		get
		{
			if (!IsStatic)
			{
				return CallingConvention.HasThis;
			}
			return CallingConvention.Default;
		}
	}

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			if (_lazyParameters.IsDefault)
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				Binder binder = CreateBinderForTypeAndParameters();
				BasePropertyDeclarationSyntax syntax = (BasePropertyDeclarationSyntax)_syntaxRef.GetSyntax();
				ImmutableArray<ParameterSymbol> value = ComputeParameters(binder, syntax, instance);
				if (ImmutableInterlocked.InterlockedInitialize(ref _lazyParameters, value))
				{
					AddDeclarationDiagnostics(instance);
				}
				instance.Free();
			}
			return _lazyParameters;
		}
	}

	internal override bool IsExplicitInterfaceImplementation => CSharpSyntaxNode.ExplicitInterfaceSpecifier != null;

	public override ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations => _explicitInterfaceImplementations;

	public override ImmutableArray<CustomModifier> TypeCustomModifiers => _customModifiers.TypeCustomModifiers;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _customModifiers.RefCustomModifiers;

	public override Accessibility DeclaredAccessibility => ModifierUtils.EffectiveAccessibility(_modifiers);

	internal bool IsAutoProperty => _isAutoProperty;

	internal SynthesizedBackingFieldSymbol BackingField => _backingField;

	internal override bool MustCallMethodsDirectly => false;

	internal SyntaxReference SyntaxReference => _syntaxRef;

	internal BasePropertyDeclarationSyntax CSharpSyntaxNode => (BasePropertyDeclarationSyntax)_syntaxRef.GetSyntax();

	internal SyntaxTree SyntaxTree => _syntaxRef.SyntaxTree;

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

	internal SynthesizedSealedPropertyAccessor SynthesizedSealedAccessorOpt
	{
		get
		{
			bool flag = (object)_getMethod != null;
			bool flag2 = (object)_setMethod != null;
			if (!IsSealed || (flag & flag2))
			{
				return null;
			}
			if ((object)_lazySynthesizedSealedAccessor == null)
			{
				Interlocked.CompareExchange(ref _lazySynthesizedSealedAccessor, MakeSynthesizedSealedAccessor(), null);
			}
			return _lazySynthesizedSealedAccessor;
		}
	}

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => this;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.Property;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations
	{
		get
		{
			if (!_isAutoProperty)
			{
				return AttributeLocation.Property;
			}
			return AttributeLocation.Field | AttributeLocation.Property;
		}
	}

	internal sealed override bool IsDirectlyExcludedFromCodeCoverage => GetDecodedWellKnownAttributeData()?.HasExcludeFromCodeCoverageAttribute ?? false;

	internal override bool HasSpecialName => GetDecodedWellKnownAttributeData()?.HasSpecialNameAttribute ?? false;

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			if (!_containingType.AnyMemberHasAttributes)
			{
				return null;
			}
			CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
			if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
			{
				return ((PropertyEarlyWellKnownAttributeData)lazyCustomAttributesBag.EarlyDecodedWellKnownAttributeData)?.ObsoleteAttributeData;
			}
			return ObsoleteAttributeData.Uninitialized;
		}
	}

	internal sealed override bool RequiresCompletion => true;

	private SourcePropertySymbol(SourceMemberContainerTypeSymbol containingType, Binder bodyBinder, BasePropertyDeclarationSyntax syntax, string name, Location location, DiagnosticBag diagnostics)
	{
		bool flag = syntax.Kind() == SyntaxKind.IndexerDeclaration;
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = GetExplicitInterfaceSpecifier(syntax);
		bool flag2 = explicitInterfaceSpecifier != null;
		_location = location;
		_containingType = containingType;
		_syntaxRef = syntax.GetReference();
		_refKind = syntax.Type.GetRefKind();
		SyntaxTokenList modifiers = syntax.Modifiers;
		bodyBinder = bodyBinder.WithUnsafeRegionIfNecessary(modifiers);
		bodyBinder = bodyBinder.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
		_modifiers = MakeModifiers(modifiers, flag2, flag, location, diagnostics, out var _);
		CheckAccessibility(location, diagnostics);
		CheckModifiers(location, flag, diagnostics);
		if (flag && !flag2)
		{
			CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = null;
			LoadAndValidateAttributes(OneOrMany.Create(CSharpSyntaxNode.AttributeLists), ref lazyCustomAttributesBag, AttributeLocation.None, earlyDecodingOnly: true);
			if (lazyCustomAttributesBag != null)
			{
				PropertyEarlyWellKnownAttributeData propertyEarlyWellKnownAttributeData = (PropertyEarlyWellKnownAttributeData)lazyCustomAttributesBag.EarlyDecodedWellKnownAttributeData;
				if (propertyEarlyWellKnownAttributeData != null)
				{
					_sourceName = propertyEarlyWellKnownAttributeData.IndexerName;
				}
			}
		}
		string memberNameAndInterfaceSymbol = ExplicitInterfaceHelpers.GetMemberNameAndInterfaceSymbol(bodyBinder, explicitInterfaceSpecifier, name, diagnostics, out _explicitInterfaceType, out var aliasQualifierOpt);
		_sourceName = _sourceName ?? memberNameAndInterfaceSymbol;
		_name = (flag ? ExplicitInterfaceHelpers.GetMemberName("this[]", _explicitInterfaceType, aliasQualifierOpt) : _sourceName);
		_isExpressionBodied = false;
		bool flag3 = syntax.AccessorList != null;
		PropertyDeclarationSyntax propertyDeclarationSyntax = syntax as PropertyDeclarationSyntax;
		ArrowExpressionClauseSyntax arrowExpressionClauseSyntax = ((propertyDeclarationSyntax != null) ? propertyDeclarationSyntax.ExpressionBody : ((IndexerDeclarationSyntax)syntax).ExpressionBody);
		bool flag4 = arrowExpressionClauseSyntax != null;
		bool flag5 = !flag && propertyDeclarationSyntax.Initializer != null;
		bool flag6 = (!IsAbstract && !IsExtern && !flag) & flag3;
		AccessorDeclarationSyntax accessorDeclarationSyntax = null;
		AccessorDeclarationSyntax accessorDeclarationSyntax2 = null;
		if (flag3)
		{
			foreach (AccessorDeclarationSyntax accessor in syntax.AccessorList.Accessors)
			{
				switch (accessor.Kind())
				{
				case SyntaxKind.GetAccessorDeclaration:
					if (accessorDeclarationSyntax == null)
					{
						accessorDeclarationSyntax = accessor;
					}
					else
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateAccessor, accessor.Keyword.GetLocation());
					}
					break;
				case SyntaxKind.SetAccessorDeclaration:
					if (accessorDeclarationSyntax2 == null)
					{
						accessorDeclarationSyntax2 = accessor;
					}
					else
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateAccessor, accessor.Keyword.GetLocation());
					}
					break;
				case SyntaxKind.AddAccessorDeclaration:
				case SyntaxKind.RemoveAccessorDeclaration:
					diagnostics.Add(ErrorCode.ERR_GetOrSetExpected, accessor.Keyword.GetLocation());
					continue;
				default:
					throw ExceptionUtilities.UnexpectedValue(accessor.Kind());
				case SyntaxKind.UnknownAccessorDeclaration:
					continue;
				}
				if (accessor.Body != null || accessor.ExpressionBody != null)
				{
					flag6 = false;
				}
			}
		}
		else
		{
			flag6 = false;
		}
		if (flag5)
		{
			CheckInitializer(flag6, location, diagnostics);
		}
		if (flag6 | flag5)
		{
			bool flag7 = accessorDeclarationSyntax != null;
			_isAutoProperty = flag6 & flag7;
			bool flag8 = flag7 && accessorDeclarationSyntax2 == null;
			if (_isAutoProperty && !flag8 && !IsStatic && ContainingType.IsReadOnly)
			{
				diagnostics.Add(ErrorCode.ERR_AutoPropsInRoStruct, location);
			}
			if (_isAutoProperty | flag5)
			{
				if (_isAutoProperty)
				{
					Binder.ReportUseSiteDiagnosticForSynthesizedAttribute(bodyBinder.Compilation, WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor, diagnostics, null, syntax);
					if (_refKind != RefKind.None && !_containingType.IsInterface)
					{
						diagnostics.Add(ErrorCode.ERR_AutoPropertyCannotBeRefReturning, location, this);
					}
				}
				string name2 = GeneratedNames.MakeBackingFieldName(_sourceName);
				_backingField = new SynthesizedBackingFieldSymbol(this, name2, flag8, IsStatic, flag5);
			}
			if (flag6)
			{
				Binder.CheckFeatureAvailability(syntax, flag8 ? MessageID.IDS_FeatureReadonlyAutoImplementedProperties : MessageID.IDS_FeatureAutoImplementedProperties, diagnostics, location);
			}
		}
		PropertySymbol propertySymbol = null;
		_customModifiers = CustomModifiersTuple.Empty;
		if (flag2 || IsOverride)
		{
			_lazyType = ComputeType(bodyBinder, syntax, diagnostics);
			_lazyParameters = ComputeParameters(bodyBinder, syntax, diagnostics);
			bool alsoCopyParamsModifier = false;
			PropertySymbol propertySymbol2;
			if (!flag2)
			{
				alsoCopyParamsModifier = true;
				propertySymbol2 = OverriddenProperty;
			}
			else
			{
				propertySymbol = this.FindExplicitlyImplementedProperty(_explicitInterfaceType, flag ? "this[]" : name, explicitInterfaceSpecifier, diagnostics);
				propertySymbol2 = propertySymbol;
			}
			if ((object)propertySymbol2 != null)
			{
				_customModifiers = CustomModifiersTuple.Create(propertySymbol2.TypeCustomModifiers, (_refKind != RefKind.None) ? propertySymbol2.RefCustomModifiers : ImmutableArray<CustomModifier>.Empty);
				TypeSymbol type = propertySymbol2.Type;
				if (_lazyType.Equals(type, TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds | TypeCompareKind.IgnoreDynamic))
				{
					_lazyType = CustomModifierUtils.CopyTypeCustomModifiers(type, _lazyType, ContainingAssembly);
				}
				_lazyParameters = CustomModifierUtils.CopyParameterCustomModifiers(propertySymbol2.Parameters, _lazyParameters, alsoCopyParamsModifier);
			}
		}
		else if (_refKind == RefKind.In)
		{
			NamedTypeSymbol wellKnownType = bodyBinder.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_InAttribute, diagnostics, syntax.Type);
			_customModifiers = CustomModifiersTuple.Create(ImmutableArray<CustomModifier>.Empty, ImmutableArray.Create(CSharpCustomModifier.CreateRequired(wellKnownType)));
		}
		if (!flag3)
		{
			if (flag4)
			{
				_isExpressionBodied = true;
				_getMethod = SourcePropertyAccessorSymbol.CreateAccessorSymbol(containingType, this, _modifiers, _sourceName, arrowExpressionClauseSyntax, propertySymbol, aliasQualifierOpt, diagnostics);
			}
			else
			{
				_getMethod = null;
			}
			_setMethod = null;
		}
		else
		{
			_getMethod = CreateAccessorSymbol(accessorDeclarationSyntax, propertySymbol, aliasQualifierOpt, flag6, diagnostics);
			_setMethod = CreateAccessorSymbol(accessorDeclarationSyntax2, propertySymbol, aliasQualifierOpt, flag6, diagnostics);
			if (accessorDeclarationSyntax == null || accessorDeclarationSyntax2 == null)
			{
				if (accessorDeclarationSyntax == null && accessorDeclarationSyntax2 == null)
				{
					diagnostics.Add(ErrorCode.ERR_PropertyWithNoAccessors, location, this);
				}
				else if (_refKind != RefKind.None)
				{
					if (accessorDeclarationSyntax == null)
					{
						diagnostics.Add(ErrorCode.ERR_RefPropertyMustHaveGetAccessor, location, this);
					}
				}
				else if (flag6)
				{
					SourcePropertyAccessorSymbol sourcePropertyAccessorSymbol = _getMethod ?? _setMethod;
					if (accessorDeclarationSyntax == null)
					{
						diagnostics.Add(ErrorCode.ERR_AutoPropertyMustHaveGetAccessor, sourcePropertyAccessorSymbol.Locations[0], sourcePropertyAccessorSymbol);
					}
				}
			}
			CheckAccessibilityMoreRestrictive(_getMethod, diagnostics);
			CheckAccessibilityMoreRestrictive(_setMethod, diagnostics);
			if ((object)_getMethod != null && (object)_setMethod != null)
			{
				if (_refKind != RefKind.None)
				{
					diagnostics.Add(ErrorCode.ERR_RefPropertyCannotHaveSetAccessor, _setMethod.Locations[0], _setMethod);
				}
				else if (_getMethod.LocalAccessibility != Accessibility.NotApplicable && _setMethod.LocalAccessibility != Accessibility.NotApplicable)
				{
					diagnostics.Add(ErrorCode.ERR_DuplicatePropertyAccessMods, location, this);
				}
				else if (IsAbstract)
				{
					CheckAbstractPropertyAccessorNotPrivate(_getMethod, diagnostics);
					CheckAbstractPropertyAccessorNotPrivate(_setMethod, diagnostics);
				}
			}
			else if (!IsOverride)
			{
				SourcePropertyAccessorSymbol sourcePropertyAccessorSymbol2 = _getMethod ?? _setMethod;
				if ((object)sourcePropertyAccessorSymbol2 != null && sourcePropertyAccessorSymbol2.LocalAccessibility != Accessibility.NotApplicable)
				{
					diagnostics.Add(ErrorCode.ERR_AccessModMissingAccessor, location, this);
				}
			}
		}
		if ((object)propertySymbol != null)
		{
			CheckExplicitImplementationAccessor(GetMethod, propertySymbol.GetMethod, propertySymbol, diagnostics);
			CheckExplicitImplementationAccessor(SetMethod, propertySymbol.SetMethod, propertySymbol, diagnostics);
		}
		_explicitInterfaceImplementations = (((object)propertySymbol == null) ? ImmutableArray<PropertySymbol>.Empty : ImmutableArray.Create(propertySymbol));
		if (_isAutoProperty && (object)_setMethod == null && !IsReadOnly)
		{
			diagnostics.Add(ErrorCode.ERR_AutoPropertyMustOverrideSet, location, this);
		}
		if (_isAutoProperty)
		{
			CheckForFieldTargetedAttribute(syntax, diagnostics);
		}
		Symbol.CheckForBlockAndExpressionBody(syntax.AccessorList, syntax.GetExpressionBodySyntax(), syntax, diagnostics);
	}

	private void CheckForFieldTargetedAttribute(BasePropertyDeclarationSyntax syntax, DiagnosticBag diagnostics)
	{
		LanguageVersion languageVersion = DeclaringCompilation.LanguageVersion;
		if (languageVersion.AllowAttributesOnBackingFields())
		{
			return;
		}
		foreach (AttributeListSyntax attributeList in syntax.AttributeLists)
		{
			AttributeTargetSpecifierSyntax target = attributeList.Target;
			if (target != null && target.GetAttributeLocation() == AttributeLocation.Field)
			{
				diagnostics.Add(new CSDiagnosticInfo(ErrorCode.WRN_AttributesOnBackingFieldsNotAvailable, languageVersion.ToDisplayString(), new CSharpRequiredLanguageVersion(MessageID.IDS_FeatureAttributesOnBackingFields.RequiredVersion())), attributeList.Target.Location);
			}
		}
	}

	private void CheckInitializer(bool isAutoProperty, Location location, DiagnosticBag diagnostics)
	{
		if (_containingType.IsInterface)
		{
			diagnostics.Add(ErrorCode.ERR_AutoPropertyInitializerInInterface, location, this);
		}
		else if (!isAutoProperty)
		{
			diagnostics.Add(ErrorCode.ERR_InitializerOnNonAutoProperty, location, this);
		}
	}

	internal static SourcePropertySymbol Create(SourceMemberContainerTypeSymbol containingType, Binder bodyBinder, PropertyDeclarationSyntax syntax, DiagnosticBag diagnostics)
	{
		SyntaxToken identifier = syntax.Identifier;
		Location location = identifier.GetLocation();
		return new SourcePropertySymbol(containingType, bodyBinder, syntax, identifier.ValueText, location, diagnostics);
	}

	internal static SourcePropertySymbol Create(SourceMemberContainerTypeSymbol containingType, Binder bodyBinder, IndexerDeclarationSyntax syntax, DiagnosticBag diagnostics)
	{
		Location location = syntax.ThisKeyword.GetLocation();
		return new SourcePropertySymbol(containingType, bodyBinder, syntax, "Item", location, diagnostics);
	}

	internal override LexicalSortKey GetLexicalSortKey()
	{
		return new LexicalSortKey(_location, DeclaringCompilation);
	}

	internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, DiagnosticBag diagnostics)
	{
		if ((object)_explicitInterfaceType != null)
		{
			ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = GetExplicitInterfaceSpecifier(CSharpSyntaxNode);
			_explicitInterfaceType.CheckAllConstraints(conversions, new SourceLocation(explicitInterfaceSpecifier.Name), diagnostics);
		}
		if (_refKind == RefKind.In)
		{
			DeclaringCompilation.EnsureIsReadOnlyAttributeExists(diagnostics, CSharpSyntaxNode.Type.Location, modifyCompilationForRefReadOnly: true);
		}
		ParameterHelpers.EnsureIsReadOnlyAttributeExists(Parameters, diagnostics, modifyCompilationForRefReadOnly: true);
	}

	private void CheckAccessibility(Location location, DiagnosticBag diagnostics)
	{
		CSDiagnosticInfo cSDiagnosticInfo = ModifierUtils.CheckAccessibility(_modifiers);
		if (cSDiagnosticInfo != null)
		{
			diagnostics.Add(new CSDiagnostic(cSDiagnosticInfo, location));
		}
	}

	private DeclarationModifiers MakeModifiers(SyntaxTokenList modifiers, bool isExplicitInterfaceImplementation, bool isIndexer, Location location, DiagnosticBag diagnostics, out bool modifierErrors)
	{
		bool isInterface = ContainingType.IsInterface;
		DeclarationModifiers defaultAccess = (isInterface ? DeclarationModifiers.Public : DeclarationModifiers.Private);
		DeclarationModifiers declarationModifiers = DeclarationModifiers.Unsafe;
		if (!isExplicitInterfaceImplementation)
		{
			declarationModifiers |= DeclarationModifiers.New;
			if (!isInterface)
			{
				declarationModifiers |= DeclarationModifiers.AccessibilityMask | DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.Virtual | DeclarationModifiers.Override;
				if (!isIndexer)
				{
					declarationModifiers |= DeclarationModifiers.Static;
				}
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
		if (isIndexer)
		{
			declarationModifiers2 |= DeclarationModifiers.Indexer;
		}
		return declarationModifiers2;
	}

	private static ImmutableArray<ParameterSymbol> MakeParameters(Binder binder, SourcePropertySymbol owner, BaseParameterListSyntax parameterSyntaxOpt, DiagnosticBag diagnostics, bool addRefReadOnlyModifier)
	{
		if (parameterSyntaxOpt == null)
		{
			return ImmutableArray<ParameterSymbol>.Empty;
		}
		if (parameterSyntaxOpt.Parameters.Count < 1)
		{
			diagnostics.Add(ErrorCode.ERR_IndexerNeedsParam, parameterSyntaxOpt.GetLastToken().GetLocation());
		}
		bool addRefReadOnlyModifier2 = addRefReadOnlyModifier;
		ImmutableArray<ParameterSymbol> result = ParameterHelpers.MakeParameters(binder, owner, parameterSyntaxOpt, out var arglistToken, diagnostics, allowRefOrOut: false, allowThis: false, addRefReadOnlyModifier2);
		if (arglistToken.Kind() != SyntaxKind.None)
		{
			diagnostics.Add(ErrorCode.ERR_IllegalVarArgs, arglistToken.GetLocation());
		}
		if (result.Length == 1 && !owner.IsExplicitInterfaceImplementation)
		{
			ParameterSyntax parameterSyntax = parameterSyntaxOpt.Parameters[0];
			if (parameterSyntax.Default != null)
			{
				SyntaxToken identifier = parameterSyntax.Identifier;
				diagnostics.Add(ErrorCode.WRN_DefaultValueForUnconsumedLocation, identifier.GetLocation(), identifier.ValueText);
			}
		}
		return result;
	}

	private void CheckModifiers(Location location, bool isIndexer, DiagnosticBag diagnostics)
	{
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
			ErrorCode code = (isIndexer ? ErrorCode.ERR_IndexerInStaticClass : ErrorCode.ERR_InstanceMemberInStaticClass);
			diagnostics.Add(code, location, this);
		}
	}

	private SourcePropertyAccessorSymbol CreateAccessorSymbol(AccessorDeclarationSyntax syntaxOpt, PropertySymbol explicitlyImplementedPropertyOpt, string aliasQualifierOpt, bool isAutoPropertyAccessor, DiagnosticBag diagnostics)
	{
		if (syntaxOpt == null)
		{
			return null;
		}
		return SourcePropertyAccessorSymbol.CreateAccessorSymbol(_containingType, this, _modifiers, _sourceName, syntaxOpt, explicitlyImplementedPropertyOpt, aliasQualifierOpt, isAutoPropertyAccessor, diagnostics);
	}

	private void CheckAccessibilityMoreRestrictive(SourcePropertyAccessorSymbol accessor, DiagnosticBag diagnostics)
	{
		if ((object)accessor != null && !IsAccessibilityMoreRestrictive(DeclaredAccessibility, accessor.LocalAccessibility))
		{
			diagnostics.Add(ErrorCode.ERR_InvalidPropertyAccessMod, accessor.Locations[0], accessor, this);
		}
	}

	private static bool IsAccessibilityMoreRestrictive(Accessibility property, Accessibility accessor)
	{
		if (accessor == Accessibility.NotApplicable)
		{
			return true;
		}
		if (accessor < property)
		{
			if (accessor == Accessibility.Protected)
			{
				return property != Accessibility.Internal;
			}
			return true;
		}
		return false;
	}

	private static void CheckAbstractPropertyAccessorNotPrivate(SourcePropertyAccessorSymbol accessor, DiagnosticBag diagnostics)
	{
		if (accessor.LocalAccessibility == Accessibility.Private)
		{
			diagnostics.Add(ErrorCode.ERR_PrivateAbstractAccessor, accessor.Locations[0], accessor);
		}
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return SourceDocumentationCommentUtils.GetAndCacheDocumentationComment(this, expandIncludes, ref _lazyDocComment);
	}

	private void CheckExplicitImplementationAccessor(MethodSymbol thisAccessor, MethodSymbol otherAccessor, PropertySymbol explicitlyImplementedProperty, DiagnosticBag diagnostics)
	{
		bool flag = (object)thisAccessor != null;
		bool flag2 = (object)otherAccessor != null;
		if (flag2 && !flag)
		{
			diagnostics.Add(ErrorCode.ERR_ExplicitPropertyMissingAccessor, Location, this, otherAccessor);
		}
		else if (!flag2 & flag)
		{
			diagnostics.Add(ErrorCode.ERR_ExplicitPropertyAddingAccessor, thisAccessor.Locations[0], thisAccessor, explicitlyImplementedProperty);
		}
	}

	private SynthesizedSealedPropertyAccessor MakeSynthesizedSealedAccessor()
	{
		if ((object)_getMethod != null)
		{
			MethodSymbol ownOrInheritedSetMethod = this.GetOwnOrInheritedSetMethod();
			if ((object)ownOrInheritedSetMethod != null)
			{
				return new SynthesizedSealedPropertyAccessor(this, ownOrInheritedSetMethod);
			}
			return null;
		}
		if ((object)_setMethod != null)
		{
			MethodSymbol ownOrInheritedGetMethod = this.GetOwnOrInheritedGetMethod();
			if ((object)ownOrInheritedGetMethod != null)
			{
				return new SynthesizedSealedPropertyAccessor(this, ownOrInheritedGetMethod);
			}
			return null;
		}
		return null;
	}

	private CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
		if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsSealed)
		{
			return lazyCustomAttributesBag;
		}
		_backingField?.GetAttributes();
		if (LoadAndValidateAttributes(OneOrMany.Create(CSharpSyntaxNode.AttributeLists), ref _lazyCustomAttributesBag))
		{
			_state.NotePartComplete(CompletionPart.Attributes);
		}
		return _lazyCustomAttributesBag;
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return GetAttributesBag().Attributes;
	}

	private CommonPropertyWellKnownAttributeData GetDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (CommonPropertyWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	internal PropertyEarlyWellKnownAttributeData GetEarlyDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (PropertyEarlyWellKnownAttributeData)customAttributesBag.EarlyDecodedWellKnownAttributeData;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		if (Type.ContainsDynamic())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, DeclaringCompilation.SynthesizeDynamicAttribute(Type, TypeCustomModifiers.Length + RefCustomModifiers.Length, _refKind));
		}
		if (Type.ContainsTupleNames())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, DeclaringCompilation.SynthesizeTupleNamesAttribute(Type));
		}
		if (ReturnsByRefReadonly)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsReadOnlyAttribute(this));
		}
	}

	internal override CSharpAttributeData EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		if (Symbol.EarlyDecodeDeprecatedOrExperimentalOrObsoleteAttribute(ref arguments, out var attributeData, out var obsoleteData))
		{
			if (obsoleteData != null)
			{
				arguments.GetOrCreateData<PropertyEarlyWellKnownAttributeData>().ObsoleteAttributeData = obsoleteData;
			}
			return attributeData;
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.IndexerNameAttribute))
		{
			attributeData = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, out var generatedDiagnostics);
			if (!attributeData.HasErrors)
			{
				string text = attributeData.CommonConstructorArguments[0].DecodeValue<string>(SpecialType.System_String);
				if (text != null)
				{
					arguments.GetOrCreateData<PropertyEarlyWellKnownAttributeData>().IndexerName = text;
				}
				if (!generatedDiagnostics)
				{
					return attributeData;
				}
			}
			return null;
		}
		return base.EarlyDecodeWellKnownAttribute(ref arguments);
	}

	internal override void DecodeWellKnownAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		if (attribute.IsTargetAttribute(this, AttributeDescription.IndexerNameAttribute))
		{
			ValidateIndexerNameAttribute(attribute, arguments.AttributeSyntaxOpt, arguments.Diagnostics);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.SpecialNameAttribute))
		{
			arguments.GetOrCreateData<CommonPropertyWellKnownAttributeData>().HasSpecialNameAttribute = true;
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.ExcludeFromCodeCoverageAttribute))
		{
			arguments.GetOrCreateData<CommonPropertyWellKnownAttributeData>().HasExcludeFromCodeCoverageAttribute = true;
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.DynamicAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitDynamicAttr, arguments.AttributeSyntaxOpt.Location);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.IsReadOnlyAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsReadOnlyAttribute.FullName);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.IsUnmanagedAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsUnmanagedAttribute.FullName);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.IsByRefLikeAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsByRefLikeAttribute.FullName);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.TupleElementNamesAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitTupleElementNamesAttribute, arguments.AttributeSyntaxOpt.Location);
		}
	}

	internal override void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, DiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
		base.PostDecodeWellKnownAttributes(boundAttributes, allAttributeSyntaxNodes, diagnostics, symbolPart, decodedData);
	}

	private void ValidateIndexerNameAttribute(CSharpAttributeData attribute, AttributeSyntax node, DiagnosticBag diagnostics)
	{
		if (!IsIndexer || IsExplicitInterfaceImplementation)
		{
			diagnostics.Add(ErrorCode.ERR_BadIndexerNameAttr, node.Name.Location, node.GetErrorDisplayName());
			return;
		}
		string text = attribute.CommonConstructorArguments[0].DecodeValue<string>(SpecialType.System_String);
		if (text == null || !SyntaxFacts.IsValidIdentifier(text))
		{
			diagnostics.Add(ErrorCode.ERR_BadArgumentToAttribute, node.ArgumentList.Arguments[0].Location, node.GetErrorDisplayName());
		}
	}

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return _state.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = _state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Attributes:
				GetAttributes();
				break;
			case CompletionPart.StartBaseType:
			case CompletionPart.FinishBaseType:
				if (_state.NotePartComplete(CompletionPart.StartBaseType))
				{
					if (Parameters.Length > 0)
					{
						DiagnosticBag instance2 = DiagnosticBag.GetInstance();
						TypeConversions conversions2 = new TypeConversions(ContainingAssembly.CorLibrary);
						foreach (ParameterSymbol parameter in Parameters)
						{
							parameter.ForceComplete(locationOpt, cancellationToken);
							parameter.Type.CheckAllConstraints(conversions2, parameter.Locations[0], instance2);
						}
						AddDeclarationDiagnostics(instance2);
						instance2.Free();
					}
					DeclaringCompilation.SymbolDeclaredEvent(this);
					_state.NotePartComplete(CompletionPart.FinishBaseType);
				}
				else
				{
					_state.SpinWaitComplete(CompletionPart.FinishBaseType, cancellationToken);
				}
				break;
			case CompletionPart.StartInterfaces:
			case CompletionPart.FinishInterfaces:
				if (_state.NotePartComplete(CompletionPart.StartInterfaces))
				{
					DiagnosticBag instance = DiagnosticBag.GetInstance();
					TypeConversions conversions = new TypeConversions(ContainingAssembly.CorLibrary);
					Type.CheckAllConstraints(conversions, _location, instance);
					TypeSymbol type = Type;
					if (type.IsRestrictedType(ignoreSpanLikeTypes: true))
					{
						instance.Add(ErrorCode.ERR_FieldCantBeRefAny, CSharpSyntaxNode.Type.Location, type);
					}
					else if (IsAutoProperty && type.IsByRefLikeType && (IsStatic || !ContainingType.IsByRefLikeType))
					{
						instance.Add(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, CSharpSyntaxNode.Type.Location, type);
					}
					AddDeclarationDiagnostics(instance);
					_state.NotePartComplete(CompletionPart.FinishInterfaces);
					instance.Free();
				}
				else
				{
					_state.SpinWaitComplete(CompletionPart.FinishInterfaces, cancellationToken);
				}
				break;
			case CompletionPart.None:
				return;
			default:
				_state.NotePartComplete(CompletionPart.NamespaceSymbolAll | CompletionPart.ReturnTypeAttributes | CompletionPart.Parameters | CompletionPart.Type | CompletionPart.EnumUnderlyingType | CompletionPart.TypeArguments | CompletionPart.TypeParameters | CompletionPart.TypeMembers | CompletionPart.SynthesizedExplicitImplementations | CompletionPart.StartMemberChecks | CompletionPart.FinishMemberChecks);
				break;
			}
			_state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	private TypeSymbol ComputeType(Binder binder, BasePropertyDeclarationSyntax syntax, DiagnosticBag diagnostics)
	{
		TypeSyntax syntax2 = syntax.Type.SkipRef(out var _);
		TypeSymbol typeSymbol = binder.BindType(syntax2, diagnostics);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (!this.IsNoMoreVisibleThan(typeSymbol, ref useSiteDiagnostics))
		{
			diagnostics.Add(IsIndexer ? ErrorCode.ERR_BadVisIndexerReturn : ErrorCode.ERR_BadVisPropertyType, _location, this, typeSymbol);
		}
		diagnostics.Add(_location, useSiteDiagnostics);
		if (typeSymbol.SpecialType == SpecialType.System_Void)
		{
			ErrorCode code = (IsIndexer ? ErrorCode.ERR_IndexerCantHaveVoidType : ErrorCode.ERR_PropertyCantHaveVoidType);
			diagnostics.Add(code, _location, this);
		}
		return typeSymbol;
	}

	private ImmutableArray<ParameterSymbol> ComputeParameters(Binder binder, BasePropertyDeclarationSyntax syntax, DiagnosticBag diagnostics)
	{
		BaseParameterListSyntax parameterListSyntax = GetParameterListSyntax(syntax);
		ImmutableArray<ParameterSymbol> result = MakeParameters(binder, this, parameterListSyntax, diagnostics, IsVirtual || IsAbstract);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		foreach (ParameterSymbol item in result)
		{
			if (!this.IsNoMoreVisibleThan(item.Type, ref useSiteDiagnostics))
			{
				diagnostics.Add(ErrorCode.ERR_BadVisIndexerParam, _location, this, item.Type);
			}
			else if ((object)_setMethod != null && item.Name == "value")
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateGeneratedName, item.Locations.FirstOrDefault() ?? _location, item.Name);
			}
		}
		diagnostics.Add(_location, useSiteDiagnostics);
		return result;
	}

	private Binder CreateBinderForTypeAndParameters()
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		SyntaxTree syntaxTree = _syntaxRef.SyntaxTree;
		BasePropertyDeclarationSyntax basePropertyDeclarationSyntax = (BasePropertyDeclarationSyntax)_syntaxRef.GetSyntax();
		Binder binder = declaringCompilation.GetBinderFactory(syntaxTree).GetBinder(basePropertyDeclarationSyntax, basePropertyDeclarationSyntax, this);
		SyntaxTokenList modifiers = basePropertyDeclarationSyntax.Modifiers;
		return binder.WithUnsafeRegionIfNecessary(modifiers).WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
	}

	private static ExplicitInterfaceSpecifierSyntax GetExplicitInterfaceSpecifier(BasePropertyDeclarationSyntax syntax)
	{
		return syntax.Kind() switch
		{
			SyntaxKind.PropertyDeclaration => ((PropertyDeclarationSyntax)syntax).ExplicitInterfaceSpecifier, 
			SyntaxKind.IndexerDeclaration => ((IndexerDeclarationSyntax)syntax).ExplicitInterfaceSpecifier, 
			_ => throw ExceptionUtilities.UnexpectedValue(syntax.Kind()), 
		};
	}

	private static BaseParameterListSyntax GetParameterListSyntax(BasePropertyDeclarationSyntax syntax)
	{
		if (syntax.Kind() != SyntaxKind.IndexerDeclaration)
		{
			return null;
		}
		return ((IndexerDeclarationSyntax)syntax).ParameterList;
	}
}
