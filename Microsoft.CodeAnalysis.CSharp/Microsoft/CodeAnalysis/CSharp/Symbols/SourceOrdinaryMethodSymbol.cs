using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.RuntimeMembers;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceOrdinaryMethodSymbol : SourceMemberMethodSymbol
{
	private readonly ImmutableArray<TypeParameterSymbol> _typeParameters;

	private readonly TypeSymbol _explicitInterfaceType;

	private readonly string _name;

	private readonly bool _isExpressionBodied;

	private readonly bool _hasAnyBody;

	private readonly RefKind _refKind;

	private ImmutableArray<MethodSymbol> _lazyExplicitInterfaceImplementations;

	private CustomModifiersTuple _lazyCustomModifiers;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private TypeSymbol _lazyReturnType;

	private bool _lazyIsVararg;

	private ImmutableArray<TypeParameterConstraintClause> _lazyTypeParameterConstraints;

	private SourceOrdinaryMethodSymbol _otherPartOfPartial;

	public override bool ReturnsVoid
	{
		get
		{
			LazyMethodChecks();
			return base.ReturnsVoid;
		}
	}

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

	public override ImmutableArray<TypeParameterConstraintClause> TypeParameterConstraintClauses
	{
		get
		{
			if (_lazyTypeParameterConstraints.IsDefault)
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				MethodDeclarationSyntax syntax = GetSyntax();
				Binder binder = DeclaringCompilation.GetBinderFactory(syntax.SyntaxTree).GetBinder(syntax.ReturnType, syntax, this);
				ImmutableArray<TypeParameterConstraintClause> value = this.MakeTypeParameterConstraints(binder, TypeParameters, syntax.ConstraintClauses, syntax.Identifier.GetLocation(), instance);
				if (ImmutableInterlocked.InterlockedInitialize(ref _lazyTypeParameterConstraints, value))
				{
					AddDeclarationDiagnostics(instance);
				}
				instance.Free();
			}
			return _lazyTypeParameterConstraints;
		}
	}

	public override bool IsVararg
	{
		get
		{
			LazyMethodChecks();
			return _lazyIsVararg;
		}
	}

	public override ImmutableArray<Location> Locations => locations;

	internal override int ParameterCount
	{
		get
		{
			if (_lazyParameters.IsDefault)
			{
				return GetSyntax().ParameterList.ParameterCount;
			}
			return _lazyParameters.Length;
		}
	}

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			LazyMethodChecks();
			return _lazyParameters;
		}
	}

	public override RefKind RefKind => _refKind;

	public override TypeSymbol ReturnType
	{
		get
		{
			LazyMethodChecks();
			return _lazyReturnType;
		}
	}

	internal SourceOrdinaryMethodSymbol OtherPartOfPartial => _otherPartOfPartial;

	internal bool IsPartialDefinition
	{
		get
		{
			if (IsPartial)
			{
				return !_hasAnyBody;
			}
			return false;
		}
	}

	internal bool IsPartialImplementation
	{
		get
		{
			if (IsPartial)
			{
				return _hasAnyBody;
			}
			return false;
		}
	}

	internal bool IsPartialWithoutImplementation
	{
		get
		{
			if (IsPartialDefinition)
			{
				return (object)_otherPartOfPartial == null;
			}
			return false;
		}
	}

	internal SourceOrdinaryMethodSymbol SourcePartialDefinition
	{
		get
		{
			if (!IsPartialImplementation)
			{
				return null;
			}
			return _otherPartOfPartial;
		}
	}

	internal SourceOrdinaryMethodSymbol SourcePartialImplementation
	{
		get
		{
			if (!IsPartialDefinition)
			{
				return null;
			}
			return _otherPartOfPartial;
		}
	}

	public override MethodSymbol PartialDefinitionPart => SourcePartialDefinition;

	public override MethodSymbol PartialImplementationPart => SourcePartialImplementation;

	internal override bool IsExplicitInterfaceImplementation => GetSyntax().ExplicitInterfaceSpecifier != null;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations
	{
		get
		{
			LazyMethodChecks();
			return _lazyExplicitInterfaceImplementations;
		}
	}

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers
	{
		get
		{
			LazyMethodChecks();
			return _lazyCustomModifiers.TypeCustomModifiers;
		}
	}

	public override ImmutableArray<CustomModifier> RefCustomModifiers
	{
		get
		{
			LazyMethodChecks();
			return _lazyCustomModifiers.RefCustomModifiers;
		}
	}

	public override string Name => _name;

	protected override SourceMemberMethodSymbol BoundAttributesSource => SourcePartialDefinition;

	private SyntaxList<AttributeListSyntax> AttributeDeclarationSyntaxList
	{
		get
		{
			if (ContainingType is SourceMemberContainerTypeSymbol { AnyMemberHasAttributes: not false })
			{
				return GetSyntax().AttributeLists;
			}
			return default;
		}
	}

	internal override bool IsExpressionBodied => _isExpressionBodied;

	internal override bool GenerateDebugInfo
	{
		get
		{
			if (!IsAsync)
			{
				return !IsIterator;
			}
			return false;
		}
	}

	public static SourceOrdinaryMethodSymbol CreateMethodSymbol(NamedTypeSymbol containingType, Binder bodyBinder, MethodDeclarationSyntax syntax, DiagnosticBag diagnostics)
	{
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = syntax.ExplicitInterfaceSpecifier;
		SyntaxToken token = syntax.Identifier;
		string memberNameAndInterfaceSymbol = ExplicitInterfaceHelpers.GetMemberNameAndInterfaceSymbol(bodyBinder, explicitInterfaceSpecifier, token.ValueText, diagnostics, out var explicitInterfaceTypeOpt, out var _);
		SourceLocation location = new SourceLocation(in token);
		MethodKind methodKind = ((explicitInterfaceSpecifier == null) ? MethodKind.Ordinary : MethodKind.ExplicitInterfaceImplementation);
		return new SourceOrdinaryMethodSymbol(containingType, explicitInterfaceTypeOpt, memberNameAndInterfaceSymbol, location, syntax, methodKind, diagnostics);
	}

	private SourceOrdinaryMethodSymbol(NamedTypeSymbol containingType, TypeSymbol explicitInterfaceType, string name, Location location, MethodDeclarationSyntax syntax, MethodKind methodKind, DiagnosticBag diagnostics)
		: base(containingType, syntax.GetReference(), location)
	{
		_name = name;
		_explicitInterfaceType = explicitInterfaceType;
		SyntaxTokenList modifiers = syntax.Modifiers;
		ParameterSyntax parameterSyntax = syntax.ParameterList.Parameters.FirstOrDefault();
		bool isExtensionMethod = parameterSyntax != null && !parameterSyntax.IsArgList && parameterSyntax.Modifiers.Any(SyntaxKind.ThisKeyword);
		DeclarationModifiers declarationModifiers = MakeModifiers(modifiers, methodKind, location, diagnostics, out var _);
		bool isMetadataVirtualIgnoringModifiers = (object)explicitInterfaceType != null;
		MakeFlags(methodKind, declarationModifiers, returnsVoid: false, isExtensionMethod, isMetadataVirtualIgnoringModifiers);
		if (syntax.Arity == 0)
		{
			_typeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
			Symbol.ReportErrorIfHasConstraints(syntax.ConstraintClauses, diagnostics);
		}
		else
		{
			_typeParameters = MakeTypeParameters(syntax, diagnostics);
		}
		bool flag = syntax.Body != null;
		_isExpressionBodied = !flag && syntax.ExpressionBody != null;
		_hasAnyBody = flag || _isExpressionBodied;
		_refKind = syntax.ReturnType.GetRefKind();
		if (_hasAnyBody)
		{
			CheckModifiersForBody(location, diagnostics);
		}
		CSDiagnosticInfo cSDiagnosticInfo = ModifierUtils.CheckAccessibility(DeclarationModifiers);
		if (cSDiagnosticInfo != null)
		{
			diagnostics.Add(cSDiagnosticInfo, location);
		}
		if (syntax.ConstraintClauses.Count > 0 && (syntax.ExplicitInterfaceSpecifier != null || syntax.Modifiers.Any(SyntaxKind.OverrideKeyword)))
		{
			diagnostics.Add(ErrorCode.ERR_OverrideWithConstraints, syntax.ConstraintClauses[0].WhereKeyword.GetLocation());
		}
		Symbol.CheckForBlockAndExpressionBody(syntax.Body, syntax.ExpressionBody, syntax, diagnostics);
	}

	private void MethodChecks(MethodDeclarationSyntax syntax, Binder withTypeParamsBinder, DiagnosticBag diagnostics)
	{
		Binder binder = withTypeParamsBinder.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
		ParameterListSyntax parameterList = syntax.ParameterList;
		bool addRefReadOnlyModifier = IsVirtual || IsAbstract;
		_lazyParameters = ParameterHelpers.MakeParameters(binder, this, parameterList, out var arglistToken, diagnostics, allowRefOrOut: true, allowThis: true, addRefReadOnlyModifier);
		_lazyIsVararg = arglistToken.Kind() == SyntaxKind.ArgListKeyword;
		TypeSyntax syntax2 = syntax.ReturnType.SkipRef(out var _);
		_lazyReturnType = binder.BindType(syntax2, diagnostics);
		if (_lazyReturnType.IsRestrictedType(ignoreSpanLikeTypes: true) && (_lazyReturnType.SpecialType != SpecialType.System_TypedReference || (ContainingType.SpecialType != SpecialType.System_TypedReference && ContainingType.SpecialType != SpecialType.System_ArgIterator)))
		{
			diagnostics.Add(ErrorCode.ERR_MethodReturnCantBeRefAny, syntax.ReturnType.Location, _lazyReturnType);
		}
		bool flag = _lazyReturnType.SpecialType == SpecialType.System_Void;
		_ = (RefKind != RefKind.None) & flag;
		SetReturnsVoid(flag);
		Location location = Locations[0];
		CheckEffectiveAccessibility(_lazyReturnType, _lazyParameters, diagnostics);
		if (Name == "Finalize" && ParameterCount == 0 && Arity == 0 && ReturnsVoid)
		{
			diagnostics.Add(ErrorCode.WRN_FinalizeMethod, location);
		}
		if (IsExtensionMethod)
		{
			TypeSymbol type = Parameters[0].Type;
			RefKind refKind2 = Parameters[0].RefKind;
			if (!type.IsValidExtensionParameterType())
			{
				Location location2 = syntax.ParameterList.Parameters[0].Type.Location;
				diagnostics.Add(ErrorCode.ERR_BadTypeforThis, location2, type);
			}
			else if (refKind2 == RefKind.Ref && !type.IsValueType)
			{
				diagnostics.Add(ErrorCode.ERR_RefExtensionMustBeValueTypeOrConstrainedToOne, location, Name);
			}
			else if (refKind2 == RefKind.In && type.TypeKind != TypeKind.Struct)
			{
				diagnostics.Add(ErrorCode.ERR_InExtensionMustBeValueType, location, Name);
			}
			else if ((object)ContainingType.ContainingType != null)
			{
				diagnostics.Add(ErrorCode.ERR_ExtensionMethodsDecl, location, ContainingType.Name);
			}
			else if (!ContainingType.IsScriptClass && (!ContainingType.IsStatic || ContainingType.Arity != 0))
			{
				Location location3 = ((syntax.Parent is TypeDeclarationSyntax typeDeclarationSyntax) ? typeDeclarationSyntax.Identifier : syntax.Identifier).GetLocation();
				diagnostics.Add(ErrorCode.ERR_BadExtensionAgg, location3);
			}
			else if (!IsStatic)
			{
				diagnostics.Add(ErrorCode.ERR_BadExtensionMeth, location);
			}
			else if ((object)withTypeParamsBinder.Compilation.GetWellKnownTypeMember(WellKnownMember.System_Runtime_CompilerServices_ExtensionAttribute__ctor) == null)
			{
				MemberDescriptor descriptor = WellKnownMembers.GetDescriptor(WellKnownMember.System_Runtime_CompilerServices_ExtensionAttribute__ctor);
				diagnostics.Add(ErrorCode.ERR_ExtensionAttrNotFound, syntax.ParameterList.Parameters[0].Modifiers.FirstOrDefault(SyntaxKind.ThisKeyword).GetLocation(), descriptor.DeclaringTypeMetadataName);
			}
		}
		if (IsPartial)
		{
			foreach (ParameterSymbol parameter in Parameters)
			{
				if (parameter.RefKind == RefKind.Out)
				{
					diagnostics.Add(ErrorCode.ERR_PartialMethodCannotHaveOutParameters, location);
					break;
				}
			}
			if (MethodKind == MethodKind.ExplicitInterfaceImplementation)
			{
				diagnostics.Add(ErrorCode.ERR_PartialMethodNotExplicit, location);
			}
			if (!ContainingType.IsPartial() || ContainingType.IsInterface)
			{
				diagnostics.Add(ErrorCode.ERR_PartialMethodOnlyInPartialClass, location);
			}
		}
		if (!IsPartial)
		{
			LazyAsyncMethodChecks(CancellationToken.None);
		}
		_lazyCustomModifiers = CustomModifiersTuple.Empty;
		if (syntax.ExplicitInterfaceSpecifier == null)
		{
			_lazyExplicitInterfaceImplementations = ImmutableArray<MethodSymbol>.Empty;
			if (IsOverride)
			{
				MethodSymbol overriddenMethod = OverriddenMethod;
				if ((object)overriddenMethod != null)
				{
					CustomModifierUtils.CopyMethodCustomModifiers(overriddenMethod, this, out _lazyReturnType, out _lazyCustomModifiers, out _lazyParameters, alsoCopyParamsModifier: true);
				}
			}
			else if (_refKind == RefKind.In)
			{
				NamedTypeSymbol wellKnownType = withTypeParamsBinder.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_InAttribute, diagnostics, syntax.ReturnType);
				_lazyCustomModifiers = CustomModifiersTuple.Create(ImmutableArray<CustomModifier>.Empty, ImmutableArray.Create(CSharpCustomModifier.CreateRequired(wellKnownType)));
			}
		}
		else if ((object)_explicitInterfaceType != null)
		{
			MethodSymbol methodSymbol = this.FindExplicitlyImplementedMethod(_explicitInterfaceType, syntax.Identifier.ValueText, syntax.ExplicitInterfaceSpecifier, diagnostics);
			if ((object)methodSymbol != null)
			{
				_lazyExplicitInterfaceImplementations = ImmutableArray.Create(methodSymbol);
				CustomModifierUtils.CopyMethodCustomModifiers(methodSymbol, this, out _lazyReturnType, out _lazyCustomModifiers, out _lazyParameters, alsoCopyParamsModifier: false);
			}
			else
			{
				_lazyExplicitInterfaceImplementations = ImmutableArray<MethodSymbol>.Empty;
			}
		}
		CheckModifiers(_hasAnyBody, location, diagnostics);
	}

	internal static void ReportAsyncParameterErrors(ImmutableArray<ParameterSymbol> parameters, DiagnosticBag diagnostics, Location location)
	{
		foreach (ParameterSymbol item in parameters)
		{
			Location location2 = (item.Locations.Any() ? item.Locations[0] : location);
			if (item.RefKind != RefKind.None)
			{
				diagnostics.Add(ErrorCode.ERR_BadAsyncArgType, location2);
			}
			else if (item.Type.IsUnsafe())
			{
				diagnostics.Add(ErrorCode.ERR_UnsafeAsyncArgType, location2);
			}
			else if (item.Type.IsRestrictedType())
			{
				diagnostics.Add(ErrorCode.ERR_BadSpecialByRefLocal, location2, item.Type);
			}
		}
	}

	protected sealed override void LazyAsyncMethodChecks(CancellationToken cancellationToken)
	{
		if (!IsAsync)
		{
			CompleteAsyncMethodChecks(null, cancellationToken);
			return;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		Location location = Locations[0];
		if (RefKind != RefKind.None)
		{
			TypeSyntax returnType = GetSyntax().ReturnType;
			if (!returnType.HasErrors)
			{
				SyntaxToken firstToken = returnType.GetFirstToken();
				instance.Add(ErrorCode.ERR_UnexpectedToken, firstToken.GetLocation(), firstToken.ToString());
			}
		}
		else if (!this.IsGenericTaskReturningAsync(DeclaringCompilation) && !this.IsTaskReturningAsync(DeclaringCompilation) && !this.IsVoidReturningAsync())
		{
			instance.Add(ErrorCode.ERR_BadAsyncReturn, location);
		}
		NamedTypeSymbol containingType = ContainingType;
		while ((object)containingType != null)
		{
			if (containingType is SourceNamedTypeSymbol { HasSecurityCriticalAttributes: not false })
			{
				instance.Add(ErrorCode.ERR_SecurityCriticalOrSecuritySafeCriticalOnAsyncInClassOrStruct, location);
				break;
			}
			containingType = containingType.ContainingType;
		}
		if ((ImplementationAttributes & MethodImplAttributes.Synchronized) != 0)
		{
			instance.Add(ErrorCode.ERR_SynchronizedAsyncMethod, location);
		}
		if (instance.IsEmptyWithoutResolution)
		{
			ReportAsyncParameterErrors(_lazyParameters, instance, location);
		}
		CompleteAsyncMethodChecks(instance, cancellationToken);
		instance.Free();
	}

	private void CompleteAsyncMethodChecks(DiagnosticBag diagnosticsOpt, CancellationToken cancellationToken)
	{
		if (state.NotePartComplete(CompletionPart.Members))
		{
			if (diagnosticsOpt != null)
			{
				AddDeclarationDiagnostics(diagnosticsOpt);
			}
			if (IsPartialDefinition)
			{
				DeclaringCompilation.SymbolDeclaredEvent(this);
			}
			state.NotePartComplete(CompletionPart.TypeMembers);
		}
		else
		{
			state.SpinWaitComplete(CompletionPart.TypeMembers, cancellationToken);
		}
	}

	protected override void MethodChecks(DiagnosticBag diagnostics)
	{
		MethodDeclarationSyntax syntax = GetSyntax();
		Binder binder = DeclaringCompilation.GetBinderFactory(syntax.SyntaxTree).GetBinder(syntax.ReturnType, syntax, this);
		MethodChecks(syntax, binder, diagnostics);
	}

	internal MethodDeclarationSyntax GetSyntax()
	{
		return (MethodDeclarationSyntax)syntaxReferenceOpt.GetSyntax();
	}

	internal static void InitializePartialMethodParts(SourceOrdinaryMethodSymbol definition, SourceOrdinaryMethodSymbol implementation)
	{
		definition._otherPartOfPartial = implementation;
		implementation._otherPartOfPartial = definition;
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return SourceDocumentationCommentUtils.GetAndCacheDocumentationComment(SourcePartialImplementation ?? this, expandIncludes, ref lazyDocComment);
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		if ((object)SourcePartialImplementation != null)
		{
			return OneOrMany.Create(ImmutableArray.Create(AttributeDeclarationSyntaxList, SourcePartialImplementation.AttributeDeclarationSyntaxList));
		}
		return OneOrMany.Create(AttributeDeclarationSyntaxList);
	}

	private DeclarationModifiers MakeModifiers(SyntaxTokenList modifiers, MethodKind methodKind, Location location, DiagnosticBag diagnostics, out bool modifierErrors)
	{
		bool isInterface = ContainingType.IsInterface;
		DeclarationModifiers defaultAccess = (isInterface ? DeclarationModifiers.Public : DeclarationModifiers.Private);
		DeclarationModifiers declarationModifiers = DeclarationModifiers.Partial | DeclarationModifiers.Unsafe;
		if (methodKind != MethodKind.ExplicitInterfaceImplementation)
		{
			declarationModifiers |= DeclarationModifiers.New;
			if (!isInterface)
			{
				declarationModifiers |= DeclarationModifiers.AccessibilityMask | DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.Static | DeclarationModifiers.Virtual | DeclarationModifiers.Override;
			}
		}
		if (!isInterface)
		{
			declarationModifiers |= DeclarationModifiers.Extern | DeclarationModifiers.Async;
		}
		DeclarationModifiers declarationModifiers2 = ModifierUtils.MakeAndCheckNontypeMemberModifiers(modifiers, defaultAccess, declarationModifiers, location, diagnostics, out modifierErrors);
		this.CheckUnsafeModifier(declarationModifiers2, diagnostics);
		return AddImpliedModifiers(declarationModifiers2, isInterface, methodKind);
	}

	private static DeclarationModifiers AddImpliedModifiers(DeclarationModifiers mods, bool containingTypeIsInterface, MethodKind methodKind)
	{
		if (containingTypeIsInterface)
		{
			mods = (mods & ~DeclarationModifiers.AccessibilityMask) | DeclarationModifiers.Public | DeclarationModifiers.Abstract;
		}
		else if (methodKind == MethodKind.ExplicitInterfaceImplementation)
		{
			mods = (mods & ~DeclarationModifiers.AccessibilityMask) | DeclarationModifiers.Private;
		}
		return mods;
	}

	private ImmutableArray<TypeParameterSymbol> MakeTypeParameters(MethodDeclarationSyntax syntax, DiagnosticBag diagnostics)
	{
		OverriddenMethodTypeParameterMapBase overriddenMethodTypeParameterMapBase = null;
		if (IsOverride)
		{
			overriddenMethodTypeParameterMapBase = new OverriddenMethodTypeParameterMap(this);
		}
		else if (IsExplicitInterfaceImplementation)
		{
			overriddenMethodTypeParameterMapBase = new ExplicitInterfaceMethodTypeParameterMap(this);
		}
		SeparatedSyntaxList<TypeParameterSyntax> parameters = syntax.TypeParameterList.Parameters;
		ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance();
		for (int i = 0; i < parameters.Count; i++)
		{
			TypeParameterSyntax typeParameterSyntax = parameters[i];
			if (typeParameterSyntax.VarianceKeyword.Kind() != SyntaxKind.None)
			{
				diagnostics.Add(ErrorCode.ERR_IllegalVarianceSyntax, typeParameterSyntax.VarianceKeyword.GetLocation());
			}
			SyntaxToken identifier = typeParameterSyntax.Identifier;
			Location location = identifier.GetLocation();
			string valueText = identifier.ValueText;
			for (int j = 0; j < instance.Count; j++)
			{
				if (valueText == instance[j].Name)
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateTypeParameter, location, valueText);
					break;
				}
			}
			TypeParameterSymbol typeParameterSymbol = ContainingType.FindEnclosingTypeParameter(valueText);
			if ((object)typeParameterSymbol != null)
			{
				diagnostics.Add(ErrorCode.WRN_TypeParameterSameAsOuterTypeParameter, location, valueText, typeParameterSymbol.ContainingType);
			}
			ImmutableArray<SyntaxReference> syntaxRefs = ImmutableArray.Create(typeParameterSyntax.GetReference());
			ImmutableArray<Location> immutableArray = ImmutableArray.Create(location);
			TypeParameterSymbol item = ((overriddenMethodTypeParameterMapBase != null) ? ((SourceTypeParameterSymbolBase)new SourceOverridingMethodTypeParameterSymbol(overriddenMethodTypeParameterMapBase, valueText, i, immutableArray, syntaxRefs)) : ((SourceTypeParameterSymbolBase)new SourceMethodTypeParameterSymbol(this, valueText, i, immutableArray, syntaxRefs)));
			instance.Add(item);
		}
		return instance.ToImmutableAndFree();
	}

	private void CheckModifiers(bool hasBody, Location location, DiagnosticBag diagnostics)
	{
		if (IsPartial && !ReturnsVoid)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMethodMustReturnVoid, location);
		}
		else if (IsPartial && !ContainingType.IsInterface && (DeclarationModifiers & (DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.New | DeclarationModifiers.Public | DeclarationModifiers.Protected | DeclarationModifiers.Internal | DeclarationModifiers.ProtectedInternal | DeclarationModifiers.PrivateProtected | DeclarationModifiers.Extern | DeclarationModifiers.Virtual | DeclarationModifiers.Override)) != 0)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMethodInvalidModifier, location);
		}
		else if (DeclaredAccessibility == Accessibility.Private && (IsVirtual || IsAbstract || IsOverride))
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
		else if (IsSealed && ContainingType.TypeKind == TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, location, SyntaxFacts.GetText(SyntaxKind.SealedKeyword));
		}
		else if (!ContainingType.IsInterfaceType() && _lazyReturnType.IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_ReturnTypeIsStaticClass, location, _lazyReturnType);
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
		else if (IsAbstract && ContainingType.TypeKind == TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, location, SyntaxFacts.GetText(SyntaxKind.AbstractKeyword));
		}
		else if (IsVirtual && ContainingType.TypeKind == TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, location, SyntaxFacts.GetText(SyntaxKind.VirtualKeyword));
		}
		else if (IsAbstract && !ContainingType.IsAbstract && (ContainingType.TypeKind == TypeKind.Class || ContainingType.TypeKind == TypeKind.Submission))
		{
			diagnostics.Add(ErrorCode.ERR_AbstractInConcreteClass, location, this, ContainingType);
		}
		else if (IsVirtual && ContainingType.IsSealed)
		{
			diagnostics.Add(ErrorCode.ERR_NewVirtualInSealed, location, this, ContainingType);
		}
		else if (!hasBody && IsAsync)
		{
			diagnostics.Add(ErrorCode.ERR_BadAsyncLacksBody, location);
		}
		else if (!hasBody && !IsExtern && !IsAbstract && !IsPartial && !IsExpressionBodied)
		{
			diagnostics.Add(ErrorCode.ERR_ConcreteMissingBody, location, this);
		}
		else if (ContainingType.IsSealed && DeclaredAccessibility.HasProtected() && !IsOverride)
		{
			diagnostics.Add(AccessCheck.GetProtectedMemberInSealedTypeError(ContainingType), location, this);
		}
		else if (ContainingType.IsStatic && !IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_InstanceMemberInStaticClass, location, Name);
		}
		else if (_lazyIsVararg && (IsGenericMethod || ContainingType.IsGenericType || (_lazyParameters.Length > 0 && _lazyParameters[_lazyParameters.Length - 1].IsParams)))
		{
			diagnostics.Add(ErrorCode.ERR_BadVarargs, location);
		}
		else if (_lazyIsVararg && IsAsync)
		{
			diagnostics.Add(ErrorCode.ERR_VarargsAsync, location);
		}
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		if (IsExtensionMethod)
		{
			CSharpCompilation declaringCompilation = DeclaringCompilation;
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_ExtensionAttribute__ctor));
		}
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		SourcePartialImplementation?.ForceComplete(locationOpt, cancellationToken);
		base.ForceComplete(locationOpt, cancellationToken);
	}

	internal override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!base.IsDefinedInSourceTree(tree, definedWithinSpan, cancellationToken))
		{
			return SourcePartialImplementation?.IsDefinedInSourceTree(tree, definedWithinSpan, cancellationToken) ?? false;
		}
		return true;
	}

	internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, DiagnosticBag diagnostics)
	{
		if ((object)_explicitInterfaceType != null)
		{
			MethodDeclarationSyntax syntax = GetSyntax();
			_explicitInterfaceType.CheckAllConstraints(conversions, new SourceLocation(syntax.ExplicitInterfaceSpecifier.Name), diagnostics);
		}
		ReturnType.CheckAllConstraints(conversions, Locations[0], diagnostics);
		foreach (ParameterSymbol parameter in Parameters)
		{
			parameter.Type.CheckAllConstraints(conversions, parameter.Locations[0], diagnostics);
		}
		SourceOrdinaryMethodSymbol sourcePartialImplementation = SourcePartialImplementation;
		if ((object)sourcePartialImplementation != null)
		{
			PartialMethodChecks(this, sourcePartialImplementation, diagnostics);
		}
		if (_refKind == RefKind.In)
		{
			DeclaringCompilation.EnsureIsReadOnlyAttributeExists(diagnostics, GetSyntax().ReturnType.Location, modifyCompilationForRefReadOnly: true);
		}
		ParameterHelpers.EnsureIsReadOnlyAttributeExists(Parameters, diagnostics, modifyCompilationForRefReadOnly: true);
	}

	private static void PartialMethodChecks(SourceOrdinaryMethodSymbol definition, SourceOrdinaryMethodSymbol implementation, DiagnosticBag diagnostics)
	{
		if (definition.IsStatic != implementation.IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMethodStaticDifference, implementation.Locations[0]);
		}
		if (definition.IsExtensionMethod != implementation.IsExtensionMethod)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMethodExtensionDifference, implementation.Locations[0]);
		}
		if (definition.IsUnsafe != implementation.IsUnsafe && definition.CompilationAllowsUnsafe())
		{
			diagnostics.Add(ErrorCode.ERR_PartialMethodUnsafeDifference, implementation.Locations[0]);
		}
		if (definition.IsParams() != implementation.IsParams())
		{
			diagnostics.Add(ErrorCode.ERR_PartialMethodParamsDifference, implementation.Locations[0]);
		}
		if (!HaveSameConstraints(definition, implementation))
		{
			diagnostics.Add(ErrorCode.ERR_PartialMethodInconsistentConstraints, implementation.Locations[0], implementation);
		}
	}

	private static bool HaveSameConstraints(SourceOrdinaryMethodSymbol part1, SourceOrdinaryMethodSymbol part2)
	{
		ImmutableArray<TypeParameterSymbol> typeParameters = part1.TypeParameters;
		int length = typeParameters.Length;
		if (length == 0)
		{
			return true;
		}
		ImmutableArray<TypeParameterSymbol> typeParameters2 = part2.TypeParameters;
		ImmutableArray<TypeParameterSymbol> to = IndexedTypeParameterSymbol.Take(length);
		TypeMap typeMap = new TypeMap(typeParameters, to, allowAlpha: true);
		TypeMap typeMap2 = new TypeMap(typeParameters2, to, allowAlpha: true);
		return MemberSignatureComparer.HaveSameConstraints(typeParameters, typeMap, typeParameters2, typeMap2);
	}

	internal override bool CallsAreOmitted(SyntaxTree syntaxTree)
	{
		if (IsPartialWithoutImplementation)
		{
			return true;
		}
		return base.CallsAreOmitted(syntaxTree);
	}
}
