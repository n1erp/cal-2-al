using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class InitializerSemanticModel : MemberSemanticModel
{
	private InitializerSemanticModel(CSharpSyntaxNode syntax, Symbol symbol, Binder rootBinder, SyntaxTreeSemanticModel containingSemanticModelOpt = null, SyntaxTreeSemanticModel parentSemanticModelOpt = null, int speculatedPosition = 0)
		: base(syntax, symbol, rootBinder, containingSemanticModelOpt, parentSemanticModelOpt, speculatedPosition)
	{
	}

	internal static InitializerSemanticModel Create(SyntaxTreeSemanticModel containingSemanticModel, CSharpSyntaxNode syntax, FieldSymbol fieldSymbol, Binder rootBinder)
	{
		return new InitializerSemanticModel(syntax, fieldSymbol, rootBinder, containingSemanticModel);
	}

	internal static InitializerSemanticModel Create(SyntaxTreeSemanticModel containingSemanticModel, CSharpSyntaxNode syntax, PropertySymbol propertySymbol, Binder rootBinder)
	{
		return new InitializerSemanticModel(syntax, propertySymbol, rootBinder, containingSemanticModel);
	}

	internal static InitializerSemanticModel Create(SyntaxTreeSemanticModel containingSemanticModel, ParameterSyntax syntax, ParameterSymbol parameterSymbol, Binder rootBinder)
	{
		return new InitializerSemanticModel(syntax, parameterSymbol, rootBinder, containingSemanticModel);
	}

	internal static InitializerSemanticModel CreateSpeculative(SyntaxTreeSemanticModel parentSemanticModel, Symbol owner, CSharpSyntaxNode syntax, Binder rootBinder, int position)
	{
		return new InitializerSemanticModel(syntax, owner, rootBinder, null, parentSemanticModel, position);
	}

	protected internal override CSharpSyntaxNode GetBindableSyntaxNode(CSharpSyntaxNode node)
	{
		if (!IsBindableInitializer(node))
		{
			return base.GetBindableSyntaxNode(node);
		}
		return node;
	}

	internal override BoundNode GetBoundRoot()
	{
		CSharpSyntaxNode cSharpSyntaxNode = Root;
		switch (cSharpSyntaxNode.Kind())
		{
		case SyntaxKind.VariableDeclarator:
			cSharpSyntaxNode = ((VariableDeclaratorSyntax)cSharpSyntaxNode).Initializer.Value;
			break;
		case SyntaxKind.Parameter:
			cSharpSyntaxNode = ((ParameterSyntax)cSharpSyntaxNode).Default?.Value;
			break;
		case SyntaxKind.EqualsValueClause:
			cSharpSyntaxNode = ((EqualsValueClauseSyntax)cSharpSyntaxNode).Value;
			break;
		case SyntaxKind.EnumMemberDeclaration:
			cSharpSyntaxNode = ((EnumMemberDeclarationSyntax)cSharpSyntaxNode).EqualsValue.Value;
			break;
		case SyntaxKind.PropertyDeclaration:
			cSharpSyntaxNode = ((PropertyDeclarationSyntax)cSharpSyntaxNode).Initializer.Value;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(cSharpSyntaxNode.Kind());
		case SyntaxKind.ArgumentList:
		case SyntaxKind.BaseConstructorInitializer:
		case SyntaxKind.ThisConstructorInitializer:
			break;
		}
		return GetUpperBoundNode(GetBindableSyntaxNode(cSharpSyntaxNode));
	}

	internal override BoundNode Bind(Binder binder, CSharpSyntaxNode node, DiagnosticBag diagnostics)
	{
		EqualsValueClauseSyntax equalsValueClauseSyntax = null;
		switch (node.Kind())
		{
		case SyntaxKind.EqualsValueClause:
			equalsValueClauseSyntax = (EqualsValueClauseSyntax)node;
			break;
		case SyntaxKind.VariableDeclarator:
			equalsValueClauseSyntax = ((VariableDeclaratorSyntax)node).Initializer;
			break;
		case SyntaxKind.PropertyDeclaration:
			equalsValueClauseSyntax = ((PropertyDeclarationSyntax)node).Initializer;
			break;
		case SyntaxKind.Parameter:
			equalsValueClauseSyntax = ((ParameterSyntax)node).Default;
			break;
		case SyntaxKind.EnumMemberDeclaration:
			equalsValueClauseSyntax = ((EnumMemberDeclarationSyntax)node).EqualsValue;
			break;
		}
		if (equalsValueClauseSyntax != null)
		{
			return BindEqualsValue(binder, equalsValueClauseSyntax, diagnostics);
		}
		return base.Bind(binder, node, diagnostics);
	}

	private BoundEqualsValue BindEqualsValue(Binder binder, EqualsValueClauseSyntax equalsValue, DiagnosticBag diagnostics)
	{
		switch (MemberSymbol.Kind)
		{
		case SymbolKind.Field:
		{
			FieldSymbol fieldSymbol = (FieldSymbol)MemberSymbol;
			if (fieldSymbol is SourceEnumConstantSymbol symbol)
			{
				return binder.BindEnumConstantInitializer(symbol, equalsValue, diagnostics);
			}
			return binder.BindFieldInitializer(fieldSymbol, equalsValue, diagnostics);
		}
		case SymbolKind.Property:
		{
			SourcePropertySymbol sourcePropertySymbol = (SourcePropertySymbol)MemberSymbol;
			BoundFieldEqualsValue boundFieldEqualsValue = binder.BindFieldInitializer(sourcePropertySymbol.BackingField, equalsValue, diagnostics);
			return new BoundPropertyEqualsValue(boundFieldEqualsValue.Syntax, sourcePropertySymbol, boundFieldEqualsValue.Locals, boundFieldEqualsValue.Value);
		}
		case SymbolKind.Parameter:
		{
			ParameterSymbol parameter = (ParameterSymbol)MemberSymbol;
			BoundExpression valueBeforeConversion;
			return binder.BindParameterDefaultValue(equalsValue, parameter, diagnostics, out valueBeforeConversion);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(MemberSymbol.Kind);
		}
	}

	private bool IsBindableInitializer(CSharpSyntaxNode node)
	{
		switch (node.Kind())
		{
		case SyntaxKind.EqualsValueClause:
			if (Root != node)
			{
				return Root == node.Parent;
			}
			return true;
		case SyntaxKind.ArgumentList:
		case SyntaxKind.BaseConstructorInitializer:
		case SyntaxKind.ThisConstructorInitializer:
			return Root == node;
		default:
			return false;
		}
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, EqualsValueClauseSyntax initializer, out SemanticModel speculativeModel)
	{
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder == null)
		{
			speculativeModel = null;
			return false;
		}
		enclosingBinder = new ExecutableCodeBinder(initializer, enclosingBinder.ContainingMemberOrLambda, enclosingBinder);
		speculativeModel = CreateSpeculative(parentModel, MemberSymbol, initializer, enclosingBinder, position);
		return true;
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ConstructorInitializerSyntax constructorInitializer, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ArrowExpressionClauseSyntax expressionBody, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, StatementSyntax statement, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}

	internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, BaseMethodDeclarationSyntax method, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}

	internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, AccessorDeclarationSyntax accessor, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}
}
