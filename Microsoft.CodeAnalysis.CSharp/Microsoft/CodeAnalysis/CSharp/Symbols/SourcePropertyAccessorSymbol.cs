using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourcePropertyAccessorSymbol : SourceMemberMethodSymbol
{
	private readonly SourcePropertySymbol _property;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private TypeSymbol _lazyReturnType;

	private CustomModifiersTuple _lazyCustomModifiers;

	private readonly ImmutableArray<MethodSymbol> _explicitInterfaceImplementations;

	private readonly string _name;

	private readonly bool _isAutoPropertyAccessor;

	private readonly bool _isExpressionBodied;

	internal override bool IsExpressionBodied => _isExpressionBodied;

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			Accessibility localAccessibility = LocalAccessibility;
			if (localAccessibility != Accessibility.NotApplicable)
			{
				return localAccessibility;
			}
			return _property.DeclaredAccessibility;
		}
	}

	public override Symbol AssociatedSymbol => _property;

	public override bool IsVararg => false;

	public override bool ReturnsVoid => ReturnType.SpecialType == SpecialType.System_Void;

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			LazyMethodChecks();
			return _lazyParameters;
		}
	}

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override ImmutableArray<TypeParameterConstraintClause> TypeParameterConstraintClauses => ImmutableArray<TypeParameterConstraintClause>.Empty;

	public override RefKind RefKind => _property.RefKind;

	public override TypeSymbol ReturnType
	{
		get
		{
			LazyMethodChecks();
			return _lazyReturnType;
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

	internal Accessibility LocalAccessibility => ModifierUtils.EffectiveAccessibility(DeclarationModifiers);

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => _explicitInterfaceImplementations;

	public override string Name => _name;

	public override bool IsImplicitlyDeclared => false;

	internal override bool GenerateDebugInfo => true;

	public static SourcePropertyAccessorSymbol CreateAccessorSymbol(NamedTypeSymbol containingType, SourcePropertySymbol property, DeclarationModifiers propertyModifiers, string propertyName, AccessorDeclarationSyntax syntax, PropertySymbol explicitlyImplementedPropertyOpt, string aliasQualifierOpt, bool isAutoPropertyAccessor, DiagnosticBag diagnostics)
	{
		bool flag = syntax.Kind() == SyntaxKind.GetAccessorDeclaration;
		GetNameAndExplicitInterfaceImplementations(explicitlyImplementedPropertyOpt, propertyName, property.IsCompilationOutputWinMdObj(), aliasQualifierOpt, flag, out var name, out var explicitInterfaceImplementations);
		MethodKind methodKind = (flag ? MethodKind.PropertyGet : MethodKind.PropertySet);
		return new SourcePropertyAccessorSymbol(containingType, name, property, propertyModifiers, explicitInterfaceImplementations, syntax.Keyword.GetLocation(), syntax, methodKind, isAutoPropertyAccessor, diagnostics);
	}

	public static SourcePropertyAccessorSymbol CreateAccessorSymbol(NamedTypeSymbol containingType, SourcePropertySymbol property, DeclarationModifiers propertyModifiers, string propertyName, ArrowExpressionClauseSyntax syntax, PropertySymbol explicitlyImplementedPropertyOpt, string aliasQualifierOpt, DiagnosticBag diagnostics)
	{
		GetNameAndExplicitInterfaceImplementations(explicitlyImplementedPropertyOpt, propertyName, property.IsCompilationOutputWinMdObj(), aliasQualifierOpt, isGetMethod: true, out var name, out var explicitInterfaceImplementations);
		return new SourcePropertyAccessorSymbol(containingType, name, property, propertyModifiers, explicitInterfaceImplementations, syntax.Expression.GetLocation(), syntax, diagnostics);
	}

	private static void GetNameAndExplicitInterfaceImplementations(PropertySymbol explicitlyImplementedPropertyOpt, string propertyName, bool isWinMd, string aliasQualifierOpt, bool isGetMethod, out string name, out ImmutableArray<MethodSymbol> explicitInterfaceImplementations)
	{
		if ((object)explicitlyImplementedPropertyOpt == null)
		{
			name = GetAccessorName(propertyName, isGetMethod, isWinMd);
			explicitInterfaceImplementations = ImmutableArray<MethodSymbol>.Empty;
			return;
		}
		MethodSymbol methodSymbol = (isGetMethod ? explicitlyImplementedPropertyOpt.GetMethod : explicitlyImplementedPropertyOpt.SetMethod);
		string name2 = (((object)methodSymbol != null) ? methodSymbol.Name : GetAccessorName(explicitlyImplementedPropertyOpt.MetadataName, isGetMethod, isWinMd));
		name = ExplicitInterfaceHelpers.GetMemberName(name2, explicitlyImplementedPropertyOpt.ContainingType, aliasQualifierOpt);
		explicitInterfaceImplementations = (((object)methodSymbol == null) ? ImmutableArray<MethodSymbol>.Empty : ImmutableArray.Create(methodSymbol));
	}

	private SourcePropertyAccessorSymbol(NamedTypeSymbol containingType, string name, SourcePropertySymbol property, DeclarationModifiers propertyModifiers, ImmutableArray<MethodSymbol> explicitInterfaceImplementations, Location location, ArrowExpressionClauseSyntax syntax, DiagnosticBag diagnostics)
		: base(containingType, syntax.GetReference(), location)
	{
		_property = property;
		_explicitInterfaceImplementations = explicitInterfaceImplementations;
		_name = name;
		_isAutoPropertyAccessor = false;
		_isExpressionBodied = true;
		DeclarationModifiers declarationModifiers = propertyModifiers & ~DeclarationModifiers.Indexer;
		MakeFlags(MethodKind.PropertyGet, declarationModifiers, returnsVoid: false, isExtensionMethod: false, explicitInterfaceImplementations.Any());
		CheckModifiersForBody(location, diagnostics);
		CSDiagnosticInfo cSDiagnosticInfo = ModifierUtils.CheckAccessibility(DeclarationModifiers);
		if (cSDiagnosticInfo != null)
		{
			diagnostics.Add(cSDiagnosticInfo, location);
		}
		CheckModifiers(location, hasBody: true, isAutoPropertyOrExpressionBodied: true, diagnostics);
		if (IsOverride)
		{
			MethodSymbol overriddenMethod = OverriddenMethod;
			if ((object)overriddenMethod != null)
			{
				_name = overriddenMethod.Name;
			}
		}
	}

	private SourcePropertyAccessorSymbol(NamedTypeSymbol containingType, string name, SourcePropertySymbol property, DeclarationModifiers propertyModifiers, ImmutableArray<MethodSymbol> explicitInterfaceImplementations, Location location, AccessorDeclarationSyntax syntax, MethodKind methodKind, bool isAutoPropertyAccessor, DiagnosticBag diagnostics)
		: base(containingType, syntax.GetReference(), location)
	{
		_property = property;
		_explicitInterfaceImplementations = explicitInterfaceImplementations;
		_name = name;
		_isAutoPropertyAccessor = isAutoPropertyAccessor;
		bool flag = syntax.Body != null;
		bool flag2 = syntax.ExpressionBody != null;
		_isExpressionBodied = !flag & flag2;
		DeclarationModifiers declarationModifiers = MakeModifiers(syntax, location, diagnostics, out var modifierErrors);
		propertyModifiers &= ~DeclarationModifiers.AccessibilityMask;
		if ((declarationModifiers & DeclarationModifiers.Private) != 0)
		{
			propertyModifiers &= ~DeclarationModifiers.Virtual;
		}
		declarationModifiers |= propertyModifiers & ~DeclarationModifiers.Indexer;
		MakeFlags(methodKind, declarationModifiers, returnsVoid: false, isExtensionMethod: false, explicitInterfaceImplementations.Any());
		if (flag | flag2)
		{
			CheckModifiersForBody(location, diagnostics);
		}
		CSDiagnosticInfo cSDiagnosticInfo = ModifierUtils.CheckAccessibility(DeclarationModifiers);
		if (cSDiagnosticInfo != null)
		{
			diagnostics.Add(cSDiagnosticInfo, location);
		}
		if (!modifierErrors)
		{
			CheckModifiers(location, flag | flag2, isAutoPropertyAccessor, diagnostics);
		}
		if (IsOverride)
		{
			MethodSymbol overriddenMethod = OverriddenMethod;
			if ((object)overriddenMethod != null)
			{
				_name = overriddenMethod.Name;
			}
		}
		Symbol.CheckForBlockAndExpressionBody(syntax.Body, syntax.ExpressionBody, syntax, diagnostics);
	}

	protected override void MethodChecks(DiagnosticBag diagnostics)
	{
		_lazyParameters = ComputeParameters(diagnostics);
		_lazyReturnType = ComputeReturnType(diagnostics);
		_lazyCustomModifiers = CustomModifiersTuple.Empty;
		if (_explicitInterfaceImplementations.Length > 0)
		{
			CustomModifierUtils.CopyMethodCustomModifiers(_explicitInterfaceImplementations[0], this, out _lazyReturnType, out _lazyCustomModifiers, out _lazyParameters, alsoCopyParamsModifier: false);
		}
		else if (IsOverride)
		{
			MethodSymbol overriddenMethod = OverriddenMethod;
			if ((object)overriddenMethod != null)
			{
				CustomModifierUtils.CopyMethodCustomModifiers(overriddenMethod, this, out _lazyReturnType, out _lazyCustomModifiers, out _lazyParameters, alsoCopyParamsModifier: true);
			}
		}
		else if (_lazyReturnType.SpecialType != SpecialType.System_Void)
		{
			PropertySymbol property = _property;
			_lazyReturnType = CustomModifierUtils.CopyTypeCustomModifiers(property.Type, _lazyReturnType, ContainingAssembly);
			_lazyCustomModifiers = CustomModifiersTuple.Create(property.TypeCustomModifiers, property.RefCustomModifiers);
		}
	}

	private TypeSymbol ComputeReturnType(DiagnosticBag diagnostics)
	{
		if (MethodKind == MethodKind.PropertyGet)
		{
			TypeSymbol type = _property.Type;
			if (!ContainingType.IsInterfaceType() && type.IsStatic)
			{
				diagnostics.Add(ErrorCode.ERR_ReturnTypeIsStaticClass, locations[0], type);
			}
			return type;
		}
		return GetBinder().GetSpecialType(SpecialType.System_Void, diagnostics, GetSyntax());
	}

	private Binder GetBinder()
	{
		CSharpSyntaxNode syntax = GetSyntax();
		return DeclaringCompilation.GetBinderFactory(syntax.SyntaxTree).GetBinder(syntax);
	}

	private DeclarationModifiers MakeModifiers(AccessorDeclarationSyntax syntax, Location location, DiagnosticBag diagnostics, out bool modifierErrors)
	{
		DeclarationModifiers declarationModifiers = ModifierUtils.MakeAndCheckNontypeMemberModifiers(syntax.Modifiers, DeclarationModifiers.None, DeclarationModifiers.AccessibilityMask, location, diagnostics, out modifierErrors);
		if (ContainingType.IsInterface && (declarationModifiers & DeclarationModifiers.AccessibilityMask) != 0)
		{
			diagnostics.Add(ErrorCode.ERR_PropertyAccessModInInterface, location, this);
			declarationModifiers &= ~DeclarationModifiers.AccessibilityMask;
		}
		return declarationModifiers;
	}

	private void CheckModifiers(Location location, bool hasBody, bool isAutoPropertyOrExpressionBodied, DiagnosticBag diagnostics)
	{
		Accessibility localAccessibility = LocalAccessibility;
		if (IsAbstract && !ContainingType.IsAbstract && (ContainingType.TypeKind == TypeKind.Class || ContainingType.TypeKind == TypeKind.Submission))
		{
			diagnostics.Add(ErrorCode.ERR_AbstractInConcreteClass, location, this, ContainingType);
		}
		else if (IsVirtual && ContainingType.IsSealed && ContainingType.TypeKind != TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_NewVirtualInSealed, location, this, ContainingType);
		}
		else if (!hasBody && !IsExtern && !IsAbstract && !isAutoPropertyOrExpressionBodied)
		{
			diagnostics.Add(ErrorCode.ERR_ConcreteMissingBody, location, this);
		}
		else if (ContainingType.IsSealed && localAccessibility.HasProtected() && !IsOverride)
		{
			diagnostics.Add(AccessCheck.GetProtectedMemberInSealedTypeError(ContainingType), location, this);
		}
	}

	internal static string GetAccessorName(string propertyName, bool getNotSet, bool isWinMdOutput)
	{
		string text;
		if (getNotSet)
		{
			text = "get_";
		}
		else
		{
			text = (isWinMdOutput ? "put_" : "set_");
		}
		return text + propertyName;
	}

	internal CSharpSyntaxNode GetSyntax()
	{
		return (CSharpSyntaxNode)syntaxReferenceOpt.GetSyntax();
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		CSharpSyntaxNode syntax = GetSyntax();
		SyntaxKind syntaxKind = syntax.Kind();
		if (syntaxKind - 8896 <= SyntaxKind.List)
		{
			return OneOrMany.Create(((AccessorDeclarationSyntax)syntax).AttributeLists);
		}
		return base.GetAttributeDeclarations();
	}

	private ImmutableArray<ParameterSymbol> ComputeParameters(DiagnosticBag diagnostics)
	{
		bool flag = MethodKind == MethodKind.PropertyGet;
		ImmutableArray<ParameterSymbol> parameters = _property.Parameters;
		int num = parameters.Length + ((!flag) ? 1 : 0);
		if (num == 0)
		{
			return ImmutableArray<ParameterSymbol>.Empty;
		}
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance(num);
		foreach (SourceParameterSymbol item in parameters)
		{
			instance.Add(new SourceClonedParameterSymbol(item, this, item.Ordinal, suppressOptional: false));
		}
		if (!flag)
		{
			TypeSymbol type = _property.Type;
			if (!ContainingType.IsInterfaceType() && type.IsStatic)
			{
				diagnostics.Add(ErrorCode.ERR_ParameterIsStaticClass, locations[0], type);
			}
			instance.Add(new SynthesizedAccessorValueParameterSymbol(this, type, instance.Count, _property.TypeCustomModifiers));
		}
		return instance.ToImmutableAndFree();
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		if (_isAutoPropertyAccessor)
		{
			CSharpCompilation declaringCompilation = DeclaringCompilation;
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
		}
	}
}
