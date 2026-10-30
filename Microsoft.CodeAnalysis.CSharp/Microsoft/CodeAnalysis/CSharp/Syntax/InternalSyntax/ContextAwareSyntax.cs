using System;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class ContextAwareSyntax
{
	private SyntaxFactoryContext context;

	public ContextAwareSyntax(SyntaxFactoryContext context)
	{
		this.context = context;
	}

	public IdentifierNameSyntax IdentifierName(SyntaxToken identifier)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8616, identifier, context, out var hash);
		if (greenNode != null)
		{
			return (IdentifierNameSyntax)greenNode;
		}
		IdentifierNameSyntax identifierNameSyntax = new IdentifierNameSyntax(SyntaxKind.IdentifierName, identifier, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(identifierNameSyntax, hash);
		}
		return identifierNameSyntax;
	}

	public QualifiedNameSyntax QualifiedName(NameSyntax left, SyntaxToken dotToken, SimpleNameSyntax right)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8617, left, dotToken, right, context, out var hash);
		if (greenNode != null)
		{
			return (QualifiedNameSyntax)greenNode;
		}
		QualifiedNameSyntax qualifiedNameSyntax = new QualifiedNameSyntax(SyntaxKind.QualifiedName, left, dotToken, right, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(qualifiedNameSyntax, hash);
		}
		return qualifiedNameSyntax;
	}

	public GenericNameSyntax GenericName(SyntaxToken identifier, TypeArgumentListSyntax typeArgumentList)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8618, identifier, typeArgumentList, context, out var hash);
		if (greenNode != null)
		{
			return (GenericNameSyntax)greenNode;
		}
		GenericNameSyntax genericNameSyntax = new GenericNameSyntax(SyntaxKind.GenericName, identifier, typeArgumentList, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(genericNameSyntax, hash);
		}
		return genericNameSyntax;
	}

	public TypeArgumentListSyntax TypeArgumentList(SyntaxToken lessThanToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeSyntax> arguments, SyntaxToken greaterThanToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8619, lessThanToken, arguments.Node, greaterThanToken, context, out var hash);
		if (greenNode != null)
		{
			return (TypeArgumentListSyntax)greenNode;
		}
		TypeArgumentListSyntax typeArgumentListSyntax = new TypeArgumentListSyntax(SyntaxKind.TypeArgumentList, lessThanToken, arguments.Node, greaterThanToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(typeArgumentListSyntax, hash);
		}
		return typeArgumentListSyntax;
	}

	public AliasQualifiedNameSyntax AliasQualifiedName(IdentifierNameSyntax alias, SyntaxToken colonColonToken, SimpleNameSyntax name)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8620, alias, colonColonToken, name, context, out var hash);
		if (greenNode != null)
		{
			return (AliasQualifiedNameSyntax)greenNode;
		}
		AliasQualifiedNameSyntax aliasQualifiedNameSyntax = new AliasQualifiedNameSyntax(SyntaxKind.AliasQualifiedName, alias, colonColonToken, name, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(aliasQualifiedNameSyntax, hash);
		}
		return aliasQualifiedNameSyntax;
	}

	public PredefinedTypeSyntax PredefinedType(SyntaxToken keyword)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8621, keyword, context, out var hash);
		if (greenNode != null)
		{
			return (PredefinedTypeSyntax)greenNode;
		}
		PredefinedTypeSyntax predefinedTypeSyntax = new PredefinedTypeSyntax(SyntaxKind.PredefinedType, keyword, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(predefinedTypeSyntax, hash);
		}
		return predefinedTypeSyntax;
	}

	public ArrayTypeSyntax ArrayType(TypeSyntax elementType, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ArrayRankSpecifierSyntax> rankSpecifiers)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8622, elementType, rankSpecifiers.Node, context, out var hash);
		if (greenNode != null)
		{
			return (ArrayTypeSyntax)greenNode;
		}
		ArrayTypeSyntax arrayTypeSyntax = new ArrayTypeSyntax(SyntaxKind.ArrayType, elementType, rankSpecifiers.Node, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(arrayTypeSyntax, hash);
		}
		return arrayTypeSyntax;
	}

	public ArrayRankSpecifierSyntax ArrayRankSpecifier(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> sizes, SyntaxToken closeBracketToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8623, openBracketToken, sizes.Node, closeBracketToken, context, out var hash);
		if (greenNode != null)
		{
			return (ArrayRankSpecifierSyntax)greenNode;
		}
		ArrayRankSpecifierSyntax arrayRankSpecifierSyntax = new ArrayRankSpecifierSyntax(SyntaxKind.ArrayRankSpecifier, openBracketToken, sizes.Node, closeBracketToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(arrayRankSpecifierSyntax, hash);
		}
		return arrayRankSpecifierSyntax;
	}

	public PointerTypeSyntax PointerType(TypeSyntax elementType, SyntaxToken asteriskToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8624, elementType, asteriskToken, context, out var hash);
		if (greenNode != null)
		{
			return (PointerTypeSyntax)greenNode;
		}
		PointerTypeSyntax pointerTypeSyntax = new PointerTypeSyntax(SyntaxKind.PointerType, elementType, asteriskToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(pointerTypeSyntax, hash);
		}
		return pointerTypeSyntax;
	}

	public NullableTypeSyntax NullableType(TypeSyntax elementType, SyntaxToken questionToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8625, elementType, questionToken, context, out var hash);
		if (greenNode != null)
		{
			return (NullableTypeSyntax)greenNode;
		}
		NullableTypeSyntax nullableTypeSyntax = new NullableTypeSyntax(SyntaxKind.NullableType, elementType, questionToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(nullableTypeSyntax, hash);
		}
		return nullableTypeSyntax;
	}

	public TupleTypeSyntax TupleType(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TupleElementSyntax> elements, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8924, openParenToken, elements.Node, closeParenToken, context, out var hash);
		if (greenNode != null)
		{
			return (TupleTypeSyntax)greenNode;
		}
		TupleTypeSyntax tupleTypeSyntax = new TupleTypeSyntax(SyntaxKind.TupleType, openParenToken, elements.Node, closeParenToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(tupleTypeSyntax, hash);
		}
		return tupleTypeSyntax;
	}

	public TupleElementSyntax TupleElement(TypeSyntax type, SyntaxToken identifier)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8925, type, identifier, context, out var hash);
		if (greenNode != null)
		{
			return (TupleElementSyntax)greenNode;
		}
		TupleElementSyntax tupleElementSyntax = new TupleElementSyntax(SyntaxKind.TupleElement, type, identifier, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(tupleElementSyntax, hash);
		}
		return tupleElementSyntax;
	}

	public OmittedTypeArgumentSyntax OmittedTypeArgument(SyntaxToken omittedTypeArgumentToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8626, omittedTypeArgumentToken, context, out var hash);
		if (greenNode != null)
		{
			return (OmittedTypeArgumentSyntax)greenNode;
		}
		OmittedTypeArgumentSyntax omittedTypeArgumentSyntax = new OmittedTypeArgumentSyntax(SyntaxKind.OmittedTypeArgument, omittedTypeArgumentToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(omittedTypeArgumentSyntax, hash);
		}
		return omittedTypeArgumentSyntax;
	}

	public RefTypeSyntax RefType(SyntaxToken refKeyword, SyntaxToken readOnlyKeyword, TypeSyntax type)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(9051, refKeyword, readOnlyKeyword, type, context, out var hash);
		if (greenNode != null)
		{
			return (RefTypeSyntax)greenNode;
		}
		RefTypeSyntax refTypeSyntax = new RefTypeSyntax(SyntaxKind.RefType, refKeyword, readOnlyKeyword, type, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(refTypeSyntax, hash);
		}
		return refTypeSyntax;
	}

	public ParenthesizedExpressionSyntax ParenthesizedExpression(SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8632, openParenToken, expression, closeParenToken, context, out var hash);
		if (greenNode != null)
		{
			return (ParenthesizedExpressionSyntax)greenNode;
		}
		ParenthesizedExpressionSyntax parenthesizedExpressionSyntax = new ParenthesizedExpressionSyntax(SyntaxKind.ParenthesizedExpression, openParenToken, expression, closeParenToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(parenthesizedExpressionSyntax, hash);
		}
		return parenthesizedExpressionSyntax;
	}

	public TupleExpressionSyntax TupleExpression(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> arguments, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8926, openParenToken, arguments.Node, closeParenToken, context, out var hash);
		if (greenNode != null)
		{
			return (TupleExpressionSyntax)greenNode;
		}
		TupleExpressionSyntax tupleExpressionSyntax = new TupleExpressionSyntax(SyntaxKind.TupleExpression, openParenToken, arguments.Node, closeParenToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(tupleExpressionSyntax, hash);
		}
		return tupleExpressionSyntax;
	}

	public PrefixUnaryExpressionSyntax PrefixUnaryExpression(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax operand)
	{
		if (kind - 8730 > (SyntaxKind)7)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode((int)kind, operatorToken, operand, context, out var hash);
		if (greenNode != null)
		{
			return (PrefixUnaryExpressionSyntax)greenNode;
		}
		PrefixUnaryExpressionSyntax prefixUnaryExpressionSyntax = new PrefixUnaryExpressionSyntax(kind, operatorToken, operand, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(prefixUnaryExpressionSyntax, hash);
		}
		return prefixUnaryExpressionSyntax;
	}

	public AwaitExpressionSyntax AwaitExpression(SyntaxToken awaitKeyword, ExpressionSyntax expression)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8740, awaitKeyword, expression, context, out var hash);
		if (greenNode != null)
		{
			return (AwaitExpressionSyntax)greenNode;
		}
		AwaitExpressionSyntax awaitExpressionSyntax = new AwaitExpressionSyntax(SyntaxKind.AwaitExpression, awaitKeyword, expression, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(awaitExpressionSyntax, hash);
		}
		return awaitExpressionSyntax;
	}

	public PostfixUnaryExpressionSyntax PostfixUnaryExpression(SyntaxKind kind, ExpressionSyntax operand, SyntaxToken operatorToken)
	{
		if (kind - 8738 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode((int)kind, operand, operatorToken, context, out var hash);
		if (greenNode != null)
		{
			return (PostfixUnaryExpressionSyntax)greenNode;
		}
		PostfixUnaryExpressionSyntax postfixUnaryExpressionSyntax = new PostfixUnaryExpressionSyntax(kind, operand, operatorToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(postfixUnaryExpressionSyntax, hash);
		}
		return postfixUnaryExpressionSyntax;
	}

	public MemberAccessExpressionSyntax MemberAccessExpression(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken operatorToken, SimpleNameSyntax name)
	{
		if (kind - 8689 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode((int)kind, expression, operatorToken, name, context, out var hash);
		if (greenNode != null)
		{
			return (MemberAccessExpressionSyntax)greenNode;
		}
		MemberAccessExpressionSyntax memberAccessExpressionSyntax = new MemberAccessExpressionSyntax(kind, expression, operatorToken, name, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(memberAccessExpressionSyntax, hash);
		}
		return memberAccessExpressionSyntax;
	}

	public ConditionalAccessExpressionSyntax ConditionalAccessExpression(ExpressionSyntax expression, SyntaxToken operatorToken, ExpressionSyntax whenNotNull)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8691, expression, operatorToken, whenNotNull, context, out var hash);
		if (greenNode != null)
		{
			return (ConditionalAccessExpressionSyntax)greenNode;
		}
		ConditionalAccessExpressionSyntax conditionalAccessExpressionSyntax = new ConditionalAccessExpressionSyntax(SyntaxKind.ConditionalAccessExpression, expression, operatorToken, whenNotNull, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(conditionalAccessExpressionSyntax, hash);
		}
		return conditionalAccessExpressionSyntax;
	}

	public MemberBindingExpressionSyntax MemberBindingExpression(SyntaxToken operatorToken, SimpleNameSyntax name)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8707, operatorToken, name, context, out var hash);
		if (greenNode != null)
		{
			return (MemberBindingExpressionSyntax)greenNode;
		}
		MemberBindingExpressionSyntax memberBindingExpressionSyntax = new MemberBindingExpressionSyntax(SyntaxKind.MemberBindingExpression, operatorToken, name, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(memberBindingExpressionSyntax, hash);
		}
		return memberBindingExpressionSyntax;
	}

	public ElementBindingExpressionSyntax ElementBindingExpression(BracketedArgumentListSyntax argumentList)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8708, argumentList, context, out var hash);
		if (greenNode != null)
		{
			return (ElementBindingExpressionSyntax)greenNode;
		}
		ElementBindingExpressionSyntax elementBindingExpressionSyntax = new ElementBindingExpressionSyntax(SyntaxKind.ElementBindingExpression, argumentList, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(elementBindingExpressionSyntax, hash);
		}
		return elementBindingExpressionSyntax;
	}

	public ImplicitElementAccessSyntax ImplicitElementAccess(BracketedArgumentListSyntax argumentList)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8656, argumentList, context, out var hash);
		if (greenNode != null)
		{
			return (ImplicitElementAccessSyntax)greenNode;
		}
		ImplicitElementAccessSyntax implicitElementAccessSyntax = new ImplicitElementAccessSyntax(SyntaxKind.ImplicitElementAccess, argumentList, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(implicitElementAccessSyntax, hash);
		}
		return implicitElementAccessSyntax;
	}

	public BinaryExpressionSyntax BinaryExpression(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
	{
		if (kind - 8668 > (SyntaxKind)20)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode((int)kind, left, operatorToken, right, context, out var hash);
		if (greenNode != null)
		{
			return (BinaryExpressionSyntax)greenNode;
		}
		BinaryExpressionSyntax binaryExpressionSyntax = new BinaryExpressionSyntax(kind, left, operatorToken, right, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(binaryExpressionSyntax, hash);
		}
		return binaryExpressionSyntax;
	}

	public AssignmentExpressionSyntax AssignmentExpression(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
	{
		if (kind - 8714 > (SyntaxKind)10)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode((int)kind, left, operatorToken, right, context, out var hash);
		if (greenNode != null)
		{
			return (AssignmentExpressionSyntax)greenNode;
		}
		AssignmentExpressionSyntax assignmentExpressionSyntax = new AssignmentExpressionSyntax(kind, left, operatorToken, right, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(assignmentExpressionSyntax, hash);
		}
		return assignmentExpressionSyntax;
	}

	public ConditionalExpressionSyntax ConditionalExpression(ExpressionSyntax condition, SyntaxToken questionToken, ExpressionSyntax whenTrue, SyntaxToken colonToken, ExpressionSyntax whenFalse)
	{
		return new ConditionalExpressionSyntax(SyntaxKind.ConditionalExpression, condition, questionToken, whenTrue, colonToken, whenFalse, context);
	}

	public ThisExpressionSyntax ThisExpression(SyntaxToken token)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8746, token, context, out var hash);
		if (greenNode != null)
		{
			return (ThisExpressionSyntax)greenNode;
		}
		ThisExpressionSyntax thisExpressionSyntax = new ThisExpressionSyntax(SyntaxKind.ThisExpression, token, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(thisExpressionSyntax, hash);
		}
		return thisExpressionSyntax;
	}

	public BaseExpressionSyntax BaseExpression(SyntaxToken token)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8747, token, context, out var hash);
		if (greenNode != null)
		{
			return (BaseExpressionSyntax)greenNode;
		}
		BaseExpressionSyntax baseExpressionSyntax = new BaseExpressionSyntax(SyntaxKind.BaseExpression, token, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(baseExpressionSyntax, hash);
		}
		return baseExpressionSyntax;
	}

	public LiteralExpressionSyntax LiteralExpression(SyntaxKind kind, SyntaxToken token)
	{
		if (kind - 8748 > (SyntaxKind)7)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode((int)kind, token, context, out var hash);
		if (greenNode != null)
		{
			return (LiteralExpressionSyntax)greenNode;
		}
		LiteralExpressionSyntax literalExpressionSyntax = new LiteralExpressionSyntax(kind, token, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(literalExpressionSyntax, hash);
		}
		return literalExpressionSyntax;
	}

	public MakeRefExpressionSyntax MakeRefExpression(SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		return new MakeRefExpressionSyntax(SyntaxKind.MakeRefExpression, keyword, openParenToken, expression, closeParenToken, context);
	}

	public RefTypeExpressionSyntax RefTypeExpression(SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		return new RefTypeExpressionSyntax(SyntaxKind.RefTypeExpression, keyword, openParenToken, expression, closeParenToken, context);
	}

	public RefValueExpressionSyntax RefValueExpression(SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken comma, TypeSyntax type, SyntaxToken closeParenToken)
	{
		return new RefValueExpressionSyntax(SyntaxKind.RefValueExpression, keyword, openParenToken, expression, comma, type, closeParenToken, context);
	}

	public CheckedExpressionSyntax CheckedExpression(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		if (kind - 8762 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		return new CheckedExpressionSyntax(kind, keyword, openParenToken, expression, closeParenToken, context);
	}

	public DefaultExpressionSyntax DefaultExpression(SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken)
	{
		return new DefaultExpressionSyntax(SyntaxKind.DefaultExpression, keyword, openParenToken, type, closeParenToken, context);
	}

	public TypeOfExpressionSyntax TypeOfExpression(SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken)
	{
		return new TypeOfExpressionSyntax(SyntaxKind.TypeOfExpression, keyword, openParenToken, type, closeParenToken, context);
	}

	public SizeOfExpressionSyntax SizeOfExpression(SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken)
	{
		return new SizeOfExpressionSyntax(SyntaxKind.SizeOfExpression, keyword, openParenToken, type, closeParenToken, context);
	}

	public InvocationExpressionSyntax InvocationExpression(ExpressionSyntax expression, ArgumentListSyntax argumentList)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8634, expression, argumentList, context, out var hash);
		if (greenNode != null)
		{
			return (InvocationExpressionSyntax)greenNode;
		}
		InvocationExpressionSyntax invocationExpressionSyntax = new InvocationExpressionSyntax(SyntaxKind.InvocationExpression, expression, argumentList, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(invocationExpressionSyntax, hash);
		}
		return invocationExpressionSyntax;
	}

	public ElementAccessExpressionSyntax ElementAccessExpression(ExpressionSyntax expression, BracketedArgumentListSyntax argumentList)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8635, expression, argumentList, context, out var hash);
		if (greenNode != null)
		{
			return (ElementAccessExpressionSyntax)greenNode;
		}
		ElementAccessExpressionSyntax elementAccessExpressionSyntax = new ElementAccessExpressionSyntax(SyntaxKind.ElementAccessExpression, expression, argumentList, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(elementAccessExpressionSyntax, hash);
		}
		return elementAccessExpressionSyntax;
	}

	public ArgumentListSyntax ArgumentList(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> arguments, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8636, openParenToken, arguments.Node, closeParenToken, context, out var hash);
		if (greenNode != null)
		{
			return (ArgumentListSyntax)greenNode;
		}
		ArgumentListSyntax argumentListSyntax = new ArgumentListSyntax(SyntaxKind.ArgumentList, openParenToken, arguments.Node, closeParenToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(argumentListSyntax, hash);
		}
		return argumentListSyntax;
	}

	public BracketedArgumentListSyntax BracketedArgumentList(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> arguments, SyntaxToken closeBracketToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8637, openBracketToken, arguments.Node, closeBracketToken, context, out var hash);
		if (greenNode != null)
		{
			return (BracketedArgumentListSyntax)greenNode;
		}
		BracketedArgumentListSyntax bracketedArgumentListSyntax = new BracketedArgumentListSyntax(SyntaxKind.BracketedArgumentList, openBracketToken, arguments.Node, closeBracketToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(bracketedArgumentListSyntax, hash);
		}
		return bracketedArgumentListSyntax;
	}

	public ArgumentSyntax Argument(NameColonSyntax nameColon, SyntaxToken refKindKeyword, ExpressionSyntax expression)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8638, nameColon, refKindKeyword, expression, context, out var hash);
		if (greenNode != null)
		{
			return (ArgumentSyntax)greenNode;
		}
		ArgumentSyntax argumentSyntax = new ArgumentSyntax(SyntaxKind.Argument, nameColon, refKindKeyword, expression, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(argumentSyntax, hash);
		}
		return argumentSyntax;
	}

	public NameColonSyntax NameColon(IdentifierNameSyntax name, SyntaxToken colonToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8639, name, colonToken, context, out var hash);
		if (greenNode != null)
		{
			return (NameColonSyntax)greenNode;
		}
		NameColonSyntax nameColonSyntax = new NameColonSyntax(SyntaxKind.NameColon, name, colonToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(nameColonSyntax, hash);
		}
		return nameColonSyntax;
	}

	public DeclarationExpressionSyntax DeclarationExpression(TypeSyntax type, VariableDesignationSyntax designation)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(9040, type, designation, context, out var hash);
		if (greenNode != null)
		{
			return (DeclarationExpressionSyntax)greenNode;
		}
		DeclarationExpressionSyntax declarationExpressionSyntax = new DeclarationExpressionSyntax(SyntaxKind.DeclarationExpression, type, designation, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(declarationExpressionSyntax, hash);
		}
		return declarationExpressionSyntax;
	}

	public CastExpressionSyntax CastExpression(SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken, ExpressionSyntax expression)
	{
		return new CastExpressionSyntax(SyntaxKind.CastExpression, openParenToken, type, closeParenToken, expression, context);
	}

	public AnonymousMethodExpressionSyntax AnonymousMethodExpression(SyntaxToken asyncKeyword, SyntaxToken delegateKeyword, ParameterListSyntax parameterList, CSharpSyntaxNode body)
	{
		return new AnonymousMethodExpressionSyntax(SyntaxKind.AnonymousMethodExpression, asyncKeyword, delegateKeyword, parameterList, body, context);
	}

	public SimpleLambdaExpressionSyntax SimpleLambdaExpression(SyntaxToken asyncKeyword, ParameterSyntax parameter, SyntaxToken arrowToken, CSharpSyntaxNode body)
	{
		return new SimpleLambdaExpressionSyntax(SyntaxKind.SimpleLambdaExpression, asyncKeyword, parameter, arrowToken, body, context);
	}

	public RefExpressionSyntax RefExpression(SyntaxToken refKeyword, ExpressionSyntax expression)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(9050, refKeyword, expression, context, out var hash);
		if (greenNode != null)
		{
			return (RefExpressionSyntax)greenNode;
		}
		RefExpressionSyntax refExpressionSyntax = new RefExpressionSyntax(SyntaxKind.RefExpression, refKeyword, expression, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(refExpressionSyntax, hash);
		}
		return refExpressionSyntax;
	}

	public ParenthesizedLambdaExpressionSyntax ParenthesizedLambdaExpression(SyntaxToken asyncKeyword, ParameterListSyntax parameterList, SyntaxToken arrowToken, CSharpSyntaxNode body)
	{
		return new ParenthesizedLambdaExpressionSyntax(SyntaxKind.ParenthesizedLambdaExpression, asyncKeyword, parameterList, arrowToken, body, context);
	}

	public InitializerExpressionSyntax InitializerExpression(SyntaxKind kind, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> expressions, SyntaxToken closeBraceToken)
	{
		if (kind - 8644 > (SyntaxKind)2 && kind != SyntaxKind.ComplexElementInitializerExpression)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode((int)kind, openBraceToken, expressions.Node, closeBraceToken, context, out var hash);
		if (greenNode != null)
		{
			return (InitializerExpressionSyntax)greenNode;
		}
		InitializerExpressionSyntax initializerExpressionSyntax = new InitializerExpressionSyntax(kind, openBraceToken, expressions.Node, closeBraceToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(initializerExpressionSyntax, hash);
		}
		return initializerExpressionSyntax;
	}

	public ObjectCreationExpressionSyntax ObjectCreationExpression(SyntaxToken newKeyword, TypeSyntax type, ArgumentListSyntax argumentList, InitializerExpressionSyntax initializer)
	{
		return new ObjectCreationExpressionSyntax(SyntaxKind.ObjectCreationExpression, newKeyword, type, argumentList, initializer, context);
	}

	public AnonymousObjectMemberDeclaratorSyntax AnonymousObjectMemberDeclarator(NameEqualsSyntax nameEquals, ExpressionSyntax expression)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8647, nameEquals, expression, context, out var hash);
		if (greenNode != null)
		{
			return (AnonymousObjectMemberDeclaratorSyntax)greenNode;
		}
		AnonymousObjectMemberDeclaratorSyntax anonymousObjectMemberDeclaratorSyntax = new AnonymousObjectMemberDeclaratorSyntax(SyntaxKind.AnonymousObjectMemberDeclarator, nameEquals, expression, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(anonymousObjectMemberDeclaratorSyntax, hash);
		}
		return anonymousObjectMemberDeclaratorSyntax;
	}

	public AnonymousObjectCreationExpressionSyntax AnonymousObjectCreationExpression(SyntaxToken newKeyword, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax> initializers, SyntaxToken closeBraceToken)
	{
		return new AnonymousObjectCreationExpressionSyntax(SyntaxKind.AnonymousObjectCreationExpression, newKeyword, openBraceToken, initializers.Node, closeBraceToken, context);
	}

	public ArrayCreationExpressionSyntax ArrayCreationExpression(SyntaxToken newKeyword, ArrayTypeSyntax type, InitializerExpressionSyntax initializer)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8651, newKeyword, type, initializer, context, out var hash);
		if (greenNode != null)
		{
			return (ArrayCreationExpressionSyntax)greenNode;
		}
		ArrayCreationExpressionSyntax arrayCreationExpressionSyntax = new ArrayCreationExpressionSyntax(SyntaxKind.ArrayCreationExpression, newKeyword, type, initializer, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(arrayCreationExpressionSyntax, hash);
		}
		return arrayCreationExpressionSyntax;
	}

	public ImplicitArrayCreationExpressionSyntax ImplicitArrayCreationExpression(SyntaxToken newKeyword, SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> commas, SyntaxToken closeBracketToken, InitializerExpressionSyntax initializer)
	{
		return new ImplicitArrayCreationExpressionSyntax(SyntaxKind.ImplicitArrayCreationExpression, newKeyword, openBracketToken, commas.Node, closeBracketToken, initializer, context);
	}

	public StackAllocArrayCreationExpressionSyntax StackAllocArrayCreationExpression(SyntaxToken stackAllocKeyword, TypeSyntax type, InitializerExpressionSyntax initializer)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8653, stackAllocKeyword, type, initializer, context, out var hash);
		if (greenNode != null)
		{
			return (StackAllocArrayCreationExpressionSyntax)greenNode;
		}
		StackAllocArrayCreationExpressionSyntax stackAllocArrayCreationExpressionSyntax = new StackAllocArrayCreationExpressionSyntax(SyntaxKind.StackAllocArrayCreationExpression, stackAllocKeyword, type, initializer, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(stackAllocArrayCreationExpressionSyntax, hash);
		}
		return stackAllocArrayCreationExpressionSyntax;
	}

	public ImplicitStackAllocArrayCreationExpressionSyntax ImplicitStackAllocArrayCreationExpression(SyntaxToken stackAllocKeyword, SyntaxToken openBracketToken, SyntaxToken closeBracketToken, InitializerExpressionSyntax initializer)
	{
		return new ImplicitStackAllocArrayCreationExpressionSyntax(SyntaxKind.ImplicitStackAllocArrayCreationExpression, stackAllocKeyword, openBracketToken, closeBracketToken, initializer, context);
	}

	public QueryExpressionSyntax QueryExpression(FromClauseSyntax fromClause, QueryBodySyntax body)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8774, fromClause, body, context, out var hash);
		if (greenNode != null)
		{
			return (QueryExpressionSyntax)greenNode;
		}
		QueryExpressionSyntax queryExpressionSyntax = new QueryExpressionSyntax(SyntaxKind.QueryExpression, fromClause, body, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(queryExpressionSyntax, hash);
		}
		return queryExpressionSyntax;
	}

	public QueryBodySyntax QueryBody(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<QueryClauseSyntax> clauses, SelectOrGroupClauseSyntax selectOrGroup, QueryContinuationSyntax continuation)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8775, clauses.Node, selectOrGroup, continuation, context, out var hash);
		if (greenNode != null)
		{
			return (QueryBodySyntax)greenNode;
		}
		QueryBodySyntax queryBodySyntax = new QueryBodySyntax(SyntaxKind.QueryBody, clauses.Node, selectOrGroup, continuation, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(queryBodySyntax, hash);
		}
		return queryBodySyntax;
	}

	public FromClauseSyntax FromClause(SyntaxToken fromKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression)
	{
		return new FromClauseSyntax(SyntaxKind.FromClause, fromKeyword, type, identifier, inKeyword, expression, context);
	}

	public LetClauseSyntax LetClause(SyntaxToken letKeyword, SyntaxToken identifier, SyntaxToken equalsToken, ExpressionSyntax expression)
	{
		return new LetClauseSyntax(SyntaxKind.LetClause, letKeyword, identifier, equalsToken, expression, context);
	}

	public JoinClauseSyntax JoinClause(SyntaxToken joinKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax inExpression, SyntaxToken onKeyword, ExpressionSyntax leftExpression, SyntaxToken equalsKeyword, ExpressionSyntax rightExpression, JoinIntoClauseSyntax into)
	{
		return new JoinClauseSyntax(SyntaxKind.JoinClause, joinKeyword, type, identifier, inKeyword, inExpression, onKeyword, leftExpression, equalsKeyword, rightExpression, into, context);
	}

	public JoinIntoClauseSyntax JoinIntoClause(SyntaxToken intoKeyword, SyntaxToken identifier)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8779, intoKeyword, identifier, context, out var hash);
		if (greenNode != null)
		{
			return (JoinIntoClauseSyntax)greenNode;
		}
		JoinIntoClauseSyntax joinIntoClauseSyntax = new JoinIntoClauseSyntax(SyntaxKind.JoinIntoClause, intoKeyword, identifier, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(joinIntoClauseSyntax, hash);
		}
		return joinIntoClauseSyntax;
	}

	public WhereClauseSyntax WhereClause(SyntaxToken whereKeyword, ExpressionSyntax condition)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8780, whereKeyword, condition, context, out var hash);
		if (greenNode != null)
		{
			return (WhereClauseSyntax)greenNode;
		}
		WhereClauseSyntax whereClauseSyntax = new WhereClauseSyntax(SyntaxKind.WhereClause, whereKeyword, condition, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(whereClauseSyntax, hash);
		}
		return whereClauseSyntax;
	}

	public OrderByClauseSyntax OrderByClause(SyntaxToken orderByKeyword, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<OrderingSyntax> orderings)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8781, orderByKeyword, orderings.Node, context, out var hash);
		if (greenNode != null)
		{
			return (OrderByClauseSyntax)greenNode;
		}
		OrderByClauseSyntax orderByClauseSyntax = new OrderByClauseSyntax(SyntaxKind.OrderByClause, orderByKeyword, orderings.Node, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(orderByClauseSyntax, hash);
		}
		return orderByClauseSyntax;
	}

	public OrderingSyntax Ordering(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken ascendingOrDescendingKeyword)
	{
		if (kind - 8782 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode((int)kind, expression, ascendingOrDescendingKeyword, context, out var hash);
		if (greenNode != null)
		{
			return (OrderingSyntax)greenNode;
		}
		OrderingSyntax orderingSyntax = new OrderingSyntax(kind, expression, ascendingOrDescendingKeyword, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(orderingSyntax, hash);
		}
		return orderingSyntax;
	}

	public SelectClauseSyntax SelectClause(SyntaxToken selectKeyword, ExpressionSyntax expression)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8784, selectKeyword, expression, context, out var hash);
		if (greenNode != null)
		{
			return (SelectClauseSyntax)greenNode;
		}
		SelectClauseSyntax selectClauseSyntax = new SelectClauseSyntax(SyntaxKind.SelectClause, selectKeyword, expression, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(selectClauseSyntax, hash);
		}
		return selectClauseSyntax;
	}

	public GroupClauseSyntax GroupClause(SyntaxToken groupKeyword, ExpressionSyntax groupExpression, SyntaxToken byKeyword, ExpressionSyntax byExpression)
	{
		return new GroupClauseSyntax(SyntaxKind.GroupClause, groupKeyword, groupExpression, byKeyword, byExpression, context);
	}

	public QueryContinuationSyntax QueryContinuation(SyntaxToken intoKeyword, SyntaxToken identifier, QueryBodySyntax body)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8786, intoKeyword, identifier, body, context, out var hash);
		if (greenNode != null)
		{
			return (QueryContinuationSyntax)greenNode;
		}
		QueryContinuationSyntax queryContinuationSyntax = new QueryContinuationSyntax(SyntaxKind.QueryContinuation, intoKeyword, identifier, body, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(queryContinuationSyntax, hash);
		}
		return queryContinuationSyntax;
	}

	public OmittedArraySizeExpressionSyntax OmittedArraySizeExpression(SyntaxToken omittedArraySizeExpressionToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8654, omittedArraySizeExpressionToken, context, out var hash);
		if (greenNode != null)
		{
			return (OmittedArraySizeExpressionSyntax)greenNode;
		}
		OmittedArraySizeExpressionSyntax omittedArraySizeExpressionSyntax = new OmittedArraySizeExpressionSyntax(SyntaxKind.OmittedArraySizeExpression, omittedArraySizeExpressionToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(omittedArraySizeExpressionSyntax, hash);
		}
		return omittedArraySizeExpressionSyntax;
	}

	public InterpolatedStringExpressionSyntax InterpolatedStringExpression(SyntaxToken stringStartToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<InterpolatedStringContentSyntax> contents, SyntaxToken stringEndToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8655, stringStartToken, contents.Node, stringEndToken, context, out var hash);
		if (greenNode != null)
		{
			return (InterpolatedStringExpressionSyntax)greenNode;
		}
		InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax = new InterpolatedStringExpressionSyntax(SyntaxKind.InterpolatedStringExpression, stringStartToken, contents.Node, stringEndToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(interpolatedStringExpressionSyntax, hash);
		}
		return interpolatedStringExpressionSyntax;
	}

	public IsPatternExpressionSyntax IsPatternExpression(ExpressionSyntax expression, SyntaxToken isKeyword, PatternSyntax pattern)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8657, expression, isKeyword, pattern, context, out var hash);
		if (greenNode != null)
		{
			return (IsPatternExpressionSyntax)greenNode;
		}
		IsPatternExpressionSyntax isPatternExpressionSyntax = new IsPatternExpressionSyntax(SyntaxKind.IsPatternExpression, expression, isKeyword, pattern, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(isPatternExpressionSyntax, hash);
		}
		return isPatternExpressionSyntax;
	}

	public ThrowExpressionSyntax ThrowExpression(SyntaxToken throwKeyword, ExpressionSyntax expression)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(9052, throwKeyword, expression, context, out var hash);
		if (greenNode != null)
		{
			return (ThrowExpressionSyntax)greenNode;
		}
		ThrowExpressionSyntax throwExpressionSyntax = new ThrowExpressionSyntax(SyntaxKind.ThrowExpression, throwKeyword, expression, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(throwExpressionSyntax, hash);
		}
		return throwExpressionSyntax;
	}

	public WhenClauseSyntax WhenClause(SyntaxToken whenKeyword, ExpressionSyntax condition)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(9013, whenKeyword, condition, context, out var hash);
		if (greenNode != null)
		{
			return (WhenClauseSyntax)greenNode;
		}
		WhenClauseSyntax whenClauseSyntax = new WhenClauseSyntax(SyntaxKind.WhenClause, whenKeyword, condition, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(whenClauseSyntax, hash);
		}
		return whenClauseSyntax;
	}

	public DeclarationPatternSyntax DeclarationPattern(TypeSyntax type, VariableDesignationSyntax designation)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(9000, type, designation, context, out var hash);
		if (greenNode != null)
		{
			return (DeclarationPatternSyntax)greenNode;
		}
		DeclarationPatternSyntax declarationPatternSyntax = new DeclarationPatternSyntax(SyntaxKind.DeclarationPattern, type, designation, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(declarationPatternSyntax, hash);
		}
		return declarationPatternSyntax;
	}

	public ConstantPatternSyntax ConstantPattern(ExpressionSyntax expression)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(9002, expression, context, out var hash);
		if (greenNode != null)
		{
			return (ConstantPatternSyntax)greenNode;
		}
		ConstantPatternSyntax constantPatternSyntax = new ConstantPatternSyntax(SyntaxKind.ConstantPattern, expression, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(constantPatternSyntax, hash);
		}
		return constantPatternSyntax;
	}

	public InterpolatedStringTextSyntax InterpolatedStringText(SyntaxToken textToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8919, textToken, context, out var hash);
		if (greenNode != null)
		{
			return (InterpolatedStringTextSyntax)greenNode;
		}
		InterpolatedStringTextSyntax interpolatedStringTextSyntax = new InterpolatedStringTextSyntax(SyntaxKind.InterpolatedStringText, textToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(interpolatedStringTextSyntax, hash);
		}
		return interpolatedStringTextSyntax;
	}

	public InterpolationSyntax Interpolation(SyntaxToken openBraceToken, ExpressionSyntax expression, InterpolationAlignmentClauseSyntax alignmentClause, InterpolationFormatClauseSyntax formatClause, SyntaxToken closeBraceToken)
	{
		return new InterpolationSyntax(SyntaxKind.Interpolation, openBraceToken, expression, alignmentClause, formatClause, closeBraceToken, context);
	}

	public InterpolationAlignmentClauseSyntax InterpolationAlignmentClause(SyntaxToken commaToken, ExpressionSyntax value)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8920, commaToken, value, context, out var hash);
		if (greenNode != null)
		{
			return (InterpolationAlignmentClauseSyntax)greenNode;
		}
		InterpolationAlignmentClauseSyntax interpolationAlignmentClauseSyntax = new InterpolationAlignmentClauseSyntax(SyntaxKind.InterpolationAlignmentClause, commaToken, value, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(interpolationAlignmentClauseSyntax, hash);
		}
		return interpolationAlignmentClauseSyntax;
	}

	public InterpolationFormatClauseSyntax InterpolationFormatClause(SyntaxToken colonToken, SyntaxToken formatStringToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8921, colonToken, formatStringToken, context, out var hash);
		if (greenNode != null)
		{
			return (InterpolationFormatClauseSyntax)greenNode;
		}
		InterpolationFormatClauseSyntax interpolationFormatClauseSyntax = new InterpolationFormatClauseSyntax(SyntaxKind.InterpolationFormatClause, colonToken, formatStringToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(interpolationFormatClauseSyntax, hash);
		}
		return interpolationFormatClauseSyntax;
	}

	public GlobalStatementSyntax GlobalStatement(StatementSyntax statement)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8841, statement, context, out var hash);
		if (greenNode != null)
		{
			return (GlobalStatementSyntax)greenNode;
		}
		GlobalStatementSyntax globalStatementSyntax = new GlobalStatementSyntax(SyntaxKind.GlobalStatement, statement, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(globalStatementSyntax, hash);
		}
		return globalStatementSyntax;
	}

	public BlockSyntax Block(SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax> statements, SyntaxToken closeBraceToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8792, openBraceToken, statements.Node, closeBraceToken, context, out var hash);
		if (greenNode != null)
		{
			return (BlockSyntax)greenNode;
		}
		BlockSyntax blockSyntax = new BlockSyntax(SyntaxKind.Block, openBraceToken, statements.Node, closeBraceToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(blockSyntax, hash);
		}
		return blockSyntax;
	}

	public LocalFunctionStatementSyntax LocalFunctionStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax returnType, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new LocalFunctionStatementSyntax(SyntaxKind.LocalFunctionStatement, modifiers.Node, returnType, identifier, typeParameterList, parameterList, constraintClauses.Node, body, expressionBody, semicolonToken, context);
	}

	public LocalDeclarationStatementSyntax LocalDeclarationStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8793, modifiers.Node, declaration, semicolonToken, context, out var hash);
		if (greenNode != null)
		{
			return (LocalDeclarationStatementSyntax)greenNode;
		}
		LocalDeclarationStatementSyntax localDeclarationStatementSyntax = new LocalDeclarationStatementSyntax(SyntaxKind.LocalDeclarationStatement, modifiers.Node, declaration, semicolonToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(localDeclarationStatementSyntax, hash);
		}
		return localDeclarationStatementSyntax;
	}

	public VariableDeclarationSyntax VariableDeclaration(TypeSyntax type, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDeclaratorSyntax> variables)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8794, type, variables.Node, context, out var hash);
		if (greenNode != null)
		{
			return (VariableDeclarationSyntax)greenNode;
		}
		VariableDeclarationSyntax variableDeclarationSyntax = new VariableDeclarationSyntax(SyntaxKind.VariableDeclaration, type, variables.Node, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(variableDeclarationSyntax, hash);
		}
		return variableDeclarationSyntax;
	}

	public VariableDeclaratorSyntax VariableDeclarator(SyntaxToken identifier, BracketedArgumentListSyntax argumentList, EqualsValueClauseSyntax initializer)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8795, identifier, argumentList, initializer, context, out var hash);
		if (greenNode != null)
		{
			return (VariableDeclaratorSyntax)greenNode;
		}
		VariableDeclaratorSyntax variableDeclaratorSyntax = new VariableDeclaratorSyntax(SyntaxKind.VariableDeclarator, identifier, argumentList, initializer, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(variableDeclaratorSyntax, hash);
		}
		return variableDeclaratorSyntax;
	}

	public EqualsValueClauseSyntax EqualsValueClause(SyntaxToken equalsToken, ExpressionSyntax value)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8796, equalsToken, value, context, out var hash);
		if (greenNode != null)
		{
			return (EqualsValueClauseSyntax)greenNode;
		}
		EqualsValueClauseSyntax equalsValueClauseSyntax = new EqualsValueClauseSyntax(SyntaxKind.EqualsValueClause, equalsToken, value, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(equalsValueClauseSyntax, hash);
		}
		return equalsValueClauseSyntax;
	}

	public SingleVariableDesignationSyntax SingleVariableDesignation(SyntaxToken identifier)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8927, identifier, context, out var hash);
		if (greenNode != null)
		{
			return (SingleVariableDesignationSyntax)greenNode;
		}
		SingleVariableDesignationSyntax singleVariableDesignationSyntax = new SingleVariableDesignationSyntax(SyntaxKind.SingleVariableDesignation, identifier, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(singleVariableDesignationSyntax, hash);
		}
		return singleVariableDesignationSyntax;
	}

	public DiscardDesignationSyntax DiscardDesignation(SyntaxToken underscoreToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(9014, underscoreToken, context, out var hash);
		if (greenNode != null)
		{
			return (DiscardDesignationSyntax)greenNode;
		}
		DiscardDesignationSyntax discardDesignationSyntax = new DiscardDesignationSyntax(SyntaxKind.DiscardDesignation, underscoreToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(discardDesignationSyntax, hash);
		}
		return discardDesignationSyntax;
	}

	public ParenthesizedVariableDesignationSyntax ParenthesizedVariableDesignation(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDesignationSyntax> variables, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8928, openParenToken, variables.Node, closeParenToken, context, out var hash);
		if (greenNode != null)
		{
			return (ParenthesizedVariableDesignationSyntax)greenNode;
		}
		ParenthesizedVariableDesignationSyntax parenthesizedVariableDesignationSyntax = new ParenthesizedVariableDesignationSyntax(SyntaxKind.ParenthesizedVariableDesignation, openParenToken, variables.Node, closeParenToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(parenthesizedVariableDesignationSyntax, hash);
		}
		return parenthesizedVariableDesignationSyntax;
	}

	public ExpressionStatementSyntax ExpressionStatement(ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8797, expression, semicolonToken, context, out var hash);
		if (greenNode != null)
		{
			return (ExpressionStatementSyntax)greenNode;
		}
		ExpressionStatementSyntax expressionStatementSyntax = new ExpressionStatementSyntax(SyntaxKind.ExpressionStatement, expression, semicolonToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(expressionStatementSyntax, hash);
		}
		return expressionStatementSyntax;
	}

	public EmptyStatementSyntax EmptyStatement(SyntaxToken semicolonToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8798, semicolonToken, context, out var hash);
		if (greenNode != null)
		{
			return (EmptyStatementSyntax)greenNode;
		}
		EmptyStatementSyntax emptyStatementSyntax = new EmptyStatementSyntax(SyntaxKind.EmptyStatement, semicolonToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(emptyStatementSyntax, hash);
		}
		return emptyStatementSyntax;
	}

	public LabeledStatementSyntax LabeledStatement(SyntaxToken identifier, SyntaxToken colonToken, StatementSyntax statement)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8799, identifier, colonToken, statement, context, out var hash);
		if (greenNode != null)
		{
			return (LabeledStatementSyntax)greenNode;
		}
		LabeledStatementSyntax labeledStatementSyntax = new LabeledStatementSyntax(SyntaxKind.LabeledStatement, identifier, colonToken, statement, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(labeledStatementSyntax, hash);
		}
		return labeledStatementSyntax;
	}

	public GotoStatementSyntax GotoStatement(SyntaxKind kind, SyntaxToken gotoKeyword, SyntaxToken caseOrDefaultKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (kind - 8800 > (SyntaxKind)2)
		{
			throw new ArgumentException("kind");
		}
		return new GotoStatementSyntax(kind, gotoKeyword, caseOrDefaultKeyword, expression, semicolonToken, context);
	}

	public BreakStatementSyntax BreakStatement(SyntaxToken breakKeyword, SyntaxToken semicolonToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8803, breakKeyword, semicolonToken, context, out var hash);
		if (greenNode != null)
		{
			return (BreakStatementSyntax)greenNode;
		}
		BreakStatementSyntax breakStatementSyntax = new BreakStatementSyntax(SyntaxKind.BreakStatement, breakKeyword, semicolonToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(breakStatementSyntax, hash);
		}
		return breakStatementSyntax;
	}

	public ContinueStatementSyntax ContinueStatement(SyntaxToken continueKeyword, SyntaxToken semicolonToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8804, continueKeyword, semicolonToken, context, out var hash);
		if (greenNode != null)
		{
			return (ContinueStatementSyntax)greenNode;
		}
		ContinueStatementSyntax continueStatementSyntax = new ContinueStatementSyntax(SyntaxKind.ContinueStatement, continueKeyword, semicolonToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(continueStatementSyntax, hash);
		}
		return continueStatementSyntax;
	}

	public ReturnStatementSyntax ReturnStatement(SyntaxToken returnKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8805, returnKeyword, expression, semicolonToken, context, out var hash);
		if (greenNode != null)
		{
			return (ReturnStatementSyntax)greenNode;
		}
		ReturnStatementSyntax returnStatementSyntax = new ReturnStatementSyntax(SyntaxKind.ReturnStatement, returnKeyword, expression, semicolonToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(returnStatementSyntax, hash);
		}
		return returnStatementSyntax;
	}

	public ThrowStatementSyntax ThrowStatement(SyntaxToken throwKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8808, throwKeyword, expression, semicolonToken, context, out var hash);
		if (greenNode != null)
		{
			return (ThrowStatementSyntax)greenNode;
		}
		ThrowStatementSyntax throwStatementSyntax = new ThrowStatementSyntax(SyntaxKind.ThrowStatement, throwKeyword, expression, semicolonToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(throwStatementSyntax, hash);
		}
		return throwStatementSyntax;
	}

	public YieldStatementSyntax YieldStatement(SyntaxKind kind, SyntaxToken yieldKeyword, SyntaxToken returnOrBreakKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (kind - 8806 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		return new YieldStatementSyntax(kind, yieldKeyword, returnOrBreakKeyword, expression, semicolonToken, context);
	}

	public WhileStatementSyntax WhileStatement(SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new WhileStatementSyntax(SyntaxKind.WhileStatement, whileKeyword, openParenToken, condition, closeParenToken, statement, context);
	}

	public DoStatementSyntax DoStatement(SyntaxToken doKeyword, StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken)
	{
		return new DoStatementSyntax(SyntaxKind.DoStatement, doKeyword, statement, whileKeyword, openParenToken, condition, closeParenToken, semicolonToken, context);
	}

	public ForStatementSyntax ForStatement(SyntaxToken forKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> initializers, SyntaxToken firstSemicolonToken, ExpressionSyntax condition, SyntaxToken secondSemicolonToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> incrementors, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new ForStatementSyntax(SyntaxKind.ForStatement, forKeyword, openParenToken, declaration, initializers.Node, firstSemicolonToken, condition, secondSemicolonToken, incrementors.Node, closeParenToken, statement, context);
	}

	public ForEachStatementSyntax ForEachStatement(SyntaxToken forEachKeyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new ForEachStatementSyntax(SyntaxKind.ForEachStatement, forEachKeyword, openParenToken, type, identifier, inKeyword, expression, closeParenToken, statement, context);
	}

	public ForEachVariableStatementSyntax ForEachVariableStatement(SyntaxToken forEachKeyword, SyntaxToken openParenToken, ExpressionSyntax variable, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new ForEachVariableStatementSyntax(SyntaxKind.ForEachVariableStatement, forEachKeyword, openParenToken, variable, inKeyword, expression, closeParenToken, statement, context);
	}

	public UsingStatementSyntax UsingStatement(SyntaxToken usingKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new UsingStatementSyntax(SyntaxKind.UsingStatement, usingKeyword, openParenToken, declaration, expression, closeParenToken, statement, context);
	}

	public FixedStatementSyntax FixedStatement(SyntaxToken fixedKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new FixedStatementSyntax(SyntaxKind.FixedStatement, fixedKeyword, openParenToken, declaration, closeParenToken, statement, context);
	}

	public CheckedStatementSyntax CheckedStatement(SyntaxKind kind, SyntaxToken keyword, BlockSyntax block)
	{
		if (kind - 8815 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode((int)kind, keyword, block, context, out var hash);
		if (greenNode != null)
		{
			return (CheckedStatementSyntax)greenNode;
		}
		CheckedStatementSyntax checkedStatementSyntax = new CheckedStatementSyntax(kind, keyword, block, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(checkedStatementSyntax, hash);
		}
		return checkedStatementSyntax;
	}

	public UnsafeStatementSyntax UnsafeStatement(SyntaxToken unsafeKeyword, BlockSyntax block)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8817, unsafeKeyword, block, context, out var hash);
		if (greenNode != null)
		{
			return (UnsafeStatementSyntax)greenNode;
		}
		UnsafeStatementSyntax unsafeStatementSyntax = new UnsafeStatementSyntax(SyntaxKind.UnsafeStatement, unsafeKeyword, block, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(unsafeStatementSyntax, hash);
		}
		return unsafeStatementSyntax;
	}

	public LockStatementSyntax LockStatement(SyntaxToken lockKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new LockStatementSyntax(SyntaxKind.LockStatement, lockKeyword, openParenToken, expression, closeParenToken, statement, context);
	}

	public IfStatementSyntax IfStatement(SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax @else)
	{
		return new IfStatementSyntax(SyntaxKind.IfStatement, ifKeyword, openParenToken, condition, closeParenToken, statement, @else, context);
	}

	public ElseClauseSyntax ElseClause(SyntaxToken elseKeyword, StatementSyntax statement)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8820, elseKeyword, statement, context, out var hash);
		if (greenNode != null)
		{
			return (ElseClauseSyntax)greenNode;
		}
		ElseClauseSyntax elseClauseSyntax = new ElseClauseSyntax(SyntaxKind.ElseClause, elseKeyword, statement, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(elseClauseSyntax, hash);
		}
		return elseClauseSyntax;
	}

	public SwitchStatementSyntax SwitchStatement(SyntaxToken switchKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SwitchSectionSyntax> sections, SyntaxToken closeBraceToken)
	{
		return new SwitchStatementSyntax(SyntaxKind.SwitchStatement, switchKeyword, openParenToken, expression, closeParenToken, openBraceToken, sections.Node, closeBraceToken, context);
	}

	public SwitchSectionSyntax SwitchSection(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SwitchLabelSyntax> labels, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax> statements)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8822, labels.Node, statements.Node, context, out var hash);
		if (greenNode != null)
		{
			return (SwitchSectionSyntax)greenNode;
		}
		SwitchSectionSyntax switchSectionSyntax = new SwitchSectionSyntax(SyntaxKind.SwitchSection, labels.Node, statements.Node, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(switchSectionSyntax, hash);
		}
		return switchSectionSyntax;
	}

	public CasePatternSwitchLabelSyntax CasePatternSwitchLabel(SyntaxToken keyword, PatternSyntax pattern, WhenClauseSyntax whenClause, SyntaxToken colonToken)
	{
		return new CasePatternSwitchLabelSyntax(SyntaxKind.CasePatternSwitchLabel, keyword, pattern, whenClause, colonToken, context);
	}

	public CaseSwitchLabelSyntax CaseSwitchLabel(SyntaxToken keyword, ExpressionSyntax value, SyntaxToken colonToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8823, keyword, value, colonToken, context, out var hash);
		if (greenNode != null)
		{
			return (CaseSwitchLabelSyntax)greenNode;
		}
		CaseSwitchLabelSyntax caseSwitchLabelSyntax = new CaseSwitchLabelSyntax(SyntaxKind.CaseSwitchLabel, keyword, value, colonToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(caseSwitchLabelSyntax, hash);
		}
		return caseSwitchLabelSyntax;
	}

	public DefaultSwitchLabelSyntax DefaultSwitchLabel(SyntaxToken keyword, SyntaxToken colonToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8824, keyword, colonToken, context, out var hash);
		if (greenNode != null)
		{
			return (DefaultSwitchLabelSyntax)greenNode;
		}
		DefaultSwitchLabelSyntax defaultSwitchLabelSyntax = new DefaultSwitchLabelSyntax(SyntaxKind.DefaultSwitchLabel, keyword, colonToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(defaultSwitchLabelSyntax, hash);
		}
		return defaultSwitchLabelSyntax;
	}

	public TryStatementSyntax TryStatement(SyntaxToken tryKeyword, BlockSyntax block, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CatchClauseSyntax> catches, FinallyClauseSyntax @finally)
	{
		return new TryStatementSyntax(SyntaxKind.TryStatement, tryKeyword, block, catches.Node, @finally, context);
	}

	public CatchClauseSyntax CatchClause(SyntaxToken catchKeyword, CatchDeclarationSyntax declaration, CatchFilterClauseSyntax filter, BlockSyntax block)
	{
		return new CatchClauseSyntax(SyntaxKind.CatchClause, catchKeyword, declaration, filter, block, context);
	}

	public CatchDeclarationSyntax CatchDeclaration(SyntaxToken openParenToken, TypeSyntax type, SyntaxToken identifier, SyntaxToken closeParenToken)
	{
		return new CatchDeclarationSyntax(SyntaxKind.CatchDeclaration, openParenToken, type, identifier, closeParenToken, context);
	}

	public CatchFilterClauseSyntax CatchFilterClause(SyntaxToken whenKeyword, SyntaxToken openParenToken, ExpressionSyntax filterExpression, SyntaxToken closeParenToken)
	{
		return new CatchFilterClauseSyntax(SyntaxKind.CatchFilterClause, whenKeyword, openParenToken, filterExpression, closeParenToken, context);
	}

	public FinallyClauseSyntax FinallyClause(SyntaxToken finallyKeyword, BlockSyntax block)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8829, finallyKeyword, block, context, out var hash);
		if (greenNode != null)
		{
			return (FinallyClauseSyntax)greenNode;
		}
		FinallyClauseSyntax finallyClauseSyntax = new FinallyClauseSyntax(SyntaxKind.FinallyClause, finallyKeyword, block, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(finallyClauseSyntax, hash);
		}
		return finallyClauseSyntax;
	}

	public CompilationUnitSyntax CompilationUnit(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax> externs, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax> usings, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken endOfFileToken)
	{
		return new CompilationUnitSyntax(SyntaxKind.CompilationUnit, externs.Node, usings.Node, attributeLists.Node, members.Node, endOfFileToken, context);
	}

	public ExternAliasDirectiveSyntax ExternAliasDirective(SyntaxToken externKeyword, SyntaxToken aliasKeyword, SyntaxToken identifier, SyntaxToken semicolonToken)
	{
		return new ExternAliasDirectiveSyntax(SyntaxKind.ExternAliasDirective, externKeyword, aliasKeyword, identifier, semicolonToken, context);
	}

	public UsingDirectiveSyntax UsingDirective(SyntaxToken usingKeyword, SyntaxToken staticKeyword, NameEqualsSyntax alias, NameSyntax name, SyntaxToken semicolonToken)
	{
		return new UsingDirectiveSyntax(SyntaxKind.UsingDirective, usingKeyword, staticKeyword, alias, name, semicolonToken, context);
	}

	public NamespaceDeclarationSyntax NamespaceDeclaration(SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax> externs, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax> usings, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		return new NamespaceDeclarationSyntax(SyntaxKind.NamespaceDeclaration, namespaceKeyword, name, openBraceToken, externs.Node, usings.Node, members.Node, closeBraceToken, semicolonToken, context);
	}

	public AttributeListSyntax AttributeList(SyntaxToken openBracketToken, AttributeTargetSpecifierSyntax target, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AttributeSyntax> attributes, SyntaxToken closeBracketToken)
	{
		return new AttributeListSyntax(SyntaxKind.AttributeList, openBracketToken, target, attributes.Node, closeBracketToken, context);
	}

	public AttributeTargetSpecifierSyntax AttributeTargetSpecifier(SyntaxToken identifier, SyntaxToken colonToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8848, identifier, colonToken, context, out var hash);
		if (greenNode != null)
		{
			return (AttributeTargetSpecifierSyntax)greenNode;
		}
		AttributeTargetSpecifierSyntax attributeTargetSpecifierSyntax = new AttributeTargetSpecifierSyntax(SyntaxKind.AttributeTargetSpecifier, identifier, colonToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(attributeTargetSpecifierSyntax, hash);
		}
		return attributeTargetSpecifierSyntax;
	}

	public AttributeSyntax Attribute(NameSyntax name, AttributeArgumentListSyntax argumentList)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8849, name, argumentList, context, out var hash);
		if (greenNode != null)
		{
			return (AttributeSyntax)greenNode;
		}
		AttributeSyntax attributeSyntax = new AttributeSyntax(SyntaxKind.Attribute, name, argumentList, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(attributeSyntax, hash);
		}
		return attributeSyntax;
	}

	public AttributeArgumentListSyntax AttributeArgumentList(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AttributeArgumentSyntax> arguments, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8850, openParenToken, arguments.Node, closeParenToken, context, out var hash);
		if (greenNode != null)
		{
			return (AttributeArgumentListSyntax)greenNode;
		}
		AttributeArgumentListSyntax attributeArgumentListSyntax = new AttributeArgumentListSyntax(SyntaxKind.AttributeArgumentList, openParenToken, arguments.Node, closeParenToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(attributeArgumentListSyntax, hash);
		}
		return attributeArgumentListSyntax;
	}

	public AttributeArgumentSyntax AttributeArgument(NameEqualsSyntax nameEquals, NameColonSyntax nameColon, ExpressionSyntax expression)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8851, nameEquals, nameColon, expression, context, out var hash);
		if (greenNode != null)
		{
			return (AttributeArgumentSyntax)greenNode;
		}
		AttributeArgumentSyntax attributeArgumentSyntax = new AttributeArgumentSyntax(SyntaxKind.AttributeArgument, nameEquals, nameColon, expression, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(attributeArgumentSyntax, hash);
		}
		return attributeArgumentSyntax;
	}

	public NameEqualsSyntax NameEquals(IdentifierNameSyntax name, SyntaxToken equalsToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8852, name, equalsToken, context, out var hash);
		if (greenNode != null)
		{
			return (NameEqualsSyntax)greenNode;
		}
		NameEqualsSyntax nameEqualsSyntax = new NameEqualsSyntax(SyntaxKind.NameEquals, name, equalsToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(nameEqualsSyntax, hash);
		}
		return nameEqualsSyntax;
	}

	public TypeParameterListSyntax TypeParameterList(SyntaxToken lessThanToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeParameterSyntax> parameters, SyntaxToken greaterThanToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8909, lessThanToken, parameters.Node, greaterThanToken, context, out var hash);
		if (greenNode != null)
		{
			return (TypeParameterListSyntax)greenNode;
		}
		TypeParameterListSyntax typeParameterListSyntax = new TypeParameterListSyntax(SyntaxKind.TypeParameterList, lessThanToken, parameters.Node, greaterThanToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(typeParameterListSyntax, hash);
		}
		return typeParameterListSyntax;
	}

	public TypeParameterSyntax TypeParameter(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken varianceKeyword, SyntaxToken identifier)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8910, attributeLists.Node, varianceKeyword, identifier, context, out var hash);
		if (greenNode != null)
		{
			return (TypeParameterSyntax)greenNode;
		}
		TypeParameterSyntax typeParameterSyntax = new TypeParameterSyntax(SyntaxKind.TypeParameter, attributeLists.Node, varianceKeyword, identifier, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(typeParameterSyntax, hash);
		}
		return typeParameterSyntax;
	}

	public ClassDeclarationSyntax ClassDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, BaseListSyntax baseList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		return new ClassDeclarationSyntax(SyntaxKind.ClassDeclaration, attributeLists.Node, modifiers.Node, keyword, identifier, typeParameterList, baseList, constraintClauses.Node, openBraceToken, members.Node, closeBraceToken, semicolonToken, context);
	}

	public StructDeclarationSyntax StructDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, BaseListSyntax baseList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		return new StructDeclarationSyntax(SyntaxKind.StructDeclaration, attributeLists.Node, modifiers.Node, keyword, identifier, typeParameterList, baseList, constraintClauses.Node, openBraceToken, members.Node, closeBraceToken, semicolonToken, context);
	}

	public InterfaceDeclarationSyntax InterfaceDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, BaseListSyntax baseList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		return new InterfaceDeclarationSyntax(SyntaxKind.InterfaceDeclaration, attributeLists.Node, modifiers.Node, keyword, identifier, typeParameterList, baseList, constraintClauses.Node, openBraceToken, members.Node, closeBraceToken, semicolonToken, context);
	}

	public EnumDeclarationSyntax EnumDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken enumKeyword, SyntaxToken identifier, BaseListSyntax baseList, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<EnumMemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		return new EnumDeclarationSyntax(SyntaxKind.EnumDeclaration, attributeLists.Node, modifiers.Node, enumKeyword, identifier, baseList, openBraceToken, members.Node, closeBraceToken, semicolonToken, context);
	}

	public DelegateDeclarationSyntax DelegateDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken delegateKeyword, TypeSyntax returnType, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken semicolonToken)
	{
		return new DelegateDeclarationSyntax(SyntaxKind.DelegateDeclaration, attributeLists.Node, modifiers.Node, delegateKeyword, returnType, identifier, typeParameterList, parameterList, constraintClauses.Node, semicolonToken, context);
	}

	public EnumMemberDeclarationSyntax EnumMemberDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken identifier, EqualsValueClauseSyntax equalsValue)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8872, attributeLists.Node, identifier, equalsValue, context, out var hash);
		if (greenNode != null)
		{
			return (EnumMemberDeclarationSyntax)greenNode;
		}
		EnumMemberDeclarationSyntax enumMemberDeclarationSyntax = new EnumMemberDeclarationSyntax(SyntaxKind.EnumMemberDeclaration, attributeLists.Node, identifier, equalsValue, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(enumMemberDeclarationSyntax, hash);
		}
		return enumMemberDeclarationSyntax;
	}

	public BaseListSyntax BaseList(SyntaxToken colonToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<BaseTypeSyntax> types)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8864, colonToken, types.Node, context, out var hash);
		if (greenNode != null)
		{
			return (BaseListSyntax)greenNode;
		}
		BaseListSyntax baseListSyntax = new BaseListSyntax(SyntaxKind.BaseList, colonToken, types.Node, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(baseListSyntax, hash);
		}
		return baseListSyntax;
	}

	public SimpleBaseTypeSyntax SimpleBaseType(TypeSyntax type)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8865, type, context, out var hash);
		if (greenNode != null)
		{
			return (SimpleBaseTypeSyntax)greenNode;
		}
		SimpleBaseTypeSyntax simpleBaseTypeSyntax = new SimpleBaseTypeSyntax(SyntaxKind.SimpleBaseType, type, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(simpleBaseTypeSyntax, hash);
		}
		return simpleBaseTypeSyntax;
	}

	public TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(SyntaxToken whereKeyword, IdentifierNameSyntax name, SyntaxToken colonToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints)
	{
		return new TypeParameterConstraintClauseSyntax(SyntaxKind.TypeParameterConstraintClause, whereKeyword, name, colonToken, constraints.Node, context);
	}

	public ConstructorConstraintSyntax ConstructorConstraint(SyntaxToken newKeyword, SyntaxToken openParenToken, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8867, newKeyword, openParenToken, closeParenToken, context, out var hash);
		if (greenNode != null)
		{
			return (ConstructorConstraintSyntax)greenNode;
		}
		ConstructorConstraintSyntax constructorConstraintSyntax = new ConstructorConstraintSyntax(SyntaxKind.ConstructorConstraint, newKeyword, openParenToken, closeParenToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(constructorConstraintSyntax, hash);
		}
		return constructorConstraintSyntax;
	}

	public ClassOrStructConstraintSyntax ClassOrStructConstraint(SyntaxKind kind, SyntaxToken classOrStructKeyword)
	{
		if (kind - 8868 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode((int)kind, classOrStructKeyword, context, out var hash);
		if (greenNode != null)
		{
			return (ClassOrStructConstraintSyntax)greenNode;
		}
		ClassOrStructConstraintSyntax classOrStructConstraintSyntax = new ClassOrStructConstraintSyntax(kind, classOrStructKeyword, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(classOrStructConstraintSyntax, hash);
		}
		return classOrStructConstraintSyntax;
	}

	public TypeConstraintSyntax TypeConstraint(TypeSyntax type)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8870, type, context, out var hash);
		if (greenNode != null)
		{
			return (TypeConstraintSyntax)greenNode;
		}
		TypeConstraintSyntax typeConstraintSyntax = new TypeConstraintSyntax(SyntaxKind.TypeConstraint, type, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(typeConstraintSyntax, hash);
		}
		return typeConstraintSyntax;
	}

	public FieldDeclarationSyntax FieldDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		return new FieldDeclarationSyntax(SyntaxKind.FieldDeclaration, attributeLists.Node, modifiers.Node, declaration, semicolonToken, context);
	}

	public EventFieldDeclarationSyntax EventFieldDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken eventKeyword, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		return new EventFieldDeclarationSyntax(SyntaxKind.EventFieldDeclaration, attributeLists.Node, modifiers.Node, eventKeyword, declaration, semicolonToken, context);
	}

	public ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier(NameSyntax name, SyntaxToken dotToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8871, name, dotToken, context, out var hash);
		if (greenNode != null)
		{
			return (ExplicitInterfaceSpecifierSyntax)greenNode;
		}
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax = new ExplicitInterfaceSpecifierSyntax(SyntaxKind.ExplicitInterfaceSpecifier, name, dotToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(explicitInterfaceSpecifierSyntax, hash);
		}
		return explicitInterfaceSpecifierSyntax;
	}

	public MethodDeclarationSyntax MethodDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax returnType, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new MethodDeclarationSyntax(SyntaxKind.MethodDeclaration, attributeLists.Node, modifiers.Node, returnType, explicitInterfaceSpecifier, identifier, typeParameterList, parameterList, constraintClauses.Node, body, expressionBody, semicolonToken, context);
	}

	public OperatorDeclarationSyntax OperatorDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax returnType, SyntaxToken operatorKeyword, SyntaxToken operatorToken, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new OperatorDeclarationSyntax(SyntaxKind.OperatorDeclaration, attributeLists.Node, modifiers.Node, returnType, operatorKeyword, operatorToken, parameterList, body, expressionBody, semicolonToken, context);
	}

	public ConversionOperatorDeclarationSyntax ConversionOperatorDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, TypeSyntax type, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new ConversionOperatorDeclarationSyntax(SyntaxKind.ConversionOperatorDeclaration, attributeLists.Node, modifiers.Node, implicitOrExplicitKeyword, operatorKeyword, type, parameterList, body, expressionBody, semicolonToken, context);
	}

	public ConstructorDeclarationSyntax ConstructorDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken identifier, ParameterListSyntax parameterList, ConstructorInitializerSyntax initializer, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new ConstructorDeclarationSyntax(SyntaxKind.ConstructorDeclaration, attributeLists.Node, modifiers.Node, identifier, parameterList, initializer, body, expressionBody, semicolonToken, context);
	}

	public ConstructorInitializerSyntax ConstructorInitializer(SyntaxKind kind, SyntaxToken colonToken, SyntaxToken thisOrBaseKeyword, ArgumentListSyntax argumentList)
	{
		if (kind - 8889 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode((int)kind, colonToken, thisOrBaseKeyword, argumentList, context, out var hash);
		if (greenNode != null)
		{
			return (ConstructorInitializerSyntax)greenNode;
		}
		ConstructorInitializerSyntax constructorInitializerSyntax = new ConstructorInitializerSyntax(kind, colonToken, thisOrBaseKeyword, argumentList, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(constructorInitializerSyntax, hash);
		}
		return constructorInitializerSyntax;
	}

	public DestructorDeclarationSyntax DestructorDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken tildeToken, SyntaxToken identifier, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new DestructorDeclarationSyntax(SyntaxKind.DestructorDeclaration, attributeLists.Node, modifiers.Node, tildeToken, identifier, parameterList, body, expressionBody, semicolonToken, context);
	}

	public PropertyDeclarationSyntax PropertyDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList, ArrowExpressionClauseSyntax expressionBody, EqualsValueClauseSyntax initializer, SyntaxToken semicolonToken)
	{
		return new PropertyDeclarationSyntax(SyntaxKind.PropertyDeclaration, attributeLists.Node, modifiers.Node, type, explicitInterfaceSpecifier, identifier, accessorList, expressionBody, initializer, semicolonToken, context);
	}

	public ArrowExpressionClauseSyntax ArrowExpressionClause(SyntaxToken arrowToken, ExpressionSyntax expression)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8917, arrowToken, expression, context, out var hash);
		if (greenNode != null)
		{
			return (ArrowExpressionClauseSyntax)greenNode;
		}
		ArrowExpressionClauseSyntax arrowExpressionClauseSyntax = new ArrowExpressionClauseSyntax(SyntaxKind.ArrowExpressionClause, arrowToken, expression, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(arrowExpressionClauseSyntax, hash);
		}
		return arrowExpressionClauseSyntax;
	}

	public EventDeclarationSyntax EventDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken eventKeyword, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList)
	{
		return new EventDeclarationSyntax(SyntaxKind.EventDeclaration, attributeLists.Node, modifiers.Node, eventKeyword, type, explicitInterfaceSpecifier, identifier, accessorList, context);
	}

	public IndexerDeclarationSyntax IndexerDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken thisKeyword, BracketedParameterListSyntax parameterList, AccessorListSyntax accessorList, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new IndexerDeclarationSyntax(SyntaxKind.IndexerDeclaration, attributeLists.Node, modifiers.Node, type, explicitInterfaceSpecifier, thisKeyword, parameterList, accessorList, expressionBody, semicolonToken, context);
	}

	public AccessorListSyntax AccessorList(SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AccessorDeclarationSyntax> accessors, SyntaxToken closeBraceToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8895, openBraceToken, accessors.Node, closeBraceToken, context, out var hash);
		if (greenNode != null)
		{
			return (AccessorListSyntax)greenNode;
		}
		AccessorListSyntax accessorListSyntax = new AccessorListSyntax(SyntaxKind.AccessorList, openBraceToken, accessors.Node, closeBraceToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(accessorListSyntax, hash);
		}
		return accessorListSyntax;
	}

	public AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (kind - 8896 > (SyntaxKind)4)
		{
			throw new ArgumentException("kind");
		}
		return new AccessorDeclarationSyntax(kind, attributeLists.Node, modifiers.Node, keyword, body, expressionBody, semicolonToken, context);
	}

	public ParameterListSyntax ParameterList(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8906, openParenToken, parameters.Node, closeParenToken, context, out var hash);
		if (greenNode != null)
		{
			return (ParameterListSyntax)greenNode;
		}
		ParameterListSyntax parameterListSyntax = new ParameterListSyntax(SyntaxKind.ParameterList, openParenToken, parameters.Node, closeParenToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(parameterListSyntax, hash);
		}
		return parameterListSyntax;
	}

	public BracketedParameterListSyntax BracketedParameterList(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closeBracketToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8907, openBracketToken, parameters.Node, closeBracketToken, context, out var hash);
		if (greenNode != null)
		{
			return (BracketedParameterListSyntax)greenNode;
		}
		BracketedParameterListSyntax bracketedParameterListSyntax = new BracketedParameterListSyntax(SyntaxKind.BracketedParameterList, openBracketToken, parameters.Node, closeBracketToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(bracketedParameterListSyntax, hash);
		}
		return bracketedParameterListSyntax;
	}

	public ParameterSyntax Parameter(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax @default)
	{
		return new ParameterSyntax(SyntaxKind.Parameter, attributeLists.Node, modifiers.Node, type, identifier, @default, context);
	}

	public IncompleteMemberSyntax IncompleteMember(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type)
	{
		return new IncompleteMemberSyntax(SyntaxKind.IncompleteMember, attributeLists.Node, modifiers.Node, type, context);
	}

	public SkippedTokensTriviaSyntax SkippedTokensTrivia(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> tokens)
	{
		return new SkippedTokensTriviaSyntax(SyntaxKind.SkippedTokensTrivia, tokens.Node, context);
	}

	public DocumentationCommentTriviaSyntax DocumentationCommentTrivia(SyntaxKind kind, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlNodeSyntax> content, SyntaxToken endOfComment)
	{
		if (kind - 8544 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		return new DocumentationCommentTriviaSyntax(kind, content.Node, endOfComment, context);
	}

	public TypeCrefSyntax TypeCref(TypeSyntax type)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8597, type, context, out var hash);
		if (greenNode != null)
		{
			return (TypeCrefSyntax)greenNode;
		}
		TypeCrefSyntax typeCrefSyntax = new TypeCrefSyntax(SyntaxKind.TypeCref, type, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(typeCrefSyntax, hash);
		}
		return typeCrefSyntax;
	}

	public QualifiedCrefSyntax QualifiedCref(TypeSyntax container, SyntaxToken dotToken, MemberCrefSyntax member)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8598, container, dotToken, member, context, out var hash);
		if (greenNode != null)
		{
			return (QualifiedCrefSyntax)greenNode;
		}
		QualifiedCrefSyntax qualifiedCrefSyntax = new QualifiedCrefSyntax(SyntaxKind.QualifiedCref, container, dotToken, member, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(qualifiedCrefSyntax, hash);
		}
		return qualifiedCrefSyntax;
	}

	public NameMemberCrefSyntax NameMemberCref(TypeSyntax name, CrefParameterListSyntax parameters)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8599, name, parameters, context, out var hash);
		if (greenNode != null)
		{
			return (NameMemberCrefSyntax)greenNode;
		}
		NameMemberCrefSyntax nameMemberCrefSyntax = new NameMemberCrefSyntax(SyntaxKind.NameMemberCref, name, parameters, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(nameMemberCrefSyntax, hash);
		}
		return nameMemberCrefSyntax;
	}

	public IndexerMemberCrefSyntax IndexerMemberCref(SyntaxToken thisKeyword, CrefBracketedParameterListSyntax parameters)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8600, thisKeyword, parameters, context, out var hash);
		if (greenNode != null)
		{
			return (IndexerMemberCrefSyntax)greenNode;
		}
		IndexerMemberCrefSyntax indexerMemberCrefSyntax = new IndexerMemberCrefSyntax(SyntaxKind.IndexerMemberCref, thisKeyword, parameters, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(indexerMemberCrefSyntax, hash);
		}
		return indexerMemberCrefSyntax;
	}

	public OperatorMemberCrefSyntax OperatorMemberCref(SyntaxToken operatorKeyword, SyntaxToken operatorToken, CrefParameterListSyntax parameters)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8601, operatorKeyword, operatorToken, parameters, context, out var hash);
		if (greenNode != null)
		{
			return (OperatorMemberCrefSyntax)greenNode;
		}
		OperatorMemberCrefSyntax operatorMemberCrefSyntax = new OperatorMemberCrefSyntax(SyntaxKind.OperatorMemberCref, operatorKeyword, operatorToken, parameters, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(operatorMemberCrefSyntax, hash);
		}
		return operatorMemberCrefSyntax;
	}

	public ConversionOperatorMemberCrefSyntax ConversionOperatorMemberCref(SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, TypeSyntax type, CrefParameterListSyntax parameters)
	{
		return new ConversionOperatorMemberCrefSyntax(SyntaxKind.ConversionOperatorMemberCref, implicitOrExplicitKeyword, operatorKeyword, type, parameters, context);
	}

	public CrefParameterListSyntax CrefParameterList(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CrefParameterSyntax> parameters, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8603, openParenToken, parameters.Node, closeParenToken, context, out var hash);
		if (greenNode != null)
		{
			return (CrefParameterListSyntax)greenNode;
		}
		CrefParameterListSyntax crefParameterListSyntax = new CrefParameterListSyntax(SyntaxKind.CrefParameterList, openParenToken, parameters.Node, closeParenToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(crefParameterListSyntax, hash);
		}
		return crefParameterListSyntax;
	}

	public CrefBracketedParameterListSyntax CrefBracketedParameterList(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CrefParameterSyntax> parameters, SyntaxToken closeBracketToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8604, openBracketToken, parameters.Node, closeBracketToken, context, out var hash);
		if (greenNode != null)
		{
			return (CrefBracketedParameterListSyntax)greenNode;
		}
		CrefBracketedParameterListSyntax crefBracketedParameterListSyntax = new CrefBracketedParameterListSyntax(SyntaxKind.CrefBracketedParameterList, openBracketToken, parameters.Node, closeBracketToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(crefBracketedParameterListSyntax, hash);
		}
		return crefBracketedParameterListSyntax;
	}

	public CrefParameterSyntax CrefParameter(SyntaxToken refKindKeyword, TypeSyntax type)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8605, refKindKeyword, type, context, out var hash);
		if (greenNode != null)
		{
			return (CrefParameterSyntax)greenNode;
		}
		CrefParameterSyntax crefParameterSyntax = new CrefParameterSyntax(SyntaxKind.CrefParameter, refKindKeyword, type, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(crefParameterSyntax, hash);
		}
		return crefParameterSyntax;
	}

	public XmlElementSyntax XmlElement(XmlElementStartTagSyntax startTag, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlNodeSyntax> content, XmlElementEndTagSyntax endTag)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8574, startTag, content.Node, endTag, context, out var hash);
		if (greenNode != null)
		{
			return (XmlElementSyntax)greenNode;
		}
		XmlElementSyntax xmlElementSyntax = new XmlElementSyntax(SyntaxKind.XmlElement, startTag, content.Node, endTag, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlElementSyntax, hash);
		}
		return xmlElementSyntax;
	}

	public XmlElementStartTagSyntax XmlElementStartTag(SyntaxToken lessThanToken, XmlNameSyntax name, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlAttributeSyntax> attributes, SyntaxToken greaterThanToken)
	{
		return new XmlElementStartTagSyntax(SyntaxKind.XmlElementStartTag, lessThanToken, name, attributes.Node, greaterThanToken, context);
	}

	public XmlElementEndTagSyntax XmlElementEndTag(SyntaxToken lessThanSlashToken, XmlNameSyntax name, SyntaxToken greaterThanToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8576, lessThanSlashToken, name, greaterThanToken, context, out var hash);
		if (greenNode != null)
		{
			return (XmlElementEndTagSyntax)greenNode;
		}
		XmlElementEndTagSyntax xmlElementEndTagSyntax = new XmlElementEndTagSyntax(SyntaxKind.XmlElementEndTag, lessThanSlashToken, name, greaterThanToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlElementEndTagSyntax, hash);
		}
		return xmlElementEndTagSyntax;
	}

	public XmlEmptyElementSyntax XmlEmptyElement(SyntaxToken lessThanToken, XmlNameSyntax name, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlAttributeSyntax> attributes, SyntaxToken slashGreaterThanToken)
	{
		return new XmlEmptyElementSyntax(SyntaxKind.XmlEmptyElement, lessThanToken, name, attributes.Node, slashGreaterThanToken, context);
	}

	public XmlNameSyntax XmlName(XmlPrefixSyntax prefix, SyntaxToken localName)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8581, prefix, localName, context, out var hash);
		if (greenNode != null)
		{
			return (XmlNameSyntax)greenNode;
		}
		XmlNameSyntax xmlNameSyntax = new XmlNameSyntax(SyntaxKind.XmlName, prefix, localName, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlNameSyntax, hash);
		}
		return xmlNameSyntax;
	}

	public XmlPrefixSyntax XmlPrefix(SyntaxToken prefix, SyntaxToken colonToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8582, prefix, colonToken, context, out var hash);
		if (greenNode != null)
		{
			return (XmlPrefixSyntax)greenNode;
		}
		XmlPrefixSyntax xmlPrefixSyntax = new XmlPrefixSyntax(SyntaxKind.XmlPrefix, prefix, colonToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlPrefixSyntax, hash);
		}
		return xmlPrefixSyntax;
	}

	public XmlTextAttributeSyntax XmlTextAttribute(XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens, SyntaxToken endQuoteToken)
	{
		return new XmlTextAttributeSyntax(SyntaxKind.XmlTextAttribute, name, equalsToken, startQuoteToken, textTokens.Node, endQuoteToken, context);
	}

	public XmlCrefAttributeSyntax XmlCrefAttribute(XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, CrefSyntax cref, SyntaxToken endQuoteToken)
	{
		return new XmlCrefAttributeSyntax(SyntaxKind.XmlCrefAttribute, name, equalsToken, startQuoteToken, cref, endQuoteToken, context);
	}

	public XmlNameAttributeSyntax XmlNameAttribute(XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, IdentifierNameSyntax identifier, SyntaxToken endQuoteToken)
	{
		return new XmlNameAttributeSyntax(SyntaxKind.XmlNameAttribute, name, equalsToken, startQuoteToken, identifier, endQuoteToken, context);
	}

	public XmlTextSyntax XmlText(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8583, textTokens.Node, context, out var hash);
		if (greenNode != null)
		{
			return (XmlTextSyntax)greenNode;
		}
		XmlTextSyntax xmlTextSyntax = new XmlTextSyntax(SyntaxKind.XmlText, textTokens.Node, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlTextSyntax, hash);
		}
		return xmlTextSyntax;
	}

	public XmlCDataSectionSyntax XmlCDataSection(SyntaxToken startCDataToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens, SyntaxToken endCDataToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8584, startCDataToken, textTokens.Node, endCDataToken, context, out var hash);
		if (greenNode != null)
		{
			return (XmlCDataSectionSyntax)greenNode;
		}
		XmlCDataSectionSyntax xmlCDataSectionSyntax = new XmlCDataSectionSyntax(SyntaxKind.XmlCDataSection, startCDataToken, textTokens.Node, endCDataToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlCDataSectionSyntax, hash);
		}
		return xmlCDataSectionSyntax;
	}

	public XmlProcessingInstructionSyntax XmlProcessingInstruction(SyntaxToken startProcessingInstructionToken, XmlNameSyntax name, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens, SyntaxToken endProcessingInstructionToken)
	{
		return new XmlProcessingInstructionSyntax(SyntaxKind.XmlProcessingInstruction, startProcessingInstructionToken, name, textTokens.Node, endProcessingInstructionToken, context);
	}

	public XmlCommentSyntax XmlComment(SyntaxToken lessThanExclamationMinusMinusToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens, SyntaxToken minusMinusGreaterThanToken)
	{
		GreenNode greenNode = CSharpSyntaxNodeCache.TryGetNode(8585, lessThanExclamationMinusMinusToken, textTokens.Node, minusMinusGreaterThanToken, context, out var hash);
		if (greenNode != null)
		{
			return (XmlCommentSyntax)greenNode;
		}
		XmlCommentSyntax xmlCommentSyntax = new XmlCommentSyntax(SyntaxKind.XmlComment, lessThanExclamationMinusMinusToken, textTokens.Node, minusMinusGreaterThanToken, context);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlCommentSyntax, hash);
		}
		return xmlCommentSyntax;
	}

	public IfDirectiveTriviaSyntax IfDirectiveTrivia(SyntaxToken hashToken, SyntaxToken ifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
	{
		return new IfDirectiveTriviaSyntax(SyntaxKind.IfDirectiveTrivia, hashToken, ifKeyword, condition, endOfDirectiveToken, isActive, branchTaken, conditionValue, context);
	}

	public ElifDirectiveTriviaSyntax ElifDirectiveTrivia(SyntaxToken hashToken, SyntaxToken elifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
	{
		return new ElifDirectiveTriviaSyntax(SyntaxKind.ElifDirectiveTrivia, hashToken, elifKeyword, condition, endOfDirectiveToken, isActive, branchTaken, conditionValue, context);
	}

	public ElseDirectiveTriviaSyntax ElseDirectiveTrivia(SyntaxToken hashToken, SyntaxToken elseKeyword, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken)
	{
		return new ElseDirectiveTriviaSyntax(SyntaxKind.ElseDirectiveTrivia, hashToken, elseKeyword, endOfDirectiveToken, isActive, branchTaken, context);
	}

	public EndIfDirectiveTriviaSyntax EndIfDirectiveTrivia(SyntaxToken hashToken, SyntaxToken endIfKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new EndIfDirectiveTriviaSyntax(SyntaxKind.EndIfDirectiveTrivia, hashToken, endIfKeyword, endOfDirectiveToken, isActive, context);
	}

	public RegionDirectiveTriviaSyntax RegionDirectiveTrivia(SyntaxToken hashToken, SyntaxToken regionKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new RegionDirectiveTriviaSyntax(SyntaxKind.RegionDirectiveTrivia, hashToken, regionKeyword, endOfDirectiveToken, isActive, context);
	}

	public EndRegionDirectiveTriviaSyntax EndRegionDirectiveTrivia(SyntaxToken hashToken, SyntaxToken endRegionKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new EndRegionDirectiveTriviaSyntax(SyntaxKind.EndRegionDirectiveTrivia, hashToken, endRegionKeyword, endOfDirectiveToken, isActive, context);
	}

	public ErrorDirectiveTriviaSyntax ErrorDirectiveTrivia(SyntaxToken hashToken, SyntaxToken errorKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new ErrorDirectiveTriviaSyntax(SyntaxKind.ErrorDirectiveTrivia, hashToken, errorKeyword, endOfDirectiveToken, isActive, context);
	}

	public WarningDirectiveTriviaSyntax WarningDirectiveTrivia(SyntaxToken hashToken, SyntaxToken warningKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new WarningDirectiveTriviaSyntax(SyntaxKind.WarningDirectiveTrivia, hashToken, warningKeyword, endOfDirectiveToken, isActive, context);
	}

	public BadDirectiveTriviaSyntax BadDirectiveTrivia(SyntaxToken hashToken, SyntaxToken identifier, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new BadDirectiveTriviaSyntax(SyntaxKind.BadDirectiveTrivia, hashToken, identifier, endOfDirectiveToken, isActive, context);
	}

	public DefineDirectiveTriviaSyntax DefineDirectiveTrivia(SyntaxToken hashToken, SyntaxToken defineKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new DefineDirectiveTriviaSyntax(SyntaxKind.DefineDirectiveTrivia, hashToken, defineKeyword, name, endOfDirectiveToken, isActive, context);
	}

	public UndefDirectiveTriviaSyntax UndefDirectiveTrivia(SyntaxToken hashToken, SyntaxToken undefKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new UndefDirectiveTriviaSyntax(SyntaxKind.UndefDirectiveTrivia, hashToken, undefKeyword, name, endOfDirectiveToken, isActive, context);
	}

	public LineDirectiveTriviaSyntax LineDirectiveTrivia(SyntaxToken hashToken, SyntaxToken lineKeyword, SyntaxToken line, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new LineDirectiveTriviaSyntax(SyntaxKind.LineDirectiveTrivia, hashToken, lineKeyword, line, file, endOfDirectiveToken, isActive, context);
	}

	public PragmaWarningDirectiveTriviaSyntax PragmaWarningDirectiveTrivia(SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken warningKeyword, SyntaxToken disableOrRestoreKeyword, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> errorCodes, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new PragmaWarningDirectiveTriviaSyntax(SyntaxKind.PragmaWarningDirectiveTrivia, hashToken, pragmaKeyword, warningKeyword, disableOrRestoreKeyword, errorCodes.Node, endOfDirectiveToken, isActive, context);
	}

	public PragmaChecksumDirectiveTriviaSyntax PragmaChecksumDirectiveTrivia(SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken checksumKeyword, SyntaxToken file, SyntaxToken guid, SyntaxToken bytes, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new PragmaChecksumDirectiveTriviaSyntax(SyntaxKind.PragmaChecksumDirectiveTrivia, hashToken, pragmaKeyword, checksumKeyword, file, guid, bytes, endOfDirectiveToken, isActive, context);
	}

	public ReferenceDirectiveTriviaSyntax ReferenceDirectiveTrivia(SyntaxToken hashToken, SyntaxToken referenceKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new ReferenceDirectiveTriviaSyntax(SyntaxKind.ReferenceDirectiveTrivia, hashToken, referenceKeyword, file, endOfDirectiveToken, isActive, context);
	}

	public LoadDirectiveTriviaSyntax LoadDirectiveTrivia(SyntaxToken hashToken, SyntaxToken loadKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new LoadDirectiveTriviaSyntax(SyntaxKind.LoadDirectiveTrivia, hashToken, loadKeyword, file, endOfDirectiveToken, isActive, context);
	}

	public ShebangDirectiveTriviaSyntax ShebangDirectiveTrivia(SyntaxToken hashToken, SyntaxToken exclamationToken, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new ShebangDirectiveTriviaSyntax(SyntaxKind.ShebangDirectiveTrivia, hashToken, exclamationToken, endOfDirectiveToken, isActive, context);
	}
}
