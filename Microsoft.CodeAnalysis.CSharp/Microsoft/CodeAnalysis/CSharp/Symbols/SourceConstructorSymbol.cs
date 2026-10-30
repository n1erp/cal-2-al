using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceConstructorSymbol : SourceMemberMethodSymbol
{
	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private TypeSymbol _lazyReturnType;

	private bool _lazyIsVararg;

	private readonly bool _isExpressionBodied;

	public override bool IsVararg
	{
		get
		{
			LazyMethodChecks();
			return _lazyIsVararg;
		}
	}

	public override bool IsImplicitlyDeclared => base.IsImplicitlyDeclared;

	internal override int ParameterCount
	{
		get
		{
			if (!_lazyParameters.IsDefault)
			{
				return _lazyParameters.Length;
			}
			return GetSyntax().ParameterList.ParameterCount;
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

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override ImmutableArray<TypeParameterConstraintClause> TypeParameterConstraintClauses => ImmutableArray<TypeParameterConstraintClause>.Empty;

	public override RefKind RefKind => RefKind.None;

	public override TypeSymbol ReturnType
	{
		get
		{
			LazyMethodChecks();
			return _lazyReturnType;
		}
	}

	public override string Name
	{
		get
		{
			if (!IsStatic)
			{
				return ".ctor";
			}
			return ".cctor";
		}
	}

	protected override IAttributeTargetSymbol AttributeOwner => base.AttributeOwner;

	internal override bool IsExpressionBodied => _isExpressionBodied;

	internal override bool GenerateDebugInfo => true;

	public static SourceConstructorSymbol CreateConstructorSymbol(SourceMemberContainerTypeSymbol containingType, ConstructorDeclarationSyntax syntax, DiagnosticBag diagnostics)
	{
		MethodKind methodKind = ((!syntax.Modifiers.Any(SyntaxKind.StaticKeyword)) ? MethodKind.Constructor : MethodKind.StaticConstructor);
		return new SourceConstructorSymbol(containingType, syntax.Identifier.GetLocation(), syntax, methodKind, diagnostics);
	}

	private SourceConstructorSymbol(SourceMemberContainerTypeSymbol containingType, Location location, ConstructorDeclarationSyntax syntax, MethodKind methodKind, DiagnosticBag diagnostics)
		: base(containingType, syntax.GetReference(), ImmutableArray.Create(location))
	{
		DeclarationModifiers declarationModifiers = MakeModifiers(syntax.Modifiers, methodKind, location, diagnostics, out var modifierErrors);
		MakeFlags(methodKind, declarationModifiers, returnsVoid: true, isExtensionMethod: false);
		if (syntax.Identifier.ValueText != containingType.Name)
		{
			diagnostics.Add(ErrorCode.ERR_MemberNeedsType, location);
		}
		bool flag = syntax.Body != null;
		_isExpressionBodied = !flag && syntax.ExpressionBody != null;
		if (IsExtern)
		{
			if (methodKind == MethodKind.Constructor && syntax.Initializer != null)
			{
				diagnostics.Add(ErrorCode.ERR_ExternHasConstructorInitializer, location, this);
			}
			if (flag || _isExpressionBodied)
			{
				diagnostics.Add(ErrorCode.ERR_ExternHasBody, location, this);
			}
		}
		CSDiagnosticInfo cSDiagnosticInfo = ModifierUtils.CheckAccessibility(DeclarationModifiers);
		if (cSDiagnosticInfo != null)
		{
			diagnostics.Add(cSDiagnosticInfo, location);
		}
		if (!modifierErrors)
		{
			CheckModifiers(methodKind, flag || _isExpressionBodied, location, diagnostics);
		}
		Symbol.CheckForBlockAndExpressionBody(syntax.Body, syntax.ExpressionBody, syntax, diagnostics);
	}

	protected override void MethodChecks(DiagnosticBag diagnostics)
	{
		ConstructorDeclarationSyntax syntax = GetSyntax();
		BinderFactory binderFactory = DeclaringCompilation.GetBinderFactory(syntax.SyntaxTree);
		ParameterListSyntax parameterList = syntax.ParameterList;
		Binder binder = binderFactory.GetBinder(parameterList, syntax, this).WithContainingMemberOrLambda(this);
		_lazyParameters = ParameterHelpers.MakeParameters(binder, this, parameterList, out var arglistToken, diagnostics, allowRefOrOut: true, allowThis: false, addRefReadOnlyModifier: false);
		_lazyIsVararg = arglistToken.Kind() == SyntaxKind.ArgListKeyword;
		_lazyReturnType = binder.GetSpecialType(SpecialType.System_Void, diagnostics, syntax);
		Location location = Locations[0];
		if (MethodKind == MethodKind.StaticConstructor && _lazyParameters.Length != 0)
		{
			diagnostics.Add(ErrorCode.ERR_StaticConstParam, location, this);
		}
		CheckEffectiveAccessibility(_lazyReturnType, _lazyParameters, diagnostics);
		if (_lazyIsVararg && (IsGenericMethod || ContainingType.IsGenericType || (_lazyParameters.Length > 0 && _lazyParameters[_lazyParameters.Length - 1].IsParams)))
		{
			diagnostics.Add(ErrorCode.ERR_BadVarargs, location);
		}
	}

	internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, DiagnosticBag diagnostics)
	{
		base.AfterAddingTypeMembersChecks(conversions, diagnostics);
		ParameterHelpers.EnsureIsReadOnlyAttributeExists(Parameters, diagnostics, modifyCompilationForRefReadOnly: true);
	}

	internal ConstructorDeclarationSyntax GetSyntax()
	{
		return (ConstructorDeclarationSyntax)syntaxReferenceOpt.GetSyntax();
	}

	private DeclarationModifiers MakeModifiers(SyntaxTokenList modifiers, MethodKind methodKind, Location location, DiagnosticBag diagnostics, out bool modifierErrors)
	{
		DeclarationModifiers defaultAccess = ((methodKind != MethodKind.StaticConstructor) ? DeclarationModifiers.Private : DeclarationModifiers.None);
		DeclarationModifiers declarationModifiers = ModifierUtils.MakeAndCheckNontypeMemberModifiers(modifiers, defaultAccess, DeclarationModifiers.AccessibilityMask | DeclarationModifiers.Static | DeclarationModifiers.Extern | DeclarationModifiers.Unsafe, location, diagnostics, out modifierErrors);
		this.CheckUnsafeModifier(declarationModifiers, diagnostics);
		if (methodKind == MethodKind.StaticConstructor)
		{
			if ((declarationModifiers & DeclarationModifiers.AccessibilityMask) != 0)
			{
				diagnostics.Add(ErrorCode.ERR_StaticConstructorWithAccessModifiers, location, this);
				declarationModifiers &= ~DeclarationModifiers.AccessibilityMask;
				modifierErrors = true;
			}
			declarationModifiers |= DeclarationModifiers.Private;
		}
		return declarationModifiers;
	}

	private void CheckModifiers(MethodKind methodKind, bool hasBody, Location location, DiagnosticBag diagnostics)
	{
		if (!hasBody && !IsExtern)
		{
			diagnostics.Add(ErrorCode.ERR_ConcreteMissingBody, location, this);
		}
		else if (ContainingType.IsSealed && DeclaredAccessibility.HasProtected() && !IsOverride)
		{
			diagnostics.Add(AccessCheck.GetProtectedMemberInSealedTypeError(ContainingType), location, this);
		}
		else if (ContainingType.IsStatic && methodKind == MethodKind.Constructor)
		{
			diagnostics.Add(ErrorCode.ERR_ConstructorInStaticClass, location);
		}
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(((ConstructorDeclarationSyntax)SyntaxNode).AttributeLists);
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetReturnTypeAttributeDeclarations()
	{
		return OneOrMany.Create(default(SyntaxList<AttributeListSyntax>));
	}

	internal override int CalculateLocalSyntaxOffset(int position, SyntaxTree tree)
	{
		ConstructorDeclarationSyntax syntax = GetSyntax();
		if (tree == syntax.SyntaxTree)
		{
			BlockSyntax body = syntax.Body;
			if (body != null && body.Span.Contains(position))
			{
				return position - syntax.Body.Span.Start;
			}
			ArrowExpressionClauseSyntax expressionBody = syntax.ExpressionBody;
			if (expressionBody != null && expressionBody.Span.Contains(position))
			{
				return position - syntax.ExpressionBody.Span.Start;
			}
		}
		if (position == syntax.SpanStart)
		{
			return -1;
		}
		ConstructorInitializerSyntax initializer = syntax.Initializer;
		int num;
		if (tree == initializer?.SyntaxTree)
		{
			TextSpan span = initializer.Span;
			num = span.Length;
			if (span.Contains(position))
			{
				return -num + (position - span.Start);
			}
		}
		else
		{
			num = 0;
		}
		if (((SourceNamedTypeSymbol)ContainingType).TryCalculateSyntaxOffsetOfPositionInInitializer(position, tree, IsStatic, num, out var syntaxOffset))
		{
			return syntaxOffset;
		}
		throw ExceptionUtilities.Unreachable;
	}
}
