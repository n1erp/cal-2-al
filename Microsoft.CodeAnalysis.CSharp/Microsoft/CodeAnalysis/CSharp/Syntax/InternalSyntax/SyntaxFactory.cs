using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal static class SyntaxFactory
{
	private const string CrLf = "\r\n";

	internal static readonly SyntaxTrivia CarriageReturnLineFeed = EndOfLine("\r\n");

	internal static readonly SyntaxTrivia LineFeed = EndOfLine("\n");

	internal static readonly SyntaxTrivia CarriageReturn = EndOfLine("\r");

	internal static readonly SyntaxTrivia Space = Whitespace(" ");

	internal static readonly SyntaxTrivia Tab = Whitespace("\t");

	internal static readonly SyntaxTrivia ElasticCarriageReturnLineFeed = EndOfLine("\r\n", elastic: true);

	internal static readonly SyntaxTrivia ElasticLineFeed = EndOfLine("\n", elastic: true);

	internal static readonly SyntaxTrivia ElasticCarriageReturn = EndOfLine("\r", elastic: true);

	internal static readonly SyntaxTrivia ElasticSpace = Whitespace(" ", elastic: true);

	internal static readonly SyntaxTrivia ElasticTab = Whitespace("\t", elastic: true);

	internal static readonly SyntaxTrivia ElasticZeroSpace = Whitespace(string.Empty, elastic: true);

	private static SyntaxToken s_xmlCarriageReturnLineFeed;

	private static SyntaxToken XmlCarriageReturnLineFeed => s_xmlCarriageReturnLineFeed ?? (s_xmlCarriageReturnLineFeed = XmlTextNewLine("\r\n"));

	public static IdentifierNameSyntax IdentifierName(SyntaxToken identifier)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8616, identifier, out var hash);
		if (greenNode != null)
		{
			return (IdentifierNameSyntax)greenNode;
		}
		IdentifierNameSyntax identifierNameSyntax = new IdentifierNameSyntax(SyntaxKind.IdentifierName, identifier);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(identifierNameSyntax, hash);
		}
		return identifierNameSyntax;
	}

	public static QualifiedNameSyntax QualifiedName(NameSyntax left, SyntaxToken dotToken, SimpleNameSyntax right)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8617, left, dotToken, right, out var hash);
		if (greenNode != null)
		{
			return (QualifiedNameSyntax)greenNode;
		}
		QualifiedNameSyntax qualifiedNameSyntax = new QualifiedNameSyntax(SyntaxKind.QualifiedName, left, dotToken, right);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(qualifiedNameSyntax, hash);
		}
		return qualifiedNameSyntax;
	}

	public static GenericNameSyntax GenericName(SyntaxToken identifier, TypeArgumentListSyntax typeArgumentList)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8618, identifier, typeArgumentList, out var hash);
		if (greenNode != null)
		{
			return (GenericNameSyntax)greenNode;
		}
		GenericNameSyntax genericNameSyntax = new GenericNameSyntax(SyntaxKind.GenericName, identifier, typeArgumentList);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(genericNameSyntax, hash);
		}
		return genericNameSyntax;
	}

	public static TypeArgumentListSyntax TypeArgumentList(SyntaxToken lessThanToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeSyntax> arguments, SyntaxToken greaterThanToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8619, lessThanToken, arguments.Node, greaterThanToken, out var hash);
		if (greenNode != null)
		{
			return (TypeArgumentListSyntax)greenNode;
		}
		TypeArgumentListSyntax typeArgumentListSyntax = new TypeArgumentListSyntax(SyntaxKind.TypeArgumentList, lessThanToken, arguments.Node, greaterThanToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(typeArgumentListSyntax, hash);
		}
		return typeArgumentListSyntax;
	}

	public static AliasQualifiedNameSyntax AliasQualifiedName(IdentifierNameSyntax alias, SyntaxToken colonColonToken, SimpleNameSyntax name)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8620, alias, colonColonToken, name, out var hash);
		if (greenNode != null)
		{
			return (AliasQualifiedNameSyntax)greenNode;
		}
		AliasQualifiedNameSyntax aliasQualifiedNameSyntax = new AliasQualifiedNameSyntax(SyntaxKind.AliasQualifiedName, alias, colonColonToken, name);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(aliasQualifiedNameSyntax, hash);
		}
		return aliasQualifiedNameSyntax;
	}

	public static PredefinedTypeSyntax PredefinedType(SyntaxToken keyword)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8621, keyword, out var hash);
		if (greenNode != null)
		{
			return (PredefinedTypeSyntax)greenNode;
		}
		PredefinedTypeSyntax predefinedTypeSyntax = new PredefinedTypeSyntax(SyntaxKind.PredefinedType, keyword);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(predefinedTypeSyntax, hash);
		}
		return predefinedTypeSyntax;
	}

	public static ArrayTypeSyntax ArrayType(TypeSyntax elementType, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ArrayRankSpecifierSyntax> rankSpecifiers)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8622, elementType, rankSpecifiers.Node, out var hash);
		if (greenNode != null)
		{
			return (ArrayTypeSyntax)greenNode;
		}
		ArrayTypeSyntax arrayTypeSyntax = new ArrayTypeSyntax(SyntaxKind.ArrayType, elementType, rankSpecifiers.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(arrayTypeSyntax, hash);
		}
		return arrayTypeSyntax;
	}

	public static ArrayRankSpecifierSyntax ArrayRankSpecifier(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> sizes, SyntaxToken closeBracketToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8623, openBracketToken, sizes.Node, closeBracketToken, out var hash);
		if (greenNode != null)
		{
			return (ArrayRankSpecifierSyntax)greenNode;
		}
		ArrayRankSpecifierSyntax arrayRankSpecifierSyntax = new ArrayRankSpecifierSyntax(SyntaxKind.ArrayRankSpecifier, openBracketToken, sizes.Node, closeBracketToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(arrayRankSpecifierSyntax, hash);
		}
		return arrayRankSpecifierSyntax;
	}

	public static PointerTypeSyntax PointerType(TypeSyntax elementType, SyntaxToken asteriskToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8624, elementType, asteriskToken, out var hash);
		if (greenNode != null)
		{
			return (PointerTypeSyntax)greenNode;
		}
		PointerTypeSyntax pointerTypeSyntax = new PointerTypeSyntax(SyntaxKind.PointerType, elementType, asteriskToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(pointerTypeSyntax, hash);
		}
		return pointerTypeSyntax;
	}

	public static NullableTypeSyntax NullableType(TypeSyntax elementType, SyntaxToken questionToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8625, elementType, questionToken, out var hash);
		if (greenNode != null)
		{
			return (NullableTypeSyntax)greenNode;
		}
		NullableTypeSyntax nullableTypeSyntax = new NullableTypeSyntax(SyntaxKind.NullableType, elementType, questionToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(nullableTypeSyntax, hash);
		}
		return nullableTypeSyntax;
	}

	public static TupleTypeSyntax TupleType(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TupleElementSyntax> elements, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8924, openParenToken, elements.Node, closeParenToken, out var hash);
		if (greenNode != null)
		{
			return (TupleTypeSyntax)greenNode;
		}
		TupleTypeSyntax tupleTypeSyntax = new TupleTypeSyntax(SyntaxKind.TupleType, openParenToken, elements.Node, closeParenToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(tupleTypeSyntax, hash);
		}
		return tupleTypeSyntax;
	}

	public static TupleElementSyntax TupleElement(TypeSyntax type, SyntaxToken identifier)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8925, type, identifier, out var hash);
		if (greenNode != null)
		{
			return (TupleElementSyntax)greenNode;
		}
		TupleElementSyntax tupleElementSyntax = new TupleElementSyntax(SyntaxKind.TupleElement, type, identifier);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(tupleElementSyntax, hash);
		}
		return tupleElementSyntax;
	}

	public static OmittedTypeArgumentSyntax OmittedTypeArgument(SyntaxToken omittedTypeArgumentToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8626, omittedTypeArgumentToken, out var hash);
		if (greenNode != null)
		{
			return (OmittedTypeArgumentSyntax)greenNode;
		}
		OmittedTypeArgumentSyntax omittedTypeArgumentSyntax = new OmittedTypeArgumentSyntax(SyntaxKind.OmittedTypeArgument, omittedTypeArgumentToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(omittedTypeArgumentSyntax, hash);
		}
		return omittedTypeArgumentSyntax;
	}

	public static RefTypeSyntax RefType(SyntaxToken refKeyword, SyntaxToken readOnlyKeyword, TypeSyntax type)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(9051, refKeyword, readOnlyKeyword, type, out var hash);
		if (greenNode != null)
		{
			return (RefTypeSyntax)greenNode;
		}
		RefTypeSyntax refTypeSyntax = new RefTypeSyntax(SyntaxKind.RefType, refKeyword, readOnlyKeyword, type);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(refTypeSyntax, hash);
		}
		return refTypeSyntax;
	}

	public static ParenthesizedExpressionSyntax ParenthesizedExpression(SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8632, openParenToken, expression, closeParenToken, out var hash);
		if (greenNode != null)
		{
			return (ParenthesizedExpressionSyntax)greenNode;
		}
		ParenthesizedExpressionSyntax parenthesizedExpressionSyntax = new ParenthesizedExpressionSyntax(SyntaxKind.ParenthesizedExpression, openParenToken, expression, closeParenToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(parenthesizedExpressionSyntax, hash);
		}
		return parenthesizedExpressionSyntax;
	}

	public static TupleExpressionSyntax TupleExpression(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> arguments, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8926, openParenToken, arguments.Node, closeParenToken, out var hash);
		if (greenNode != null)
		{
			return (TupleExpressionSyntax)greenNode;
		}
		TupleExpressionSyntax tupleExpressionSyntax = new TupleExpressionSyntax(SyntaxKind.TupleExpression, openParenToken, arguments.Node, closeParenToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(tupleExpressionSyntax, hash);
		}
		return tupleExpressionSyntax;
	}

	public static PrefixUnaryExpressionSyntax PrefixUnaryExpression(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax operand)
	{
		if (kind - 8730 > (SyntaxKind)7)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = SyntaxNodeCache.TryGetNode((int)kind, operatorToken, operand, out var hash);
		if (greenNode != null)
		{
			return (PrefixUnaryExpressionSyntax)greenNode;
		}
		PrefixUnaryExpressionSyntax prefixUnaryExpressionSyntax = new PrefixUnaryExpressionSyntax(kind, operatorToken, operand);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(prefixUnaryExpressionSyntax, hash);
		}
		return prefixUnaryExpressionSyntax;
	}

	public static AwaitExpressionSyntax AwaitExpression(SyntaxToken awaitKeyword, ExpressionSyntax expression)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8740, awaitKeyword, expression, out var hash);
		if (greenNode != null)
		{
			return (AwaitExpressionSyntax)greenNode;
		}
		AwaitExpressionSyntax awaitExpressionSyntax = new AwaitExpressionSyntax(SyntaxKind.AwaitExpression, awaitKeyword, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(awaitExpressionSyntax, hash);
		}
		return awaitExpressionSyntax;
	}

	public static PostfixUnaryExpressionSyntax PostfixUnaryExpression(SyntaxKind kind, ExpressionSyntax operand, SyntaxToken operatorToken)
	{
		if (kind - 8738 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = SyntaxNodeCache.TryGetNode((int)kind, operand, operatorToken, out var hash);
		if (greenNode != null)
		{
			return (PostfixUnaryExpressionSyntax)greenNode;
		}
		PostfixUnaryExpressionSyntax postfixUnaryExpressionSyntax = new PostfixUnaryExpressionSyntax(kind, operand, operatorToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(postfixUnaryExpressionSyntax, hash);
		}
		return postfixUnaryExpressionSyntax;
	}

	public static MemberAccessExpressionSyntax MemberAccessExpression(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken operatorToken, SimpleNameSyntax name)
	{
		if (kind - 8689 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = SyntaxNodeCache.TryGetNode((int)kind, expression, operatorToken, name, out var hash);
		if (greenNode != null)
		{
			return (MemberAccessExpressionSyntax)greenNode;
		}
		MemberAccessExpressionSyntax memberAccessExpressionSyntax = new MemberAccessExpressionSyntax(kind, expression, operatorToken, name);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(memberAccessExpressionSyntax, hash);
		}
		return memberAccessExpressionSyntax;
	}

	public static ConditionalAccessExpressionSyntax ConditionalAccessExpression(ExpressionSyntax expression, SyntaxToken operatorToken, ExpressionSyntax whenNotNull)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8691, expression, operatorToken, whenNotNull, out var hash);
		if (greenNode != null)
		{
			return (ConditionalAccessExpressionSyntax)greenNode;
		}
		ConditionalAccessExpressionSyntax conditionalAccessExpressionSyntax = new ConditionalAccessExpressionSyntax(SyntaxKind.ConditionalAccessExpression, expression, operatorToken, whenNotNull);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(conditionalAccessExpressionSyntax, hash);
		}
		return conditionalAccessExpressionSyntax;
	}

	public static MemberBindingExpressionSyntax MemberBindingExpression(SyntaxToken operatorToken, SimpleNameSyntax name)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8707, operatorToken, name, out var hash);
		if (greenNode != null)
		{
			return (MemberBindingExpressionSyntax)greenNode;
		}
		MemberBindingExpressionSyntax memberBindingExpressionSyntax = new MemberBindingExpressionSyntax(SyntaxKind.MemberBindingExpression, operatorToken, name);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(memberBindingExpressionSyntax, hash);
		}
		return memberBindingExpressionSyntax;
	}

	public static ElementBindingExpressionSyntax ElementBindingExpression(BracketedArgumentListSyntax argumentList)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8708, argumentList, out var hash);
		if (greenNode != null)
		{
			return (ElementBindingExpressionSyntax)greenNode;
		}
		ElementBindingExpressionSyntax elementBindingExpressionSyntax = new ElementBindingExpressionSyntax(SyntaxKind.ElementBindingExpression, argumentList);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(elementBindingExpressionSyntax, hash);
		}
		return elementBindingExpressionSyntax;
	}

	public static ImplicitElementAccessSyntax ImplicitElementAccess(BracketedArgumentListSyntax argumentList)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8656, argumentList, out var hash);
		if (greenNode != null)
		{
			return (ImplicitElementAccessSyntax)greenNode;
		}
		ImplicitElementAccessSyntax implicitElementAccessSyntax = new ImplicitElementAccessSyntax(SyntaxKind.ImplicitElementAccess, argumentList);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(implicitElementAccessSyntax, hash);
		}
		return implicitElementAccessSyntax;
	}

	public static BinaryExpressionSyntax BinaryExpression(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
	{
		if (kind - 8668 > (SyntaxKind)20)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = SyntaxNodeCache.TryGetNode((int)kind, left, operatorToken, right, out var hash);
		if (greenNode != null)
		{
			return (BinaryExpressionSyntax)greenNode;
		}
		BinaryExpressionSyntax binaryExpressionSyntax = new BinaryExpressionSyntax(kind, left, operatorToken, right);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(binaryExpressionSyntax, hash);
		}
		return binaryExpressionSyntax;
	}

	public static AssignmentExpressionSyntax AssignmentExpression(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
	{
		if (kind - 8714 > (SyntaxKind)10)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = SyntaxNodeCache.TryGetNode((int)kind, left, operatorToken, right, out var hash);
		if (greenNode != null)
		{
			return (AssignmentExpressionSyntax)greenNode;
		}
		AssignmentExpressionSyntax assignmentExpressionSyntax = new AssignmentExpressionSyntax(kind, left, operatorToken, right);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(assignmentExpressionSyntax, hash);
		}
		return assignmentExpressionSyntax;
	}

	public static ConditionalExpressionSyntax ConditionalExpression(ExpressionSyntax condition, SyntaxToken questionToken, ExpressionSyntax whenTrue, SyntaxToken colonToken, ExpressionSyntax whenFalse)
	{
		return new ConditionalExpressionSyntax(SyntaxKind.ConditionalExpression, condition, questionToken, whenTrue, colonToken, whenFalse);
	}

	public static ThisExpressionSyntax ThisExpression(SyntaxToken token)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8746, token, out var hash);
		if (greenNode != null)
		{
			return (ThisExpressionSyntax)greenNode;
		}
		ThisExpressionSyntax thisExpressionSyntax = new ThisExpressionSyntax(SyntaxKind.ThisExpression, token);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(thisExpressionSyntax, hash);
		}
		return thisExpressionSyntax;
	}

	public static BaseExpressionSyntax BaseExpression(SyntaxToken token)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8747, token, out var hash);
		if (greenNode != null)
		{
			return (BaseExpressionSyntax)greenNode;
		}
		BaseExpressionSyntax baseExpressionSyntax = new BaseExpressionSyntax(SyntaxKind.BaseExpression, token);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(baseExpressionSyntax, hash);
		}
		return baseExpressionSyntax;
	}

	public static LiteralExpressionSyntax LiteralExpression(SyntaxKind kind, SyntaxToken token)
	{
		if (kind - 8748 > (SyntaxKind)7)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = SyntaxNodeCache.TryGetNode((int)kind, token, out var hash);
		if (greenNode != null)
		{
			return (LiteralExpressionSyntax)greenNode;
		}
		LiteralExpressionSyntax literalExpressionSyntax = new LiteralExpressionSyntax(kind, token);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(literalExpressionSyntax, hash);
		}
		return literalExpressionSyntax;
	}

	public static MakeRefExpressionSyntax MakeRefExpression(SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		return new MakeRefExpressionSyntax(SyntaxKind.MakeRefExpression, keyword, openParenToken, expression, closeParenToken);
	}

	public static RefTypeExpressionSyntax RefTypeExpression(SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		return new RefTypeExpressionSyntax(SyntaxKind.RefTypeExpression, keyword, openParenToken, expression, closeParenToken);
	}

	public static RefValueExpressionSyntax RefValueExpression(SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken comma, TypeSyntax type, SyntaxToken closeParenToken)
	{
		return new RefValueExpressionSyntax(SyntaxKind.RefValueExpression, keyword, openParenToken, expression, comma, type, closeParenToken);
	}

	public static CheckedExpressionSyntax CheckedExpression(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		if (kind - 8762 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		return new CheckedExpressionSyntax(kind, keyword, openParenToken, expression, closeParenToken);
	}

	public static DefaultExpressionSyntax DefaultExpression(SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken)
	{
		return new DefaultExpressionSyntax(SyntaxKind.DefaultExpression, keyword, openParenToken, type, closeParenToken);
	}

	public static TypeOfExpressionSyntax TypeOfExpression(SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken)
	{
		return new TypeOfExpressionSyntax(SyntaxKind.TypeOfExpression, keyword, openParenToken, type, closeParenToken);
	}

	public static SizeOfExpressionSyntax SizeOfExpression(SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken)
	{
		return new SizeOfExpressionSyntax(SyntaxKind.SizeOfExpression, keyword, openParenToken, type, closeParenToken);
	}

	public static InvocationExpressionSyntax InvocationExpression(ExpressionSyntax expression, ArgumentListSyntax argumentList)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8634, expression, argumentList, out var hash);
		if (greenNode != null)
		{
			return (InvocationExpressionSyntax)greenNode;
		}
		InvocationExpressionSyntax invocationExpressionSyntax = new InvocationExpressionSyntax(SyntaxKind.InvocationExpression, expression, argumentList);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(invocationExpressionSyntax, hash);
		}
		return invocationExpressionSyntax;
	}

	public static ElementAccessExpressionSyntax ElementAccessExpression(ExpressionSyntax expression, BracketedArgumentListSyntax argumentList)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8635, expression, argumentList, out var hash);
		if (greenNode != null)
		{
			return (ElementAccessExpressionSyntax)greenNode;
		}
		ElementAccessExpressionSyntax elementAccessExpressionSyntax = new ElementAccessExpressionSyntax(SyntaxKind.ElementAccessExpression, expression, argumentList);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(elementAccessExpressionSyntax, hash);
		}
		return elementAccessExpressionSyntax;
	}

	public static ArgumentListSyntax ArgumentList(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> arguments, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8636, openParenToken, arguments.Node, closeParenToken, out var hash);
		if (greenNode != null)
		{
			return (ArgumentListSyntax)greenNode;
		}
		ArgumentListSyntax argumentListSyntax = new ArgumentListSyntax(SyntaxKind.ArgumentList, openParenToken, arguments.Node, closeParenToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(argumentListSyntax, hash);
		}
		return argumentListSyntax;
	}

	public static BracketedArgumentListSyntax BracketedArgumentList(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> arguments, SyntaxToken closeBracketToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8637, openBracketToken, arguments.Node, closeBracketToken, out var hash);
		if (greenNode != null)
		{
			return (BracketedArgumentListSyntax)greenNode;
		}
		BracketedArgumentListSyntax bracketedArgumentListSyntax = new BracketedArgumentListSyntax(SyntaxKind.BracketedArgumentList, openBracketToken, arguments.Node, closeBracketToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(bracketedArgumentListSyntax, hash);
		}
		return bracketedArgumentListSyntax;
	}

	public static ArgumentSyntax Argument(NameColonSyntax nameColon, SyntaxToken refKindKeyword, ExpressionSyntax expression)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8638, nameColon, refKindKeyword, expression, out var hash);
		if (greenNode != null)
		{
			return (ArgumentSyntax)greenNode;
		}
		ArgumentSyntax argumentSyntax = new ArgumentSyntax(SyntaxKind.Argument, nameColon, refKindKeyword, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(argumentSyntax, hash);
		}
		return argumentSyntax;
	}

	public static NameColonSyntax NameColon(IdentifierNameSyntax name, SyntaxToken colonToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8639, name, colonToken, out var hash);
		if (greenNode != null)
		{
			return (NameColonSyntax)greenNode;
		}
		NameColonSyntax nameColonSyntax = new NameColonSyntax(SyntaxKind.NameColon, name, colonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(nameColonSyntax, hash);
		}
		return nameColonSyntax;
	}

	public static DeclarationExpressionSyntax DeclarationExpression(TypeSyntax type, VariableDesignationSyntax designation)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(9040, type, designation, out var hash);
		if (greenNode != null)
		{
			return (DeclarationExpressionSyntax)greenNode;
		}
		DeclarationExpressionSyntax declarationExpressionSyntax = new DeclarationExpressionSyntax(SyntaxKind.DeclarationExpression, type, designation);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(declarationExpressionSyntax, hash);
		}
		return declarationExpressionSyntax;
	}

	public static CastExpressionSyntax CastExpression(SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken, ExpressionSyntax expression)
	{
		return new CastExpressionSyntax(SyntaxKind.CastExpression, openParenToken, type, closeParenToken, expression);
	}

	public static AnonymousMethodExpressionSyntax AnonymousMethodExpression(SyntaxToken asyncKeyword, SyntaxToken delegateKeyword, ParameterListSyntax parameterList, CSharpSyntaxNode body)
	{
		return new AnonymousMethodExpressionSyntax(SyntaxKind.AnonymousMethodExpression, asyncKeyword, delegateKeyword, parameterList, body);
	}

	public static SimpleLambdaExpressionSyntax SimpleLambdaExpression(SyntaxToken asyncKeyword, ParameterSyntax parameter, SyntaxToken arrowToken, CSharpSyntaxNode body)
	{
		return new SimpleLambdaExpressionSyntax(SyntaxKind.SimpleLambdaExpression, asyncKeyword, parameter, arrowToken, body);
	}

	public static RefExpressionSyntax RefExpression(SyntaxToken refKeyword, ExpressionSyntax expression)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(9050, refKeyword, expression, out var hash);
		if (greenNode != null)
		{
			return (RefExpressionSyntax)greenNode;
		}
		RefExpressionSyntax refExpressionSyntax = new RefExpressionSyntax(SyntaxKind.RefExpression, refKeyword, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(refExpressionSyntax, hash);
		}
		return refExpressionSyntax;
	}

	public static ParenthesizedLambdaExpressionSyntax ParenthesizedLambdaExpression(SyntaxToken asyncKeyword, ParameterListSyntax parameterList, SyntaxToken arrowToken, CSharpSyntaxNode body)
	{
		return new ParenthesizedLambdaExpressionSyntax(SyntaxKind.ParenthesizedLambdaExpression, asyncKeyword, parameterList, arrowToken, body);
	}

	public static InitializerExpressionSyntax InitializerExpression(SyntaxKind kind, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> expressions, SyntaxToken closeBraceToken)
	{
		if (kind - 8644 > (SyntaxKind)2 && kind != SyntaxKind.ComplexElementInitializerExpression)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = SyntaxNodeCache.TryGetNode((int)kind, openBraceToken, expressions.Node, closeBraceToken, out var hash);
		if (greenNode != null)
		{
			return (InitializerExpressionSyntax)greenNode;
		}
		InitializerExpressionSyntax initializerExpressionSyntax = new InitializerExpressionSyntax(kind, openBraceToken, expressions.Node, closeBraceToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(initializerExpressionSyntax, hash);
		}
		return initializerExpressionSyntax;
	}

	public static ObjectCreationExpressionSyntax ObjectCreationExpression(SyntaxToken newKeyword, TypeSyntax type, ArgumentListSyntax argumentList, InitializerExpressionSyntax initializer)
	{
		return new ObjectCreationExpressionSyntax(SyntaxKind.ObjectCreationExpression, newKeyword, type, argumentList, initializer);
	}

	public static AnonymousObjectMemberDeclaratorSyntax AnonymousObjectMemberDeclarator(NameEqualsSyntax nameEquals, ExpressionSyntax expression)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8647, nameEquals, expression, out var hash);
		if (greenNode != null)
		{
			return (AnonymousObjectMemberDeclaratorSyntax)greenNode;
		}
		AnonymousObjectMemberDeclaratorSyntax anonymousObjectMemberDeclaratorSyntax = new AnonymousObjectMemberDeclaratorSyntax(SyntaxKind.AnonymousObjectMemberDeclarator, nameEquals, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(anonymousObjectMemberDeclaratorSyntax, hash);
		}
		return anonymousObjectMemberDeclaratorSyntax;
	}

	public static AnonymousObjectCreationExpressionSyntax AnonymousObjectCreationExpression(SyntaxToken newKeyword, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax> initializers, SyntaxToken closeBraceToken)
	{
		return new AnonymousObjectCreationExpressionSyntax(SyntaxKind.AnonymousObjectCreationExpression, newKeyword, openBraceToken, initializers.Node, closeBraceToken);
	}

	public static ArrayCreationExpressionSyntax ArrayCreationExpression(SyntaxToken newKeyword, ArrayTypeSyntax type, InitializerExpressionSyntax initializer)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8651, newKeyword, type, initializer, out var hash);
		if (greenNode != null)
		{
			return (ArrayCreationExpressionSyntax)greenNode;
		}
		ArrayCreationExpressionSyntax arrayCreationExpressionSyntax = new ArrayCreationExpressionSyntax(SyntaxKind.ArrayCreationExpression, newKeyword, type, initializer);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(arrayCreationExpressionSyntax, hash);
		}
		return arrayCreationExpressionSyntax;
	}

	public static ImplicitArrayCreationExpressionSyntax ImplicitArrayCreationExpression(SyntaxToken newKeyword, SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> commas, SyntaxToken closeBracketToken, InitializerExpressionSyntax initializer)
	{
		return new ImplicitArrayCreationExpressionSyntax(SyntaxKind.ImplicitArrayCreationExpression, newKeyword, openBracketToken, commas.Node, closeBracketToken, initializer);
	}

	public static StackAllocArrayCreationExpressionSyntax StackAllocArrayCreationExpression(SyntaxToken stackAllocKeyword, TypeSyntax type, InitializerExpressionSyntax initializer)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8653, stackAllocKeyword, type, initializer, out var hash);
		if (greenNode != null)
		{
			return (StackAllocArrayCreationExpressionSyntax)greenNode;
		}
		StackAllocArrayCreationExpressionSyntax stackAllocArrayCreationExpressionSyntax = new StackAllocArrayCreationExpressionSyntax(SyntaxKind.StackAllocArrayCreationExpression, stackAllocKeyword, type, initializer);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(stackAllocArrayCreationExpressionSyntax, hash);
		}
		return stackAllocArrayCreationExpressionSyntax;
	}

	public static ImplicitStackAllocArrayCreationExpressionSyntax ImplicitStackAllocArrayCreationExpression(SyntaxToken stackAllocKeyword, SyntaxToken openBracketToken, SyntaxToken closeBracketToken, InitializerExpressionSyntax initializer)
	{
		return new ImplicitStackAllocArrayCreationExpressionSyntax(SyntaxKind.ImplicitStackAllocArrayCreationExpression, stackAllocKeyword, openBracketToken, closeBracketToken, initializer);
	}

	public static QueryExpressionSyntax QueryExpression(FromClauseSyntax fromClause, QueryBodySyntax body)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8774, fromClause, body, out var hash);
		if (greenNode != null)
		{
			return (QueryExpressionSyntax)greenNode;
		}
		QueryExpressionSyntax queryExpressionSyntax = new QueryExpressionSyntax(SyntaxKind.QueryExpression, fromClause, body);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(queryExpressionSyntax, hash);
		}
		return queryExpressionSyntax;
	}

	public static QueryBodySyntax QueryBody(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<QueryClauseSyntax> clauses, SelectOrGroupClauseSyntax selectOrGroup, QueryContinuationSyntax continuation)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8775, clauses.Node, selectOrGroup, continuation, out var hash);
		if (greenNode != null)
		{
			return (QueryBodySyntax)greenNode;
		}
		QueryBodySyntax queryBodySyntax = new QueryBodySyntax(SyntaxKind.QueryBody, clauses.Node, selectOrGroup, continuation);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(queryBodySyntax, hash);
		}
		return queryBodySyntax;
	}

	public static FromClauseSyntax FromClause(SyntaxToken fromKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression)
	{
		return new FromClauseSyntax(SyntaxKind.FromClause, fromKeyword, type, identifier, inKeyword, expression);
	}

	public static LetClauseSyntax LetClause(SyntaxToken letKeyword, SyntaxToken identifier, SyntaxToken equalsToken, ExpressionSyntax expression)
	{
		return new LetClauseSyntax(SyntaxKind.LetClause, letKeyword, identifier, equalsToken, expression);
	}

	public static JoinClauseSyntax JoinClause(SyntaxToken joinKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax inExpression, SyntaxToken onKeyword, ExpressionSyntax leftExpression, SyntaxToken equalsKeyword, ExpressionSyntax rightExpression, JoinIntoClauseSyntax into)
	{
		return new JoinClauseSyntax(SyntaxKind.JoinClause, joinKeyword, type, identifier, inKeyword, inExpression, onKeyword, leftExpression, equalsKeyword, rightExpression, into);
	}

	public static JoinIntoClauseSyntax JoinIntoClause(SyntaxToken intoKeyword, SyntaxToken identifier)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8779, intoKeyword, identifier, out var hash);
		if (greenNode != null)
		{
			return (JoinIntoClauseSyntax)greenNode;
		}
		JoinIntoClauseSyntax joinIntoClauseSyntax = new JoinIntoClauseSyntax(SyntaxKind.JoinIntoClause, intoKeyword, identifier);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(joinIntoClauseSyntax, hash);
		}
		return joinIntoClauseSyntax;
	}

	public static WhereClauseSyntax WhereClause(SyntaxToken whereKeyword, ExpressionSyntax condition)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8780, whereKeyword, condition, out var hash);
		if (greenNode != null)
		{
			return (WhereClauseSyntax)greenNode;
		}
		WhereClauseSyntax whereClauseSyntax = new WhereClauseSyntax(SyntaxKind.WhereClause, whereKeyword, condition);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(whereClauseSyntax, hash);
		}
		return whereClauseSyntax;
	}

	public static OrderByClauseSyntax OrderByClause(SyntaxToken orderByKeyword, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<OrderingSyntax> orderings)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8781, orderByKeyword, orderings.Node, out var hash);
		if (greenNode != null)
		{
			return (OrderByClauseSyntax)greenNode;
		}
		OrderByClauseSyntax orderByClauseSyntax = new OrderByClauseSyntax(SyntaxKind.OrderByClause, orderByKeyword, orderings.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(orderByClauseSyntax, hash);
		}
		return orderByClauseSyntax;
	}

	public static OrderingSyntax Ordering(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken ascendingOrDescendingKeyword)
	{
		if (kind - 8782 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = SyntaxNodeCache.TryGetNode((int)kind, expression, ascendingOrDescendingKeyword, out var hash);
		if (greenNode != null)
		{
			return (OrderingSyntax)greenNode;
		}
		OrderingSyntax orderingSyntax = new OrderingSyntax(kind, expression, ascendingOrDescendingKeyword);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(orderingSyntax, hash);
		}
		return orderingSyntax;
	}

	public static SelectClauseSyntax SelectClause(SyntaxToken selectKeyword, ExpressionSyntax expression)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8784, selectKeyword, expression, out var hash);
		if (greenNode != null)
		{
			return (SelectClauseSyntax)greenNode;
		}
		SelectClauseSyntax selectClauseSyntax = new SelectClauseSyntax(SyntaxKind.SelectClause, selectKeyword, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(selectClauseSyntax, hash);
		}
		return selectClauseSyntax;
	}

	public static GroupClauseSyntax GroupClause(SyntaxToken groupKeyword, ExpressionSyntax groupExpression, SyntaxToken byKeyword, ExpressionSyntax byExpression)
	{
		return new GroupClauseSyntax(SyntaxKind.GroupClause, groupKeyword, groupExpression, byKeyword, byExpression);
	}

	public static QueryContinuationSyntax QueryContinuation(SyntaxToken intoKeyword, SyntaxToken identifier, QueryBodySyntax body)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8786, intoKeyword, identifier, body, out var hash);
		if (greenNode != null)
		{
			return (QueryContinuationSyntax)greenNode;
		}
		QueryContinuationSyntax queryContinuationSyntax = new QueryContinuationSyntax(SyntaxKind.QueryContinuation, intoKeyword, identifier, body);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(queryContinuationSyntax, hash);
		}
		return queryContinuationSyntax;
	}

	public static OmittedArraySizeExpressionSyntax OmittedArraySizeExpression(SyntaxToken omittedArraySizeExpressionToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8654, omittedArraySizeExpressionToken, out var hash);
		if (greenNode != null)
		{
			return (OmittedArraySizeExpressionSyntax)greenNode;
		}
		OmittedArraySizeExpressionSyntax omittedArraySizeExpressionSyntax = new OmittedArraySizeExpressionSyntax(SyntaxKind.OmittedArraySizeExpression, omittedArraySizeExpressionToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(omittedArraySizeExpressionSyntax, hash);
		}
		return omittedArraySizeExpressionSyntax;
	}

	public static InterpolatedStringExpressionSyntax InterpolatedStringExpression(SyntaxToken stringStartToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<InterpolatedStringContentSyntax> contents, SyntaxToken stringEndToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8655, stringStartToken, contents.Node, stringEndToken, out var hash);
		if (greenNode != null)
		{
			return (InterpolatedStringExpressionSyntax)greenNode;
		}
		InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax = new InterpolatedStringExpressionSyntax(SyntaxKind.InterpolatedStringExpression, stringStartToken, contents.Node, stringEndToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(interpolatedStringExpressionSyntax, hash);
		}
		return interpolatedStringExpressionSyntax;
	}

	public static IsPatternExpressionSyntax IsPatternExpression(ExpressionSyntax expression, SyntaxToken isKeyword, PatternSyntax pattern)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8657, expression, isKeyword, pattern, out var hash);
		if (greenNode != null)
		{
			return (IsPatternExpressionSyntax)greenNode;
		}
		IsPatternExpressionSyntax isPatternExpressionSyntax = new IsPatternExpressionSyntax(SyntaxKind.IsPatternExpression, expression, isKeyword, pattern);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(isPatternExpressionSyntax, hash);
		}
		return isPatternExpressionSyntax;
	}

	public static ThrowExpressionSyntax ThrowExpression(SyntaxToken throwKeyword, ExpressionSyntax expression)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(9052, throwKeyword, expression, out var hash);
		if (greenNode != null)
		{
			return (ThrowExpressionSyntax)greenNode;
		}
		ThrowExpressionSyntax throwExpressionSyntax = new ThrowExpressionSyntax(SyntaxKind.ThrowExpression, throwKeyword, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(throwExpressionSyntax, hash);
		}
		return throwExpressionSyntax;
	}

	public static WhenClauseSyntax WhenClause(SyntaxToken whenKeyword, ExpressionSyntax condition)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(9013, whenKeyword, condition, out var hash);
		if (greenNode != null)
		{
			return (WhenClauseSyntax)greenNode;
		}
		WhenClauseSyntax whenClauseSyntax = new WhenClauseSyntax(SyntaxKind.WhenClause, whenKeyword, condition);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(whenClauseSyntax, hash);
		}
		return whenClauseSyntax;
	}

	public static DeclarationPatternSyntax DeclarationPattern(TypeSyntax type, VariableDesignationSyntax designation)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(9000, type, designation, out var hash);
		if (greenNode != null)
		{
			return (DeclarationPatternSyntax)greenNode;
		}
		DeclarationPatternSyntax declarationPatternSyntax = new DeclarationPatternSyntax(SyntaxKind.DeclarationPattern, type, designation);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(declarationPatternSyntax, hash);
		}
		return declarationPatternSyntax;
	}

	public static ConstantPatternSyntax ConstantPattern(ExpressionSyntax expression)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(9002, expression, out var hash);
		if (greenNode != null)
		{
			return (ConstantPatternSyntax)greenNode;
		}
		ConstantPatternSyntax constantPatternSyntax = new ConstantPatternSyntax(SyntaxKind.ConstantPattern, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(constantPatternSyntax, hash);
		}
		return constantPatternSyntax;
	}

	public static InterpolatedStringTextSyntax InterpolatedStringText(SyntaxToken textToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8919, textToken, out var hash);
		if (greenNode != null)
		{
			return (InterpolatedStringTextSyntax)greenNode;
		}
		InterpolatedStringTextSyntax interpolatedStringTextSyntax = new InterpolatedStringTextSyntax(SyntaxKind.InterpolatedStringText, textToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(interpolatedStringTextSyntax, hash);
		}
		return interpolatedStringTextSyntax;
	}

	public static InterpolationSyntax Interpolation(SyntaxToken openBraceToken, ExpressionSyntax expression, InterpolationAlignmentClauseSyntax alignmentClause, InterpolationFormatClauseSyntax formatClause, SyntaxToken closeBraceToken)
	{
		return new InterpolationSyntax(SyntaxKind.Interpolation, openBraceToken, expression, alignmentClause, formatClause, closeBraceToken);
	}

	public static InterpolationAlignmentClauseSyntax InterpolationAlignmentClause(SyntaxToken commaToken, ExpressionSyntax value)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8920, commaToken, value, out var hash);
		if (greenNode != null)
		{
			return (InterpolationAlignmentClauseSyntax)greenNode;
		}
		InterpolationAlignmentClauseSyntax interpolationAlignmentClauseSyntax = new InterpolationAlignmentClauseSyntax(SyntaxKind.InterpolationAlignmentClause, commaToken, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(interpolationAlignmentClauseSyntax, hash);
		}
		return interpolationAlignmentClauseSyntax;
	}

	public static InterpolationFormatClauseSyntax InterpolationFormatClause(SyntaxToken colonToken, SyntaxToken formatStringToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8921, colonToken, formatStringToken, out var hash);
		if (greenNode != null)
		{
			return (InterpolationFormatClauseSyntax)greenNode;
		}
		InterpolationFormatClauseSyntax interpolationFormatClauseSyntax = new InterpolationFormatClauseSyntax(SyntaxKind.InterpolationFormatClause, colonToken, formatStringToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(interpolationFormatClauseSyntax, hash);
		}
		return interpolationFormatClauseSyntax;
	}

	public static GlobalStatementSyntax GlobalStatement(StatementSyntax statement)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8841, statement, out var hash);
		if (greenNode != null)
		{
			return (GlobalStatementSyntax)greenNode;
		}
		GlobalStatementSyntax globalStatementSyntax = new GlobalStatementSyntax(SyntaxKind.GlobalStatement, statement);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(globalStatementSyntax, hash);
		}
		return globalStatementSyntax;
	}

	public static BlockSyntax Block(SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax> statements, SyntaxToken closeBraceToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8792, openBraceToken, statements.Node, closeBraceToken, out var hash);
		if (greenNode != null)
		{
			return (BlockSyntax)greenNode;
		}
		BlockSyntax blockSyntax = new BlockSyntax(SyntaxKind.Block, openBraceToken, statements.Node, closeBraceToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(blockSyntax, hash);
		}
		return blockSyntax;
	}

	public static LocalFunctionStatementSyntax LocalFunctionStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax returnType, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new LocalFunctionStatementSyntax(SyntaxKind.LocalFunctionStatement, modifiers.Node, returnType, identifier, typeParameterList, parameterList, constraintClauses.Node, body, expressionBody, semicolonToken);
	}

	public static LocalDeclarationStatementSyntax LocalDeclarationStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8793, modifiers.Node, declaration, semicolonToken, out var hash);
		if (greenNode != null)
		{
			return (LocalDeclarationStatementSyntax)greenNode;
		}
		LocalDeclarationStatementSyntax localDeclarationStatementSyntax = new LocalDeclarationStatementSyntax(SyntaxKind.LocalDeclarationStatement, modifiers.Node, declaration, semicolonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(localDeclarationStatementSyntax, hash);
		}
		return localDeclarationStatementSyntax;
	}

	public static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDeclaratorSyntax> variables)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8794, type, variables.Node, out var hash);
		if (greenNode != null)
		{
			return (VariableDeclarationSyntax)greenNode;
		}
		VariableDeclarationSyntax variableDeclarationSyntax = new VariableDeclarationSyntax(SyntaxKind.VariableDeclaration, type, variables.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(variableDeclarationSyntax, hash);
		}
		return variableDeclarationSyntax;
	}

	public static VariableDeclaratorSyntax VariableDeclarator(SyntaxToken identifier, BracketedArgumentListSyntax argumentList, EqualsValueClauseSyntax initializer)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8795, identifier, argumentList, initializer, out var hash);
		if (greenNode != null)
		{
			return (VariableDeclaratorSyntax)greenNode;
		}
		VariableDeclaratorSyntax variableDeclaratorSyntax = new VariableDeclaratorSyntax(SyntaxKind.VariableDeclarator, identifier, argumentList, initializer);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(variableDeclaratorSyntax, hash);
		}
		return variableDeclaratorSyntax;
	}

	public static EqualsValueClauseSyntax EqualsValueClause(SyntaxToken equalsToken, ExpressionSyntax value)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8796, equalsToken, value, out var hash);
		if (greenNode != null)
		{
			return (EqualsValueClauseSyntax)greenNode;
		}
		EqualsValueClauseSyntax equalsValueClauseSyntax = new EqualsValueClauseSyntax(SyntaxKind.EqualsValueClause, equalsToken, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(equalsValueClauseSyntax, hash);
		}
		return equalsValueClauseSyntax;
	}

	public static SingleVariableDesignationSyntax SingleVariableDesignation(SyntaxToken identifier)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8927, identifier, out var hash);
		if (greenNode != null)
		{
			return (SingleVariableDesignationSyntax)greenNode;
		}
		SingleVariableDesignationSyntax singleVariableDesignationSyntax = new SingleVariableDesignationSyntax(SyntaxKind.SingleVariableDesignation, identifier);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(singleVariableDesignationSyntax, hash);
		}
		return singleVariableDesignationSyntax;
	}

	public static DiscardDesignationSyntax DiscardDesignation(SyntaxToken underscoreToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(9014, underscoreToken, out var hash);
		if (greenNode != null)
		{
			return (DiscardDesignationSyntax)greenNode;
		}
		DiscardDesignationSyntax discardDesignationSyntax = new DiscardDesignationSyntax(SyntaxKind.DiscardDesignation, underscoreToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(discardDesignationSyntax, hash);
		}
		return discardDesignationSyntax;
	}

	public static ParenthesizedVariableDesignationSyntax ParenthesizedVariableDesignation(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDesignationSyntax> variables, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8928, openParenToken, variables.Node, closeParenToken, out var hash);
		if (greenNode != null)
		{
			return (ParenthesizedVariableDesignationSyntax)greenNode;
		}
		ParenthesizedVariableDesignationSyntax parenthesizedVariableDesignationSyntax = new ParenthesizedVariableDesignationSyntax(SyntaxKind.ParenthesizedVariableDesignation, openParenToken, variables.Node, closeParenToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(parenthesizedVariableDesignationSyntax, hash);
		}
		return parenthesizedVariableDesignationSyntax;
	}

	public static ExpressionStatementSyntax ExpressionStatement(ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8797, expression, semicolonToken, out var hash);
		if (greenNode != null)
		{
			return (ExpressionStatementSyntax)greenNode;
		}
		ExpressionStatementSyntax expressionStatementSyntax = new ExpressionStatementSyntax(SyntaxKind.ExpressionStatement, expression, semicolonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(expressionStatementSyntax, hash);
		}
		return expressionStatementSyntax;
	}

	public static EmptyStatementSyntax EmptyStatement(SyntaxToken semicolonToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8798, semicolonToken, out var hash);
		if (greenNode != null)
		{
			return (EmptyStatementSyntax)greenNode;
		}
		EmptyStatementSyntax emptyStatementSyntax = new EmptyStatementSyntax(SyntaxKind.EmptyStatement, semicolonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(emptyStatementSyntax, hash);
		}
		return emptyStatementSyntax;
	}

	public static LabeledStatementSyntax LabeledStatement(SyntaxToken identifier, SyntaxToken colonToken, StatementSyntax statement)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8799, identifier, colonToken, statement, out var hash);
		if (greenNode != null)
		{
			return (LabeledStatementSyntax)greenNode;
		}
		LabeledStatementSyntax labeledStatementSyntax = new LabeledStatementSyntax(SyntaxKind.LabeledStatement, identifier, colonToken, statement);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(labeledStatementSyntax, hash);
		}
		return labeledStatementSyntax;
	}

	public static GotoStatementSyntax GotoStatement(SyntaxKind kind, SyntaxToken gotoKeyword, SyntaxToken caseOrDefaultKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (kind - 8800 > (SyntaxKind)2)
		{
			throw new ArgumentException("kind");
		}
		return new GotoStatementSyntax(kind, gotoKeyword, caseOrDefaultKeyword, expression, semicolonToken);
	}

	public static BreakStatementSyntax BreakStatement(SyntaxToken breakKeyword, SyntaxToken semicolonToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8803, breakKeyword, semicolonToken, out var hash);
		if (greenNode != null)
		{
			return (BreakStatementSyntax)greenNode;
		}
		BreakStatementSyntax breakStatementSyntax = new BreakStatementSyntax(SyntaxKind.BreakStatement, breakKeyword, semicolonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(breakStatementSyntax, hash);
		}
		return breakStatementSyntax;
	}

	public static ContinueStatementSyntax ContinueStatement(SyntaxToken continueKeyword, SyntaxToken semicolonToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8804, continueKeyword, semicolonToken, out var hash);
		if (greenNode != null)
		{
			return (ContinueStatementSyntax)greenNode;
		}
		ContinueStatementSyntax continueStatementSyntax = new ContinueStatementSyntax(SyntaxKind.ContinueStatement, continueKeyword, semicolonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(continueStatementSyntax, hash);
		}
		return continueStatementSyntax;
	}

	public static ReturnStatementSyntax ReturnStatement(SyntaxToken returnKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8805, returnKeyword, expression, semicolonToken, out var hash);
		if (greenNode != null)
		{
			return (ReturnStatementSyntax)greenNode;
		}
		ReturnStatementSyntax returnStatementSyntax = new ReturnStatementSyntax(SyntaxKind.ReturnStatement, returnKeyword, expression, semicolonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(returnStatementSyntax, hash);
		}
		return returnStatementSyntax;
	}

	public static ThrowStatementSyntax ThrowStatement(SyntaxToken throwKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8808, throwKeyword, expression, semicolonToken, out var hash);
		if (greenNode != null)
		{
			return (ThrowStatementSyntax)greenNode;
		}
		ThrowStatementSyntax throwStatementSyntax = new ThrowStatementSyntax(SyntaxKind.ThrowStatement, throwKeyword, expression, semicolonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(throwStatementSyntax, hash);
		}
		return throwStatementSyntax;
	}

	public static YieldStatementSyntax YieldStatement(SyntaxKind kind, SyntaxToken yieldKeyword, SyntaxToken returnOrBreakKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (kind - 8806 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		return new YieldStatementSyntax(kind, yieldKeyword, returnOrBreakKeyword, expression, semicolonToken);
	}

	public static WhileStatementSyntax WhileStatement(SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new WhileStatementSyntax(SyntaxKind.WhileStatement, whileKeyword, openParenToken, condition, closeParenToken, statement);
	}

	public static DoStatementSyntax DoStatement(SyntaxToken doKeyword, StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken)
	{
		return new DoStatementSyntax(SyntaxKind.DoStatement, doKeyword, statement, whileKeyword, openParenToken, condition, closeParenToken, semicolonToken);
	}

	public static ForStatementSyntax ForStatement(SyntaxToken forKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> initializers, SyntaxToken firstSemicolonToken, ExpressionSyntax condition, SyntaxToken secondSemicolonToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> incrementors, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new ForStatementSyntax(SyntaxKind.ForStatement, forKeyword, openParenToken, declaration, initializers.Node, firstSemicolonToken, condition, secondSemicolonToken, incrementors.Node, closeParenToken, statement);
	}

	public static ForEachStatementSyntax ForEachStatement(SyntaxToken forEachKeyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new ForEachStatementSyntax(SyntaxKind.ForEachStatement, forEachKeyword, openParenToken, type, identifier, inKeyword, expression, closeParenToken, statement);
	}

	public static ForEachVariableStatementSyntax ForEachVariableStatement(SyntaxToken forEachKeyword, SyntaxToken openParenToken, ExpressionSyntax variable, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new ForEachVariableStatementSyntax(SyntaxKind.ForEachVariableStatement, forEachKeyword, openParenToken, variable, inKeyword, expression, closeParenToken, statement);
	}

	public static UsingStatementSyntax UsingStatement(SyntaxToken usingKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new UsingStatementSyntax(SyntaxKind.UsingStatement, usingKeyword, openParenToken, declaration, expression, closeParenToken, statement);
	}

	public static FixedStatementSyntax FixedStatement(SyntaxToken fixedKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new FixedStatementSyntax(SyntaxKind.FixedStatement, fixedKeyword, openParenToken, declaration, closeParenToken, statement);
	}

	public static CheckedStatementSyntax CheckedStatement(SyntaxKind kind, SyntaxToken keyword, BlockSyntax block)
	{
		if (kind - 8815 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = SyntaxNodeCache.TryGetNode((int)kind, keyword, block, out var hash);
		if (greenNode != null)
		{
			return (CheckedStatementSyntax)greenNode;
		}
		CheckedStatementSyntax checkedStatementSyntax = new CheckedStatementSyntax(kind, keyword, block);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(checkedStatementSyntax, hash);
		}
		return checkedStatementSyntax;
	}

	public static UnsafeStatementSyntax UnsafeStatement(SyntaxToken unsafeKeyword, BlockSyntax block)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8817, unsafeKeyword, block, out var hash);
		if (greenNode != null)
		{
			return (UnsafeStatementSyntax)greenNode;
		}
		UnsafeStatementSyntax unsafeStatementSyntax = new UnsafeStatementSyntax(SyntaxKind.UnsafeStatement, unsafeKeyword, block);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(unsafeStatementSyntax, hash);
		}
		return unsafeStatementSyntax;
	}

	public static LockStatementSyntax LockStatement(SyntaxToken lockKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		return new LockStatementSyntax(SyntaxKind.LockStatement, lockKeyword, openParenToken, expression, closeParenToken, statement);
	}

	public static IfStatementSyntax IfStatement(SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax @else)
	{
		return new IfStatementSyntax(SyntaxKind.IfStatement, ifKeyword, openParenToken, condition, closeParenToken, statement, @else);
	}

	public static ElseClauseSyntax ElseClause(SyntaxToken elseKeyword, StatementSyntax statement)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8820, elseKeyword, statement, out var hash);
		if (greenNode != null)
		{
			return (ElseClauseSyntax)greenNode;
		}
		ElseClauseSyntax elseClauseSyntax = new ElseClauseSyntax(SyntaxKind.ElseClause, elseKeyword, statement);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(elseClauseSyntax, hash);
		}
		return elseClauseSyntax;
	}

	public static SwitchStatementSyntax SwitchStatement(SyntaxToken switchKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SwitchSectionSyntax> sections, SyntaxToken closeBraceToken)
	{
		return new SwitchStatementSyntax(SyntaxKind.SwitchStatement, switchKeyword, openParenToken, expression, closeParenToken, openBraceToken, sections.Node, closeBraceToken);
	}

	public static SwitchSectionSyntax SwitchSection(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SwitchLabelSyntax> labels, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax> statements)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8822, labels.Node, statements.Node, out var hash);
		if (greenNode != null)
		{
			return (SwitchSectionSyntax)greenNode;
		}
		SwitchSectionSyntax switchSectionSyntax = new SwitchSectionSyntax(SyntaxKind.SwitchSection, labels.Node, statements.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(switchSectionSyntax, hash);
		}
		return switchSectionSyntax;
	}

	public static CasePatternSwitchLabelSyntax CasePatternSwitchLabel(SyntaxToken keyword, PatternSyntax pattern, WhenClauseSyntax whenClause, SyntaxToken colonToken)
	{
		return new CasePatternSwitchLabelSyntax(SyntaxKind.CasePatternSwitchLabel, keyword, pattern, whenClause, colonToken);
	}

	public static CaseSwitchLabelSyntax CaseSwitchLabel(SyntaxToken keyword, ExpressionSyntax value, SyntaxToken colonToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8823, keyword, value, colonToken, out var hash);
		if (greenNode != null)
		{
			return (CaseSwitchLabelSyntax)greenNode;
		}
		CaseSwitchLabelSyntax caseSwitchLabelSyntax = new CaseSwitchLabelSyntax(SyntaxKind.CaseSwitchLabel, keyword, value, colonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(caseSwitchLabelSyntax, hash);
		}
		return caseSwitchLabelSyntax;
	}

	public static DefaultSwitchLabelSyntax DefaultSwitchLabel(SyntaxToken keyword, SyntaxToken colonToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8824, keyword, colonToken, out var hash);
		if (greenNode != null)
		{
			return (DefaultSwitchLabelSyntax)greenNode;
		}
		DefaultSwitchLabelSyntax defaultSwitchLabelSyntax = new DefaultSwitchLabelSyntax(SyntaxKind.DefaultSwitchLabel, keyword, colonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(defaultSwitchLabelSyntax, hash);
		}
		return defaultSwitchLabelSyntax;
	}

	public static TryStatementSyntax TryStatement(SyntaxToken tryKeyword, BlockSyntax block, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CatchClauseSyntax> catches, FinallyClauseSyntax @finally)
	{
		return new TryStatementSyntax(SyntaxKind.TryStatement, tryKeyword, block, catches.Node, @finally);
	}

	public static CatchClauseSyntax CatchClause(SyntaxToken catchKeyword, CatchDeclarationSyntax declaration, CatchFilterClauseSyntax filter, BlockSyntax block)
	{
		return new CatchClauseSyntax(SyntaxKind.CatchClause, catchKeyword, declaration, filter, block);
	}

	public static CatchDeclarationSyntax CatchDeclaration(SyntaxToken openParenToken, TypeSyntax type, SyntaxToken identifier, SyntaxToken closeParenToken)
	{
		return new CatchDeclarationSyntax(SyntaxKind.CatchDeclaration, openParenToken, type, identifier, closeParenToken);
	}

	public static CatchFilterClauseSyntax CatchFilterClause(SyntaxToken whenKeyword, SyntaxToken openParenToken, ExpressionSyntax filterExpression, SyntaxToken closeParenToken)
	{
		return new CatchFilterClauseSyntax(SyntaxKind.CatchFilterClause, whenKeyword, openParenToken, filterExpression, closeParenToken);
	}

	public static FinallyClauseSyntax FinallyClause(SyntaxToken finallyKeyword, BlockSyntax block)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8829, finallyKeyword, block, out var hash);
		if (greenNode != null)
		{
			return (FinallyClauseSyntax)greenNode;
		}
		FinallyClauseSyntax finallyClauseSyntax = new FinallyClauseSyntax(SyntaxKind.FinallyClause, finallyKeyword, block);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(finallyClauseSyntax, hash);
		}
		return finallyClauseSyntax;
	}

	public static CompilationUnitSyntax CompilationUnit(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax> externs, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax> usings, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken endOfFileToken)
	{
		return new CompilationUnitSyntax(SyntaxKind.CompilationUnit, externs.Node, usings.Node, attributeLists.Node, members.Node, endOfFileToken);
	}

	public static ExternAliasDirectiveSyntax ExternAliasDirective(SyntaxToken externKeyword, SyntaxToken aliasKeyword, SyntaxToken identifier, SyntaxToken semicolonToken)
	{
		return new ExternAliasDirectiveSyntax(SyntaxKind.ExternAliasDirective, externKeyword, aliasKeyword, identifier, semicolonToken);
	}

	public static UsingDirectiveSyntax UsingDirective(SyntaxToken usingKeyword, SyntaxToken staticKeyword, NameEqualsSyntax alias, NameSyntax name, SyntaxToken semicolonToken)
	{
		return new UsingDirectiveSyntax(SyntaxKind.UsingDirective, usingKeyword, staticKeyword, alias, name, semicolonToken);
	}

	public static NamespaceDeclarationSyntax NamespaceDeclaration(SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax> externs, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax> usings, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		return new NamespaceDeclarationSyntax(SyntaxKind.NamespaceDeclaration, namespaceKeyword, name, openBraceToken, externs.Node, usings.Node, members.Node, closeBraceToken, semicolonToken);
	}

	public static AttributeListSyntax AttributeList(SyntaxToken openBracketToken, AttributeTargetSpecifierSyntax target, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AttributeSyntax> attributes, SyntaxToken closeBracketToken)
	{
		return new AttributeListSyntax(SyntaxKind.AttributeList, openBracketToken, target, attributes.Node, closeBracketToken);
	}

	public static AttributeTargetSpecifierSyntax AttributeTargetSpecifier(SyntaxToken identifier, SyntaxToken colonToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8848, identifier, colonToken, out var hash);
		if (greenNode != null)
		{
			return (AttributeTargetSpecifierSyntax)greenNode;
		}
		AttributeTargetSpecifierSyntax attributeTargetSpecifierSyntax = new AttributeTargetSpecifierSyntax(SyntaxKind.AttributeTargetSpecifier, identifier, colonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(attributeTargetSpecifierSyntax, hash);
		}
		return attributeTargetSpecifierSyntax;
	}

	public static AttributeSyntax Attribute(NameSyntax name, AttributeArgumentListSyntax argumentList)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8849, name, argumentList, out var hash);
		if (greenNode != null)
		{
			return (AttributeSyntax)greenNode;
		}
		AttributeSyntax attributeSyntax = new AttributeSyntax(SyntaxKind.Attribute, name, argumentList);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(attributeSyntax, hash);
		}
		return attributeSyntax;
	}

	public static AttributeArgumentListSyntax AttributeArgumentList(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AttributeArgumentSyntax> arguments, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8850, openParenToken, arguments.Node, closeParenToken, out var hash);
		if (greenNode != null)
		{
			return (AttributeArgumentListSyntax)greenNode;
		}
		AttributeArgumentListSyntax attributeArgumentListSyntax = new AttributeArgumentListSyntax(SyntaxKind.AttributeArgumentList, openParenToken, arguments.Node, closeParenToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(attributeArgumentListSyntax, hash);
		}
		return attributeArgumentListSyntax;
	}

	public static AttributeArgumentSyntax AttributeArgument(NameEqualsSyntax nameEquals, NameColonSyntax nameColon, ExpressionSyntax expression)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8851, nameEquals, nameColon, expression, out var hash);
		if (greenNode != null)
		{
			return (AttributeArgumentSyntax)greenNode;
		}
		AttributeArgumentSyntax attributeArgumentSyntax = new AttributeArgumentSyntax(SyntaxKind.AttributeArgument, nameEquals, nameColon, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(attributeArgumentSyntax, hash);
		}
		return attributeArgumentSyntax;
	}

	public static NameEqualsSyntax NameEquals(IdentifierNameSyntax name, SyntaxToken equalsToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8852, name, equalsToken, out var hash);
		if (greenNode != null)
		{
			return (NameEqualsSyntax)greenNode;
		}
		NameEqualsSyntax nameEqualsSyntax = new NameEqualsSyntax(SyntaxKind.NameEquals, name, equalsToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(nameEqualsSyntax, hash);
		}
		return nameEqualsSyntax;
	}

	public static TypeParameterListSyntax TypeParameterList(SyntaxToken lessThanToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeParameterSyntax> parameters, SyntaxToken greaterThanToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8909, lessThanToken, parameters.Node, greaterThanToken, out var hash);
		if (greenNode != null)
		{
			return (TypeParameterListSyntax)greenNode;
		}
		TypeParameterListSyntax typeParameterListSyntax = new TypeParameterListSyntax(SyntaxKind.TypeParameterList, lessThanToken, parameters.Node, greaterThanToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(typeParameterListSyntax, hash);
		}
		return typeParameterListSyntax;
	}

	public static TypeParameterSyntax TypeParameter(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken varianceKeyword, SyntaxToken identifier)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8910, attributeLists.Node, varianceKeyword, identifier, out var hash);
		if (greenNode != null)
		{
			return (TypeParameterSyntax)greenNode;
		}
		TypeParameterSyntax typeParameterSyntax = new TypeParameterSyntax(SyntaxKind.TypeParameter, attributeLists.Node, varianceKeyword, identifier);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(typeParameterSyntax, hash);
		}
		return typeParameterSyntax;
	}

	public static ClassDeclarationSyntax ClassDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, BaseListSyntax baseList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		return new ClassDeclarationSyntax(SyntaxKind.ClassDeclaration, attributeLists.Node, modifiers.Node, keyword, identifier, typeParameterList, baseList, constraintClauses.Node, openBraceToken, members.Node, closeBraceToken, semicolonToken);
	}

	public static StructDeclarationSyntax StructDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, BaseListSyntax baseList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		return new StructDeclarationSyntax(SyntaxKind.StructDeclaration, attributeLists.Node, modifiers.Node, keyword, identifier, typeParameterList, baseList, constraintClauses.Node, openBraceToken, members.Node, closeBraceToken, semicolonToken);
	}

	public static InterfaceDeclarationSyntax InterfaceDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, BaseListSyntax baseList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		return new InterfaceDeclarationSyntax(SyntaxKind.InterfaceDeclaration, attributeLists.Node, modifiers.Node, keyword, identifier, typeParameterList, baseList, constraintClauses.Node, openBraceToken, members.Node, closeBraceToken, semicolonToken);
	}

	public static EnumDeclarationSyntax EnumDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken enumKeyword, SyntaxToken identifier, BaseListSyntax baseList, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<EnumMemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		return new EnumDeclarationSyntax(SyntaxKind.EnumDeclaration, attributeLists.Node, modifiers.Node, enumKeyword, identifier, baseList, openBraceToken, members.Node, closeBraceToken, semicolonToken);
	}

	public static DelegateDeclarationSyntax DelegateDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken delegateKeyword, TypeSyntax returnType, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken semicolonToken)
	{
		return new DelegateDeclarationSyntax(SyntaxKind.DelegateDeclaration, attributeLists.Node, modifiers.Node, delegateKeyword, returnType, identifier, typeParameterList, parameterList, constraintClauses.Node, semicolonToken);
	}

	public static EnumMemberDeclarationSyntax EnumMemberDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken identifier, EqualsValueClauseSyntax equalsValue)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8872, attributeLists.Node, identifier, equalsValue, out var hash);
		if (greenNode != null)
		{
			return (EnumMemberDeclarationSyntax)greenNode;
		}
		EnumMemberDeclarationSyntax enumMemberDeclarationSyntax = new EnumMemberDeclarationSyntax(SyntaxKind.EnumMemberDeclaration, attributeLists.Node, identifier, equalsValue);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(enumMemberDeclarationSyntax, hash);
		}
		return enumMemberDeclarationSyntax;
	}

	public static BaseListSyntax BaseList(SyntaxToken colonToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<BaseTypeSyntax> types)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8864, colonToken, types.Node, out var hash);
		if (greenNode != null)
		{
			return (BaseListSyntax)greenNode;
		}
		BaseListSyntax baseListSyntax = new BaseListSyntax(SyntaxKind.BaseList, colonToken, types.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(baseListSyntax, hash);
		}
		return baseListSyntax;
	}

	public static SimpleBaseTypeSyntax SimpleBaseType(TypeSyntax type)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8865, type, out var hash);
		if (greenNode != null)
		{
			return (SimpleBaseTypeSyntax)greenNode;
		}
		SimpleBaseTypeSyntax simpleBaseTypeSyntax = new SimpleBaseTypeSyntax(SyntaxKind.SimpleBaseType, type);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(simpleBaseTypeSyntax, hash);
		}
		return simpleBaseTypeSyntax;
	}

	public static TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(SyntaxToken whereKeyword, IdentifierNameSyntax name, SyntaxToken colonToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints)
	{
		return new TypeParameterConstraintClauseSyntax(SyntaxKind.TypeParameterConstraintClause, whereKeyword, name, colonToken, constraints.Node);
	}

	public static ConstructorConstraintSyntax ConstructorConstraint(SyntaxToken newKeyword, SyntaxToken openParenToken, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8867, newKeyword, openParenToken, closeParenToken, out var hash);
		if (greenNode != null)
		{
			return (ConstructorConstraintSyntax)greenNode;
		}
		ConstructorConstraintSyntax constructorConstraintSyntax = new ConstructorConstraintSyntax(SyntaxKind.ConstructorConstraint, newKeyword, openParenToken, closeParenToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(constructorConstraintSyntax, hash);
		}
		return constructorConstraintSyntax;
	}

	public static ClassOrStructConstraintSyntax ClassOrStructConstraint(SyntaxKind kind, SyntaxToken classOrStructKeyword)
	{
		if (kind - 8868 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = SyntaxNodeCache.TryGetNode((int)kind, classOrStructKeyword, out var hash);
		if (greenNode != null)
		{
			return (ClassOrStructConstraintSyntax)greenNode;
		}
		ClassOrStructConstraintSyntax classOrStructConstraintSyntax = new ClassOrStructConstraintSyntax(kind, classOrStructKeyword);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(classOrStructConstraintSyntax, hash);
		}
		return classOrStructConstraintSyntax;
	}

	public static TypeConstraintSyntax TypeConstraint(TypeSyntax type)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8870, type, out var hash);
		if (greenNode != null)
		{
			return (TypeConstraintSyntax)greenNode;
		}
		TypeConstraintSyntax typeConstraintSyntax = new TypeConstraintSyntax(SyntaxKind.TypeConstraint, type);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(typeConstraintSyntax, hash);
		}
		return typeConstraintSyntax;
	}

	public static FieldDeclarationSyntax FieldDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		return new FieldDeclarationSyntax(SyntaxKind.FieldDeclaration, attributeLists.Node, modifiers.Node, declaration, semicolonToken);
	}

	public static EventFieldDeclarationSyntax EventFieldDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken eventKeyword, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		return new EventFieldDeclarationSyntax(SyntaxKind.EventFieldDeclaration, attributeLists.Node, modifiers.Node, eventKeyword, declaration, semicolonToken);
	}

	public static ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier(NameSyntax name, SyntaxToken dotToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8871, name, dotToken, out var hash);
		if (greenNode != null)
		{
			return (ExplicitInterfaceSpecifierSyntax)greenNode;
		}
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax = new ExplicitInterfaceSpecifierSyntax(SyntaxKind.ExplicitInterfaceSpecifier, name, dotToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(explicitInterfaceSpecifierSyntax, hash);
		}
		return explicitInterfaceSpecifierSyntax;
	}

	public static MethodDeclarationSyntax MethodDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax returnType, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new MethodDeclarationSyntax(SyntaxKind.MethodDeclaration, attributeLists.Node, modifiers.Node, returnType, explicitInterfaceSpecifier, identifier, typeParameterList, parameterList, constraintClauses.Node, body, expressionBody, semicolonToken);
	}

	public static OperatorDeclarationSyntax OperatorDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax returnType, SyntaxToken operatorKeyword, SyntaxToken operatorToken, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new OperatorDeclarationSyntax(SyntaxKind.OperatorDeclaration, attributeLists.Node, modifiers.Node, returnType, operatorKeyword, operatorToken, parameterList, body, expressionBody, semicolonToken);
	}

	public static ConversionOperatorDeclarationSyntax ConversionOperatorDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, TypeSyntax type, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new ConversionOperatorDeclarationSyntax(SyntaxKind.ConversionOperatorDeclaration, attributeLists.Node, modifiers.Node, implicitOrExplicitKeyword, operatorKeyword, type, parameterList, body, expressionBody, semicolonToken);
	}

	public static ConstructorDeclarationSyntax ConstructorDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken identifier, ParameterListSyntax parameterList, ConstructorInitializerSyntax initializer, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new ConstructorDeclarationSyntax(SyntaxKind.ConstructorDeclaration, attributeLists.Node, modifiers.Node, identifier, parameterList, initializer, body, expressionBody, semicolonToken);
	}

	public static ConstructorInitializerSyntax ConstructorInitializer(SyntaxKind kind, SyntaxToken colonToken, SyntaxToken thisOrBaseKeyword, ArgumentListSyntax argumentList)
	{
		if (kind - 8889 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		GreenNode greenNode = SyntaxNodeCache.TryGetNode((int)kind, colonToken, thisOrBaseKeyword, argumentList, out var hash);
		if (greenNode != null)
		{
			return (ConstructorInitializerSyntax)greenNode;
		}
		ConstructorInitializerSyntax constructorInitializerSyntax = new ConstructorInitializerSyntax(kind, colonToken, thisOrBaseKeyword, argumentList);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(constructorInitializerSyntax, hash);
		}
		return constructorInitializerSyntax;
	}

	public static DestructorDeclarationSyntax DestructorDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken tildeToken, SyntaxToken identifier, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new DestructorDeclarationSyntax(SyntaxKind.DestructorDeclaration, attributeLists.Node, modifiers.Node, tildeToken, identifier, parameterList, body, expressionBody, semicolonToken);
	}

	public static PropertyDeclarationSyntax PropertyDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList, ArrowExpressionClauseSyntax expressionBody, EqualsValueClauseSyntax initializer, SyntaxToken semicolonToken)
	{
		return new PropertyDeclarationSyntax(SyntaxKind.PropertyDeclaration, attributeLists.Node, modifiers.Node, type, explicitInterfaceSpecifier, identifier, accessorList, expressionBody, initializer, semicolonToken);
	}

	public static ArrowExpressionClauseSyntax ArrowExpressionClause(SyntaxToken arrowToken, ExpressionSyntax expression)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8917, arrowToken, expression, out var hash);
		if (greenNode != null)
		{
			return (ArrowExpressionClauseSyntax)greenNode;
		}
		ArrowExpressionClauseSyntax arrowExpressionClauseSyntax = new ArrowExpressionClauseSyntax(SyntaxKind.ArrowExpressionClause, arrowToken, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(arrowExpressionClauseSyntax, hash);
		}
		return arrowExpressionClauseSyntax;
	}

	public static EventDeclarationSyntax EventDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken eventKeyword, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList)
	{
		return new EventDeclarationSyntax(SyntaxKind.EventDeclaration, attributeLists.Node, modifiers.Node, eventKeyword, type, explicitInterfaceSpecifier, identifier, accessorList);
	}

	public static IndexerDeclarationSyntax IndexerDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken thisKeyword, BracketedParameterListSyntax parameterList, AccessorListSyntax accessorList, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return new IndexerDeclarationSyntax(SyntaxKind.IndexerDeclaration, attributeLists.Node, modifiers.Node, type, explicitInterfaceSpecifier, thisKeyword, parameterList, accessorList, expressionBody, semicolonToken);
	}

	public static AccessorListSyntax AccessorList(SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AccessorDeclarationSyntax> accessors, SyntaxToken closeBraceToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8895, openBraceToken, accessors.Node, closeBraceToken, out var hash);
		if (greenNode != null)
		{
			return (AccessorListSyntax)greenNode;
		}
		AccessorListSyntax accessorListSyntax = new AccessorListSyntax(SyntaxKind.AccessorList, openBraceToken, accessors.Node, closeBraceToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(accessorListSyntax, hash);
		}
		return accessorListSyntax;
	}

	public static AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (kind - 8896 > (SyntaxKind)4)
		{
			throw new ArgumentException("kind");
		}
		return new AccessorDeclarationSyntax(kind, attributeLists.Node, modifiers.Node, keyword, body, expressionBody, semicolonToken);
	}

	public static ParameterListSyntax ParameterList(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8906, openParenToken, parameters.Node, closeParenToken, out var hash);
		if (greenNode != null)
		{
			return (ParameterListSyntax)greenNode;
		}
		ParameterListSyntax parameterListSyntax = new ParameterListSyntax(SyntaxKind.ParameterList, openParenToken, parameters.Node, closeParenToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(parameterListSyntax, hash);
		}
		return parameterListSyntax;
	}

	public static BracketedParameterListSyntax BracketedParameterList(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closeBracketToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8907, openBracketToken, parameters.Node, closeBracketToken, out var hash);
		if (greenNode != null)
		{
			return (BracketedParameterListSyntax)greenNode;
		}
		BracketedParameterListSyntax bracketedParameterListSyntax = new BracketedParameterListSyntax(SyntaxKind.BracketedParameterList, openBracketToken, parameters.Node, closeBracketToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(bracketedParameterListSyntax, hash);
		}
		return bracketedParameterListSyntax;
	}

	public static ParameterSyntax Parameter(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax @default)
	{
		return new ParameterSyntax(SyntaxKind.Parameter, attributeLists.Node, modifiers.Node, type, identifier, @default);
	}

	public static IncompleteMemberSyntax IncompleteMember(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type)
	{
		return new IncompleteMemberSyntax(SyntaxKind.IncompleteMember, attributeLists.Node, modifiers.Node, type);
	}

	public static SkippedTokensTriviaSyntax SkippedTokensTrivia(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> tokens)
	{
		return new SkippedTokensTriviaSyntax(SyntaxKind.SkippedTokensTrivia, tokens.Node);
	}

	public static DocumentationCommentTriviaSyntax DocumentationCommentTrivia(SyntaxKind kind, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlNodeSyntax> content, SyntaxToken endOfComment)
	{
		if (kind - 8544 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		return new DocumentationCommentTriviaSyntax(kind, content.Node, endOfComment);
	}

	public static TypeCrefSyntax TypeCref(TypeSyntax type)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8597, type, out var hash);
		if (greenNode != null)
		{
			return (TypeCrefSyntax)greenNode;
		}
		TypeCrefSyntax typeCrefSyntax = new TypeCrefSyntax(SyntaxKind.TypeCref, type);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(typeCrefSyntax, hash);
		}
		return typeCrefSyntax;
	}

	public static QualifiedCrefSyntax QualifiedCref(TypeSyntax container, SyntaxToken dotToken, MemberCrefSyntax member)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8598, container, dotToken, member, out var hash);
		if (greenNode != null)
		{
			return (QualifiedCrefSyntax)greenNode;
		}
		QualifiedCrefSyntax qualifiedCrefSyntax = new QualifiedCrefSyntax(SyntaxKind.QualifiedCref, container, dotToken, member);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(qualifiedCrefSyntax, hash);
		}
		return qualifiedCrefSyntax;
	}

	public static NameMemberCrefSyntax NameMemberCref(TypeSyntax name, CrefParameterListSyntax parameters)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8599, name, parameters, out var hash);
		if (greenNode != null)
		{
			return (NameMemberCrefSyntax)greenNode;
		}
		NameMemberCrefSyntax nameMemberCrefSyntax = new NameMemberCrefSyntax(SyntaxKind.NameMemberCref, name, parameters);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(nameMemberCrefSyntax, hash);
		}
		return nameMemberCrefSyntax;
	}

	public static IndexerMemberCrefSyntax IndexerMemberCref(SyntaxToken thisKeyword, CrefBracketedParameterListSyntax parameters)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8600, thisKeyword, parameters, out var hash);
		if (greenNode != null)
		{
			return (IndexerMemberCrefSyntax)greenNode;
		}
		IndexerMemberCrefSyntax indexerMemberCrefSyntax = new IndexerMemberCrefSyntax(SyntaxKind.IndexerMemberCref, thisKeyword, parameters);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(indexerMemberCrefSyntax, hash);
		}
		return indexerMemberCrefSyntax;
	}

	public static OperatorMemberCrefSyntax OperatorMemberCref(SyntaxToken operatorKeyword, SyntaxToken operatorToken, CrefParameterListSyntax parameters)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8601, operatorKeyword, operatorToken, parameters, out var hash);
		if (greenNode != null)
		{
			return (OperatorMemberCrefSyntax)greenNode;
		}
		OperatorMemberCrefSyntax operatorMemberCrefSyntax = new OperatorMemberCrefSyntax(SyntaxKind.OperatorMemberCref, operatorKeyword, operatorToken, parameters);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(operatorMemberCrefSyntax, hash);
		}
		return operatorMemberCrefSyntax;
	}

	public static ConversionOperatorMemberCrefSyntax ConversionOperatorMemberCref(SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, TypeSyntax type, CrefParameterListSyntax parameters)
	{
		return new ConversionOperatorMemberCrefSyntax(SyntaxKind.ConversionOperatorMemberCref, implicitOrExplicitKeyword, operatorKeyword, type, parameters);
	}

	public static CrefParameterListSyntax CrefParameterList(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CrefParameterSyntax> parameters, SyntaxToken closeParenToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8603, openParenToken, parameters.Node, closeParenToken, out var hash);
		if (greenNode != null)
		{
			return (CrefParameterListSyntax)greenNode;
		}
		CrefParameterListSyntax crefParameterListSyntax = new CrefParameterListSyntax(SyntaxKind.CrefParameterList, openParenToken, parameters.Node, closeParenToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(crefParameterListSyntax, hash);
		}
		return crefParameterListSyntax;
	}

	public static CrefBracketedParameterListSyntax CrefBracketedParameterList(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CrefParameterSyntax> parameters, SyntaxToken closeBracketToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8604, openBracketToken, parameters.Node, closeBracketToken, out var hash);
		if (greenNode != null)
		{
			return (CrefBracketedParameterListSyntax)greenNode;
		}
		CrefBracketedParameterListSyntax crefBracketedParameterListSyntax = new CrefBracketedParameterListSyntax(SyntaxKind.CrefBracketedParameterList, openBracketToken, parameters.Node, closeBracketToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(crefBracketedParameterListSyntax, hash);
		}
		return crefBracketedParameterListSyntax;
	}

	public static CrefParameterSyntax CrefParameter(SyntaxToken refKindKeyword, TypeSyntax type)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8605, refKindKeyword, type, out var hash);
		if (greenNode != null)
		{
			return (CrefParameterSyntax)greenNode;
		}
		CrefParameterSyntax crefParameterSyntax = new CrefParameterSyntax(SyntaxKind.CrefParameter, refKindKeyword, type);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(crefParameterSyntax, hash);
		}
		return crefParameterSyntax;
	}

	public static XmlElementSyntax XmlElement(XmlElementStartTagSyntax startTag, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlNodeSyntax> content, XmlElementEndTagSyntax endTag)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8574, startTag, content.Node, endTag, out var hash);
		if (greenNode != null)
		{
			return (XmlElementSyntax)greenNode;
		}
		XmlElementSyntax xmlElementSyntax = new XmlElementSyntax(SyntaxKind.XmlElement, startTag, content.Node, endTag);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlElementSyntax, hash);
		}
		return xmlElementSyntax;
	}

	public static XmlElementStartTagSyntax XmlElementStartTag(SyntaxToken lessThanToken, XmlNameSyntax name, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlAttributeSyntax> attributes, SyntaxToken greaterThanToken)
	{
		return new XmlElementStartTagSyntax(SyntaxKind.XmlElementStartTag, lessThanToken, name, attributes.Node, greaterThanToken);
	}

	public static XmlElementEndTagSyntax XmlElementEndTag(SyntaxToken lessThanSlashToken, XmlNameSyntax name, SyntaxToken greaterThanToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8576, lessThanSlashToken, name, greaterThanToken, out var hash);
		if (greenNode != null)
		{
			return (XmlElementEndTagSyntax)greenNode;
		}
		XmlElementEndTagSyntax xmlElementEndTagSyntax = new XmlElementEndTagSyntax(SyntaxKind.XmlElementEndTag, lessThanSlashToken, name, greaterThanToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlElementEndTagSyntax, hash);
		}
		return xmlElementEndTagSyntax;
	}

	public static XmlEmptyElementSyntax XmlEmptyElement(SyntaxToken lessThanToken, XmlNameSyntax name, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlAttributeSyntax> attributes, SyntaxToken slashGreaterThanToken)
	{
		return new XmlEmptyElementSyntax(SyntaxKind.XmlEmptyElement, lessThanToken, name, attributes.Node, slashGreaterThanToken);
	}

	public static XmlNameSyntax XmlName(XmlPrefixSyntax prefix, SyntaxToken localName)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8581, prefix, localName, out var hash);
		if (greenNode != null)
		{
			return (XmlNameSyntax)greenNode;
		}
		XmlNameSyntax xmlNameSyntax = new XmlNameSyntax(SyntaxKind.XmlName, prefix, localName);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlNameSyntax, hash);
		}
		return xmlNameSyntax;
	}

	public static XmlPrefixSyntax XmlPrefix(SyntaxToken prefix, SyntaxToken colonToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8582, prefix, colonToken, out var hash);
		if (greenNode != null)
		{
			return (XmlPrefixSyntax)greenNode;
		}
		XmlPrefixSyntax xmlPrefixSyntax = new XmlPrefixSyntax(SyntaxKind.XmlPrefix, prefix, colonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlPrefixSyntax, hash);
		}
		return xmlPrefixSyntax;
	}

	public static XmlTextAttributeSyntax XmlTextAttribute(XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens, SyntaxToken endQuoteToken)
	{
		return new XmlTextAttributeSyntax(SyntaxKind.XmlTextAttribute, name, equalsToken, startQuoteToken, textTokens.Node, endQuoteToken);
	}

	public static XmlCrefAttributeSyntax XmlCrefAttribute(XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, CrefSyntax cref, SyntaxToken endQuoteToken)
	{
		return new XmlCrefAttributeSyntax(SyntaxKind.XmlCrefAttribute, name, equalsToken, startQuoteToken, cref, endQuoteToken);
	}

	public static XmlNameAttributeSyntax XmlNameAttribute(XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, IdentifierNameSyntax identifier, SyntaxToken endQuoteToken)
	{
		return new XmlNameAttributeSyntax(SyntaxKind.XmlNameAttribute, name, equalsToken, startQuoteToken, identifier, endQuoteToken);
	}

	public static XmlTextSyntax XmlText(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8583, textTokens.Node, out var hash);
		if (greenNode != null)
		{
			return (XmlTextSyntax)greenNode;
		}
		XmlTextSyntax xmlTextSyntax = new XmlTextSyntax(SyntaxKind.XmlText, textTokens.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlTextSyntax, hash);
		}
		return xmlTextSyntax;
	}

	public static XmlCDataSectionSyntax XmlCDataSection(SyntaxToken startCDataToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens, SyntaxToken endCDataToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8584, startCDataToken, textTokens.Node, endCDataToken, out var hash);
		if (greenNode != null)
		{
			return (XmlCDataSectionSyntax)greenNode;
		}
		XmlCDataSectionSyntax xmlCDataSectionSyntax = new XmlCDataSectionSyntax(SyntaxKind.XmlCDataSection, startCDataToken, textTokens.Node, endCDataToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlCDataSectionSyntax, hash);
		}
		return xmlCDataSectionSyntax;
	}

	public static XmlProcessingInstructionSyntax XmlProcessingInstruction(SyntaxToken startProcessingInstructionToken, XmlNameSyntax name, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens, SyntaxToken endProcessingInstructionToken)
	{
		return new XmlProcessingInstructionSyntax(SyntaxKind.XmlProcessingInstruction, startProcessingInstructionToken, name, textTokens.Node, endProcessingInstructionToken);
	}

	public static XmlCommentSyntax XmlComment(SyntaxToken lessThanExclamationMinusMinusToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens, SyntaxToken minusMinusGreaterThanToken)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(8585, lessThanExclamationMinusMinusToken, textTokens.Node, minusMinusGreaterThanToken, out var hash);
		if (greenNode != null)
		{
			return (XmlCommentSyntax)greenNode;
		}
		XmlCommentSyntax xmlCommentSyntax = new XmlCommentSyntax(SyntaxKind.XmlComment, lessThanExclamationMinusMinusToken, textTokens.Node, minusMinusGreaterThanToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(xmlCommentSyntax, hash);
		}
		return xmlCommentSyntax;
	}

	public static IfDirectiveTriviaSyntax IfDirectiveTrivia(SyntaxToken hashToken, SyntaxToken ifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
	{
		return new IfDirectiveTriviaSyntax(SyntaxKind.IfDirectiveTrivia, hashToken, ifKeyword, condition, endOfDirectiveToken, isActive, branchTaken, conditionValue);
	}

	public static ElifDirectiveTriviaSyntax ElifDirectiveTrivia(SyntaxToken hashToken, SyntaxToken elifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
	{
		return new ElifDirectiveTriviaSyntax(SyntaxKind.ElifDirectiveTrivia, hashToken, elifKeyword, condition, endOfDirectiveToken, isActive, branchTaken, conditionValue);
	}

	public static ElseDirectiveTriviaSyntax ElseDirectiveTrivia(SyntaxToken hashToken, SyntaxToken elseKeyword, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken)
	{
		return new ElseDirectiveTriviaSyntax(SyntaxKind.ElseDirectiveTrivia, hashToken, elseKeyword, endOfDirectiveToken, isActive, branchTaken);
	}

	public static EndIfDirectiveTriviaSyntax EndIfDirectiveTrivia(SyntaxToken hashToken, SyntaxToken endIfKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new EndIfDirectiveTriviaSyntax(SyntaxKind.EndIfDirectiveTrivia, hashToken, endIfKeyword, endOfDirectiveToken, isActive);
	}

	public static RegionDirectiveTriviaSyntax RegionDirectiveTrivia(SyntaxToken hashToken, SyntaxToken regionKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new RegionDirectiveTriviaSyntax(SyntaxKind.RegionDirectiveTrivia, hashToken, regionKeyword, endOfDirectiveToken, isActive);
	}

	public static EndRegionDirectiveTriviaSyntax EndRegionDirectiveTrivia(SyntaxToken hashToken, SyntaxToken endRegionKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new EndRegionDirectiveTriviaSyntax(SyntaxKind.EndRegionDirectiveTrivia, hashToken, endRegionKeyword, endOfDirectiveToken, isActive);
	}

	public static ErrorDirectiveTriviaSyntax ErrorDirectiveTrivia(SyntaxToken hashToken, SyntaxToken errorKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new ErrorDirectiveTriviaSyntax(SyntaxKind.ErrorDirectiveTrivia, hashToken, errorKeyword, endOfDirectiveToken, isActive);
	}

	public static WarningDirectiveTriviaSyntax WarningDirectiveTrivia(SyntaxToken hashToken, SyntaxToken warningKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new WarningDirectiveTriviaSyntax(SyntaxKind.WarningDirectiveTrivia, hashToken, warningKeyword, endOfDirectiveToken, isActive);
	}

	public static BadDirectiveTriviaSyntax BadDirectiveTrivia(SyntaxToken hashToken, SyntaxToken identifier, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new BadDirectiveTriviaSyntax(SyntaxKind.BadDirectiveTrivia, hashToken, identifier, endOfDirectiveToken, isActive);
	}

	public static DefineDirectiveTriviaSyntax DefineDirectiveTrivia(SyntaxToken hashToken, SyntaxToken defineKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new DefineDirectiveTriviaSyntax(SyntaxKind.DefineDirectiveTrivia, hashToken, defineKeyword, name, endOfDirectiveToken, isActive);
	}

	public static UndefDirectiveTriviaSyntax UndefDirectiveTrivia(SyntaxToken hashToken, SyntaxToken undefKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new UndefDirectiveTriviaSyntax(SyntaxKind.UndefDirectiveTrivia, hashToken, undefKeyword, name, endOfDirectiveToken, isActive);
	}

	public static LineDirectiveTriviaSyntax LineDirectiveTrivia(SyntaxToken hashToken, SyntaxToken lineKeyword, SyntaxToken line, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new LineDirectiveTriviaSyntax(SyntaxKind.LineDirectiveTrivia, hashToken, lineKeyword, line, file, endOfDirectiveToken, isActive);
	}

	public static PragmaWarningDirectiveTriviaSyntax PragmaWarningDirectiveTrivia(SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken warningKeyword, SyntaxToken disableOrRestoreKeyword, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> errorCodes, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new PragmaWarningDirectiveTriviaSyntax(SyntaxKind.PragmaWarningDirectiveTrivia, hashToken, pragmaKeyword, warningKeyword, disableOrRestoreKeyword, errorCodes.Node, endOfDirectiveToken, isActive);
	}

	public static PragmaChecksumDirectiveTriviaSyntax PragmaChecksumDirectiveTrivia(SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken checksumKeyword, SyntaxToken file, SyntaxToken guid, SyntaxToken bytes, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new PragmaChecksumDirectiveTriviaSyntax(SyntaxKind.PragmaChecksumDirectiveTrivia, hashToken, pragmaKeyword, checksumKeyword, file, guid, bytes, endOfDirectiveToken, isActive);
	}

	public static ReferenceDirectiveTriviaSyntax ReferenceDirectiveTrivia(SyntaxToken hashToken, SyntaxToken referenceKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new ReferenceDirectiveTriviaSyntax(SyntaxKind.ReferenceDirectiveTrivia, hashToken, referenceKeyword, file, endOfDirectiveToken, isActive);
	}

	public static LoadDirectiveTriviaSyntax LoadDirectiveTrivia(SyntaxToken hashToken, SyntaxToken loadKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new LoadDirectiveTriviaSyntax(SyntaxKind.LoadDirectiveTrivia, hashToken, loadKeyword, file, endOfDirectiveToken, isActive);
	}

	public static ShebangDirectiveTriviaSyntax ShebangDirectiveTrivia(SyntaxToken hashToken, SyntaxToken exclamationToken, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		return new ShebangDirectiveTriviaSyntax(SyntaxKind.ShebangDirectiveTrivia, hashToken, exclamationToken, endOfDirectiveToken, isActive);
	}

	internal static IEnumerable<Type> GetNodeTypes()
	{
		return new Type[205]
		{
			typeof(IdentifierNameSyntax),
			typeof(QualifiedNameSyntax),
			typeof(GenericNameSyntax),
			typeof(TypeArgumentListSyntax),
			typeof(AliasQualifiedNameSyntax),
			typeof(PredefinedTypeSyntax),
			typeof(ArrayTypeSyntax),
			typeof(ArrayRankSpecifierSyntax),
			typeof(PointerTypeSyntax),
			typeof(NullableTypeSyntax),
			typeof(TupleTypeSyntax),
			typeof(TupleElementSyntax),
			typeof(OmittedTypeArgumentSyntax),
			typeof(RefTypeSyntax),
			typeof(ParenthesizedExpressionSyntax),
			typeof(TupleExpressionSyntax),
			typeof(PrefixUnaryExpressionSyntax),
			typeof(AwaitExpressionSyntax),
			typeof(PostfixUnaryExpressionSyntax),
			typeof(MemberAccessExpressionSyntax),
			typeof(ConditionalAccessExpressionSyntax),
			typeof(MemberBindingExpressionSyntax),
			typeof(ElementBindingExpressionSyntax),
			typeof(ImplicitElementAccessSyntax),
			typeof(BinaryExpressionSyntax),
			typeof(AssignmentExpressionSyntax),
			typeof(ConditionalExpressionSyntax),
			typeof(ThisExpressionSyntax),
			typeof(BaseExpressionSyntax),
			typeof(LiteralExpressionSyntax),
			typeof(MakeRefExpressionSyntax),
			typeof(RefTypeExpressionSyntax),
			typeof(RefValueExpressionSyntax),
			typeof(CheckedExpressionSyntax),
			typeof(DefaultExpressionSyntax),
			typeof(TypeOfExpressionSyntax),
			typeof(SizeOfExpressionSyntax),
			typeof(InvocationExpressionSyntax),
			typeof(ElementAccessExpressionSyntax),
			typeof(ArgumentListSyntax),
			typeof(BracketedArgumentListSyntax),
			typeof(ArgumentSyntax),
			typeof(NameColonSyntax),
			typeof(DeclarationExpressionSyntax),
			typeof(CastExpressionSyntax),
			typeof(AnonymousMethodExpressionSyntax),
			typeof(SimpleLambdaExpressionSyntax),
			typeof(RefExpressionSyntax),
			typeof(ParenthesizedLambdaExpressionSyntax),
			typeof(InitializerExpressionSyntax),
			typeof(ObjectCreationExpressionSyntax),
			typeof(AnonymousObjectMemberDeclaratorSyntax),
			typeof(AnonymousObjectCreationExpressionSyntax),
			typeof(ArrayCreationExpressionSyntax),
			typeof(ImplicitArrayCreationExpressionSyntax),
			typeof(StackAllocArrayCreationExpressionSyntax),
			typeof(ImplicitStackAllocArrayCreationExpressionSyntax),
			typeof(QueryExpressionSyntax),
			typeof(QueryBodySyntax),
			typeof(FromClauseSyntax),
			typeof(LetClauseSyntax),
			typeof(JoinClauseSyntax),
			typeof(JoinIntoClauseSyntax),
			typeof(WhereClauseSyntax),
			typeof(OrderByClauseSyntax),
			typeof(OrderingSyntax),
			typeof(SelectClauseSyntax),
			typeof(GroupClauseSyntax),
			typeof(QueryContinuationSyntax),
			typeof(OmittedArraySizeExpressionSyntax),
			typeof(InterpolatedStringExpressionSyntax),
			typeof(IsPatternExpressionSyntax),
			typeof(ThrowExpressionSyntax),
			typeof(WhenClauseSyntax),
			typeof(DeclarationPatternSyntax),
			typeof(ConstantPatternSyntax),
			typeof(InterpolatedStringTextSyntax),
			typeof(InterpolationSyntax),
			typeof(InterpolationAlignmentClauseSyntax),
			typeof(InterpolationFormatClauseSyntax),
			typeof(GlobalStatementSyntax),
			typeof(BlockSyntax),
			typeof(LocalFunctionStatementSyntax),
			typeof(LocalDeclarationStatementSyntax),
			typeof(VariableDeclarationSyntax),
			typeof(VariableDeclaratorSyntax),
			typeof(EqualsValueClauseSyntax),
			typeof(SingleVariableDesignationSyntax),
			typeof(DiscardDesignationSyntax),
			typeof(ParenthesizedVariableDesignationSyntax),
			typeof(ExpressionStatementSyntax),
			typeof(EmptyStatementSyntax),
			typeof(LabeledStatementSyntax),
			typeof(GotoStatementSyntax),
			typeof(BreakStatementSyntax),
			typeof(ContinueStatementSyntax),
			typeof(ReturnStatementSyntax),
			typeof(ThrowStatementSyntax),
			typeof(YieldStatementSyntax),
			typeof(WhileStatementSyntax),
			typeof(DoStatementSyntax),
			typeof(ForStatementSyntax),
			typeof(ForEachStatementSyntax),
			typeof(ForEachVariableStatementSyntax),
			typeof(UsingStatementSyntax),
			typeof(FixedStatementSyntax),
			typeof(CheckedStatementSyntax),
			typeof(UnsafeStatementSyntax),
			typeof(LockStatementSyntax),
			typeof(IfStatementSyntax),
			typeof(ElseClauseSyntax),
			typeof(SwitchStatementSyntax),
			typeof(SwitchSectionSyntax),
			typeof(CasePatternSwitchLabelSyntax),
			typeof(CaseSwitchLabelSyntax),
			typeof(DefaultSwitchLabelSyntax),
			typeof(TryStatementSyntax),
			typeof(CatchClauseSyntax),
			typeof(CatchDeclarationSyntax),
			typeof(CatchFilterClauseSyntax),
			typeof(FinallyClauseSyntax),
			typeof(CompilationUnitSyntax),
			typeof(ExternAliasDirectiveSyntax),
			typeof(UsingDirectiveSyntax),
			typeof(NamespaceDeclarationSyntax),
			typeof(AttributeListSyntax),
			typeof(AttributeTargetSpecifierSyntax),
			typeof(AttributeSyntax),
			typeof(AttributeArgumentListSyntax),
			typeof(AttributeArgumentSyntax),
			typeof(NameEqualsSyntax),
			typeof(TypeParameterListSyntax),
			typeof(TypeParameterSyntax),
			typeof(ClassDeclarationSyntax),
			typeof(StructDeclarationSyntax),
			typeof(InterfaceDeclarationSyntax),
			typeof(EnumDeclarationSyntax),
			typeof(DelegateDeclarationSyntax),
			typeof(EnumMemberDeclarationSyntax),
			typeof(BaseListSyntax),
			typeof(SimpleBaseTypeSyntax),
			typeof(TypeParameterConstraintClauseSyntax),
			typeof(ConstructorConstraintSyntax),
			typeof(ClassOrStructConstraintSyntax),
			typeof(TypeConstraintSyntax),
			typeof(FieldDeclarationSyntax),
			typeof(EventFieldDeclarationSyntax),
			typeof(ExplicitInterfaceSpecifierSyntax),
			typeof(MethodDeclarationSyntax),
			typeof(OperatorDeclarationSyntax),
			typeof(ConversionOperatorDeclarationSyntax),
			typeof(ConstructorDeclarationSyntax),
			typeof(ConstructorInitializerSyntax),
			typeof(DestructorDeclarationSyntax),
			typeof(PropertyDeclarationSyntax),
			typeof(ArrowExpressionClauseSyntax),
			typeof(EventDeclarationSyntax),
			typeof(IndexerDeclarationSyntax),
			typeof(AccessorListSyntax),
			typeof(AccessorDeclarationSyntax),
			typeof(ParameterListSyntax),
			typeof(BracketedParameterListSyntax),
			typeof(ParameterSyntax),
			typeof(IncompleteMemberSyntax),
			typeof(SkippedTokensTriviaSyntax),
			typeof(DocumentationCommentTriviaSyntax),
			typeof(TypeCrefSyntax),
			typeof(QualifiedCrefSyntax),
			typeof(NameMemberCrefSyntax),
			typeof(IndexerMemberCrefSyntax),
			typeof(OperatorMemberCrefSyntax),
			typeof(ConversionOperatorMemberCrefSyntax),
			typeof(CrefParameterListSyntax),
			typeof(CrefBracketedParameterListSyntax),
			typeof(CrefParameterSyntax),
			typeof(XmlElementSyntax),
			typeof(XmlElementStartTagSyntax),
			typeof(XmlElementEndTagSyntax),
			typeof(XmlEmptyElementSyntax),
			typeof(XmlNameSyntax),
			typeof(XmlPrefixSyntax),
			typeof(XmlTextAttributeSyntax),
			typeof(XmlCrefAttributeSyntax),
			typeof(XmlNameAttributeSyntax),
			typeof(XmlTextSyntax),
			typeof(XmlCDataSectionSyntax),
			typeof(XmlProcessingInstructionSyntax),
			typeof(XmlCommentSyntax),
			typeof(IfDirectiveTriviaSyntax),
			typeof(ElifDirectiveTriviaSyntax),
			typeof(ElseDirectiveTriviaSyntax),
			typeof(EndIfDirectiveTriviaSyntax),
			typeof(RegionDirectiveTriviaSyntax),
			typeof(EndRegionDirectiveTriviaSyntax),
			typeof(ErrorDirectiveTriviaSyntax),
			typeof(WarningDirectiveTriviaSyntax),
			typeof(BadDirectiveTriviaSyntax),
			typeof(DefineDirectiveTriviaSyntax),
			typeof(UndefDirectiveTriviaSyntax),
			typeof(LineDirectiveTriviaSyntax),
			typeof(PragmaWarningDirectiveTriviaSyntax),
			typeof(PragmaChecksumDirectiveTriviaSyntax),
			typeof(ReferenceDirectiveTriviaSyntax),
			typeof(LoadDirectiveTriviaSyntax),
			typeof(ShebangDirectiveTriviaSyntax)
		};
	}

	internal static SyntaxTrivia EndOfLine(string text, bool elastic = false)
	{
		SyntaxTrivia syntaxTrivia = null;
		switch (text)
		{
		case "\r":
			syntaxTrivia = (elastic ? ElasticCarriageReturn : CarriageReturn);
			break;
		case "\n":
			syntaxTrivia = (elastic ? ElasticLineFeed : LineFeed);
			break;
		case "\r\n":
			syntaxTrivia = (elastic ? ElasticCarriageReturnLineFeed : CarriageReturnLineFeed);
			break;
		}
		if (syntaxTrivia != null)
		{
			return syntaxTrivia;
		}
		syntaxTrivia = SyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, text);
		if (!elastic)
		{
			return syntaxTrivia;
		}
		return syntaxTrivia.WithAnnotationsGreen(new SyntaxAnnotation[1] { SyntaxAnnotation.ElasticAnnotation });
	}

	internal static SyntaxTrivia Whitespace(string text, bool elastic = false)
	{
		SyntaxTrivia syntaxTrivia = SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, text);
		if (!elastic)
		{
			return syntaxTrivia;
		}
		return syntaxTrivia.WithAnnotationsGreen(new SyntaxAnnotation[1] { SyntaxAnnotation.ElasticAnnotation });
	}

	internal static SyntaxTrivia Comment(string text)
	{
		if (text.StartsWith("/*", StringComparison.Ordinal))
		{
			return SyntaxTrivia.Create(SyntaxKind.MultiLineCommentTrivia, text);
		}
		return SyntaxTrivia.Create(SyntaxKind.SingleLineCommentTrivia, text);
	}

	internal static SyntaxTrivia ConflictMarker(string text)
	{
		return SyntaxTrivia.Create(SyntaxKind.ConflictMarkerTrivia, text);
	}

	internal static SyntaxTrivia DisabledText(string text)
	{
		return SyntaxTrivia.Create(SyntaxKind.DisabledTextTrivia, text);
	}

	internal static SyntaxTrivia PreprocessingMessage(string text)
	{
		return SyntaxTrivia.Create(SyntaxKind.PreprocessingMessageTrivia, text);
	}

	public static SyntaxToken Token(SyntaxKind kind)
	{
		return SyntaxToken.Create(kind);
	}

	internal static SyntaxToken Token(GreenNode leading, SyntaxKind kind, GreenNode trailing)
	{
		return SyntaxToken.Create(kind, leading, trailing);
	}

	internal static SyntaxToken Token(GreenNode leading, SyntaxKind kind, string text, string valueText, GreenNode trailing)
	{
		string text2 = SyntaxFacts.GetText(kind);
		if ((int)kind < 8193 || (int)kind > 8496 || !(text == text2) || !(valueText == text2))
		{
			return SyntaxToken.WithValue(kind, leading, text, valueText, trailing);
		}
		return Token(leading, kind, trailing);
	}

	internal static SyntaxToken MissingToken(SyntaxKind kind)
	{
		return SyntaxToken.CreateMissing(kind, null, null);
	}

	internal static SyntaxToken MissingToken(GreenNode leading, SyntaxKind kind, GreenNode trailing)
	{
		return SyntaxToken.CreateMissing(kind, leading, trailing);
	}

	internal static SyntaxToken Identifier(string text)
	{
		return Identifier(SyntaxKind.IdentifierToken, null, text, text, null);
	}

	internal static SyntaxToken Identifier(GreenNode leading, string text, GreenNode trailing)
	{
		return Identifier(SyntaxKind.IdentifierToken, leading, text, text, trailing);
	}

	internal static SyntaxToken Identifier(SyntaxKind contextualKind, GreenNode leading, string text, string valueText, GreenNode trailing)
	{
		return SyntaxToken.Identifier(contextualKind, leading, text, valueText, trailing);
	}

	internal static SyntaxToken Literal(GreenNode leading, string text, int value, GreenNode trailing)
	{
		return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
	}

	internal static SyntaxToken Literal(GreenNode leading, string text, uint value, GreenNode trailing)
	{
		return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
	}

	internal static SyntaxToken Literal(GreenNode leading, string text, long value, GreenNode trailing)
	{
		return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
	}

	internal static SyntaxToken Literal(GreenNode leading, string text, ulong value, GreenNode trailing)
	{
		return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
	}

	internal static SyntaxToken Literal(GreenNode leading, string text, float value, GreenNode trailing)
	{
		return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
	}

	internal static SyntaxToken Literal(GreenNode leading, string text, double value, GreenNode trailing)
	{
		return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
	}

	internal static SyntaxToken Literal(GreenNode leading, string text, decimal value, GreenNode trailing)
	{
		return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
	}

	internal static SyntaxToken Literal(GreenNode leading, string text, string value, GreenNode trailing)
	{
		return SyntaxToken.WithValue(SyntaxKind.StringLiteralToken, leading, text, value, trailing);
	}

	internal static SyntaxToken Literal(GreenNode leading, string text, SyntaxKind kind, string value, GreenNode trailing)
	{
		return SyntaxToken.WithValue(kind, leading, text, value, trailing);
	}

	internal static SyntaxToken Literal(GreenNode leading, string text, char value, GreenNode trailing)
	{
		return SyntaxToken.WithValue(SyntaxKind.CharacterLiteralToken, leading, text, value, trailing);
	}

	internal static SyntaxToken BadToken(GreenNode leading, string text, GreenNode trailing)
	{
		return SyntaxToken.WithValue(SyntaxKind.BadToken, leading, text, text, trailing);
	}

	internal static SyntaxToken XmlTextLiteral(GreenNode leading, string text, string value, GreenNode trailing)
	{
		return SyntaxToken.WithValue(SyntaxKind.XmlTextLiteralToken, leading, text, value, trailing);
	}

	internal static SyntaxToken XmlTextNewLine(GreenNode leading, string text, string value, GreenNode trailing)
	{
		if (leading == null && trailing == null && text == "\r\n" && value == "\r\n")
		{
			return XmlCarriageReturnLineFeed;
		}
		return SyntaxToken.WithValue(SyntaxKind.XmlTextLiteralNewLineToken, leading, text, value, trailing);
	}

	internal static SyntaxToken XmlTextNewLine(string text)
	{
		return SyntaxToken.WithValue(SyntaxKind.XmlTextLiteralNewLineToken, null, text, text, null);
	}

	internal static SyntaxToken XmlEntity(GreenNode leading, string text, string value, GreenNode trailing)
	{
		return SyntaxToken.WithValue(SyntaxKind.XmlEntityLiteralToken, leading, text, value, trailing);
	}

	internal static SyntaxTrivia DocumentationCommentExteriorTrivia(string text)
	{
		return SyntaxTrivia.Create(SyntaxKind.DocumentationCommentExteriorTrivia, text);
	}

	public static Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode> List<TNode>() where TNode : CSharpSyntaxNode
	{
		return default;
	}

	public static Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode> List<TNode>(TNode node) where TNode : CSharpSyntaxNode
	{
		return new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode>(SyntaxList.List(node));
	}

	public static Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode> List<TNode>(TNode node0, TNode node1) where TNode : CSharpSyntaxNode
	{
		return new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode>(SyntaxList.List(node0, node1));
	}

	internal static GreenNode ListNode(CSharpSyntaxNode node0, CSharpSyntaxNode node1)
	{
		return SyntaxList.List(node0, node1);
	}

	public static Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode> List<TNode>(TNode node0, TNode node1, TNode node2) where TNode : CSharpSyntaxNode
	{
		return new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode>(SyntaxList.List(node0, node1, node2));
	}

	internal static GreenNode ListNode(CSharpSyntaxNode node0, CSharpSyntaxNode node1, CSharpSyntaxNode node2)
	{
		return SyntaxList.List(node0, node1, node2);
	}

	public static Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode> List<TNode>(params TNode[] nodes) where TNode : CSharpSyntaxNode
	{
		if (nodes != null)
		{
			return new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode>(SyntaxList.List(nodes));
		}
		return default;
	}

	internal static GreenNode ListNode(params ArrayElement<GreenNode>[] nodes)
	{
		return SyntaxList.List(nodes);
	}

	public static Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode> SeparatedList<TNode>(TNode node) where TNode : CSharpSyntaxNode
	{
		return new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(node));
	}

	public static Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode> SeparatedList<TNode>(SyntaxToken token) where TNode : CSharpSyntaxNode
	{
		return new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(token));
	}

	public static Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode> SeparatedList<TNode>(TNode node1, SyntaxToken token, TNode node2) where TNode : CSharpSyntaxNode
	{
		return new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(SyntaxList.List(node1, token, node2)));
	}

	public static Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode> SeparatedList<TNode>(params CSharpSyntaxNode[] nodes) where TNode : CSharpSyntaxNode
	{
		if (nodes != null)
		{
			return new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode>(SyntaxList.List(nodes));
		}
		return default;
	}

	internal static IEnumerable<SyntaxTrivia> GetWellKnownTrivia()
	{
		yield return CarriageReturnLineFeed;
		yield return LineFeed;
		yield return CarriageReturn;
		yield return Space;
		yield return Tab;
		yield return ElasticCarriageReturnLineFeed;
		yield return ElasticLineFeed;
		yield return ElasticCarriageReturn;
		yield return ElasticSpace;
		yield return ElasticTab;
		yield return ElasticZeroSpace;
	}

	internal static IEnumerable<SyntaxToken> GetWellKnownTokens()
	{
		return SyntaxToken.GetWellKnownTokens();
	}
}
