using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public static class SyntaxFactory
{
	public static SyntaxTrivia CarriageReturnLineFeed { get; } = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CarriageReturnLineFeed;

	public static SyntaxTrivia LineFeed { get; } = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.LineFeed;

	public static SyntaxTrivia CarriageReturn { get; } = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CarriageReturn;

	public static SyntaxTrivia Space { get; } = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Space;

	public static SyntaxTrivia Tab { get; } = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Tab;

	public static SyntaxTrivia ElasticCarriageReturnLineFeed { get; } = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ElasticCarriageReturnLineFeed;

	public static SyntaxTrivia ElasticLineFeed { get; } = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ElasticLineFeed;

	public static SyntaxTrivia ElasticCarriageReturn { get; } = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ElasticCarriageReturn;

	public static SyntaxTrivia ElasticSpace { get; } = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ElasticSpace;

	public static SyntaxTrivia ElasticTab { get; } = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ElasticTab;

	public static SyntaxTrivia ElasticMarker { get; } = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ElasticZeroSpace;

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax IdentifierName(SyntaxToken identifier)
	{
		SyntaxKind syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.GlobalKeyword && syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.IdentifierName((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax QualifiedName(Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax left, SyntaxToken dotToken, Microsoft.CodeAnalysis.CSharp.Syntax.SimpleNameSyntax right)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		SyntaxKind syntaxKind = dotToken.Kind();
		if (syntaxKind != SyntaxKind.DotToken)
		{
			throw new ArgumentException("dotToken");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.QualifiedName((left == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameSyntax)left.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)dotToken.Node, (right == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SimpleNameSyntax)right.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax QualifiedName(Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax left, Microsoft.CodeAnalysis.CSharp.Syntax.SimpleNameSyntax right)
	{
		return QualifiedName(left, Token(SyntaxKind.DotToken), right);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax GenericName(SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeArgumentListSyntax typeArgumentList)
	{
		SyntaxKind syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		if (typeArgumentList == null)
		{
			throw new ArgumentNullException("typeArgumentList");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.GenericName((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (typeArgumentList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeArgumentListSyntax)typeArgumentList.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax GenericName(SyntaxToken identifier)
	{
		return GenericName(identifier, TypeArgumentList());
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax GenericName(string identifier)
	{
		return GenericName(Identifier(identifier), TypeArgumentList());
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeArgumentListSyntax TypeArgumentList(SyntaxToken lessThanToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax> arguments, SyntaxToken greaterThanToken)
	{
		SyntaxKind syntaxKind = lessThanToken.Kind();
		if (syntaxKind != SyntaxKind.LessThanToken)
		{
			throw new ArgumentException("lessThanToken");
		}
		syntaxKind = greaterThanToken.Kind();
		if (syntaxKind != SyntaxKind.GreaterThanToken)
		{
			throw new ArgumentException("greaterThanToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.TypeArgumentListSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.TypeArgumentList((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)lessThanToken.Node, arguments.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)greaterThanToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeArgumentListSyntax TypeArgumentList(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax> arguments = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax>))
	{
		return TypeArgumentList(Token(SyntaxKind.LessThanToken), arguments, Token(SyntaxKind.GreaterThanToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax AliasQualifiedName(Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax alias, SyntaxToken colonColonToken, Microsoft.CodeAnalysis.CSharp.Syntax.SimpleNameSyntax name)
	{
		if (alias == null)
		{
			throw new ArgumentNullException("alias");
		}
		SyntaxKind syntaxKind = colonColonToken.Kind();
		if (syntaxKind != SyntaxKind.ColonColonToken)
		{
			throw new ArgumentException("colonColonToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.AliasQualifiedName((alias == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IdentifierNameSyntax)alias.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)colonColonToken.Node, (name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SimpleNameSyntax)name.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax AliasQualifiedName(Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax alias, Microsoft.CodeAnalysis.CSharp.Syntax.SimpleNameSyntax name)
	{
		return AliasQualifiedName(alias, Token(SyntaxKind.ColonColonToken), name);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax AliasQualifiedName(string alias, Microsoft.CodeAnalysis.CSharp.Syntax.SimpleNameSyntax name)
	{
		return AliasQualifiedName(IdentifierName(alias), Token(SyntaxKind.ColonColonToken), name);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax PredefinedType(SyntaxToken keyword)
	{
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind - 8304 > (SyntaxKind)15)
		{
			throw new ArgumentException("keyword");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.PredefinedType((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArrayTypeSyntax ArrayType(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax elementType, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ArrayRankSpecifierSyntax> rankSpecifiers)
	{
		if (elementType == null)
		{
			throw new ArgumentNullException("elementType");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ArrayTypeSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ArrayType((elementType == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)elementType.Green), rankSpecifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArrayRankSpecifierSyntax>()).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArrayTypeSyntax ArrayType(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax elementType)
	{
		return ArrayType(elementType, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArrayRankSpecifierSyntax ArrayRankSpecifier(SyntaxToken openBracketToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax> sizes, SyntaxToken closeBracketToken)
	{
		SyntaxKind syntaxKind = openBracketToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBracketToken)
		{
			throw new ArgumentException("openBracketToken");
		}
		syntaxKind = closeBracketToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBracketToken)
		{
			throw new ArgumentException("closeBracketToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ArrayRankSpecifierSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ArrayRankSpecifier((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBracketToken.Node, sizes.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBracketToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArrayRankSpecifierSyntax ArrayRankSpecifier(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax> sizes = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax>))
	{
		return ArrayRankSpecifier(Token(SyntaxKind.OpenBracketToken), sizes, Token(SyntaxKind.CloseBracketToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PointerTypeSyntax PointerType(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax elementType, SyntaxToken asteriskToken)
	{
		if (elementType == null)
		{
			throw new ArgumentNullException("elementType");
		}
		SyntaxKind syntaxKind = asteriskToken.Kind();
		if (syntaxKind != SyntaxKind.AsteriskToken)
		{
			throw new ArgumentException("asteriskToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.PointerTypeSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.PointerType((elementType == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)elementType.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)asteriskToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PointerTypeSyntax PointerType(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax elementType)
	{
		return PointerType(elementType, Token(SyntaxKind.AsteriskToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NullableTypeSyntax NullableType(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax elementType, SyntaxToken questionToken)
	{
		if (elementType == null)
		{
			throw new ArgumentNullException("elementType");
		}
		SyntaxKind syntaxKind = questionToken.Kind();
		if (syntaxKind != SyntaxKind.QuestionToken)
		{
			throw new ArgumentException("questionToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.NullableTypeSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.NullableType((elementType == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)elementType.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)questionToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NullableTypeSyntax NullableType(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax elementType)
	{
		return NullableType(elementType, Token(SyntaxKind.QuestionToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TupleTypeSyntax TupleType(SyntaxToken openParenToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TupleElementSyntax> elements, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.TupleTypeSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.TupleType((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, elements.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TupleElementSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TupleTypeSyntax TupleType(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TupleElementSyntax> elements = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TupleElementSyntax>))
	{
		return TupleType(Token(SyntaxKind.OpenParenToken), elements, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TupleElementSyntax TupleElement(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken identifier)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		SyntaxKind syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.TupleElementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.TupleElement((type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TupleElementSyntax TupleElement(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return TupleElement(type, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OmittedTypeArgumentSyntax OmittedTypeArgument(SyntaxToken omittedTypeArgumentToken)
	{
		SyntaxKind syntaxKind = omittedTypeArgumentToken.Kind();
		if (syntaxKind != SyntaxKind.OmittedTypeArgumentToken)
		{
			throw new ArgumentException("omittedTypeArgumentToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.OmittedTypeArgumentSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.OmittedTypeArgument((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)omittedTypeArgumentToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OmittedTypeArgumentSyntax OmittedTypeArgument()
	{
		return OmittedTypeArgument(Token(SyntaxKind.OmittedTypeArgumentToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.RefTypeSyntax RefType(SyntaxToken refKeyword, SyntaxToken readOnlyKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		SyntaxKind syntaxKind = refKeyword.Kind();
		if (syntaxKind != SyntaxKind.RefKeyword)
		{
			throw new ArgumentException("refKeyword");
		}
		syntaxKind = readOnlyKeyword.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.ReadOnlyKeyword)
		{
			throw new ArgumentException("readOnlyKeyword");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.RefTypeSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.RefType((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)refKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)readOnlyKeyword.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.RefTypeSyntax RefType(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return RefType(Token(SyntaxKind.RefKeyword), default, type);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedExpressionSyntax ParenthesizedExpression(SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ParenthesizedExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedExpressionSyntax ParenthesizedExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return ParenthesizedExpression(Token(SyntaxKind.OpenParenToken), expression, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TupleExpressionSyntax TupleExpression(SyntaxToken openParenToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax> arguments, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.TupleExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.TupleExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, arguments.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArgumentSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TupleExpressionSyntax TupleExpression(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax> arguments = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax>))
	{
		return TupleExpression(Token(SyntaxKind.OpenParenToken), arguments, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax PrefixUnaryExpression(SyntaxKind kind, SyntaxToken operatorToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax operand)
	{
		if (kind - 8730 > (SyntaxKind)7)
		{
			throw new ArgumentException("kind");
		}
		switch (operatorToken.Kind())
		{
		default:
			throw new ArgumentException("operatorToken");
		case SyntaxKind.TildeToken:
		case SyntaxKind.ExclamationToken:
		case SyntaxKind.AmpersandToken:
		case SyntaxKind.AsteriskToken:
		case SyntaxKind.MinusToken:
		case SyntaxKind.PlusToken:
		case SyntaxKind.MinusMinusToken:
		case SyntaxKind.PlusPlusToken:
			if (operand == null)
			{
				throw new ArgumentNullException("operand");
			}
			return (Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.PrefixUnaryExpression(kind, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)operatorToken.Node, (operand == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)operand.Green)).CreateRed();
		}
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax PrefixUnaryExpression(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax operand)
	{
		return PrefixUnaryExpression(kind, Token(GetPrefixUnaryExpressionOperatorTokenKind(kind)), operand);
	}

	private static SyntaxKind GetPrefixUnaryExpressionOperatorTokenKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.UnaryPlusExpression => SyntaxKind.PlusToken, 
			SyntaxKind.UnaryMinusExpression => SyntaxKind.MinusToken, 
			SyntaxKind.BitwiseNotExpression => SyntaxKind.TildeToken, 
			SyntaxKind.LogicalNotExpression => SyntaxKind.ExclamationToken, 
			SyntaxKind.PreIncrementExpression => SyntaxKind.PlusPlusToken, 
			SyntaxKind.PreDecrementExpression => SyntaxKind.MinusMinusToken, 
			SyntaxKind.AddressOfExpression => SyntaxKind.AmpersandToken, 
			SyntaxKind.PointerIndirectionExpression => SyntaxKind.AsteriskToken, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax AwaitExpression(SyntaxToken awaitKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		SyntaxKind syntaxKind = awaitKeyword.Kind();
		if (syntaxKind != SyntaxKind.AwaitKeyword)
		{
			throw new ArgumentException("awaitKeyword");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.AwaitExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)awaitKeyword.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax AwaitExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return AwaitExpression(Token(SyntaxKind.AwaitKeyword), expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax PostfixUnaryExpression(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax operand, SyntaxToken operatorToken)
	{
		if (kind - 8738 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		if (operand == null)
		{
			throw new ArgumentNullException("operand");
		}
		SyntaxKind syntaxKind = operatorToken.Kind();
		if (syntaxKind - 8262 > SyntaxKind.List)
		{
			throw new ArgumentException("operatorToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.PostfixUnaryExpression(kind, (operand == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)operand.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)operatorToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax PostfixUnaryExpression(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax operand)
	{
		return PostfixUnaryExpression(kind, operand, Token(GetPostfixUnaryExpressionOperatorTokenKind(kind)));
	}

	private static SyntaxKind GetPostfixUnaryExpressionOperatorTokenKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.PostIncrementExpression => SyntaxKind.PlusPlusToken, 
			SyntaxKind.PostDecrementExpression => SyntaxKind.MinusMinusToken, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax MemberAccessExpression(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken operatorToken, Microsoft.CodeAnalysis.CSharp.Syntax.SimpleNameSyntax name)
	{
		if (kind - 8689 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		SyntaxKind syntaxKind = operatorToken.Kind();
		if (syntaxKind != SyntaxKind.DotToken && syntaxKind != SyntaxKind.MinusGreaterThanToken)
		{
			throw new ArgumentException("operatorToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.MemberAccessExpression(kind, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)operatorToken.Node, (name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SimpleNameSyntax)name.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax MemberAccessExpression(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Syntax.SimpleNameSyntax name)
	{
		return MemberAccessExpression(kind, expression, Token(GetMemberAccessExpressionOperatorTokenKind(kind)), name);
	}

	private static SyntaxKind GetMemberAccessExpressionOperatorTokenKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.SimpleMemberAccessExpression => SyntaxKind.DotToken, 
			SyntaxKind.PointerMemberAccessExpression => SyntaxKind.MinusGreaterThanToken, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalAccessExpressionSyntax ConditionalAccessExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken operatorToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax whenNotNull)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		SyntaxKind syntaxKind = operatorToken.Kind();
		if (syntaxKind != SyntaxKind.QuestionToken)
		{
			throw new ArgumentException("operatorToken");
		}
		if (whenNotNull == null)
		{
			throw new ArgumentNullException("whenNotNull");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalAccessExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ConditionalAccessExpression((expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)operatorToken.Node, (whenNotNull == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)whenNotNull.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalAccessExpressionSyntax ConditionalAccessExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax whenNotNull)
	{
		return ConditionalAccessExpression(expression, Token(SyntaxKind.QuestionToken), whenNotNull);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.MemberBindingExpressionSyntax MemberBindingExpression(SyntaxToken operatorToken, Microsoft.CodeAnalysis.CSharp.Syntax.SimpleNameSyntax name)
	{
		SyntaxKind syntaxKind = operatorToken.Kind();
		if (syntaxKind != SyntaxKind.DotToken)
		{
			throw new ArgumentException("operatorToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.MemberBindingExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.MemberBindingExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)operatorToken.Node, (name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SimpleNameSyntax)name.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.MemberBindingExpressionSyntax MemberBindingExpression(Microsoft.CodeAnalysis.CSharp.Syntax.SimpleNameSyntax name)
	{
		return MemberBindingExpression(Token(SyntaxKind.DotToken), name);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ElementBindingExpressionSyntax ElementBindingExpression(Microsoft.CodeAnalysis.CSharp.Syntax.BracketedArgumentListSyntax argumentList)
	{
		if (argumentList == null)
		{
			throw new ArgumentNullException("argumentList");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ElementBindingExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ElementBindingExpression((argumentList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BracketedArgumentListSyntax)argumentList.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ElementBindingExpressionSyntax ElementBindingExpression()
	{
		return ElementBindingExpression(BracketedArgumentList());
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitElementAccessSyntax ImplicitElementAccess(Microsoft.CodeAnalysis.CSharp.Syntax.BracketedArgumentListSyntax argumentList)
	{
		if (argumentList == null)
		{
			throw new ArgumentNullException("argumentList");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitElementAccessSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ImplicitElementAccess((argumentList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BracketedArgumentListSyntax)argumentList.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitElementAccessSyntax ImplicitElementAccess()
	{
		return ImplicitElementAccess(BracketedArgumentList());
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax BinaryExpression(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax left, SyntaxToken operatorToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax right)
	{
		if (kind - 8668 > (SyntaxKind)20)
		{
			throw new ArgumentException("kind");
		}
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		switch (operatorToken.Kind())
		{
		default:
			throw new ArgumentException("operatorToken");
		case SyntaxKind.PercentToken:
		case SyntaxKind.CaretToken:
		case SyntaxKind.AmpersandToken:
		case SyntaxKind.AsteriskToken:
		case SyntaxKind.MinusToken:
		case SyntaxKind.PlusToken:
		case SyntaxKind.BarToken:
		case SyntaxKind.LessThanToken:
		case SyntaxKind.GreaterThanToken:
		case SyntaxKind.SlashToken:
		case SyntaxKind.BarBarToken:
		case SyntaxKind.AmpersandAmpersandToken:
		case SyntaxKind.QuestionQuestionToken:
		case SyntaxKind.ExclamationEqualsToken:
		case SyntaxKind.EqualsEqualsToken:
		case SyntaxKind.LessThanEqualsToken:
		case SyntaxKind.LessThanLessThanToken:
		case SyntaxKind.GreaterThanEqualsToken:
		case SyntaxKind.GreaterThanGreaterThanToken:
		case SyntaxKind.IsKeyword:
		case SyntaxKind.AsKeyword:
			if (right == null)
			{
				throw new ArgumentNullException("right");
			}
			return (Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.BinaryExpression(kind, (left == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)left.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)operatorToken.Node, (right == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)right.Green)).CreateRed();
		}
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax BinaryExpression(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax left, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax right)
	{
		return BinaryExpression(kind, left, Token(GetBinaryExpressionOperatorTokenKind(kind)), right);
	}

	private static SyntaxKind GetBinaryExpressionOperatorTokenKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.AddExpression => SyntaxKind.PlusToken, 
			SyntaxKind.SubtractExpression => SyntaxKind.MinusToken, 
			SyntaxKind.MultiplyExpression => SyntaxKind.AsteriskToken, 
			SyntaxKind.DivideExpression => SyntaxKind.SlashToken, 
			SyntaxKind.ModuloExpression => SyntaxKind.PercentToken, 
			SyntaxKind.LeftShiftExpression => SyntaxKind.LessThanLessThanToken, 
			SyntaxKind.RightShiftExpression => SyntaxKind.GreaterThanGreaterThanToken, 
			SyntaxKind.LogicalOrExpression => SyntaxKind.BarBarToken, 
			SyntaxKind.LogicalAndExpression => SyntaxKind.AmpersandAmpersandToken, 
			SyntaxKind.BitwiseOrExpression => SyntaxKind.BarToken, 
			SyntaxKind.BitwiseAndExpression => SyntaxKind.AmpersandToken, 
			SyntaxKind.ExclusiveOrExpression => SyntaxKind.CaretToken, 
			SyntaxKind.EqualsExpression => SyntaxKind.EqualsEqualsToken, 
			SyntaxKind.NotEqualsExpression => SyntaxKind.ExclamationEqualsToken, 
			SyntaxKind.LessThanExpression => SyntaxKind.LessThanToken, 
			SyntaxKind.LessThanOrEqualExpression => SyntaxKind.LessThanEqualsToken, 
			SyntaxKind.GreaterThanExpression => SyntaxKind.GreaterThanToken, 
			SyntaxKind.GreaterThanOrEqualExpression => SyntaxKind.GreaterThanEqualsToken, 
			SyntaxKind.IsExpression => SyntaxKind.IsKeyword, 
			SyntaxKind.AsExpression => SyntaxKind.AsKeyword, 
			SyntaxKind.CoalesceExpression => SyntaxKind.QuestionQuestionToken, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax AssignmentExpression(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax left, SyntaxToken operatorToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax right)
	{
		if (kind - 8714 > (SyntaxKind)10)
		{
			throw new ArgumentException("kind");
		}
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		SyntaxKind syntaxKind = operatorToken.Kind();
		if (syntaxKind != SyntaxKind.EqualsToken && syntaxKind != SyntaxKind.LessThanLessThanEqualsToken && syntaxKind - 8275 > (SyntaxKind)8)
		{
			throw new ArgumentException("operatorToken");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.AssignmentExpression(kind, (left == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)left.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)operatorToken.Node, (right == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)right.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax AssignmentExpression(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax left, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax right)
	{
		return AssignmentExpression(kind, left, Token(GetAssignmentExpressionOperatorTokenKind(kind)), right);
	}

	private static SyntaxKind GetAssignmentExpressionOperatorTokenKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.SimpleAssignmentExpression => SyntaxKind.EqualsToken, 
			SyntaxKind.AddAssignmentExpression => SyntaxKind.PlusEqualsToken, 
			SyntaxKind.SubtractAssignmentExpression => SyntaxKind.MinusEqualsToken, 
			SyntaxKind.MultiplyAssignmentExpression => SyntaxKind.AsteriskEqualsToken, 
			SyntaxKind.DivideAssignmentExpression => SyntaxKind.SlashEqualsToken, 
			SyntaxKind.ModuloAssignmentExpression => SyntaxKind.PercentEqualsToken, 
			SyntaxKind.AndAssignmentExpression => SyntaxKind.AmpersandEqualsToken, 
			SyntaxKind.ExclusiveOrAssignmentExpression => SyntaxKind.CaretEqualsToken, 
			SyntaxKind.OrAssignmentExpression => SyntaxKind.BarEqualsToken, 
			SyntaxKind.LeftShiftAssignmentExpression => SyntaxKind.LessThanLessThanEqualsToken, 
			SyntaxKind.RightShiftAssignmentExpression => SyntaxKind.GreaterThanGreaterThanEqualsToken, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalExpressionSyntax ConditionalExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, SyntaxToken questionToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax whenTrue, SyntaxToken colonToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax whenFalse)
	{
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		SyntaxKind syntaxKind = questionToken.Kind();
		if (syntaxKind != SyntaxKind.QuestionToken)
		{
			throw new ArgumentException("questionToken");
		}
		if (whenTrue == null)
		{
			throw new ArgumentNullException("whenTrue");
		}
		syntaxKind = colonToken.Kind();
		if (syntaxKind != SyntaxKind.ColonToken)
		{
			throw new ArgumentException("colonToken");
		}
		if (whenFalse == null)
		{
			throw new ArgumentNullException("whenFalse");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ConditionalExpression((condition == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)condition.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)questionToken.Node, (whenTrue == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)whenTrue.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)colonToken.Node, (whenFalse == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)whenFalse.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalExpressionSyntax ConditionalExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax whenTrue, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax whenFalse)
	{
		return ConditionalExpression(condition, Token(SyntaxKind.QuestionToken), whenTrue, Token(SyntaxKind.ColonToken), whenFalse);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ThisExpressionSyntax ThisExpression(SyntaxToken token)
	{
		SyntaxKind syntaxKind = token.Kind();
		if (syntaxKind != SyntaxKind.ThisKeyword)
		{
			throw new ArgumentException("token");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ThisExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ThisExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)token.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ThisExpressionSyntax ThisExpression()
	{
		return ThisExpression(Token(SyntaxKind.ThisKeyword));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BaseExpressionSyntax BaseExpression(SyntaxToken token)
	{
		SyntaxKind syntaxKind = token.Kind();
		if (syntaxKind != SyntaxKind.BaseKeyword)
		{
			throw new ArgumentException("token");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.BaseExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.BaseExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)token.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BaseExpressionSyntax BaseExpression()
	{
		return BaseExpression(Token(SyntaxKind.BaseKeyword));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax LiteralExpression(SyntaxKind kind, SyntaxToken token)
	{
		if (kind - 8748 > (SyntaxKind)7)
		{
			throw new ArgumentException("kind");
		}
		switch (token.Kind())
		{
		default:
			throw new ArgumentException("token");
		case SyntaxKind.NullKeyword:
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.DefaultKeyword:
		case SyntaxKind.ArgListKeyword:
		case SyntaxKind.NumericLiteralToken:
		case SyntaxKind.CharacterLiteralToken:
		case SyntaxKind.StringLiteralToken:
			return (Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.LiteralExpression(kind, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)token.Node).CreateRed();
		}
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax LiteralExpression(SyntaxKind kind)
	{
		return LiteralExpression(kind, Token(GetLiteralExpressionTokenKind(kind)));
	}

	private static SyntaxKind GetLiteralExpressionTokenKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.ArgListExpression => SyntaxKind.ArgListKeyword, 
			SyntaxKind.NumericLiteralExpression => SyntaxKind.NumericLiteralToken, 
			SyntaxKind.StringLiteralExpression => SyntaxKind.StringLiteralToken, 
			SyntaxKind.CharacterLiteralExpression => SyntaxKind.CharacterLiteralToken, 
			SyntaxKind.TrueLiteralExpression => SyntaxKind.TrueKeyword, 
			SyntaxKind.FalseLiteralExpression => SyntaxKind.FalseKeyword, 
			SyntaxKind.NullLiteralExpression => SyntaxKind.NullKeyword, 
			SyntaxKind.DefaultLiteralExpression => SyntaxKind.DefaultKeyword, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.MakeRefExpressionSyntax MakeRefExpression(SyntaxToken keyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind != SyntaxKind.MakeRefKeyword)
		{
			throw new ArgumentException("keyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.MakeRefExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.MakeRefExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.MakeRefExpressionSyntax MakeRefExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return MakeRefExpression(Token(SyntaxKind.MakeRefKeyword), Token(SyntaxKind.OpenParenToken), expression, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.RefTypeExpressionSyntax RefTypeExpression(SyntaxToken keyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind != SyntaxKind.RefTypeKeyword)
		{
			throw new ArgumentException("keyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.RefTypeExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.RefTypeExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.RefTypeExpressionSyntax RefTypeExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return RefTypeExpression(Token(SyntaxKind.RefTypeKeyword), Token(SyntaxKind.OpenParenToken), expression, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.RefValueExpressionSyntax RefValueExpression(SyntaxToken keyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken comma, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind != SyntaxKind.RefValueKeyword)
		{
			throw new ArgumentException("keyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		syntaxKind = comma.Kind();
		if (syntaxKind != SyntaxKind.CommaToken)
		{
			throw new ArgumentException("comma");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.RefValueExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.RefValueExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)comma.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.RefValueExpressionSyntax RefValueExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return RefValueExpression(Token(SyntaxKind.RefValueKeyword), Token(SyntaxKind.OpenParenToken), expression, Token(SyntaxKind.CommaToken), type, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CheckedExpressionSyntax CheckedExpression(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		if (kind - 8762 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind - 8379 > SyntaxKind.List)
		{
			throw new ArgumentException("keyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CheckedExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CheckedExpression(kind, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CheckedExpressionSyntax CheckedExpression(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return CheckedExpression(kind, Token(GetCheckedExpressionKeywordKind(kind)), Token(SyntaxKind.OpenParenToken), expression, Token(SyntaxKind.CloseParenToken));
	}

	private static SyntaxKind GetCheckedExpressionKeywordKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.CheckedExpression => SyntaxKind.CheckedKeyword, 
			SyntaxKind.UncheckedExpression => SyntaxKind.UncheckedKeyword, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DefaultExpressionSyntax DefaultExpression(SyntaxToken keyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind != SyntaxKind.DefaultKeyword)
		{
			throw new ArgumentException("keyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.DefaultExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.DefaultExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DefaultExpressionSyntax DefaultExpression(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return DefaultExpression(Token(SyntaxKind.DefaultKeyword), Token(SyntaxKind.OpenParenToken), type, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeOfExpressionSyntax TypeOfExpression(SyntaxToken keyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind != SyntaxKind.TypeOfKeyword)
		{
			throw new ArgumentException("keyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.TypeOfExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.TypeOfExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeOfExpressionSyntax TypeOfExpression(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return TypeOfExpression(Token(SyntaxKind.TypeOfKeyword), Token(SyntaxKind.OpenParenToken), type, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SizeOfExpressionSyntax SizeOfExpression(SyntaxToken keyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind != SyntaxKind.SizeOfKeyword)
		{
			throw new ArgumentException("keyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.SizeOfExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.SizeOfExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SizeOfExpressionSyntax SizeOfExpression(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return SizeOfExpression(Token(SyntaxKind.SizeOfKeyword), Token(SyntaxKind.OpenParenToken), type, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax InvocationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax argumentList)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		if (argumentList == null)
		{
			throw new ArgumentNullException("argumentList");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.InvocationExpression((expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (argumentList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArgumentListSyntax)argumentList.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax InvocationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return InvocationExpression(expression, ArgumentList());
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ElementAccessExpressionSyntax ElementAccessExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Syntax.BracketedArgumentListSyntax argumentList)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		if (argumentList == null)
		{
			throw new ArgumentNullException("argumentList");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ElementAccessExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ElementAccessExpression((expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (argumentList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BracketedArgumentListSyntax)argumentList.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ElementAccessExpressionSyntax ElementAccessExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return ElementAccessExpression(expression, BracketedArgumentList());
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax ArgumentList(SyntaxToken openParenToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax> arguments, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ArgumentList((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, arguments.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArgumentSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax ArgumentList(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax> arguments = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax>))
	{
		return ArgumentList(Token(SyntaxKind.OpenParenToken), arguments, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BracketedArgumentListSyntax BracketedArgumentList(SyntaxToken openBracketToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax> arguments, SyntaxToken closeBracketToken)
	{
		SyntaxKind syntaxKind = openBracketToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBracketToken)
		{
			throw new ArgumentException("openBracketToken");
		}
		syntaxKind = closeBracketToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBracketToken)
		{
			throw new ArgumentException("closeBracketToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.BracketedArgumentListSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.BracketedArgumentList((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBracketToken.Node, arguments.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArgumentSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBracketToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BracketedArgumentListSyntax BracketedArgumentList(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax> arguments = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax>))
	{
		return BracketedArgumentList(Token(SyntaxKind.OpenBracketToken), arguments, Token(SyntaxKind.CloseBracketToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax Argument(Microsoft.CodeAnalysis.CSharp.Syntax.NameColonSyntax nameColon, SyntaxToken refKindKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		SyntaxKind syntaxKind = refKindKeyword.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind - 8360 > (SyntaxKind)2)
		{
			throw new ArgumentException("refKindKeyword");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Argument((nameColon == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameColonSyntax)nameColon.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)refKindKeyword.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax Argument(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return Argument(null, default, expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NameColonSyntax NameColon(Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax name, SyntaxToken colonToken)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		SyntaxKind syntaxKind = colonToken.Kind();
		if (syntaxKind != SyntaxKind.ColonToken)
		{
			throw new ArgumentException("colonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.NameColonSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.NameColon((name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IdentifierNameSyntax)name.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)colonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NameColonSyntax NameColon(Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax name)
	{
		return NameColon(name, Token(SyntaxKind.ColonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NameColonSyntax NameColon(string name)
	{
		return NameColon(IdentifierName(name), Token(SyntaxKind.ColonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DeclarationExpressionSyntax DeclarationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.VariableDesignationSyntax designation)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (designation == null)
		{
			throw new ArgumentNullException("designation");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.DeclarationExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.DeclarationExpression((type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (designation == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDesignationSyntax)designation.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CastExpressionSyntax CastExpression(SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken closeParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		SyntaxKind syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CastExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CastExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CastExpressionSyntax CastExpression(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return CastExpression(Token(SyntaxKind.OpenParenToken), type, Token(SyntaxKind.CloseParenToken), expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousMethodExpressionSyntax AnonymousMethodExpression(SyntaxToken asyncKeyword, SyntaxToken delegateKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, CSharpSyntaxNode body)
	{
		SyntaxKind syntaxKind = asyncKeyword.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.AsyncKeyword)
		{
			throw new ArgumentException("asyncKeyword");
		}
		syntaxKind = delegateKeyword.Kind();
		if (syntaxKind != SyntaxKind.DelegateKeyword)
		{
			throw new ArgumentException("delegateKeyword");
		}
		if (body == null)
		{
			throw new ArgumentNullException("body");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousMethodExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.AnonymousMethodExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)asyncKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)delegateKeyword.Node, (parameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax)parameterList.Green), (body == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode)body.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousMethodExpressionSyntax AnonymousMethodExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, CSharpSyntaxNode body)
	{
		return AnonymousMethodExpression(default, Token(SyntaxKind.DelegateKeyword), parameterList, body);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousMethodExpressionSyntax AnonymousMethodExpression(CSharpSyntaxNode body)
	{
		return AnonymousMethodExpression(default, Token(SyntaxKind.DelegateKeyword), null, body);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SimpleLambdaExpressionSyntax SimpleLambdaExpression(SyntaxToken asyncKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax parameter, SyntaxToken arrowToken, CSharpSyntaxNode body)
	{
		SyntaxKind syntaxKind = asyncKeyword.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.AsyncKeyword)
		{
			throw new ArgumentException("asyncKeyword");
		}
		if (parameter == null)
		{
			throw new ArgumentNullException("parameter");
		}
		syntaxKind = arrowToken.Kind();
		if (syntaxKind != SyntaxKind.EqualsGreaterThanToken)
		{
			throw new ArgumentException("arrowToken");
		}
		if (body == null)
		{
			throw new ArgumentNullException("body");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.SimpleLambdaExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.SimpleLambdaExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)asyncKeyword.Node, (parameter == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterSyntax)parameter.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)arrowToken.Node, (body == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode)body.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SimpleLambdaExpressionSyntax SimpleLambdaExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax parameter, CSharpSyntaxNode body)
	{
		return SimpleLambdaExpression(default, parameter, Token(SyntaxKind.EqualsGreaterThanToken), body);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.RefExpressionSyntax RefExpression(SyntaxToken refKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		SyntaxKind syntaxKind = refKeyword.Kind();
		if (syntaxKind != SyntaxKind.RefKeyword)
		{
			throw new ArgumentException("refKeyword");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.RefExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.RefExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)refKeyword.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.RefExpressionSyntax RefExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return RefExpression(Token(SyntaxKind.RefKeyword), expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedLambdaExpressionSyntax ParenthesizedLambdaExpression(SyntaxToken asyncKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, SyntaxToken arrowToken, CSharpSyntaxNode body)
	{
		SyntaxKind syntaxKind = asyncKeyword.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.AsyncKeyword)
		{
			throw new ArgumentException("asyncKeyword");
		}
		if (parameterList == null)
		{
			throw new ArgumentNullException("parameterList");
		}
		syntaxKind = arrowToken.Kind();
		if (syntaxKind != SyntaxKind.EqualsGreaterThanToken)
		{
			throw new ArgumentException("arrowToken");
		}
		if (body == null)
		{
			throw new ArgumentNullException("body");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedLambdaExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ParenthesizedLambdaExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)asyncKeyword.Node, (parameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax)parameterList.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)arrowToken.Node, (body == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode)body.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedLambdaExpressionSyntax ParenthesizedLambdaExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, CSharpSyntaxNode body)
	{
		return ParenthesizedLambdaExpression(default, parameterList, Token(SyntaxKind.EqualsGreaterThanToken), body);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedLambdaExpressionSyntax ParenthesizedLambdaExpression(CSharpSyntaxNode body)
	{
		return ParenthesizedLambdaExpression(default, ParameterList(), Token(SyntaxKind.EqualsGreaterThanToken), body);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax InitializerExpression(SyntaxKind kind, SyntaxToken openBraceToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax> expressions, SyntaxToken closeBraceToken)
	{
		if (kind - 8644 > (SyntaxKind)2 && kind != SyntaxKind.ComplexElementInitializerExpression)
		{
			throw new ArgumentException("kind");
		}
		SyntaxKind syntaxKind = openBraceToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException("openBraceToken");
		}
		syntaxKind = closeBraceToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException("closeBraceToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.InitializerExpression(kind, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBraceToken.Node, expressions.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax InitializerExpression(SyntaxKind kind, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax> expressions = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax>))
	{
		return InitializerExpression(kind, Token(SyntaxKind.OpenBraceToken), expressions, Token(SyntaxKind.CloseBraceToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax ObjectCreationExpression(SyntaxToken newKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax argumentList, Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax initializer)
	{
		SyntaxKind syntaxKind = newKeyword.Kind();
		if (syntaxKind != SyntaxKind.NewKeyword)
		{
			throw new ArgumentException("newKeyword");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ObjectCreationExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)newKeyword.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (argumentList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArgumentListSyntax)argumentList.Green), (initializer == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.InitializerExpressionSyntax)initializer.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax ObjectCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax argumentList, Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax initializer)
	{
		return ObjectCreationExpression(Token(SyntaxKind.NewKeyword), type, argumentList, initializer);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax ObjectCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return ObjectCreationExpression(Token(SyntaxKind.NewKeyword), type, null, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectMemberDeclaratorSyntax AnonymousObjectMemberDeclarator(Microsoft.CodeAnalysis.CSharp.Syntax.NameEqualsSyntax nameEquals, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectMemberDeclaratorSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.AnonymousObjectMemberDeclarator((nameEquals == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameEqualsSyntax)nameEquals.Green), (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectMemberDeclaratorSyntax AnonymousObjectMemberDeclarator(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return AnonymousObjectMemberDeclarator(null, expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectCreationExpressionSyntax AnonymousObjectCreationExpression(SyntaxToken newKeyword, SyntaxToken openBraceToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectMemberDeclaratorSyntax> initializers, SyntaxToken closeBraceToken)
	{
		SyntaxKind syntaxKind = newKeyword.Kind();
		if (syntaxKind != SyntaxKind.NewKeyword)
		{
			throw new ArgumentException("newKeyword");
		}
		syntaxKind = openBraceToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException("openBraceToken");
		}
		syntaxKind = closeBraceToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException("closeBraceToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectCreationExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.AnonymousObjectCreationExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)newKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBraceToken.Node, initializers.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AnonymousObjectMemberDeclaratorSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectCreationExpressionSyntax AnonymousObjectCreationExpression(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectMemberDeclaratorSyntax> initializers = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectMemberDeclaratorSyntax>))
	{
		return AnonymousObjectCreationExpression(Token(SyntaxKind.NewKeyword), Token(SyntaxKind.OpenBraceToken), initializers, Token(SyntaxKind.CloseBraceToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArrayCreationExpressionSyntax ArrayCreationExpression(SyntaxToken newKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ArrayTypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax initializer)
	{
		SyntaxKind syntaxKind = newKeyword.Kind();
		if (syntaxKind != SyntaxKind.NewKeyword)
		{
			throw new ArgumentException("newKeyword");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ArrayCreationExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ArrayCreationExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)newKeyword.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArrayTypeSyntax)type.Green), (initializer == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.InitializerExpressionSyntax)initializer.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArrayCreationExpressionSyntax ArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ArrayTypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax initializer)
	{
		return ArrayCreationExpression(Token(SyntaxKind.NewKeyword), type, initializer);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArrayCreationExpressionSyntax ArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ArrayTypeSyntax type)
	{
		return ArrayCreationExpression(Token(SyntaxKind.NewKeyword), type, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitArrayCreationExpressionSyntax ImplicitArrayCreationExpression(SyntaxToken newKeyword, SyntaxToken openBracketToken, SyntaxTokenList commas, SyntaxToken closeBracketToken, Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax initializer)
	{
		SyntaxKind syntaxKind = newKeyword.Kind();
		if (syntaxKind != SyntaxKind.NewKeyword)
		{
			throw new ArgumentException("newKeyword");
		}
		syntaxKind = openBracketToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBracketToken)
		{
			throw new ArgumentException("openBracketToken");
		}
		syntaxKind = closeBracketToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBracketToken)
		{
			throw new ArgumentException("closeBracketToken");
		}
		if (initializer == null)
		{
			throw new ArgumentNullException("initializer");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitArrayCreationExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ImplicitArrayCreationExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)newKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBracketToken.Node, commas.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBracketToken.Node, (initializer == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.InitializerExpressionSyntax)initializer.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitArrayCreationExpressionSyntax ImplicitArrayCreationExpression(SyntaxTokenList commas, Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax initializer)
	{
		return ImplicitArrayCreationExpression(Token(SyntaxKind.NewKeyword), Token(SyntaxKind.OpenBracketToken), commas, Token(SyntaxKind.CloseBracketToken), initializer);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitArrayCreationExpressionSyntax ImplicitArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax initializer)
	{
		return ImplicitArrayCreationExpression(Token(SyntaxKind.NewKeyword), Token(SyntaxKind.OpenBracketToken), default, Token(SyntaxKind.CloseBracketToken), initializer);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.StackAllocArrayCreationExpressionSyntax StackAllocArrayCreationExpression(SyntaxToken stackAllocKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax initializer)
	{
		SyntaxKind syntaxKind = stackAllocKeyword.Kind();
		if (syntaxKind != SyntaxKind.StackAllocKeyword)
		{
			throw new ArgumentException("stackAllocKeyword");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.StackAllocArrayCreationExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.StackAllocArrayCreationExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)stackAllocKeyword.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (initializer == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.InitializerExpressionSyntax)initializer.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.StackAllocArrayCreationExpressionSyntax StackAllocArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax initializer)
	{
		return StackAllocArrayCreationExpression(Token(SyntaxKind.StackAllocKeyword), type, initializer);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.StackAllocArrayCreationExpressionSyntax StackAllocArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return StackAllocArrayCreationExpression(Token(SyntaxKind.StackAllocKeyword), type, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitStackAllocArrayCreationExpressionSyntax ImplicitStackAllocArrayCreationExpression(SyntaxToken stackAllocKeyword, SyntaxToken openBracketToken, SyntaxToken closeBracketToken, Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax initializer)
	{
		SyntaxKind syntaxKind = stackAllocKeyword.Kind();
		if (syntaxKind != SyntaxKind.StackAllocKeyword)
		{
			throw new ArgumentException("stackAllocKeyword");
		}
		syntaxKind = openBracketToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBracketToken)
		{
			throw new ArgumentException("openBracketToken");
		}
		syntaxKind = closeBracketToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBracketToken)
		{
			throw new ArgumentException("closeBracketToken");
		}
		if (initializer == null)
		{
			throw new ArgumentNullException("initializer");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitStackAllocArrayCreationExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ImplicitStackAllocArrayCreationExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)stackAllocKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBracketToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBracketToken.Node, (initializer == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.InitializerExpressionSyntax)initializer.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitStackAllocArrayCreationExpressionSyntax ImplicitStackAllocArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax initializer)
	{
		return ImplicitStackAllocArrayCreationExpression(Token(SyntaxKind.StackAllocKeyword), Token(SyntaxKind.OpenBracketToken), Token(SyntaxKind.CloseBracketToken), initializer);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.QueryExpressionSyntax QueryExpression(Microsoft.CodeAnalysis.CSharp.Syntax.FromClauseSyntax fromClause, Microsoft.CodeAnalysis.CSharp.Syntax.QueryBodySyntax body)
	{
		if (fromClause == null)
		{
			throw new ArgumentNullException("fromClause");
		}
		if (body == null)
		{
			throw new ArgumentNullException("body");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.QueryExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.QueryExpression((fromClause == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FromClauseSyntax)fromClause.Green), (body == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.QueryBodySyntax)body.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.QueryBodySyntax QueryBody(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.QueryClauseSyntax> clauses, Microsoft.CodeAnalysis.CSharp.Syntax.SelectOrGroupClauseSyntax selectOrGroup, Microsoft.CodeAnalysis.CSharp.Syntax.QueryContinuationSyntax continuation)
	{
		if (selectOrGroup == null)
		{
			throw new ArgumentNullException("selectOrGroup");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.QueryBodySyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.QueryBody(clauses.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.QueryClauseSyntax>(), (selectOrGroup == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SelectOrGroupClauseSyntax)selectOrGroup.Green), (continuation == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.QueryContinuationSyntax)continuation.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.QueryBodySyntax QueryBody(Microsoft.CodeAnalysis.CSharp.Syntax.SelectOrGroupClauseSyntax selectOrGroup)
	{
		return QueryBody(default, selectOrGroup, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.FromClauseSyntax FromClause(SyntaxToken fromKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		SyntaxKind syntaxKind = fromKeyword.Kind();
		if (syntaxKind != SyntaxKind.FromKeyword)
		{
			throw new ArgumentException("fromKeyword");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		syntaxKind = inKeyword.Kind();
		if (syntaxKind != SyntaxKind.InKeyword)
		{
			throw new ArgumentException("inKeyword");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.FromClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.FromClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)fromKeyword.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)inKeyword.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.FromClauseSyntax FromClause(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return FromClause(Token(SyntaxKind.FromKeyword), type, identifier, Token(SyntaxKind.InKeyword), expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.FromClauseSyntax FromClause(SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return FromClause(Token(SyntaxKind.FromKeyword), null, identifier, Token(SyntaxKind.InKeyword), expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.FromClauseSyntax FromClause(string identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return FromClause(Token(SyntaxKind.FromKeyword), null, Identifier(identifier), Token(SyntaxKind.InKeyword), expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LetClauseSyntax LetClause(SyntaxToken letKeyword, SyntaxToken identifier, SyntaxToken equalsToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		SyntaxKind syntaxKind = letKeyword.Kind();
		if (syntaxKind != SyntaxKind.LetKeyword)
		{
			throw new ArgumentException("letKeyword");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		syntaxKind = equalsToken.Kind();
		if (syntaxKind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException("equalsToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.LetClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.LetClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)letKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)equalsToken.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LetClauseSyntax LetClause(SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return LetClause(Token(SyntaxKind.LetKeyword), identifier, Token(SyntaxKind.EqualsToken), expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LetClauseSyntax LetClause(string identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return LetClause(Token(SyntaxKind.LetKeyword), Identifier(identifier), Token(SyntaxKind.EqualsToken), expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.JoinClauseSyntax JoinClause(SyntaxToken joinKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax inExpression, SyntaxToken onKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax leftExpression, SyntaxToken equalsKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax rightExpression, Microsoft.CodeAnalysis.CSharp.Syntax.JoinIntoClauseSyntax into)
	{
		SyntaxKind syntaxKind = joinKeyword.Kind();
		if (syntaxKind != SyntaxKind.JoinKeyword)
		{
			throw new ArgumentException("joinKeyword");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		syntaxKind = inKeyword.Kind();
		if (syntaxKind != SyntaxKind.InKeyword)
		{
			throw new ArgumentException("inKeyword");
		}
		if (inExpression == null)
		{
			throw new ArgumentNullException("inExpression");
		}
		syntaxKind = onKeyword.Kind();
		if (syntaxKind != SyntaxKind.OnKeyword)
		{
			throw new ArgumentException("onKeyword");
		}
		if (leftExpression == null)
		{
			throw new ArgumentNullException("leftExpression");
		}
		syntaxKind = equalsKeyword.Kind();
		if (syntaxKind != SyntaxKind.EqualsKeyword)
		{
			throw new ArgumentException("equalsKeyword");
		}
		if (rightExpression == null)
		{
			throw new ArgumentNullException("rightExpression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.JoinClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.JoinClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)joinKeyword.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)inKeyword.Node, (inExpression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)inExpression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)onKeyword.Node, (leftExpression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)leftExpression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)equalsKeyword.Node, (rightExpression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)rightExpression.Green), (into == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.JoinIntoClauseSyntax)into.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.JoinClauseSyntax JoinClause(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax inExpression, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax leftExpression, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax rightExpression, Microsoft.CodeAnalysis.CSharp.Syntax.JoinIntoClauseSyntax into)
	{
		return JoinClause(Token(SyntaxKind.JoinKeyword), type, identifier, Token(SyntaxKind.InKeyword), inExpression, Token(SyntaxKind.OnKeyword), leftExpression, Token(SyntaxKind.EqualsKeyword), rightExpression, into);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.JoinClauseSyntax JoinClause(SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax inExpression, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax leftExpression, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax rightExpression)
	{
		return JoinClause(Token(SyntaxKind.JoinKeyword), null, identifier, Token(SyntaxKind.InKeyword), inExpression, Token(SyntaxKind.OnKeyword), leftExpression, Token(SyntaxKind.EqualsKeyword), rightExpression, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.JoinClauseSyntax JoinClause(string identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax inExpression, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax leftExpression, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax rightExpression)
	{
		return JoinClause(Token(SyntaxKind.JoinKeyword), null, Identifier(identifier), Token(SyntaxKind.InKeyword), inExpression, Token(SyntaxKind.OnKeyword), leftExpression, Token(SyntaxKind.EqualsKeyword), rightExpression, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.JoinIntoClauseSyntax JoinIntoClause(SyntaxToken intoKeyword, SyntaxToken identifier)
	{
		SyntaxKind syntaxKind = intoKeyword.Kind();
		if (syntaxKind != SyntaxKind.IntoKeyword)
		{
			throw new ArgumentException("intoKeyword");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.JoinIntoClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.JoinIntoClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)intoKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.JoinIntoClauseSyntax JoinIntoClause(SyntaxToken identifier)
	{
		return JoinIntoClause(Token(SyntaxKind.IntoKeyword), identifier);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.JoinIntoClauseSyntax JoinIntoClause(string identifier)
	{
		return JoinIntoClause(Token(SyntaxKind.IntoKeyword), Identifier(identifier));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.WhereClauseSyntax WhereClause(SyntaxToken whereKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition)
	{
		SyntaxKind syntaxKind = whereKeyword.Kind();
		if (syntaxKind != SyntaxKind.WhereKeyword)
		{
			throw new ArgumentException("whereKeyword");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.WhereClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.WhereClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)whereKeyword.Node, (condition == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)condition.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.WhereClauseSyntax WhereClause(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition)
	{
		return WhereClause(Token(SyntaxKind.WhereKeyword), condition);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OrderByClauseSyntax OrderByClause(SyntaxToken orderByKeyword, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.OrderingSyntax> orderings)
	{
		SyntaxKind syntaxKind = orderByKeyword.Kind();
		if (syntaxKind != SyntaxKind.OrderByKeyword)
		{
			throw new ArgumentException("orderByKeyword");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.OrderByClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.OrderByClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)orderByKeyword.Node, orderings.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OrderingSyntax>()).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OrderByClauseSyntax OrderByClause(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.OrderingSyntax> orderings = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.OrderingSyntax>))
	{
		return OrderByClause(Token(SyntaxKind.OrderByKeyword), orderings);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OrderingSyntax Ordering(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken ascendingOrDescendingKeyword)
	{
		if (kind - 8782 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		SyntaxKind syntaxKind = ascendingOrDescendingKeyword.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind - 8432 > SyntaxKind.List)
		{
			throw new ArgumentException("ascendingOrDescendingKeyword");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.OrderingSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Ordering(kind, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)ascendingOrDescendingKeyword.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OrderingSyntax Ordering(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return Ordering(kind, expression, default);
	}

	private static SyntaxKind GetOrderingAscendingOrDescendingKeywordKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.AscendingOrdering => SyntaxKind.AscendingKeyword, 
			SyntaxKind.DescendingOrdering => SyntaxKind.DescendingKeyword, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SelectClauseSyntax SelectClause(SyntaxToken selectKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		SyntaxKind syntaxKind = selectKeyword.Kind();
		if (syntaxKind != SyntaxKind.SelectKeyword)
		{
			throw new ArgumentException("selectKeyword");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.SelectClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.SelectClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)selectKeyword.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SelectClauseSyntax SelectClause(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return SelectClause(Token(SyntaxKind.SelectKeyword), expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.GroupClauseSyntax GroupClause(SyntaxToken groupKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax groupExpression, SyntaxToken byKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax byExpression)
	{
		SyntaxKind syntaxKind = groupKeyword.Kind();
		if (syntaxKind != SyntaxKind.GroupKeyword)
		{
			throw new ArgumentException("groupKeyword");
		}
		if (groupExpression == null)
		{
			throw new ArgumentNullException("groupExpression");
		}
		syntaxKind = byKeyword.Kind();
		if (syntaxKind != SyntaxKind.ByKeyword)
		{
			throw new ArgumentException("byKeyword");
		}
		if (byExpression == null)
		{
			throw new ArgumentNullException("byExpression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.GroupClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.GroupClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)groupKeyword.Node, (groupExpression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)groupExpression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)byKeyword.Node, (byExpression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)byExpression.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.GroupClauseSyntax GroupClause(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax groupExpression, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax byExpression)
	{
		return GroupClause(Token(SyntaxKind.GroupKeyword), groupExpression, Token(SyntaxKind.ByKeyword), byExpression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.QueryContinuationSyntax QueryContinuation(SyntaxToken intoKeyword, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.QueryBodySyntax body)
	{
		SyntaxKind syntaxKind = intoKeyword.Kind();
		if (syntaxKind != SyntaxKind.IntoKeyword)
		{
			throw new ArgumentException("intoKeyword");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		if (body == null)
		{
			throw new ArgumentNullException("body");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.QueryContinuationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.QueryContinuation((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)intoKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (body == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.QueryBodySyntax)body.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.QueryContinuationSyntax QueryContinuation(SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.QueryBodySyntax body)
	{
		return QueryContinuation(Token(SyntaxKind.IntoKeyword), identifier, body);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.QueryContinuationSyntax QueryContinuation(string identifier, Microsoft.CodeAnalysis.CSharp.Syntax.QueryBodySyntax body)
	{
		return QueryContinuation(Token(SyntaxKind.IntoKeyword), Identifier(identifier), body);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OmittedArraySizeExpressionSyntax OmittedArraySizeExpression(SyntaxToken omittedArraySizeExpressionToken)
	{
		SyntaxKind syntaxKind = omittedArraySizeExpressionToken.Kind();
		if (syntaxKind != SyntaxKind.OmittedArraySizeExpressionToken)
		{
			throw new ArgumentException("omittedArraySizeExpressionToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.OmittedArraySizeExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.OmittedArraySizeExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)omittedArraySizeExpressionToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OmittedArraySizeExpressionSyntax OmittedArraySizeExpression()
	{
		return OmittedArraySizeExpression(Token(SyntaxKind.OmittedArraySizeExpressionToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterpolatedStringExpressionSyntax InterpolatedStringExpression(SyntaxToken stringStartToken, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InterpolatedStringContentSyntax> contents, SyntaxToken stringEndToken)
	{
		SyntaxKind syntaxKind = stringStartToken.Kind();
		if (syntaxKind != SyntaxKind.InterpolatedStringStartToken && syntaxKind != SyntaxKind.InterpolatedVerbatimStringStartToken)
		{
			throw new ArgumentException("stringStartToken");
		}
		syntaxKind = stringEndToken.Kind();
		if (syntaxKind != SyntaxKind.InterpolatedStringEndToken)
		{
			throw new ArgumentException("stringEndToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.InterpolatedStringExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.InterpolatedStringExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)stringStartToken.Node, contents.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.InterpolatedStringContentSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)stringEndToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterpolatedStringExpressionSyntax InterpolatedStringExpression(SyntaxToken stringStartToken, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InterpolatedStringContentSyntax> contents)
	{
		return InterpolatedStringExpression(stringStartToken, contents, Token(SyntaxKind.InterpolatedStringEndToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterpolatedStringExpressionSyntax InterpolatedStringExpression(SyntaxToken stringStartToken)
	{
		return InterpolatedStringExpression(stringStartToken, default, Token(SyntaxKind.InterpolatedStringEndToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IsPatternExpressionSyntax IsPatternExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken isKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.PatternSyntax pattern)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		SyntaxKind syntaxKind = isKeyword.Kind();
		if (syntaxKind != SyntaxKind.IsKeyword)
		{
			throw new ArgumentException("isKeyword");
		}
		if (pattern == null)
		{
			throw new ArgumentNullException("pattern");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.IsPatternExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.IsPatternExpression((expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)isKeyword.Node, (pattern == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PatternSyntax)pattern.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IsPatternExpressionSyntax IsPatternExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Syntax.PatternSyntax pattern)
	{
		return IsPatternExpression(expression, Token(SyntaxKind.IsKeyword), pattern);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ThrowExpressionSyntax ThrowExpression(SyntaxToken throwKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		SyntaxKind syntaxKind = throwKeyword.Kind();
		if (syntaxKind != SyntaxKind.ThrowKeyword)
		{
			throw new ArgumentException("throwKeyword");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ThrowExpressionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ThrowExpression((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)throwKeyword.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ThrowExpressionSyntax ThrowExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return ThrowExpression(Token(SyntaxKind.ThrowKeyword), expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.WhenClauseSyntax WhenClause(SyntaxToken whenKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition)
	{
		SyntaxKind syntaxKind = whenKeyword.Kind();
		if (syntaxKind != SyntaxKind.WhenKeyword)
		{
			throw new ArgumentException("whenKeyword");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.WhenClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.WhenClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)whenKeyword.Node, (condition == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)condition.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.WhenClauseSyntax WhenClause(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition)
	{
		return WhenClause(Token(SyntaxKind.WhenKeyword), condition);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DeclarationPatternSyntax DeclarationPattern(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.VariableDesignationSyntax designation)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (designation == null)
		{
			throw new ArgumentNullException("designation");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.DeclarationPatternSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.DeclarationPattern((type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (designation == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDesignationSyntax)designation.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConstantPatternSyntax ConstantPattern(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ConstantPatternSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ConstantPattern((expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterpolatedStringTextSyntax InterpolatedStringText(SyntaxToken textToken)
	{
		SyntaxKind syntaxKind = textToken.Kind();
		if (syntaxKind != SyntaxKind.InterpolatedStringTextToken)
		{
			throw new ArgumentException("textToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.InterpolatedStringTextSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.InterpolatedStringText((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)textToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterpolatedStringTextSyntax InterpolatedStringText()
	{
		return InterpolatedStringText(Token(SyntaxKind.InterpolatedStringTextToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationSyntax Interpolation(SyntaxToken openBraceToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationAlignmentClauseSyntax alignmentClause, Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationFormatClauseSyntax formatClause, SyntaxToken closeBraceToken)
	{
		SyntaxKind syntaxKind = openBraceToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException("openBraceToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		syntaxKind = closeBraceToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException("closeBraceToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Interpolation((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBraceToken.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (alignmentClause == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.InterpolationAlignmentClauseSyntax)alignmentClause.Green), (formatClause == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.InterpolationFormatClauseSyntax)formatClause.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationSyntax Interpolation(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationAlignmentClauseSyntax alignmentClause, Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationFormatClauseSyntax formatClause)
	{
		return Interpolation(Token(SyntaxKind.OpenBraceToken), expression, alignmentClause, formatClause, Token(SyntaxKind.CloseBraceToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationSyntax Interpolation(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return Interpolation(Token(SyntaxKind.OpenBraceToken), expression, null, null, Token(SyntaxKind.CloseBraceToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationAlignmentClauseSyntax InterpolationAlignmentClause(SyntaxToken commaToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationAlignmentClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.InterpolationAlignmentClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)commaToken.Node, (value == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)value.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationFormatClauseSyntax InterpolationFormatClause(SyntaxToken colonToken, SyntaxToken formatStringToken)
	{
		SyntaxKind syntaxKind = formatStringToken.Kind();
		if (syntaxKind != SyntaxKind.InterpolatedStringTextToken)
		{
			throw new ArgumentException("formatStringToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationFormatClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.InterpolationFormatClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)colonToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)formatStringToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationFormatClauseSyntax InterpolationFormatClause(SyntaxToken colonToken)
	{
		return InterpolationFormatClause(colonToken, Token(SyntaxKind.InterpolatedStringTextToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.GlobalStatementSyntax GlobalStatement(Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.GlobalStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.GlobalStatement((statement == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax)statement.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax Block(SyntaxToken openBraceToken, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statements, SyntaxToken closeBraceToken)
	{
		SyntaxKind syntaxKind = openBraceToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException("openBraceToken");
		}
		syntaxKind = closeBraceToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException("closeBraceToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Block((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBraceToken.Node, statements.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax Block(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statements = default(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax>))
	{
		return Block(Token(SyntaxKind.OpenBraceToken), statements, Token(SyntaxKind.CloseBraceToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax LocalFunctionStatement(SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (returnType == null)
		{
			throw new ArgumentNullException("returnType");
		}
		SyntaxKind syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		if (parameterList == null)
		{
			throw new ArgumentNullException("parameterList");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.LocalFunctionStatement(modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (returnType == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)returnType.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (typeParameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterListSyntax)typeParameterList.Green), (parameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax)parameterList.Green), constraintClauses.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterConstraintClauseSyntax>(), (body == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)body.Green), (expressionBody == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArrowExpressionClauseSyntax)expressionBody.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax LocalFunctionStatement(SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody)
	{
		return LocalFunctionStatement(modifiers, returnType, identifier, typeParameterList, parameterList, constraintClauses, body, expressionBody, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax LocalFunctionStatement(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, SyntaxToken identifier)
	{
		return LocalFunctionStatement(default, returnType, identifier, null, ParameterList(), default, null, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax LocalFunctionStatement(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, string identifier)
	{
		return LocalFunctionStatement(default, returnType, Identifier(identifier), null, ParameterList(), default, null, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax LocalDeclarationStatement(SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		if (declaration == null)
		{
			throw new ArgumentNullException("declaration");
		}
		SyntaxKind syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.LocalDeclarationStatement(modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (declaration == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDeclarationSyntax)declaration.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax LocalDeclarationStatement(SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration)
	{
		return LocalDeclarationStatement(modifiers, declaration, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration)
	{
		return LocalDeclarationStatement(default, declaration, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax VariableDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax> variables)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.VariableDeclaration((type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), variables.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDeclaratorSyntax>()).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax VariableDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return VariableDeclaration(type, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax VariableDeclarator(SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.BracketedArgumentListSyntax argumentList, Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax initializer)
	{
		SyntaxKind syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.VariableDeclarator((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (argumentList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BracketedArgumentListSyntax)argumentList.Green), (initializer == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EqualsValueClauseSyntax)initializer.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax VariableDeclarator(SyntaxToken identifier)
	{
		return VariableDeclarator(identifier, null, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax VariableDeclarator(string identifier)
	{
		return VariableDeclarator(Identifier(identifier), null, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax EqualsValueClause(SyntaxToken equalsToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax value)
	{
		SyntaxKind syntaxKind = equalsToken.Kind();
		if (syntaxKind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException("equalsToken");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.EqualsValueClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)equalsToken.Node, (value == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)value.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax EqualsValueClause(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax value)
	{
		return EqualsValueClause(Token(SyntaxKind.EqualsToken), value);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SingleVariableDesignationSyntax SingleVariableDesignation(SyntaxToken identifier)
	{
		SyntaxKind syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.SingleVariableDesignationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.SingleVariableDesignation((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DiscardDesignationSyntax DiscardDesignation(SyntaxToken underscoreToken)
	{
		SyntaxKind syntaxKind = underscoreToken.Kind();
		if (syntaxKind != SyntaxKind.UnderscoreToken)
		{
			throw new ArgumentException("underscoreToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.DiscardDesignationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.DiscardDesignation((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)underscoreToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DiscardDesignationSyntax DiscardDesignation()
	{
		return DiscardDesignation(Token(SyntaxKind.UnderscoreToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedVariableDesignationSyntax ParenthesizedVariableDesignation(SyntaxToken openParenToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.VariableDesignationSyntax> variables, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedVariableDesignationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ParenthesizedVariableDesignation((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, variables.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDesignationSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedVariableDesignationSyntax ParenthesizedVariableDesignation(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.VariableDesignationSyntax> variables = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.VariableDesignationSyntax>))
	{
		return ParenthesizedVariableDesignation(Token(SyntaxKind.OpenParenToken), variables, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax ExpressionStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		SyntaxKind syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ExpressionStatement((expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax ExpressionStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return ExpressionStatement(expression, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EmptyStatementSyntax EmptyStatement(SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.EmptyStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.EmptyStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EmptyStatementSyntax EmptyStatement()
	{
		return EmptyStatement(Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LabeledStatementSyntax LabeledStatement(SyntaxToken identifier, SyntaxToken colonToken, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		SyntaxKind syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		syntaxKind = colonToken.Kind();
		if (syntaxKind != SyntaxKind.ColonToken)
		{
			throw new ArgumentException("colonToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.LabeledStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.LabeledStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)colonToken.Node, (statement == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax)statement.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LabeledStatementSyntax LabeledStatement(SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return LabeledStatement(identifier, Token(SyntaxKind.ColonToken), statement);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LabeledStatementSyntax LabeledStatement(string identifier, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return LabeledStatement(Identifier(identifier), Token(SyntaxKind.ColonToken), statement);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.GotoStatementSyntax GotoStatement(SyntaxKind kind, SyntaxToken gotoKeyword, SyntaxToken caseOrDefaultKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (kind - 8800 > (SyntaxKind)2)
		{
			throw new ArgumentException("kind");
		}
		SyntaxKind syntaxKind = gotoKeyword.Kind();
		if (syntaxKind != SyntaxKind.GotoKeyword)
		{
			throw new ArgumentException("gotoKeyword");
		}
		syntaxKind = caseOrDefaultKeyword.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind - 8332 > SyntaxKind.List)
		{
			throw new ArgumentException("caseOrDefaultKeyword");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.GotoStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.GotoStatement(kind, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)gotoKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)caseOrDefaultKeyword.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.GotoStatementSyntax GotoStatement(SyntaxKind kind, SyntaxToken caseOrDefaultKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return GotoStatement(kind, Token(SyntaxKind.GotoKeyword), caseOrDefaultKeyword, expression, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.GotoStatementSyntax GotoStatement(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression = null)
	{
		return GotoStatement(kind, Token(SyntaxKind.GotoKeyword), default, expression, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BreakStatementSyntax BreakStatement(SyntaxToken breakKeyword, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = breakKeyword.Kind();
		if (syntaxKind != SyntaxKind.BreakKeyword)
		{
			throw new ArgumentException("breakKeyword");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.BreakStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.BreakStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)breakKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BreakStatementSyntax BreakStatement()
	{
		return BreakStatement(Token(SyntaxKind.BreakKeyword), Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ContinueStatementSyntax ContinueStatement(SyntaxToken continueKeyword, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = continueKeyword.Kind();
		if (syntaxKind != SyntaxKind.ContinueKeyword)
		{
			throw new ArgumentException("continueKeyword");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ContinueStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ContinueStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)continueKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ContinueStatementSyntax ContinueStatement()
	{
		return ContinueStatement(Token(SyntaxKind.ContinueKeyword), Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax ReturnStatement(SyntaxToken returnKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = returnKeyword.Kind();
		if (syntaxKind != SyntaxKind.ReturnKeyword)
		{
			throw new ArgumentException("returnKeyword");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ReturnStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)returnKeyword.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax ReturnStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression = null)
	{
		return ReturnStatement(Token(SyntaxKind.ReturnKeyword), expression, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ThrowStatementSyntax ThrowStatement(SyntaxToken throwKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = throwKeyword.Kind();
		if (syntaxKind != SyntaxKind.ThrowKeyword)
		{
			throw new ArgumentException("throwKeyword");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ThrowStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ThrowStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)throwKeyword.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ThrowStatementSyntax ThrowStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression = null)
	{
		return ThrowStatement(Token(SyntaxKind.ThrowKeyword), expression, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.YieldStatementSyntax YieldStatement(SyntaxKind kind, SyntaxToken yieldKeyword, SyntaxToken returnOrBreakKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (kind - 8806 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		SyntaxKind syntaxKind = yieldKeyword.Kind();
		if (syntaxKind != SyntaxKind.YieldKeyword)
		{
			throw new ArgumentException("yieldKeyword");
		}
		syntaxKind = returnOrBreakKeyword.Kind();
		if (syntaxKind != SyntaxKind.BreakKeyword && syntaxKind != SyntaxKind.ReturnKeyword)
		{
			throw new ArgumentException("returnOrBreakKeyword");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.YieldStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.YieldStatement(kind, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)yieldKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)returnOrBreakKeyword.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.YieldStatementSyntax YieldStatement(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression = null)
	{
		return YieldStatement(kind, Token(SyntaxKind.YieldKeyword), Token(GetYieldStatementReturnOrBreakKeywordKind(kind)), expression, Token(SyntaxKind.SemicolonToken));
	}

	private static SyntaxKind GetYieldStatementReturnOrBreakKeywordKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.YieldReturnStatement => SyntaxKind.ReturnKeyword, 
			SyntaxKind.YieldBreakStatement => SyntaxKind.BreakKeyword, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.WhileStatementSyntax WhileStatement(SyntaxToken whileKeyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, SyntaxToken closeParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		SyntaxKind syntaxKind = whileKeyword.Kind();
		if (syntaxKind != SyntaxKind.WhileKeyword)
		{
			throw new ArgumentException("whileKeyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.WhileStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.WhileStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)whileKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (condition == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)condition.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node, (statement == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax)statement.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.WhileStatementSyntax WhileStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return WhileStatement(Token(SyntaxKind.WhileKeyword), Token(SyntaxKind.OpenParenToken), condition, Token(SyntaxKind.CloseParenToken), statement);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DoStatementSyntax DoStatement(SyntaxToken doKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = doKeyword.Kind();
		if (syntaxKind != SyntaxKind.DoKeyword)
		{
			throw new ArgumentException("doKeyword");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		syntaxKind = whileKeyword.Kind();
		if (syntaxKind != SyntaxKind.WhileKeyword)
		{
			throw new ArgumentException("whileKeyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.DoStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.DoStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)doKeyword.Node, (statement == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax)statement.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)whileKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (condition == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)condition.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DoStatementSyntax DoStatement(Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition)
	{
		return DoStatement(Token(SyntaxKind.DoKeyword), statement, Token(SyntaxKind.WhileKeyword), Token(SyntaxKind.OpenParenToken), condition, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax ForStatement(SyntaxToken forKeyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax> initializers, SyntaxToken firstSemicolonToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, SyntaxToken secondSemicolonToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax> incrementors, SyntaxToken closeParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		SyntaxKind syntaxKind = forKeyword.Kind();
		if (syntaxKind != SyntaxKind.ForKeyword)
		{
			throw new ArgumentException("forKeyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		syntaxKind = firstSemicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("firstSemicolonToken");
		}
		syntaxKind = secondSemicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("secondSemicolonToken");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ForStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)forKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (declaration == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDeclarationSyntax)declaration.Green), initializers.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)firstSemicolonToken.Node, (condition == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)condition.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)secondSemicolonToken.Node, incrementors.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node, (statement == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax)statement.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax ForStatement(Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax> initializers, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax> incrementors, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return ForStatement(Token(SyntaxKind.ForKeyword), Token(SyntaxKind.OpenParenToken), declaration, initializers, Token(SyntaxKind.SemicolonToken), condition, Token(SyntaxKind.SemicolonToken), incrementors, Token(SyntaxKind.CloseParenToken), statement);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax ForStatement(Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return ForStatement(Token(SyntaxKind.ForKeyword), Token(SyntaxKind.OpenParenToken), null, default, Token(SyntaxKind.SemicolonToken), null, Token(SyntaxKind.SemicolonToken), default, Token(SyntaxKind.CloseParenToken), statement);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax ForEachStatement(SyntaxToken forEachKeyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken closeParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		SyntaxKind syntaxKind = forEachKeyword.Kind();
		if (syntaxKind != SyntaxKind.ForEachKeyword)
		{
			throw new ArgumentException("forEachKeyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		syntaxKind = inKeyword.Kind();
		if (syntaxKind != SyntaxKind.InKeyword)
		{
			throw new ArgumentException("inKeyword");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ForEachStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)forEachKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)inKeyword.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node, (statement == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax)statement.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax ForEachStatement(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return ForEachStatement(Token(SyntaxKind.ForEachKeyword), Token(SyntaxKind.OpenParenToken), type, identifier, Token(SyntaxKind.InKeyword), expression, Token(SyntaxKind.CloseParenToken), statement);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax ForEachStatement(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, string identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return ForEachStatement(Token(SyntaxKind.ForEachKeyword), Token(SyntaxKind.OpenParenToken), type, Identifier(identifier), Token(SyntaxKind.InKeyword), expression, Token(SyntaxKind.CloseParenToken), statement);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ForEachVariableStatementSyntax ForEachVariableStatement(SyntaxToken forEachKeyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax variable, SyntaxToken inKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken closeParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		SyntaxKind syntaxKind = forEachKeyword.Kind();
		if (syntaxKind != SyntaxKind.ForEachKeyword)
		{
			throw new ArgumentException("forEachKeyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (variable == null)
		{
			throw new ArgumentNullException("variable");
		}
		syntaxKind = inKeyword.Kind();
		if (syntaxKind != SyntaxKind.InKeyword)
		{
			throw new ArgumentException("inKeyword");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ForEachVariableStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ForEachVariableStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)forEachKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (variable == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)variable.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)inKeyword.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node, (statement == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax)statement.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ForEachVariableStatementSyntax ForEachVariableStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax variable, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return ForEachVariableStatement(Token(SyntaxKind.ForEachKeyword), Token(SyntaxKind.OpenParenToken), variable, Token(SyntaxKind.InKeyword), expression, Token(SyntaxKind.CloseParenToken), statement);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.UsingStatementSyntax UsingStatement(SyntaxToken usingKeyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken closeParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		SyntaxKind syntaxKind = usingKeyword.Kind();
		if (syntaxKind != SyntaxKind.UsingKeyword)
		{
			throw new ArgumentException("usingKeyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.UsingStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.UsingStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)usingKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (declaration == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDeclarationSyntax)declaration.Green), (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node, (statement == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax)statement.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.UsingStatementSyntax UsingStatement(Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return UsingStatement(Token(SyntaxKind.UsingKeyword), Token(SyntaxKind.OpenParenToken), declaration, expression, Token(SyntaxKind.CloseParenToken), statement);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.UsingStatementSyntax UsingStatement(Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return UsingStatement(Token(SyntaxKind.UsingKeyword), Token(SyntaxKind.OpenParenToken), null, null, Token(SyntaxKind.CloseParenToken), statement);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.FixedStatementSyntax FixedStatement(SyntaxToken fixedKeyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration, SyntaxToken closeParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		SyntaxKind syntaxKind = fixedKeyword.Kind();
		if (syntaxKind != SyntaxKind.FixedKeyword)
		{
			throw new ArgumentException("fixedKeyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (declaration == null)
		{
			throw new ArgumentNullException("declaration");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.FixedStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.FixedStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)fixedKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (declaration == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDeclarationSyntax)declaration.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node, (statement == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax)statement.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.FixedStatementSyntax FixedStatement(Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return FixedStatement(Token(SyntaxKind.FixedKeyword), Token(SyntaxKind.OpenParenToken), declaration, Token(SyntaxKind.CloseParenToken), statement);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CheckedStatementSyntax CheckedStatement(SyntaxKind kind, SyntaxToken keyword, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax block)
	{
		if (kind - 8815 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind - 8379 > SyntaxKind.List)
		{
			throw new ArgumentException("keyword");
		}
		if (block == null)
		{
			throw new ArgumentNullException("block");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CheckedStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CheckedStatement(kind, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (block == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)block.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CheckedStatementSyntax CheckedStatement(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax block = null)
	{
		return CheckedStatement(kind, Token(GetCheckedStatementKeywordKind(kind)), block ?? Block());
	}

	private static SyntaxKind GetCheckedStatementKeywordKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.CheckedStatement => SyntaxKind.CheckedKeyword, 
			SyntaxKind.UncheckedStatement => SyntaxKind.UncheckedKeyword, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.UnsafeStatementSyntax UnsafeStatement(SyntaxToken unsafeKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax block)
	{
		SyntaxKind syntaxKind = unsafeKeyword.Kind();
		if (syntaxKind != SyntaxKind.UnsafeKeyword)
		{
			throw new ArgumentException("unsafeKeyword");
		}
		if (block == null)
		{
			throw new ArgumentNullException("block");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.UnsafeStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.UnsafeStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)unsafeKeyword.Node, (block == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)block.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.UnsafeStatementSyntax UnsafeStatement(Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax block = null)
	{
		return UnsafeStatement(Token(SyntaxKind.UnsafeKeyword), block ?? Block());
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LockStatementSyntax LockStatement(SyntaxToken lockKeyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken closeParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		SyntaxKind syntaxKind = lockKeyword.Kind();
		if (syntaxKind != SyntaxKind.LockKeyword)
		{
			throw new ArgumentException("lockKeyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.LockStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.LockStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)lockKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node, (statement == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax)statement.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LockStatementSyntax LockStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return LockStatement(Token(SyntaxKind.LockKeyword), Token(SyntaxKind.OpenParenToken), expression, Token(SyntaxKind.CloseParenToken), statement);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax IfStatement(SyntaxToken ifKeyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, SyntaxToken closeParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement, Microsoft.CodeAnalysis.CSharp.Syntax.ElseClauseSyntax @else)
	{
		SyntaxKind syntaxKind = ifKeyword.Kind();
		if (syntaxKind != SyntaxKind.IfKeyword)
		{
			throw new ArgumentException("ifKeyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.IfStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)ifKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (condition == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)condition.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node, (statement == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax)statement.Green), (@else == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ElseClauseSyntax)@else.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax IfStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement, Microsoft.CodeAnalysis.CSharp.Syntax.ElseClauseSyntax @else)
	{
		return IfStatement(Token(SyntaxKind.IfKeyword), Token(SyntaxKind.OpenParenToken), condition, Token(SyntaxKind.CloseParenToken), statement, @else);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax IfStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return IfStatement(Token(SyntaxKind.IfKeyword), Token(SyntaxKind.OpenParenToken), condition, Token(SyntaxKind.CloseParenToken), statement, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ElseClauseSyntax ElseClause(SyntaxToken elseKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		SyntaxKind syntaxKind = elseKeyword.Kind();
		if (syntaxKind != SyntaxKind.ElseKeyword)
		{
			throw new ArgumentException("elseKeyword");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ElseClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ElseClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)elseKeyword.Node, (statement == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax)statement.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ElseClauseSyntax ElseClause(Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return ElseClause(Token(SyntaxKind.ElseKeyword), statement);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SwitchStatementSyntax SwitchStatement(SyntaxToken switchKeyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxToken closeParenToken, SyntaxToken openBraceToken, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.SwitchSectionSyntax> sections, SyntaxToken closeBraceToken)
	{
		SyntaxKind syntaxKind = switchKeyword.Kind();
		if (syntaxKind != SyntaxKind.SwitchKeyword)
		{
			throw new ArgumentException("switchKeyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		syntaxKind = openBraceToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException("openBraceToken");
		}
		syntaxKind = closeBraceToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException("closeBraceToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.SwitchStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.SwitchStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)switchKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBraceToken.Node, sections.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SwitchSectionSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SwitchStatementSyntax SwitchStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.SwitchSectionSyntax> sections)
	{
		return SwitchStatement(Token(SyntaxKind.SwitchKeyword), Token(SyntaxKind.OpenParenToken), expression, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), sections, Token(SyntaxKind.CloseBraceToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SwitchStatementSyntax SwitchStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return SwitchStatement(Token(SyntaxKind.SwitchKeyword), Token(SyntaxKind.OpenParenToken), expression, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SwitchSectionSyntax SwitchSection(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.SwitchLabelSyntax> labels, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statements)
	{
		return (Microsoft.CodeAnalysis.CSharp.Syntax.SwitchSectionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.SwitchSection(labels.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SwitchLabelSyntax>(), statements.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax>()).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SwitchSectionSyntax SwitchSection()
	{
		return SwitchSection(default, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CasePatternSwitchLabelSyntax CasePatternSwitchLabel(SyntaxToken keyword, Microsoft.CodeAnalysis.CSharp.Syntax.PatternSyntax pattern, Microsoft.CodeAnalysis.CSharp.Syntax.WhenClauseSyntax whenClause, SyntaxToken colonToken)
	{
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind != SyntaxKind.CaseKeyword)
		{
			throw new ArgumentException("keyword");
		}
		if (pattern == null)
		{
			throw new ArgumentNullException("pattern");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CasePatternSwitchLabelSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CasePatternSwitchLabel((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (pattern == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PatternSyntax)pattern.Green), (whenClause == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.WhenClauseSyntax)whenClause.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)colonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CasePatternSwitchLabelSyntax CasePatternSwitchLabel(Microsoft.CodeAnalysis.CSharp.Syntax.PatternSyntax pattern, Microsoft.CodeAnalysis.CSharp.Syntax.WhenClauseSyntax whenClause, SyntaxToken colonToken)
	{
		return CasePatternSwitchLabel(Token(SyntaxKind.CaseKeyword), pattern, whenClause, colonToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CasePatternSwitchLabelSyntax CasePatternSwitchLabel(Microsoft.CodeAnalysis.CSharp.Syntax.PatternSyntax pattern, SyntaxToken colonToken)
	{
		return CasePatternSwitchLabel(Token(SyntaxKind.CaseKeyword), pattern, null, colonToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CaseSwitchLabelSyntax CaseSwitchLabel(SyntaxToken keyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax value, SyntaxToken colonToken)
	{
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind != SyntaxKind.CaseKeyword)
		{
			throw new ArgumentException("keyword");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CaseSwitchLabelSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CaseSwitchLabel((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (value == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)value.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)colonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CaseSwitchLabelSyntax CaseSwitchLabel(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax value, SyntaxToken colonToken)
	{
		return CaseSwitchLabel(Token(SyntaxKind.CaseKeyword), value, colonToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DefaultSwitchLabelSyntax DefaultSwitchLabel(SyntaxToken keyword, SyntaxToken colonToken)
	{
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind != SyntaxKind.DefaultKeyword)
		{
			throw new ArgumentException("keyword");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.DefaultSwitchLabelSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.DefaultSwitchLabel((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)colonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DefaultSwitchLabelSyntax DefaultSwitchLabel(SyntaxToken colonToken)
	{
		return DefaultSwitchLabel(Token(SyntaxKind.DefaultKeyword), colonToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax TryStatement(SyntaxToken tryKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax block, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.CatchClauseSyntax> catches, Microsoft.CodeAnalysis.CSharp.Syntax.FinallyClauseSyntax @finally)
	{
		SyntaxKind syntaxKind = tryKeyword.Kind();
		if (syntaxKind != SyntaxKind.TryKeyword)
		{
			throw new ArgumentException("tryKeyword");
		}
		if (block == null)
		{
			throw new ArgumentNullException("block");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.TryStatement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)tryKeyword.Node, (block == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)block.Green), catches.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CatchClauseSyntax>(), (@finally == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FinallyClauseSyntax)@finally.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax TryStatement(Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax block, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.CatchClauseSyntax> catches, Microsoft.CodeAnalysis.CSharp.Syntax.FinallyClauseSyntax @finally)
	{
		return TryStatement(Token(SyntaxKind.TryKeyword), block, catches, @finally);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax TryStatement(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.CatchClauseSyntax> catches = default(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.CatchClauseSyntax>))
	{
		return TryStatement(Token(SyntaxKind.TryKeyword), Block(), catches, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CatchClauseSyntax CatchClause(SyntaxToken catchKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.CatchDeclarationSyntax declaration, Microsoft.CodeAnalysis.CSharp.Syntax.CatchFilterClauseSyntax filter, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax block)
	{
		SyntaxKind syntaxKind = catchKeyword.Kind();
		if (syntaxKind != SyntaxKind.CatchKeyword)
		{
			throw new ArgumentException("catchKeyword");
		}
		if (block == null)
		{
			throw new ArgumentNullException("block");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CatchClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CatchClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)catchKeyword.Node, (declaration == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CatchDeclarationSyntax)declaration.Green), (filter == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CatchFilterClauseSyntax)filter.Green), (block == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)block.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CatchClauseSyntax CatchClause(Microsoft.CodeAnalysis.CSharp.Syntax.CatchDeclarationSyntax declaration, Microsoft.CodeAnalysis.CSharp.Syntax.CatchFilterClauseSyntax filter, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax block)
	{
		return CatchClause(Token(SyntaxKind.CatchKeyword), declaration, filter, block);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CatchClauseSyntax CatchClause()
	{
		return CatchClause(Token(SyntaxKind.CatchKeyword), null, null, Block());
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CatchDeclarationSyntax CatchDeclaration(SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken identifier, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CatchDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CatchDeclaration((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CatchDeclarationSyntax CatchDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken identifier)
	{
		return CatchDeclaration(Token(SyntaxKind.OpenParenToken), type, identifier, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CatchDeclarationSyntax CatchDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return CatchDeclaration(Token(SyntaxKind.OpenParenToken), type, default, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CatchFilterClauseSyntax CatchFilterClause(SyntaxToken whenKeyword, SyntaxToken openParenToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax filterExpression, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = whenKeyword.Kind();
		if (syntaxKind != SyntaxKind.WhenKeyword)
		{
			throw new ArgumentException("whenKeyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		if (filterExpression == null)
		{
			throw new ArgumentNullException("filterExpression");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CatchFilterClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CatchFilterClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)whenKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (filterExpression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)filterExpression.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CatchFilterClauseSyntax CatchFilterClause(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax filterExpression)
	{
		return CatchFilterClause(Token(SyntaxKind.WhenKeyword), Token(SyntaxKind.OpenParenToken), filterExpression, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.FinallyClauseSyntax FinallyClause(SyntaxToken finallyKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax block)
	{
		SyntaxKind syntaxKind = finallyKeyword.Kind();
		if (syntaxKind != SyntaxKind.FinallyKeyword)
		{
			throw new ArgumentException("finallyKeyword");
		}
		if (block == null)
		{
			throw new ArgumentNullException("block");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.FinallyClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.FinallyClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)finallyKeyword.Node, (block == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)block.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.FinallyClauseSyntax FinallyClause(Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax block = null)
	{
		return FinallyClause(Token(SyntaxKind.FinallyKeyword), block ?? Block());
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax CompilationUnit(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExternAliasDirectiveSyntax> externs, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax> usings, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members, SyntaxToken endOfFileToken)
	{
		SyntaxKind syntaxKind = endOfFileToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfFileToken)
		{
			throw new ArgumentException("endOfFileToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CompilationUnit(externs.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExternAliasDirectiveSyntax>(), usings.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingDirectiveSyntax>(), attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), members.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberDeclarationSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfFileToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax CompilationUnit(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExternAliasDirectiveSyntax> externs, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax> usings, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members)
	{
		return CompilationUnit(externs, usings, attributeLists, members, Token(SyntaxKind.EndOfFileToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax CompilationUnit()
	{
		return CompilationUnit(default, default, default, default, Token(SyntaxKind.EndOfFileToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ExternAliasDirectiveSyntax ExternAliasDirective(SyntaxToken externKeyword, SyntaxToken aliasKeyword, SyntaxToken identifier, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = externKeyword.Kind();
		if (syntaxKind != SyntaxKind.ExternKeyword)
		{
			throw new ArgumentException("externKeyword");
		}
		syntaxKind = aliasKeyword.Kind();
		if (syntaxKind != SyntaxKind.AliasKeyword)
		{
			throw new ArgumentException("aliasKeyword");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ExternAliasDirectiveSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ExternAliasDirective((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)externKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)aliasKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ExternAliasDirectiveSyntax ExternAliasDirective(SyntaxToken identifier)
	{
		return ExternAliasDirective(Token(SyntaxKind.ExternKeyword), Token(SyntaxKind.AliasKeyword), identifier, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ExternAliasDirectiveSyntax ExternAliasDirective(string identifier)
	{
		return ExternAliasDirective(Token(SyntaxKind.ExternKeyword), Token(SyntaxKind.AliasKeyword), Identifier(identifier), Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax UsingDirective(SyntaxToken usingKeyword, SyntaxToken staticKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.NameEqualsSyntax alias, Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = usingKeyword.Kind();
		if (syntaxKind != SyntaxKind.UsingKeyword)
		{
			throw new ArgumentException("usingKeyword");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.UsingDirective((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)usingKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)staticKeyword.Node, (alias == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameEqualsSyntax)alias.Green), (name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameSyntax)name.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax UsingDirective(SyntaxToken staticKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.NameEqualsSyntax alias, Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name)
	{
		return UsingDirective(Token(SyntaxKind.UsingKeyword), staticKeyword, alias, name, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax UsingDirective(Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name)
	{
		return UsingDirective(Token(SyntaxKind.UsingKeyword), default, null, name, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax NamespaceDeclaration(SyntaxToken namespaceKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name, SyntaxToken openBraceToken, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExternAliasDirectiveSyntax> externs, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax> usings, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = namespaceKeyword.Kind();
		if (syntaxKind != SyntaxKind.NamespaceKeyword)
		{
			throw new ArgumentException("namespaceKeyword");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		syntaxKind = openBraceToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException("openBraceToken");
		}
		syntaxKind = closeBraceToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException("closeBraceToken");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.NamespaceDeclaration((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)namespaceKeyword.Node, (name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameSyntax)name.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBraceToken.Node, externs.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExternAliasDirectiveSyntax>(), usings.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingDirectiveSyntax>(), members.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberDeclarationSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBraceToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax NamespaceDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExternAliasDirectiveSyntax> externs, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax> usings, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members)
	{
		return NamespaceDeclaration(Token(SyntaxKind.NamespaceKeyword), name, Token(SyntaxKind.OpenBraceToken), externs, usings, members, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax NamespaceDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name)
	{
		return NamespaceDeclaration(Token(SyntaxKind.NamespaceKeyword), name, Token(SyntaxKind.OpenBraceToken), default, default, default, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax AttributeList(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.CSharp.Syntax.AttributeTargetSpecifierSyntax target, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax> attributes, SyntaxToken closeBracketToken)
	{
		SyntaxKind syntaxKind = openBracketToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBracketToken)
		{
			throw new ArgumentException("openBracketToken");
		}
		syntaxKind = closeBracketToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBracketToken)
		{
			throw new ArgumentException("closeBracketToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.AttributeList((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBracketToken.Node, (target == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeTargetSpecifierSyntax)target.Green), attributes.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBracketToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax AttributeList(Microsoft.CodeAnalysis.CSharp.Syntax.AttributeTargetSpecifierSyntax target, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax> attributes)
	{
		return AttributeList(Token(SyntaxKind.OpenBracketToken), target, attributes, Token(SyntaxKind.CloseBracketToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax AttributeList(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax> attributes = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax>))
	{
		return AttributeList(Token(SyntaxKind.OpenBracketToken), null, attributes, Token(SyntaxKind.CloseBracketToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AttributeTargetSpecifierSyntax AttributeTargetSpecifier(SyntaxToken identifier, SyntaxToken colonToken)
	{
		SyntaxKind syntaxKind = colonToken.Kind();
		if (syntaxKind != SyntaxKind.ColonToken)
		{
			throw new ArgumentException("colonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AttributeTargetSpecifierSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.AttributeTargetSpecifier((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)colonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AttributeTargetSpecifierSyntax AttributeTargetSpecifier(SyntaxToken identifier)
	{
		return AttributeTargetSpecifier(identifier, Token(SyntaxKind.ColonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax Attribute(Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name, Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentListSyntax argumentList)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Attribute((name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameSyntax)name.Green), (argumentList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeArgumentListSyntax)argumentList.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax Attribute(Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name)
	{
		return Attribute(name, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentListSyntax AttributeArgumentList(SyntaxToken openParenToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentSyntax> arguments, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentListSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.AttributeArgumentList((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, arguments.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeArgumentSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentListSyntax AttributeArgumentList(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentSyntax> arguments = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentSyntax>))
	{
		return AttributeArgumentList(Token(SyntaxKind.OpenParenToken), arguments, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentSyntax AttributeArgument(Microsoft.CodeAnalysis.CSharp.Syntax.NameEqualsSyntax nameEquals, Microsoft.CodeAnalysis.CSharp.Syntax.NameColonSyntax nameColon, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.AttributeArgument((nameEquals == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameEqualsSyntax)nameEquals.Green), (nameColon == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameColonSyntax)nameColon.Green), (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentSyntax AttributeArgument(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return AttributeArgument(null, null, expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NameEqualsSyntax NameEquals(Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax name, SyntaxToken equalsToken)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		SyntaxKind syntaxKind = equalsToken.Kind();
		if (syntaxKind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException("equalsToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.NameEqualsSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.NameEquals((name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IdentifierNameSyntax)name.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)equalsToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NameEqualsSyntax NameEquals(Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax name)
	{
		return NameEquals(name, Token(SyntaxKind.EqualsToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NameEqualsSyntax NameEquals(string name)
	{
		return NameEquals(IdentifierName(name), Token(SyntaxKind.EqualsToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax TypeParameterList(SyntaxToken lessThanToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax> parameters, SyntaxToken greaterThanToken)
	{
		SyntaxKind syntaxKind = lessThanToken.Kind();
		if (syntaxKind != SyntaxKind.LessThanToken)
		{
			throw new ArgumentException("lessThanToken");
		}
		syntaxKind = greaterThanToken.Kind();
		if (syntaxKind != SyntaxKind.GreaterThanToken)
		{
			throw new ArgumentException("greaterThanToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.TypeParameterList((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)lessThanToken.Node, parameters.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)greaterThanToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax TypeParameterList(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax> parameters = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax>))
	{
		return TypeParameterList(Token(SyntaxKind.LessThanToken), parameters, Token(SyntaxKind.GreaterThanToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax TypeParameter(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxToken varianceKeyword, SyntaxToken identifier)
	{
		SyntaxKind syntaxKind = varianceKeyword.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind - 8361 > SyntaxKind.List)
		{
			throw new ArgumentException("varianceKeyword");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.TypeParameter(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)varianceKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax TypeParameter(SyntaxToken identifier)
	{
		return TypeParameter(default, default, identifier);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax TypeParameter(string identifier)
	{
		return TypeParameter(default, default, Identifier(identifier));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax ClassDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken keyword, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax baseList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind != SyntaxKind.ClassKeyword)
		{
			throw new ArgumentException("keyword");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		syntaxKind = openBraceToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException("openBraceToken");
		}
		syntaxKind = closeBraceToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException("closeBraceToken");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ClassDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (typeParameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterListSyntax)typeParameterList.Green), (baseList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseListSyntax)baseList.Green), constraintClauses.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterConstraintClauseSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBraceToken.Node, members.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberDeclarationSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBraceToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax ClassDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax baseList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members)
	{
		return ClassDeclaration(attributeLists, modifiers, Token(SyntaxKind.ClassKeyword), identifier, typeParameterList, baseList, constraintClauses, Token(SyntaxKind.OpenBraceToken), members, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax ClassDeclaration(SyntaxToken identifier)
	{
		return ClassDeclaration(default, default, Token(SyntaxKind.ClassKeyword), identifier, null, null, default, Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax ClassDeclaration(string identifier)
	{
		return ClassDeclaration(default, default, Token(SyntaxKind.ClassKeyword), Identifier(identifier), null, null, default, Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax StructDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken keyword, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax baseList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind != SyntaxKind.StructKeyword)
		{
			throw new ArgumentException("keyword");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		syntaxKind = openBraceToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException("openBraceToken");
		}
		syntaxKind = closeBraceToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException("closeBraceToken");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.StructDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (typeParameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterListSyntax)typeParameterList.Green), (baseList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseListSyntax)baseList.Green), constraintClauses.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterConstraintClauseSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBraceToken.Node, members.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberDeclarationSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBraceToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax StructDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax baseList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members)
	{
		return StructDeclaration(attributeLists, modifiers, Token(SyntaxKind.StructKeyword), identifier, typeParameterList, baseList, constraintClauses, Token(SyntaxKind.OpenBraceToken), members, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax StructDeclaration(SyntaxToken identifier)
	{
		return StructDeclaration(default, default, Token(SyntaxKind.StructKeyword), identifier, null, null, default, Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax StructDeclaration(string identifier)
	{
		return StructDeclaration(default, default, Token(SyntaxKind.StructKeyword), Identifier(identifier), null, null, default, Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax InterfaceDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken keyword, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax baseList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind != SyntaxKind.InterfaceKeyword)
		{
			throw new ArgumentException("keyword");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		syntaxKind = openBraceToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException("openBraceToken");
		}
		syntaxKind = closeBraceToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException("closeBraceToken");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.InterfaceDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (typeParameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterListSyntax)typeParameterList.Green), (baseList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseListSyntax)baseList.Green), constraintClauses.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterConstraintClauseSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBraceToken.Node, members.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberDeclarationSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBraceToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax InterfaceDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax baseList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members)
	{
		return InterfaceDeclaration(attributeLists, modifiers, Token(SyntaxKind.InterfaceKeyword), identifier, typeParameterList, baseList, constraintClauses, Token(SyntaxKind.OpenBraceToken), members, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax InterfaceDeclaration(SyntaxToken identifier)
	{
		return InterfaceDeclaration(default, default, Token(SyntaxKind.InterfaceKeyword), identifier, null, null, default, Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax InterfaceDeclaration(string identifier)
	{
		return InterfaceDeclaration(default, default, Token(SyntaxKind.InterfaceKeyword), Identifier(identifier), null, null, default, Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax EnumDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken enumKeyword, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax baseList, SyntaxToken openBraceToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = enumKeyword.Kind();
		if (syntaxKind != SyntaxKind.EnumKeyword)
		{
			throw new ArgumentException("enumKeyword");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		syntaxKind = openBraceToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException("openBraceToken");
		}
		syntaxKind = closeBraceToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException("closeBraceToken");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.EnumDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)enumKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (baseList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseListSyntax)baseList.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBraceToken.Node, members.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EnumMemberDeclarationSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBraceToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax EnumDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax baseList, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax> members)
	{
		return EnumDeclaration(attributeLists, modifiers, Token(SyntaxKind.EnumKeyword), identifier, baseList, Token(SyntaxKind.OpenBraceToken), members, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax EnumDeclaration(SyntaxToken identifier)
	{
		return EnumDeclaration(default, default, Token(SyntaxKind.EnumKeyword), identifier, null, Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax EnumDeclaration(string identifier)
	{
		return EnumDeclaration(default, default, Token(SyntaxKind.EnumKeyword), Identifier(identifier), null, Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DelegateDeclarationSyntax DelegateDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken delegateKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = delegateKeyword.Kind();
		if (syntaxKind != SyntaxKind.DelegateKeyword)
		{
			throw new ArgumentException("delegateKeyword");
		}
		if (returnType == null)
		{
			throw new ArgumentNullException("returnType");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		if (parameterList == null)
		{
			throw new ArgumentNullException("parameterList");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.DelegateDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.DelegateDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)delegateKeyword.Node, (returnType == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)returnType.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (typeParameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterListSyntax)typeParameterList.Green), (parameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax)parameterList.Green), constraintClauses.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterConstraintClauseSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DelegateDeclarationSyntax DelegateDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses)
	{
		return DelegateDeclaration(attributeLists, modifiers, Token(SyntaxKind.DelegateKeyword), returnType, identifier, typeParameterList, parameterList, constraintClauses, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DelegateDeclarationSyntax DelegateDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, SyntaxToken identifier)
	{
		return DelegateDeclaration(default, default, Token(SyntaxKind.DelegateKeyword), returnType, identifier, null, ParameterList(), default, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DelegateDeclarationSyntax DelegateDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, string identifier)
	{
		return DelegateDeclaration(default, default, Token(SyntaxKind.DelegateKeyword), returnType, Identifier(identifier), null, ParameterList(), default, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax EnumMemberDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax equalsValue)
	{
		SyntaxKind syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.EnumMemberDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (equalsValue == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EqualsValueClauseSyntax)equalsValue.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax EnumMemberDeclaration(SyntaxToken identifier)
	{
		return EnumMemberDeclaration(default, identifier, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax EnumMemberDeclaration(string identifier)
	{
		return EnumMemberDeclaration(default, Identifier(identifier), null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax BaseList(SyntaxToken colonToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeSyntax> types)
	{
		SyntaxKind syntaxKind = colonToken.Kind();
		if (syntaxKind != SyntaxKind.ColonToken)
		{
			throw new ArgumentException("colonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.BaseList((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)colonToken.Node, types.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseTypeSyntax>()).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax BaseList(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeSyntax> types = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeSyntax>))
	{
		return BaseList(Token(SyntaxKind.ColonToken), types);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SimpleBaseTypeSyntax SimpleBaseType(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.SimpleBaseTypeSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.SimpleBaseType((type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(SyntaxToken whereKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax name, SyntaxToken colonToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintSyntax> constraints)
	{
		SyntaxKind syntaxKind = whereKeyword.Kind();
		if (syntaxKind != SyntaxKind.WhereKeyword)
		{
			throw new ArgumentException("whereKeyword");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		syntaxKind = colonToken.Kind();
		if (syntaxKind != SyntaxKind.ColonToken)
		{
			throw new ArgumentException("colonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.TypeParameterConstraintClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)whereKeyword.Node, (name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IdentifierNameSyntax)name.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)colonToken.Node, constraints.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterConstraintSyntax>()).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax name, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintSyntax> constraints)
	{
		return TypeParameterConstraintClause(Token(SyntaxKind.WhereKeyword), name, Token(SyntaxKind.ColonToken), constraints);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax name)
	{
		return TypeParameterConstraintClause(Token(SyntaxKind.WhereKeyword), name, Token(SyntaxKind.ColonToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(string name)
	{
		return TypeParameterConstraintClause(Token(SyntaxKind.WhereKeyword), IdentifierName(name), Token(SyntaxKind.ColonToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorConstraintSyntax ConstructorConstraint(SyntaxToken newKeyword, SyntaxToken openParenToken, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = newKeyword.Kind();
		if (syntaxKind != SyntaxKind.NewKeyword)
		{
			throw new ArgumentException("newKeyword");
		}
		syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorConstraintSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ConstructorConstraint((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)newKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorConstraintSyntax ConstructorConstraint()
	{
		return ConstructorConstraint(Token(SyntaxKind.NewKeyword), Token(SyntaxKind.OpenParenToken), Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ClassOrStructConstraintSyntax ClassOrStructConstraint(SyntaxKind kind, SyntaxToken classOrStructKeyword)
	{
		if (kind - 8868 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		SyntaxKind syntaxKind = classOrStructKeyword.Kind();
		if (syntaxKind - 8374 > SyntaxKind.List)
		{
			throw new ArgumentException("classOrStructKeyword");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ClassOrStructConstraintSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ClassOrStructConstraint(kind, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)classOrStructKeyword.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ClassOrStructConstraintSyntax ClassOrStructConstraint(SyntaxKind kind)
	{
		return ClassOrStructConstraint(kind, Token(GetClassOrStructConstraintClassOrStructKeywordKind(kind)));
	}

	private static SyntaxKind GetClassOrStructConstraintClassOrStructKeywordKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.ClassConstraint => SyntaxKind.ClassKeyword, 
			SyntaxKind.StructConstraint => SyntaxKind.StructKeyword, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeConstraintSyntax TypeConstraint(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.TypeConstraintSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.TypeConstraint((type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax FieldDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		if (declaration == null)
		{
			throw new ArgumentNullException("declaration");
		}
		SyntaxKind syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.FieldDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (declaration == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDeclarationSyntax)declaration.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax FieldDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration)
	{
		return FieldDeclaration(attributeLists, modifiers, declaration, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax FieldDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration)
	{
		return FieldDeclaration(default, default, declaration, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax EventFieldDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken eventKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = eventKeyword.Kind();
		if (syntaxKind != SyntaxKind.EventKeyword)
		{
			throw new ArgumentException("eventKeyword");
		}
		if (declaration == null)
		{
			throw new ArgumentNullException("declaration");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.EventFieldDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)eventKeyword.Node, (declaration == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDeclarationSyntax)declaration.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax EventFieldDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration)
	{
		return EventFieldDeclaration(attributeLists, modifiers, Token(SyntaxKind.EventKeyword), declaration, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax EventFieldDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax declaration)
	{
		return EventFieldDeclaration(default, default, Token(SyntaxKind.EventKeyword), declaration, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier(Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name, SyntaxToken dotToken)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		SyntaxKind syntaxKind = dotToken.Kind();
		if (syntaxKind != SyntaxKind.DotToken)
		{
			throw new ArgumentException("dotToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ExplicitInterfaceSpecifier((name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameSyntax)name.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)dotToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier(Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name)
	{
		return ExplicitInterfaceSpecifier(name, Token(SyntaxKind.DotToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax MethodDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (returnType == null)
		{
			throw new ArgumentNullException("returnType");
		}
		SyntaxKind syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		if (parameterList == null)
		{
			throw new ArgumentNullException("parameterList");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.MethodDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (returnType == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)returnType.Green), (explicitInterfaceSpecifier == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExplicitInterfaceSpecifierSyntax)explicitInterfaceSpecifier.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (typeParameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterListSyntax)typeParameterList.Green), (parameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax)parameterList.Green), constraintClauses.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterConstraintClauseSyntax>(), (body == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)body.Green), (expressionBody == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArrowExpressionClauseSyntax)expressionBody.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax MethodDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody)
	{
		return MethodDeclaration(attributeLists, modifiers, returnType, explicitInterfaceSpecifier, identifier, typeParameterList, parameterList, constraintClauses, body, expressionBody, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax MethodDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, SyntaxToken identifier)
	{
		return MethodDeclaration(default, default, returnType, null, identifier, null, ParameterList(), default, null, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax MethodDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, string identifier)
	{
		return MethodDeclaration(default, default, returnType, null, Identifier(identifier), null, ParameterList(), default, null, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OperatorDeclarationSyntax OperatorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, SyntaxToken operatorKeyword, SyntaxToken operatorToken, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (returnType == null)
		{
			throw new ArgumentNullException("returnType");
		}
		SyntaxKind syntaxKind = operatorKeyword.Kind();
		if (syntaxKind != SyntaxKind.OperatorKeyword)
		{
			throw new ArgumentException("operatorKeyword");
		}
		switch (operatorToken.Kind())
		{
		default:
			throw new ArgumentException("operatorToken");
		case SyntaxKind.TildeToken:
		case SyntaxKind.ExclamationToken:
		case SyntaxKind.PercentToken:
		case SyntaxKind.CaretToken:
		case SyntaxKind.AmpersandToken:
		case SyntaxKind.AsteriskToken:
		case SyntaxKind.MinusToken:
		case SyntaxKind.PlusToken:
		case SyntaxKind.BarToken:
		case SyntaxKind.LessThanToken:
		case SyntaxKind.GreaterThanToken:
		case SyntaxKind.SlashToken:
		case SyntaxKind.MinusMinusToken:
		case SyntaxKind.PlusPlusToken:
		case SyntaxKind.ExclamationEqualsToken:
		case SyntaxKind.EqualsEqualsToken:
		case SyntaxKind.LessThanEqualsToken:
		case SyntaxKind.LessThanLessThanToken:
		case SyntaxKind.GreaterThanEqualsToken:
		case SyntaxKind.GreaterThanGreaterThanToken:
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.IsKeyword:
			if (parameterList == null)
			{
				throw new ArgumentNullException("parameterList");
			}
			syntaxKind = semicolonToken.Kind();
			if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
			{
				throw new ArgumentException("semicolonToken");
			}
			return (Microsoft.CodeAnalysis.CSharp.Syntax.OperatorDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.OperatorDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (returnType == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)returnType.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)operatorKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)operatorToken.Node, (parameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax)parameterList.Green), (body == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)body.Green), (expressionBody == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArrowExpressionClauseSyntax)expressionBody.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
		}
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OperatorDeclarationSyntax OperatorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, SyntaxToken operatorToken, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody)
	{
		return OperatorDeclaration(attributeLists, modifiers, returnType, Token(SyntaxKind.OperatorKeyword), operatorToken, parameterList, body, expressionBody, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OperatorDeclarationSyntax OperatorDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, SyntaxToken operatorToken)
	{
		return OperatorDeclaration(default, default, returnType, Token(SyntaxKind.OperatorKeyword), operatorToken, ParameterList(), null, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConversionOperatorDeclarationSyntax ConversionOperatorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = implicitOrExplicitKeyword.Kind();
		if (syntaxKind - 8383 > SyntaxKind.List)
		{
			throw new ArgumentException("implicitOrExplicitKeyword");
		}
		syntaxKind = operatorKeyword.Kind();
		if (syntaxKind != SyntaxKind.OperatorKeyword)
		{
			throw new ArgumentException("operatorKeyword");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (parameterList == null)
		{
			throw new ArgumentNullException("parameterList");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ConversionOperatorDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ConversionOperatorDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)implicitOrExplicitKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)operatorKeyword.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (parameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax)parameterList.Green), (body == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)body.Green), (expressionBody == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArrowExpressionClauseSyntax)expressionBody.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConversionOperatorDeclarationSyntax ConversionOperatorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken implicitOrExplicitKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody)
	{
		return ConversionOperatorDeclaration(attributeLists, modifiers, implicitOrExplicitKeyword, Token(SyntaxKind.OperatorKeyword), type, parameterList, body, expressionBody, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConversionOperatorDeclarationSyntax ConversionOperatorDeclaration(SyntaxToken implicitOrExplicitKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return ConversionOperatorDeclaration(default, default, implicitOrExplicitKeyword, Token(SyntaxKind.OperatorKeyword), type, ParameterList(), null, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax ConstructorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax initializer, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		if (parameterList == null)
		{
			throw new ArgumentNullException("parameterList");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ConstructorDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (parameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax)parameterList.Green), (initializer == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConstructorInitializerSyntax)initializer.Green), (body == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)body.Green), (expressionBody == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArrowExpressionClauseSyntax)expressionBody.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax ConstructorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax initializer, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody)
	{
		return ConstructorDeclaration(attributeLists, modifiers, identifier, parameterList, initializer, body, expressionBody, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax ConstructorDeclaration(SyntaxToken identifier)
	{
		return ConstructorDeclaration(default, default, identifier, ParameterList(), null, null, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax ConstructorDeclaration(string identifier)
	{
		return ConstructorDeclaration(default, default, Identifier(identifier), ParameterList(), null, null, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax ConstructorInitializer(SyntaxKind kind, SyntaxToken colonToken, SyntaxToken thisOrBaseKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax argumentList)
	{
		if (kind - 8889 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		SyntaxKind syntaxKind = colonToken.Kind();
		if (syntaxKind != SyntaxKind.ColonToken)
		{
			throw new ArgumentException("colonToken");
		}
		syntaxKind = thisOrBaseKeyword.Kind();
		if (syntaxKind - 8370 > SyntaxKind.List)
		{
			throw new ArgumentException("thisOrBaseKeyword");
		}
		if (argumentList == null)
		{
			throw new ArgumentNullException("argumentList");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ConstructorInitializer(kind, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)colonToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)thisOrBaseKeyword.Node, (argumentList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArgumentListSyntax)argumentList.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax ConstructorInitializer(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax argumentList = null)
	{
		return ConstructorInitializer(kind, Token(SyntaxKind.ColonToken), Token(GetConstructorInitializerThisOrBaseKeywordKind(kind)), argumentList ?? ArgumentList());
	}

	private static SyntaxKind GetConstructorInitializerThisOrBaseKeywordKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.BaseConstructorInitializer => SyntaxKind.BaseKeyword, 
			SyntaxKind.ThisConstructorInitializer => SyntaxKind.ThisKeyword, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DestructorDeclarationSyntax DestructorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken tildeToken, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		SyntaxKind syntaxKind = tildeToken.Kind();
		if (syntaxKind != SyntaxKind.TildeToken)
		{
			throw new ArgumentException("tildeToken");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		if (parameterList == null)
		{
			throw new ArgumentNullException("parameterList");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.DestructorDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.DestructorDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)tildeToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (parameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax)parameterList.Green), (body == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)body.Green), (expressionBody == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArrowExpressionClauseSyntax)expressionBody.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DestructorDeclarationSyntax DestructorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody)
	{
		return DestructorDeclaration(attributeLists, modifiers, Token(SyntaxKind.TildeToken), identifier, parameterList, body, expressionBody, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DestructorDeclarationSyntax DestructorDeclaration(SyntaxToken identifier)
	{
		return DestructorDeclaration(default, default, Token(SyntaxKind.TildeToken), identifier, ParameterList(), null, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DestructorDeclarationSyntax DestructorDeclaration(string identifier)
	{
		return DestructorDeclaration(default, default, Token(SyntaxKind.TildeToken), Identifier(identifier), ParameterList(), null, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax PropertyDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.AccessorListSyntax accessorList, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax initializer, SyntaxToken semicolonToken)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		SyntaxKind syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.PropertyDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (explicitInterfaceSpecifier == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExplicitInterfaceSpecifierSyntax)explicitInterfaceSpecifier.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (accessorList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AccessorListSyntax)accessorList.Green), (expressionBody == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArrowExpressionClauseSyntax)expressionBody.Green), (initializer == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EqualsValueClauseSyntax)initializer.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax PropertyDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.AccessorListSyntax accessorList, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax initializer)
	{
		return PropertyDeclaration(attributeLists, modifiers, type, explicitInterfaceSpecifier, identifier, accessorList, expressionBody, initializer, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax PropertyDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken identifier)
	{
		return PropertyDeclaration(default, default, type, null, identifier, null, null, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax PropertyDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, string identifier)
	{
		return PropertyDeclaration(default, default, type, null, Identifier(identifier), null, null, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax ArrowExpressionClause(SyntaxToken arrowToken, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		SyntaxKind syntaxKind = arrowToken.Kind();
		if (syntaxKind != SyntaxKind.EqualsGreaterThanToken)
		{
			throw new ArgumentException("arrowToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ArrowExpressionClause((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)arrowToken.Node, (expression == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)expression.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax ArrowExpressionClause(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return ArrowExpressionClause(Token(SyntaxKind.EqualsGreaterThanToken), expression);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EventDeclarationSyntax EventDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken eventKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.AccessorListSyntax accessorList)
	{
		SyntaxKind syntaxKind = eventKeyword.Kind();
		if (syntaxKind != SyntaxKind.EventKeyword)
		{
			throw new ArgumentException("eventKeyword");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		if (accessorList == null)
		{
			throw new ArgumentNullException("accessorList");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.EventDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.EventDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)eventKeyword.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (explicitInterfaceSpecifier == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExplicitInterfaceSpecifierSyntax)explicitInterfaceSpecifier.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (accessorList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AccessorListSyntax)accessorList.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EventDeclarationSyntax EventDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.AccessorListSyntax accessorList)
	{
		return EventDeclaration(attributeLists, modifiers, Token(SyntaxKind.EventKeyword), type, explicitInterfaceSpecifier, identifier, accessorList);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EventDeclarationSyntax EventDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken identifier)
	{
		return EventDeclaration(default, default, Token(SyntaxKind.EventKeyword), type, null, identifier, AccessorList());
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EventDeclarationSyntax EventDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, string identifier)
	{
		return EventDeclaration(default, default, Token(SyntaxKind.EventKeyword), type, null, Identifier(identifier), AccessorList());
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IndexerDeclarationSyntax IndexerDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken thisKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.BracketedParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.AccessorListSyntax accessorList, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		SyntaxKind syntaxKind = thisKeyword.Kind();
		if (syntaxKind != SyntaxKind.ThisKeyword)
		{
			throw new ArgumentException("thisKeyword");
		}
		if (parameterList == null)
		{
			throw new ArgumentNullException("parameterList");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.IndexerDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.IndexerDeclaration(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (explicitInterfaceSpecifier == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExplicitInterfaceSpecifierSyntax)explicitInterfaceSpecifier.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)thisKeyword.Node, (parameterList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BracketedParameterListSyntax)parameterList.Green), (accessorList == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AccessorListSyntax)accessorList.Green), (expressionBody == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArrowExpressionClauseSyntax)expressionBody.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IndexerDeclarationSyntax IndexerDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, Microsoft.CodeAnalysis.CSharp.Syntax.BracketedParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.AccessorListSyntax accessorList, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody)
	{
		return IndexerDeclaration(attributeLists, modifiers, type, explicitInterfaceSpecifier, Token(SyntaxKind.ThisKeyword), parameterList, accessorList, expressionBody, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IndexerDeclarationSyntax IndexerDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return IndexerDeclaration(default, default, type, null, Token(SyntaxKind.ThisKeyword), BracketedParameterList(), null, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AccessorListSyntax AccessorList(SyntaxToken openBraceToken, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax> accessors, SyntaxToken closeBraceToken)
	{
		SyntaxKind syntaxKind = openBraceToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException("openBraceToken");
		}
		syntaxKind = closeBraceToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException("closeBraceToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AccessorListSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.AccessorList((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBraceToken.Node, accessors.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AccessorDeclarationSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AccessorListSyntax AccessorList(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax> accessors = default(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax>))
	{
		return AccessorList(Token(SyntaxKind.OpenBraceToken), accessors, Token(SyntaxKind.CloseBraceToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken keyword, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (kind - 8896 > (SyntaxKind)4)
		{
			throw new ArgumentException("kind");
		}
		SyntaxKind syntaxKind = keyword.Kind();
		if (syntaxKind - 8417 > (SyntaxKind)3 && syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("keyword");
		}
		syntaxKind = semicolonToken.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException("semicolonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.AccessorDeclaration(kind, attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)keyword.Node, (body == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)body.Green), (expressionBody == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArrowExpressionClauseSyntax)expressionBody.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody)
	{
		return AccessorDeclaration(kind, attributeLists, modifiers, Token(GetAccessorDeclarationKeywordKind(kind)), body, expressionBody, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind)
	{
		return AccessorDeclaration(kind, default, default, Token(GetAccessorDeclarationKeywordKind(kind)), null, null, default);
	}

	private static SyntaxKind GetAccessorDeclarationKeywordKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.GetAccessorDeclaration => SyntaxKind.GetKeyword, 
			SyntaxKind.SetAccessorDeclaration => SyntaxKind.SetKeyword, 
			SyntaxKind.AddAccessorDeclaration => SyntaxKind.AddKeyword, 
			SyntaxKind.RemoveAccessorDeclaration => SyntaxKind.RemoveKeyword, 
			SyntaxKind.UnknownAccessorDeclaration => SyntaxKind.IdentifierToken, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax ParameterList(SyntaxToken openParenToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax> parameters, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ParameterList((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, parameters.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax ParameterList(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax> parameters = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax>))
	{
		return ParameterList(Token(SyntaxKind.OpenParenToken), parameters, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BracketedParameterListSyntax BracketedParameterList(SyntaxToken openBracketToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax> parameters, SyntaxToken closeBracketToken)
	{
		SyntaxKind syntaxKind = openBracketToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBracketToken)
		{
			throw new ArgumentException("openBracketToken");
		}
		syntaxKind = closeBracketToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBracketToken)
		{
			throw new ArgumentException("closeBracketToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.BracketedParameterListSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.BracketedParameterList((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBracketToken.Node, parameters.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBracketToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BracketedParameterListSyntax BracketedParameterList(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax> parameters = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax>))
	{
		return BracketedParameterList(Token(SyntaxKind.OpenBracketToken), parameters, Token(SyntaxKind.CloseBracketToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax Parameter(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax @default)
	{
		SyntaxKind syntaxKind = identifier.Kind();
		if (syntaxKind != SyntaxKind.ArgListKeyword && syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("identifier");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Parameter(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (@default == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EqualsValueClauseSyntax)@default.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax Parameter(SyntaxToken identifier)
	{
		return Parameter(default, default, null, identifier, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IncompleteMemberSyntax IncompleteMember(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return (Microsoft.CodeAnalysis.CSharp.Syntax.IncompleteMemberSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.IncompleteMember(attributeLists.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax>(), modifiers.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IncompleteMemberSyntax IncompleteMember(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type = null)
	{
		return IncompleteMember(default, default, type);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SkippedTokensTriviaSyntax SkippedTokensTrivia(SyntaxTokenList tokens)
	{
		return (Microsoft.CodeAnalysis.CSharp.Syntax.SkippedTokensTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.SkippedTokensTrivia(tokens.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>()).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.SkippedTokensTriviaSyntax SkippedTokensTrivia()
	{
		return SkippedTokensTrivia(default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DocumentationCommentTriviaSyntax DocumentationCommentTrivia(SyntaxKind kind, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content, SyntaxToken endOfComment)
	{
		if (kind - 8544 > SyntaxKind.List)
		{
			throw new ArgumentException("kind");
		}
		SyntaxKind syntaxKind = endOfComment.Kind();
		if (syntaxKind != SyntaxKind.EndOfDocumentationCommentToken)
		{
			throw new ArgumentException("endOfComment");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.DocumentationCommentTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.DocumentationCommentTrivia(kind, content.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlNodeSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfComment.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DocumentationCommentTriviaSyntax DocumentationCommentTrivia(SyntaxKind kind, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content = default(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax>))
	{
		return DocumentationCommentTrivia(kind, content, Token(SyntaxKind.EndOfDocumentationCommentToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeCrefSyntax TypeCref(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.TypeCrefSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.TypeCref((type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedCrefSyntax QualifiedCref(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax container, SyntaxToken dotToken, Microsoft.CodeAnalysis.CSharp.Syntax.MemberCrefSyntax member)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		SyntaxKind syntaxKind = dotToken.Kind();
		if (syntaxKind != SyntaxKind.DotToken)
		{
			throw new ArgumentException("dotToken");
		}
		if (member == null)
		{
			throw new ArgumentNullException("member");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedCrefSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.QualifiedCref((container == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)container.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)dotToken.Node, (member == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberCrefSyntax)member.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedCrefSyntax QualifiedCref(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax container, Microsoft.CodeAnalysis.CSharp.Syntax.MemberCrefSyntax member)
	{
		return QualifiedCref(container, Token(SyntaxKind.DotToken), member);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NameMemberCrefSyntax NameMemberCref(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax name, Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterListSyntax parameters)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.NameMemberCrefSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.NameMemberCref((name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)name.Green), (parameters == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefParameterListSyntax)parameters.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NameMemberCrefSyntax NameMemberCref(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax name)
	{
		return NameMemberCref(name, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IndexerMemberCrefSyntax IndexerMemberCref(SyntaxToken thisKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.CrefBracketedParameterListSyntax parameters)
	{
		SyntaxKind syntaxKind = thisKeyword.Kind();
		if (syntaxKind != SyntaxKind.ThisKeyword)
		{
			throw new ArgumentException("thisKeyword");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.IndexerMemberCrefSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.IndexerMemberCref((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)thisKeyword.Node, (parameters == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefBracketedParameterListSyntax)parameters.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IndexerMemberCrefSyntax IndexerMemberCref(Microsoft.CodeAnalysis.CSharp.Syntax.CrefBracketedParameterListSyntax parameters = null)
	{
		return IndexerMemberCref(Token(SyntaxKind.ThisKeyword), parameters);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OperatorMemberCrefSyntax OperatorMemberCref(SyntaxToken operatorKeyword, SyntaxToken operatorToken, Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterListSyntax parameters)
	{
		SyntaxKind syntaxKind = operatorKeyword.Kind();
		if (syntaxKind != SyntaxKind.OperatorKeyword)
		{
			throw new ArgumentException("operatorKeyword");
		}
		switch (operatorToken.Kind())
		{
		default:
			throw new ArgumentException("operatorToken");
		case SyntaxKind.TildeToken:
		case SyntaxKind.ExclamationToken:
		case SyntaxKind.PercentToken:
		case SyntaxKind.CaretToken:
		case SyntaxKind.AmpersandToken:
		case SyntaxKind.AsteriskToken:
		case SyntaxKind.MinusToken:
		case SyntaxKind.PlusToken:
		case SyntaxKind.BarToken:
		case SyntaxKind.LessThanToken:
		case SyntaxKind.GreaterThanToken:
		case SyntaxKind.SlashToken:
		case SyntaxKind.MinusMinusToken:
		case SyntaxKind.PlusPlusToken:
		case SyntaxKind.ExclamationEqualsToken:
		case SyntaxKind.EqualsEqualsToken:
		case SyntaxKind.LessThanEqualsToken:
		case SyntaxKind.LessThanLessThanToken:
		case SyntaxKind.GreaterThanEqualsToken:
		case SyntaxKind.GreaterThanGreaterThanToken:
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.FalseKeyword:
			return (Microsoft.CodeAnalysis.CSharp.Syntax.OperatorMemberCrefSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.OperatorMemberCref((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)operatorKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)operatorToken.Node, (parameters == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefParameterListSyntax)parameters.Green)).CreateRed();
		}
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OperatorMemberCrefSyntax OperatorMemberCref(SyntaxToken operatorToken, Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterListSyntax parameters)
	{
		return OperatorMemberCref(Token(SyntaxKind.OperatorKeyword), operatorToken, parameters);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OperatorMemberCrefSyntax OperatorMemberCref(SyntaxToken operatorToken)
	{
		return OperatorMemberCref(Token(SyntaxKind.OperatorKeyword), operatorToken, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConversionOperatorMemberCrefSyntax ConversionOperatorMemberCref(SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterListSyntax parameters)
	{
		SyntaxKind syntaxKind = implicitOrExplicitKeyword.Kind();
		if (syntaxKind - 8383 > SyntaxKind.List)
		{
			throw new ArgumentException("implicitOrExplicitKeyword");
		}
		syntaxKind = operatorKeyword.Kind();
		if (syntaxKind != SyntaxKind.OperatorKeyword)
		{
			throw new ArgumentException("operatorKeyword");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ConversionOperatorMemberCrefSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ConversionOperatorMemberCref((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)implicitOrExplicitKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)operatorKeyword.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green), (parameters == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefParameterListSyntax)parameters.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConversionOperatorMemberCrefSyntax ConversionOperatorMemberCref(SyntaxToken implicitOrExplicitKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterListSyntax parameters)
	{
		return ConversionOperatorMemberCref(implicitOrExplicitKeyword, Token(SyntaxKind.OperatorKeyword), type, parameters);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConversionOperatorMemberCrefSyntax ConversionOperatorMemberCref(SyntaxToken implicitOrExplicitKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return ConversionOperatorMemberCref(implicitOrExplicitKeyword, Token(SyntaxKind.OperatorKeyword), type, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterListSyntax CrefParameterList(SyntaxToken openParenToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterSyntax> parameters, SyntaxToken closeParenToken)
	{
		SyntaxKind syntaxKind = openParenToken.Kind();
		if (syntaxKind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException("openParenToken");
		}
		syntaxKind = closeParenToken.Kind();
		if (syntaxKind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException("closeParenToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterListSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CrefParameterList((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openParenToken.Node, parameters.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefParameterSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeParenToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterListSyntax CrefParameterList(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterSyntax> parameters = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterSyntax>))
	{
		return CrefParameterList(Token(SyntaxKind.OpenParenToken), parameters, Token(SyntaxKind.CloseParenToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CrefBracketedParameterListSyntax CrefBracketedParameterList(SyntaxToken openBracketToken, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterSyntax> parameters, SyntaxToken closeBracketToken)
	{
		SyntaxKind syntaxKind = openBracketToken.Kind();
		if (syntaxKind != SyntaxKind.OpenBracketToken)
		{
			throw new ArgumentException("openBracketToken");
		}
		syntaxKind = closeBracketToken.Kind();
		if (syntaxKind != SyntaxKind.CloseBracketToken)
		{
			throw new ArgumentException("closeBracketToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CrefBracketedParameterListSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CrefBracketedParameterList((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)openBracketToken.Node, parameters.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefParameterSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)closeBracketToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CrefBracketedParameterListSyntax CrefBracketedParameterList(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterSyntax> parameters = default(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterSyntax>))
	{
		return CrefBracketedParameterList(Token(SyntaxKind.OpenBracketToken), parameters, Token(SyntaxKind.CloseBracketToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterSyntax CrefParameter(SyntaxToken refKindKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		SyntaxKind syntaxKind = refKindKeyword.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind - 8360 > (SyntaxKind)2)
		{
			throw new ArgumentException("refKindKeyword");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.CrefParameter((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)refKindKeyword.Node, (type == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)type.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterSyntax CrefParameter(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return CrefParameter(default, type);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlElement(Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementStartTagSyntax startTag, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content, Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementEndTagSyntax endTag)
	{
		if (startTag == null)
		{
			throw new ArgumentNullException("startTag");
		}
		if (endTag == null)
		{
			throw new ArgumentNullException("endTag");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlElement((startTag == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlElementStartTagSyntax)startTag.Green), content.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlNodeSyntax>(), (endTag == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlElementEndTagSyntax)endTag.Green)).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlElement(Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementStartTagSyntax startTag, Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementEndTagSyntax endTag)
	{
		return XmlElement(startTag, default, endTag);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementStartTagSyntax XmlElementStartTag(SyntaxToken lessThanToken, Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlAttributeSyntax> attributes, SyntaxToken greaterThanToken)
	{
		SyntaxKind syntaxKind = lessThanToken.Kind();
		if (syntaxKind != SyntaxKind.LessThanToken)
		{
			throw new ArgumentException("lessThanToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		syntaxKind = greaterThanToken.Kind();
		if (syntaxKind != SyntaxKind.GreaterThanToken)
		{
			throw new ArgumentException("greaterThanToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementStartTagSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlElementStartTag((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)lessThanToken.Node, (name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlNameSyntax)name.Green), attributes.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlAttributeSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)greaterThanToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementStartTagSyntax XmlElementStartTag(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlAttributeSyntax> attributes)
	{
		return XmlElementStartTag(Token(SyntaxKind.LessThanToken), name, attributes, Token(SyntaxKind.GreaterThanToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementStartTagSyntax XmlElementStartTag(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name)
	{
		return XmlElementStartTag(Token(SyntaxKind.LessThanToken), name, default, Token(SyntaxKind.GreaterThanToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementEndTagSyntax XmlElementEndTag(SyntaxToken lessThanSlashToken, Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxToken greaterThanToken)
	{
		SyntaxKind syntaxKind = lessThanSlashToken.Kind();
		if (syntaxKind != SyntaxKind.LessThanSlashToken)
		{
			throw new ArgumentException("lessThanSlashToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		syntaxKind = greaterThanToken.Kind();
		if (syntaxKind != SyntaxKind.GreaterThanToken)
		{
			throw new ArgumentException("greaterThanToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementEndTagSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlElementEndTag((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)lessThanSlashToken.Node, (name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlNameSyntax)name.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)greaterThanToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementEndTagSyntax XmlElementEndTag(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name)
	{
		return XmlElementEndTag(Token(SyntaxKind.LessThanSlashToken), name, Token(SyntaxKind.GreaterThanToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax XmlEmptyElement(SyntaxToken lessThanToken, Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlAttributeSyntax> attributes, SyntaxToken slashGreaterThanToken)
	{
		SyntaxKind syntaxKind = lessThanToken.Kind();
		if (syntaxKind != SyntaxKind.LessThanToken)
		{
			throw new ArgumentException("lessThanToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		syntaxKind = slashGreaterThanToken.Kind();
		if (syntaxKind != SyntaxKind.SlashGreaterThanToken)
		{
			throw new ArgumentException("slashGreaterThanToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlEmptyElement((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)lessThanToken.Node, (name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlNameSyntax)name.Green), attributes.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlAttributeSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)slashGreaterThanToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax XmlEmptyElement(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlAttributeSyntax> attributes)
	{
		return XmlEmptyElement(Token(SyntaxKind.LessThanToken), name, attributes, Token(SyntaxKind.SlashGreaterThanToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax XmlEmptyElement(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name)
	{
		return XmlEmptyElement(Token(SyntaxKind.LessThanToken), name, default, Token(SyntaxKind.SlashGreaterThanToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax XmlName(Microsoft.CodeAnalysis.CSharp.Syntax.XmlPrefixSyntax prefix, SyntaxToken localName)
	{
		SyntaxKind syntaxKind = localName.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("localName");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlName((prefix == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlPrefixSyntax)prefix.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)localName.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax XmlName(SyntaxToken localName)
	{
		return XmlName(null, localName);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax XmlName(string localName)
	{
		return XmlName(null, Identifier(localName));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlPrefixSyntax XmlPrefix(SyntaxToken prefix, SyntaxToken colonToken)
	{
		SyntaxKind syntaxKind = prefix.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("prefix");
		}
		syntaxKind = colonToken.Kind();
		if (syntaxKind != SyntaxKind.ColonToken)
		{
			throw new ArgumentException("colonToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.XmlPrefixSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlPrefix((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)prefix.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)colonToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlPrefixSyntax XmlPrefix(SyntaxToken prefix)
	{
		return XmlPrefix(prefix, Token(SyntaxKind.ColonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlPrefixSyntax XmlPrefix(string prefix)
	{
		return XmlPrefix(Identifier(prefix), Token(SyntaxKind.ColonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextAttributeSyntax XmlTextAttribute(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, SyntaxTokenList textTokens, SyntaxToken endQuoteToken)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		SyntaxKind syntaxKind = equalsToken.Kind();
		if (syntaxKind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException("equalsToken");
		}
		syntaxKind = startQuoteToken.Kind();
		if (syntaxKind - 8213 > SyntaxKind.List)
		{
			throw new ArgumentException("startQuoteToken");
		}
		syntaxKind = endQuoteToken.Kind();
		if (syntaxKind - 8213 > SyntaxKind.List)
		{
			throw new ArgumentException("endQuoteToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextAttributeSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlTextAttribute((name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlNameSyntax)name.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)equalsToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)startQuoteToken.Node, textTokens.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endQuoteToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextAttributeSyntax XmlTextAttribute(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxToken startQuoteToken, SyntaxTokenList textTokens, SyntaxToken endQuoteToken)
	{
		return XmlTextAttribute(name, Token(SyntaxKind.EqualsToken), startQuoteToken, textTokens, endQuoteToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextAttributeSyntax XmlTextAttribute(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxToken startQuoteToken, SyntaxToken endQuoteToken)
	{
		return XmlTextAttribute(name, Token(SyntaxKind.EqualsToken), startQuoteToken, default, endQuoteToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlCrefAttributeSyntax XmlCrefAttribute(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax cref, SyntaxToken endQuoteToken)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		SyntaxKind syntaxKind = equalsToken.Kind();
		if (syntaxKind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException("equalsToken");
		}
		syntaxKind = startQuoteToken.Kind();
		if (syntaxKind - 8213 > SyntaxKind.List)
		{
			throw new ArgumentException("startQuoteToken");
		}
		if (cref == null)
		{
			throw new ArgumentNullException("cref");
		}
		syntaxKind = endQuoteToken.Kind();
		if (syntaxKind - 8213 > SyntaxKind.List)
		{
			throw new ArgumentException("endQuoteToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.XmlCrefAttributeSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlCrefAttribute((name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlNameSyntax)name.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)equalsToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)startQuoteToken.Node, (cref == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefSyntax)cref.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endQuoteToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlCrefAttributeSyntax XmlCrefAttribute(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxToken startQuoteToken, Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax cref, SyntaxToken endQuoteToken)
	{
		return XmlCrefAttribute(name, Token(SyntaxKind.EqualsToken), startQuoteToken, cref, endQuoteToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameAttributeSyntax XmlNameAttribute(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax identifier, SyntaxToken endQuoteToken)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		SyntaxKind syntaxKind = equalsToken.Kind();
		if (syntaxKind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException("equalsToken");
		}
		syntaxKind = startQuoteToken.Kind();
		if (syntaxKind - 8213 > SyntaxKind.List)
		{
			throw new ArgumentException("startQuoteToken");
		}
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		syntaxKind = endQuoteToken.Kind();
		if (syntaxKind - 8213 > SyntaxKind.List)
		{
			throw new ArgumentException("endQuoteToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameAttributeSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlNameAttribute((name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlNameSyntax)name.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)equalsToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)startQuoteToken.Node, (identifier == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IdentifierNameSyntax)identifier.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endQuoteToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameAttributeSyntax XmlNameAttribute(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxToken startQuoteToken, Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax identifier, SyntaxToken endQuoteToken)
	{
		return XmlNameAttribute(name, Token(SyntaxKind.EqualsToken), startQuoteToken, identifier, endQuoteToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameAttributeSyntax XmlNameAttribute(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxToken startQuoteToken, string identifier, SyntaxToken endQuoteToken)
	{
		return XmlNameAttribute(name, Token(SyntaxKind.EqualsToken), startQuoteToken, IdentifierName(identifier), endQuoteToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextSyntax XmlText(SyntaxTokenList textTokens)
	{
		return (Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlText(textTokens.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>()).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextSyntax XmlText()
	{
		return XmlText(default(SyntaxTokenList));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlCDataSectionSyntax XmlCDataSection(SyntaxToken startCDataToken, SyntaxTokenList textTokens, SyntaxToken endCDataToken)
	{
		SyntaxKind syntaxKind = startCDataToken.Kind();
		if (syntaxKind != SyntaxKind.XmlCDataStartToken)
		{
			throw new ArgumentException("startCDataToken");
		}
		syntaxKind = endCDataToken.Kind();
		if (syntaxKind != SyntaxKind.XmlCDataEndToken)
		{
			throw new ArgumentException("endCDataToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.XmlCDataSectionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlCDataSection((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)startCDataToken.Node, textTokens.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endCDataToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlCDataSectionSyntax XmlCDataSection(SyntaxTokenList textTokens = default(SyntaxTokenList))
	{
		return XmlCDataSection(Token(SyntaxKind.XmlCDataStartToken), textTokens, Token(SyntaxKind.XmlCDataEndToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlProcessingInstructionSyntax XmlProcessingInstruction(SyntaxToken startProcessingInstructionToken, Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxTokenList textTokens, SyntaxToken endProcessingInstructionToken)
	{
		SyntaxKind syntaxKind = startProcessingInstructionToken.Kind();
		if (syntaxKind != SyntaxKind.XmlProcessingInstructionStartToken)
		{
			throw new ArgumentException("startProcessingInstructionToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		syntaxKind = endProcessingInstructionToken.Kind();
		if (syntaxKind != SyntaxKind.XmlProcessingInstructionEndToken)
		{
			throw new ArgumentException("endProcessingInstructionToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.XmlProcessingInstructionSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlProcessingInstruction((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)startProcessingInstructionToken.Node, (name == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlNameSyntax)name.Green), textTokens.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endProcessingInstructionToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlProcessingInstructionSyntax XmlProcessingInstruction(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxTokenList textTokens)
	{
		return XmlProcessingInstruction(Token(SyntaxKind.XmlProcessingInstructionStartToken), name, textTokens, Token(SyntaxKind.XmlProcessingInstructionEndToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlProcessingInstructionSyntax XmlProcessingInstruction(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name)
	{
		return XmlProcessingInstruction(Token(SyntaxKind.XmlProcessingInstructionStartToken), name, default, Token(SyntaxKind.XmlProcessingInstructionEndToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlCommentSyntax XmlComment(SyntaxToken lessThanExclamationMinusMinusToken, SyntaxTokenList textTokens, SyntaxToken minusMinusGreaterThanToken)
	{
		SyntaxKind syntaxKind = lessThanExclamationMinusMinusToken.Kind();
		if (syntaxKind != SyntaxKind.XmlCommentStartToken)
		{
			throw new ArgumentException("lessThanExclamationMinusMinusToken");
		}
		syntaxKind = minusMinusGreaterThanToken.Kind();
		if (syntaxKind != SyntaxKind.XmlCommentEndToken)
		{
			throw new ArgumentException("minusMinusGreaterThanToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.XmlCommentSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlComment((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)lessThanExclamationMinusMinusToken.Node, textTokens.Node.ToGreenList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)minusMinusGreaterThanToken.Node).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlCommentSyntax XmlComment(SyntaxTokenList textTokens = default(SyntaxTokenList))
	{
		return XmlComment(Token(SyntaxKind.XmlCommentStartToken), textTokens, Token(SyntaxKind.XmlCommentEndToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IfDirectiveTriviaSyntax IfDirectiveTrivia(SyntaxToken hashToken, SyntaxToken ifKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = ifKeyword.Kind();
		if (syntaxKind != SyntaxKind.IfKeyword)
		{
			throw new ArgumentException("ifKeyword");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.IfDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.IfDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)ifKeyword.Node, (condition == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)condition.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive, branchTaken, conditionValue).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IfDirectiveTriviaSyntax IfDirectiveTrivia(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, bool isActive, bool branchTaken, bool conditionValue)
	{
		return IfDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.IfKeyword), condition, Token(SyntaxKind.EndOfDirectiveToken), isActive, branchTaken, conditionValue);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ElifDirectiveTriviaSyntax ElifDirectiveTrivia(SyntaxToken hashToken, SyntaxToken elifKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = elifKeyword.Kind();
		if (syntaxKind != SyntaxKind.ElifKeyword)
		{
			throw new ArgumentException("elifKeyword");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ElifDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ElifDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)elifKeyword.Node, (condition == null) ? null : ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax)condition.Green), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive, branchTaken, conditionValue).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ElifDirectiveTriviaSyntax ElifDirectiveTrivia(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition, bool isActive, bool branchTaken, bool conditionValue)
	{
		return ElifDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.ElifKeyword), condition, Token(SyntaxKind.EndOfDirectiveToken), isActive, branchTaken, conditionValue);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ElseDirectiveTriviaSyntax ElseDirectiveTrivia(SyntaxToken hashToken, SyntaxToken elseKeyword, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = elseKeyword.Kind();
		if (syntaxKind != SyntaxKind.ElseKeyword)
		{
			throw new ArgumentException("elseKeyword");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ElseDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ElseDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)elseKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive, branchTaken).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ElseDirectiveTriviaSyntax ElseDirectiveTrivia(bool isActive, bool branchTaken)
	{
		return ElseDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.ElseKeyword), Token(SyntaxKind.EndOfDirectiveToken), isActive, branchTaken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EndIfDirectiveTriviaSyntax EndIfDirectiveTrivia(SyntaxToken hashToken, SyntaxToken endIfKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = endIfKeyword.Kind();
		if (syntaxKind != SyntaxKind.EndIfKeyword)
		{
			throw new ArgumentException("endIfKeyword");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.EndIfDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.EndIfDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endIfKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EndIfDirectiveTriviaSyntax EndIfDirectiveTrivia(bool isActive)
	{
		return EndIfDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.EndIfKeyword), Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.RegionDirectiveTriviaSyntax RegionDirectiveTrivia(SyntaxToken hashToken, SyntaxToken regionKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = regionKeyword.Kind();
		if (syntaxKind != SyntaxKind.RegionKeyword)
		{
			throw new ArgumentException("regionKeyword");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.RegionDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.RegionDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)regionKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.RegionDirectiveTriviaSyntax RegionDirectiveTrivia(bool isActive)
	{
		return RegionDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.RegionKeyword), Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EndRegionDirectiveTriviaSyntax EndRegionDirectiveTrivia(SyntaxToken hashToken, SyntaxToken endRegionKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = endRegionKeyword.Kind();
		if (syntaxKind != SyntaxKind.EndRegionKeyword)
		{
			throw new ArgumentException("endRegionKeyword");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.EndRegionDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.EndRegionDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endRegionKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.EndRegionDirectiveTriviaSyntax EndRegionDirectiveTrivia(bool isActive)
	{
		return EndRegionDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.EndRegionKeyword), Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ErrorDirectiveTriviaSyntax ErrorDirectiveTrivia(SyntaxToken hashToken, SyntaxToken errorKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = errorKeyword.Kind();
		if (syntaxKind != SyntaxKind.ErrorKeyword)
		{
			throw new ArgumentException("errorKeyword");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ErrorDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ErrorDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)errorKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ErrorDirectiveTriviaSyntax ErrorDirectiveTrivia(bool isActive)
	{
		return ErrorDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.ErrorKeyword), Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.WarningDirectiveTriviaSyntax WarningDirectiveTrivia(SyntaxToken hashToken, SyntaxToken warningKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = warningKeyword.Kind();
		if (syntaxKind != SyntaxKind.WarningKeyword)
		{
			throw new ArgumentException("warningKeyword");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.WarningDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.WarningDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)warningKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.WarningDirectiveTriviaSyntax WarningDirectiveTrivia(bool isActive)
	{
		return WarningDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.WarningKeyword), Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BadDirectiveTriviaSyntax BadDirectiveTrivia(SyntaxToken hashToken, SyntaxToken identifier, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.BadDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.BadDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)identifier.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BadDirectiveTriviaSyntax BadDirectiveTrivia(SyntaxToken identifier, bool isActive)
	{
		return BadDirectiveTrivia(Token(SyntaxKind.HashToken), identifier, Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DefineDirectiveTriviaSyntax DefineDirectiveTrivia(SyntaxToken hashToken, SyntaxToken defineKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = defineKeyword.Kind();
		if (syntaxKind != SyntaxKind.DefineKeyword)
		{
			throw new ArgumentException("defineKeyword");
		}
		syntaxKind = name.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("name");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.DefineDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.DefineDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)defineKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)name.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DefineDirectiveTriviaSyntax DefineDirectiveTrivia(SyntaxToken name, bool isActive)
	{
		return DefineDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.DefineKeyword), name, Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DefineDirectiveTriviaSyntax DefineDirectiveTrivia(string name, bool isActive)
	{
		return DefineDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.DefineKeyword), Identifier(name), Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.UndefDirectiveTriviaSyntax UndefDirectiveTrivia(SyntaxToken hashToken, SyntaxToken undefKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = undefKeyword.Kind();
		if (syntaxKind != SyntaxKind.UndefKeyword)
		{
			throw new ArgumentException("undefKeyword");
		}
		syntaxKind = name.Kind();
		if (syntaxKind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException("name");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.UndefDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.UndefDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)undefKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)name.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.UndefDirectiveTriviaSyntax UndefDirectiveTrivia(SyntaxToken name, bool isActive)
	{
		return UndefDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.UndefKeyword), name, Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.UndefDirectiveTriviaSyntax UndefDirectiveTrivia(string name, bool isActive)
	{
		return UndefDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.UndefKeyword), Identifier(name), Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LineDirectiveTriviaSyntax LineDirectiveTrivia(SyntaxToken hashToken, SyntaxToken lineKeyword, SyntaxToken line, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = lineKeyword.Kind();
		if (syntaxKind != SyntaxKind.LineKeyword)
		{
			throw new ArgumentException("lineKeyword");
		}
		syntaxKind = line.Kind();
		if (syntaxKind != SyntaxKind.DefaultKeyword && syntaxKind != SyntaxKind.HiddenKeyword && syntaxKind != SyntaxKind.NumericLiteralToken)
		{
			throw new ArgumentException("line");
		}
		syntaxKind = file.Kind();
		if (syntaxKind != SyntaxKind.None && syntaxKind != SyntaxKind.StringLiteralToken)
		{
			throw new ArgumentException("file");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.LineDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.LineDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)lineKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)line.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)file.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LineDirectiveTriviaSyntax LineDirectiveTrivia(SyntaxToken line, SyntaxToken file, bool isActive)
	{
		return LineDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.LineKeyword), line, file, Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LineDirectiveTriviaSyntax LineDirectiveTrivia(SyntaxToken line, bool isActive)
	{
		return LineDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.LineKeyword), line, default, Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PragmaWarningDirectiveTriviaSyntax PragmaWarningDirectiveTrivia(SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken warningKeyword, SyntaxToken disableOrRestoreKeyword, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax> errorCodes, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = pragmaKeyword.Kind();
		if (syntaxKind != SyntaxKind.PragmaKeyword)
		{
			throw new ArgumentException("pragmaKeyword");
		}
		syntaxKind = warningKeyword.Kind();
		if (syntaxKind != SyntaxKind.WarningKeyword)
		{
			throw new ArgumentException("warningKeyword");
		}
		syntaxKind = disableOrRestoreKeyword.Kind();
		if (syntaxKind - 8479 > SyntaxKind.List)
		{
			throw new ArgumentException("disableOrRestoreKeyword");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.PragmaWarningDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.PragmaWarningDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)pragmaKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)warningKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)disableOrRestoreKeyword.Node, errorCodes.Node.ToGreenSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax>(), (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PragmaWarningDirectiveTriviaSyntax PragmaWarningDirectiveTrivia(SyntaxToken disableOrRestoreKeyword, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax> errorCodes, bool isActive)
	{
		return PragmaWarningDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.PragmaKeyword), Token(SyntaxKind.WarningKeyword), disableOrRestoreKeyword, errorCodes, Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PragmaWarningDirectiveTriviaSyntax PragmaWarningDirectiveTrivia(SyntaxToken disableOrRestoreKeyword, bool isActive)
	{
		return PragmaWarningDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.PragmaKeyword), Token(SyntaxKind.WarningKeyword), disableOrRestoreKeyword, default, Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PragmaChecksumDirectiveTriviaSyntax PragmaChecksumDirectiveTrivia(SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken checksumKeyword, SyntaxToken file, SyntaxToken guid, SyntaxToken bytes, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = pragmaKeyword.Kind();
		if (syntaxKind != SyntaxKind.PragmaKeyword)
		{
			throw new ArgumentException("pragmaKeyword");
		}
		syntaxKind = checksumKeyword.Kind();
		if (syntaxKind != SyntaxKind.ChecksumKeyword)
		{
			throw new ArgumentException("checksumKeyword");
		}
		syntaxKind = file.Kind();
		if (syntaxKind != SyntaxKind.StringLiteralToken)
		{
			throw new ArgumentException("file");
		}
		syntaxKind = guid.Kind();
		if (syntaxKind != SyntaxKind.StringLiteralToken)
		{
			throw new ArgumentException("guid");
		}
		syntaxKind = bytes.Kind();
		if (syntaxKind != SyntaxKind.StringLiteralToken)
		{
			throw new ArgumentException("bytes");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.PragmaChecksumDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.PragmaChecksumDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)pragmaKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)checksumKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)file.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)guid.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)bytes.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PragmaChecksumDirectiveTriviaSyntax PragmaChecksumDirectiveTrivia(SyntaxToken file, SyntaxToken guid, SyntaxToken bytes, bool isActive)
	{
		return PragmaChecksumDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.PragmaKeyword), Token(SyntaxKind.ChecksumKeyword), file, guid, bytes, Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ReferenceDirectiveTriviaSyntax ReferenceDirectiveTrivia(SyntaxToken hashToken, SyntaxToken referenceKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = referenceKeyword.Kind();
		if (syntaxKind != SyntaxKind.ReferenceKeyword)
		{
			throw new ArgumentException("referenceKeyword");
		}
		syntaxKind = file.Kind();
		if (syntaxKind != SyntaxKind.StringLiteralToken)
		{
			throw new ArgumentException("file");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ReferenceDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ReferenceDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)referenceKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)file.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ReferenceDirectiveTriviaSyntax ReferenceDirectiveTrivia(SyntaxToken file, bool isActive)
	{
		return ReferenceDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.ReferenceKeyword), file, Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LoadDirectiveTriviaSyntax LoadDirectiveTrivia(SyntaxToken hashToken, SyntaxToken loadKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = loadKeyword.Kind();
		if (syntaxKind != SyntaxKind.LoadKeyword)
		{
			throw new ArgumentException("loadKeyword");
		}
		syntaxKind = file.Kind();
		if (syntaxKind != SyntaxKind.StringLiteralToken)
		{
			throw new ArgumentException("file");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.LoadDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.LoadDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)loadKeyword.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)file.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.LoadDirectiveTriviaSyntax LoadDirectiveTrivia(SyntaxToken file, bool isActive)
	{
		return LoadDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.LoadKeyword), file, Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ShebangDirectiveTriviaSyntax ShebangDirectiveTrivia(SyntaxToken hashToken, SyntaxToken exclamationToken, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		SyntaxKind syntaxKind = hashToken.Kind();
		if (syntaxKind != SyntaxKind.HashToken)
		{
			throw new ArgumentException("hashToken");
		}
		syntaxKind = exclamationToken.Kind();
		if (syntaxKind != SyntaxKind.ExclamationToken)
		{
			throw new ArgumentException("exclamationToken");
		}
		syntaxKind = endOfDirectiveToken.Kind();
		if (syntaxKind != SyntaxKind.EndOfDirectiveToken)
		{
			throw new ArgumentException("endOfDirectiveToken");
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ShebangDirectiveTriviaSyntax)Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.ShebangDirectiveTrivia((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)hashToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)exclamationToken.Node, (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)endOfDirectiveToken.Node, isActive).CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ShebangDirectiveTriviaSyntax ShebangDirectiveTrivia(bool isActive)
	{
		return ShebangDirectiveTrivia(Token(SyntaxKind.HashToken), Token(SyntaxKind.ExclamationToken), Token(SyntaxKind.EndOfDirectiveToken), isActive);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousMethodExpressionSyntax AnonymousMethodExpression()
	{
		return AnonymousMethodExpression(default, Token(SyntaxKind.DelegateKeyword), null, Block());
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax ConstructorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax initializer, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body)
	{
		return ConstructorDeclaration(attributeLists, modifiers, identifier, parameterList, initializer, body, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax ConstructorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax initializer, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, SyntaxToken semicolonToken)
	{
		return ConstructorDeclaration(attributeLists, modifiers, identifier, parameterList, initializer, body, null, semicolonToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax ConstructorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax initializer, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody)
	{
		return ConstructorDeclaration(attributeLists, modifiers, identifier, parameterList, initializer, null, expressionBody, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax ConstructorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax initializer, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return ConstructorDeclaration(attributeLists, modifiers, identifier, parameterList, initializer, null, expressionBody, semicolonToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DestructorDeclarationSyntax DestructorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body)
	{
		return DestructorDeclaration(attributeLists, modifiers, Token(SyntaxKind.TildeToken), identifier, parameterList, body, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DestructorDeclarationSyntax DestructorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken tildeToken, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, SyntaxToken semicolonToken)
	{
		return DestructorDeclaration(attributeLists, modifiers, tildeToken, identifier, parameterList, body, null, semicolonToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DestructorDeclarationSyntax DestructorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody)
	{
		return DestructorDeclaration(attributeLists, modifiers, Token(SyntaxKind.TildeToken), identifier, parameterList, null, expressionBody, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DestructorDeclarationSyntax DestructorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken tildeToken, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return DestructorDeclaration(attributeLists, modifiers, tildeToken, identifier, parameterList, null, expressionBody, semicolonToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IndexerDeclarationSyntax IndexerDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, Microsoft.CodeAnalysis.CSharp.Syntax.BracketedParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.AccessorListSyntax accessorList)
	{
		return IndexerDeclaration(attributeLists, modifiers, type, explicitInterfaceSpecifier, parameterList, accessorList, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax MethodDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, SyntaxToken semicolonToken)
	{
		return MethodDeclaration(attributeLists, modifiers, returnType, explicitInterfaceSpecifier, identifier, typeParameterList, parameterList, constraintClauses, body, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body)
	{
		return AccessorDeclaration(kind, default, default, Token(GetAccessorDeclarationKeywordKind(kind)), body, null, default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.RefTypeSyntax RefType(SyntaxToken refKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return RefType(refKeyword, default, type);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.StackAllocArrayCreationExpressionSyntax StackAllocArrayCreationExpression(SyntaxToken stackAllocKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type)
	{
		return StackAllocArrayCreationExpression(stackAllocKeyword, type, null);
	}

	public static SyntaxTrivia EndOfLine(string text)
	{
		return Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.EndOfLine(text);
	}

	public static SyntaxTrivia ElasticEndOfLine(string text)
	{
		return Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.EndOfLine(text, elastic: true);
	}

	[Obsolete("Use SyntaxFactory.EndOfLine or SyntaxFactory.ElasticEndOfLine")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static SyntaxTrivia EndOfLine(string text, bool elastic)
	{
		return Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.EndOfLine(text, elastic);
	}

	public static SyntaxTrivia Whitespace(string text)
	{
		return Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Whitespace(text);
	}

	public static SyntaxTrivia ElasticWhitespace(string text)
	{
		return Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Whitespace(text);
	}

	[Obsolete("Use SyntaxFactory.Whitespace or SyntaxFactory.ElasticWhitespace")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static SyntaxTrivia Whitespace(string text, bool elastic)
	{
		return Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Whitespace(text, elastic);
	}

	public static SyntaxTrivia Comment(string text)
	{
		return Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Comment(text);
	}

	public static SyntaxTrivia DisabledText(string text)
	{
		return Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.DisabledText(text);
	}

	public static SyntaxTrivia PreprocessingMessage(string text)
	{
		return Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.PreprocessingMessage(text);
	}

	public static SyntaxTrivia SyntaxTrivia(SyntaxKind kind, string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (kind - 8539 <= (SyntaxKind)4 || kind == SyntaxKind.DisabledTextTrivia)
		{
			return new SyntaxTrivia(default(SyntaxToken), new Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxTrivia(kind, text), 0, 0);
		}
		throw new ArgumentException("kind");
	}

	public static SyntaxToken Token(SyntaxKind kind)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Token(ElasticMarker.UnderlyingNode, kind, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken Token(SyntaxTriviaList leading, SyntaxKind kind, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Token(leading.Node, kind, trailing.Node));
	}

	public static SyntaxToken Token(SyntaxTriviaList leading, SyntaxKind kind, string text, string valueText, SyntaxTriviaList trailing)
	{
		switch (kind)
		{
		case SyntaxKind.IdentifierToken:
			throw new ArgumentException(CSharpResources.UseVerbatimIdentifier, "kind");
		case SyntaxKind.CharacterLiteralToken:
			throw new ArgumentException(CSharpResources.UseLiteralForTokens, "kind");
		case SyntaxKind.NumericLiteralToken:
			throw new ArgumentException(CSharpResources.UseLiteralForNumeric, "kind");
		default:
			if (!SyntaxFacts.IsAnyToken(kind))
			{
				throw new ArgumentException(string.Format(CSharpResources.ThisMethodCanOnlyBeUsedToCreateTokens, kind), "kind");
			}
			return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Token(leading.Node, kind, text, valueText, trailing.Node));
		}
	}

	public static SyntaxToken MissingToken(SyntaxKind kind)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.MissingToken(ElasticMarker.UnderlyingNode, kind, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken MissingToken(SyntaxTriviaList leading, SyntaxKind kind, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.MissingToken(leading.Node, kind, trailing.Node));
	}

	public static SyntaxToken Identifier(string text)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Identifier(ElasticMarker.UnderlyingNode, text, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken Identifier(SyntaxTriviaList leading, string text, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Identifier(leading.Node, text, trailing.Node));
	}

	public static SyntaxToken VerbatimIdentifier(SyntaxTriviaList leading, string text, string valueText, SyntaxTriviaList trailing)
	{
		if (text.StartsWith("@", StringComparison.Ordinal))
		{
			throw new ArgumentException("text should not start with an @ character.");
		}
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Identifier(SyntaxKind.IdentifierName, leading.Node, "@" + text, valueText, trailing.Node));
	}

	public static SyntaxToken Identifier(SyntaxTriviaList leading, SyntaxKind contextualKind, string text, string valueText, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Identifier(contextualKind, leading.Node, text, valueText, trailing.Node));
	}

	public static SyntaxToken Literal(int value)
	{
		return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None), value);
	}

	public static SyntaxToken Literal(string text, int value)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken Literal(SyntaxTriviaList leading, string text, int value, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
	}

	public static SyntaxToken Literal(uint value)
	{
		return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.IncludeTypeSuffix), value);
	}

	public static SyntaxToken Literal(string text, uint value)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken Literal(SyntaxTriviaList leading, string text, uint value, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
	}

	public static SyntaxToken Literal(long value)
	{
		return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.IncludeTypeSuffix), value);
	}

	public static SyntaxToken Literal(string text, long value)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken Literal(SyntaxTriviaList leading, string text, long value, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
	}

	public static SyntaxToken Literal(ulong value)
	{
		return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.IncludeTypeSuffix), value);
	}

	public static SyntaxToken Literal(string text, ulong value)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken Literal(SyntaxTriviaList leading, string text, ulong value, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
	}

	public static SyntaxToken Literal(float value)
	{
		return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.IncludeTypeSuffix), value);
	}

	public static SyntaxToken Literal(string text, float value)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken Literal(SyntaxTriviaList leading, string text, float value, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
	}

	public static SyntaxToken Literal(double value)
	{
		return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None), value);
	}

	public static SyntaxToken Literal(string text, double value)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken Literal(SyntaxTriviaList leading, string text, double value, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
	}

	public static SyntaxToken Literal(decimal value)
	{
		return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.IncludeTypeSuffix), value);
	}

	public static SyntaxToken Literal(string text, decimal value)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken Literal(SyntaxTriviaList leading, string text, decimal value, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
	}

	public static SyntaxToken Literal(string value)
	{
		return Literal(SymbolDisplay.FormatLiteral(value, quote: true), value);
	}

	public static SyntaxToken Literal(string text, string value)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken Literal(SyntaxTriviaList leading, string text, string value, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
	}

	public static SyntaxToken Literal(char value)
	{
		return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.UseQuotes | ObjectDisplayOptions.EscapeNonPrintableCharacters), value);
	}

	public static SyntaxToken Literal(string text, char value)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken Literal(SyntaxTriviaList leading, string text, char value, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
	}

	public static SyntaxToken BadToken(SyntaxTriviaList leading, string text, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.BadToken(leading.Node, text, trailing.Node));
	}

	public static SyntaxToken XmlTextLiteral(SyntaxTriviaList leading, string text, string value, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlTextLiteral(leading.Node, text, value, trailing.Node));
	}

	public static SyntaxToken XmlEntity(SyntaxTriviaList leading, string text, string value, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlEntity(leading.Node, text, value, trailing.Node));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DocumentationCommentTriviaSyntax DocumentationComment(params Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax[] content)
	{
		return DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, List(content)).WithLeadingTrivia(DocumentationCommentExterior("/// ")).WithTrailingTrivia(EndOfLine(""));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlSummaryElement(params Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax[] content)
	{
		return XmlSummaryElement(List(content));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlSummaryElement(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		return XmlMultiLineElement("summary", content);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax XmlSeeElement(Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax cref)
	{
		return XmlEmptyElement("see").AddAttributes(XmlCrefAttribute(cref));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax XmlSeeAlsoElement(Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax cref)
	{
		return XmlEmptyElement("seealso").AddAttributes(XmlCrefAttribute(cref));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlSeeAlsoElement(Uri linkAddress, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> linkText)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax xmlElementSyntax = XmlElement("seealso", linkText);
		return xmlElementSyntax.WithStartTag(xmlElementSyntax.StartTag.AddAttributes(XmlTextAttribute("cref", linkAddress.ToString())));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax XmlThreadSafetyElement()
	{
		return XmlThreadSafetyElement(isStatic: true, isInstance: false);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax XmlThreadSafetyElement(bool isStatic, bool isInstance)
	{
		return XmlEmptyElement("threadsafety").AddAttributes(XmlTextAttribute("static", isStatic.ToString().ToLowerInvariant()), XmlTextAttribute("instance", isInstance.ToString().ToLowerInvariant()));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameAttributeSyntax XmlNameAttribute(string parameterName)
	{
		return XmlNameAttribute(XmlName("name"), Token(SyntaxKind.DoubleQuoteToken), parameterName, Token(SyntaxKind.DoubleQuoteToken)).WithLeadingTrivia(Whitespace(" "));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax XmlPreliminaryElement()
	{
		return XmlEmptyElement("preliminary");
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlCrefAttributeSyntax XmlCrefAttribute(Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax cref)
	{
		return XmlCrefAttribute(cref, SyntaxKind.DoubleQuoteToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlCrefAttributeSyntax XmlCrefAttribute(Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax cref, SyntaxKind quoteKind)
	{
		cref = cref.ReplaceTokens(cref.DescendantTokens(), XmlReplaceBracketTokens);
		return XmlCrefAttribute(XmlName("cref"), Token(quoteKind), cref, Token(quoteKind)).WithLeadingTrivia(Whitespace(" "));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlRemarksElement(params Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax[] content)
	{
		return XmlRemarksElement(List(content));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlRemarksElement(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		return XmlMultiLineElement("remarks", content);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlReturnsElement(params Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax[] content)
	{
		return XmlReturnsElement(List(content));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlReturnsElement(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		return XmlMultiLineElement("returns", content);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlValueElement(params Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax[] content)
	{
		return XmlValueElement(List(content));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlValueElement(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		return XmlMultiLineElement("value", content);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlExceptionElement(Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax cref, params Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax[] content)
	{
		return XmlExceptionElement(cref, List(content));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlExceptionElement(Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax cref, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax xmlElementSyntax = XmlElement("exception", content);
		return xmlElementSyntax.WithStartTag(xmlElementSyntax.StartTag.AddAttributes(XmlCrefAttribute(cref)));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlPermissionElement(Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax cref, params Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax[] content)
	{
		return XmlPermissionElement(cref, List(content));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlPermissionElement(Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax cref, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax xmlElementSyntax = XmlElement("permission", content);
		return xmlElementSyntax.WithStartTag(xmlElementSyntax.StartTag.AddAttributes(XmlCrefAttribute(cref)));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlExampleElement(params Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax[] content)
	{
		return XmlExampleElement(List(content));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlExampleElement(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax xmlElementSyntax = XmlElement("example", content);
		return xmlElementSyntax.WithStartTag(xmlElementSyntax.StartTag);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlParaElement(params Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax[] content)
	{
		return XmlParaElement(List(content));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlParaElement(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		return XmlElement("para", content);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlParamElement(string parameterName, params Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax[] content)
	{
		return XmlParamElement(parameterName, List(content));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlParamElement(string parameterName, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax xmlElementSyntax = XmlElement("param", content);
		return xmlElementSyntax.WithStartTag(xmlElementSyntax.StartTag.AddAttributes(XmlNameAttribute(parameterName)));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax XmlParamRefElement(string parameterName)
	{
		return XmlEmptyElement("paramref").AddAttributes(XmlNameAttribute(parameterName));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax XmlNullKeywordElement()
	{
		return XmlKeywordElement("null");
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax XmlKeywordElement(string keyword)
	{
		return XmlEmptyElement("see").AddAttributes(XmlTextAttribute("langword", keyword));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlPlaceholderElement(params Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax[] content)
	{
		return XmlPlaceholderElement(List(content));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlPlaceholderElement(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		return XmlElement("placeholder", content);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax XmlEmptyElement(string localName)
	{
		return XmlEmptyElement(XmlName(localName));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlElement(string localName, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		return XmlElement(XmlName(localName), content);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlElement(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		return XmlElement(XmlElementStartTag(name), content, XmlElementEndTag(name));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextAttributeSyntax XmlTextAttribute(string name, string value)
	{
		return XmlTextAttribute(name, XmlTextLiteral(value));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextAttributeSyntax XmlTextAttribute(string name, params SyntaxToken[] textTokens)
	{
		return XmlTextAttribute(XmlName(name), SyntaxKind.DoubleQuoteToken, TokenList(textTokens));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextAttributeSyntax XmlTextAttribute(string name, SyntaxKind quoteKind, SyntaxTokenList textTokens)
	{
		return XmlTextAttribute(XmlName(name), quoteKind, textTokens);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextAttributeSyntax XmlTextAttribute(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxKind quoteKind, SyntaxTokenList textTokens)
	{
		return XmlTextAttribute(name, Token(quoteKind), textTokens, Token(quoteKind)).WithLeadingTrivia(Whitespace(" "));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlMultiLineElement(string localName, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		return XmlMultiLineElement(XmlName(localName), content);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax XmlMultiLineElement(Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax name, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax> content)
	{
		return XmlElement(XmlElementStartTag(name), content, XmlElementEndTag(name));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextSyntax XmlNewLine(string text)
	{
		return XmlText(XmlTextNewLine(text));
	}

	public static SyntaxToken XmlTextNewLine(string text)
	{
		return XmlTextNewLine(text, continueXmlDocumentationComment: true);
	}

	public static SyntaxToken XmlTextNewLine(SyntaxTriviaList leading, string text, string value, SyntaxTriviaList trailing)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlTextNewLine(leading.Node, text, value, trailing.Node));
	}

	public static SyntaxToken XmlTextNewLine(string text, bool continueXmlDocumentationComment)
	{
		SyntaxToken result = new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlTextNewLine(ElasticMarker.UnderlyingNode, text, text, ElasticMarker.UnderlyingNode));
		if (continueXmlDocumentationComment)
		{
			return result.WithTrailingTrivia(result.TrailingTrivia.Add(DocumentationCommentExterior("/// ")));
		}
		return result;
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextSyntax XmlText(string value)
	{
		return XmlText(XmlTextLiteral(value));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextSyntax XmlText(params SyntaxToken[] textTokens)
	{
		return XmlText(TokenList(textTokens));
	}

	public static SyntaxToken XmlTextLiteral(string value)
	{
		string text = new XText(value).ToString();
		return XmlTextLiteral(TriviaList(), text, value, TriviaList());
	}

	public static SyntaxToken XmlTextLiteral(string text, string value)
	{
		return new SyntaxToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.XmlTextLiteral(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
	}

	private static SyntaxToken XmlReplaceBracketTokens(SyntaxToken originalToken, SyntaxToken rewrittenToken)
	{
		if (rewrittenToken.IsKind(SyntaxKind.LessThanToken) && string.Equals("<", rewrittenToken.Text, StringComparison.Ordinal))
		{
			return Token(rewrittenToken.LeadingTrivia, SyntaxKind.LessThanToken, "{", rewrittenToken.ValueText, rewrittenToken.TrailingTrivia);
		}
		if (rewrittenToken.IsKind(SyntaxKind.GreaterThanToken) && string.Equals(">", rewrittenToken.Text, StringComparison.Ordinal))
		{
			return Token(rewrittenToken.LeadingTrivia, SyntaxKind.GreaterThanToken, "}", rewrittenToken.ValueText, rewrittenToken.TrailingTrivia);
		}
		return rewrittenToken;
	}

	public static SyntaxTrivia DocumentationCommentExterior(string text)
	{
		return Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.DocumentationCommentExteriorTrivia(text);
	}

	public static SyntaxList<TNode> List<TNode>() where TNode : SyntaxNode
	{
		return default;
	}

	public static SyntaxList<TNode> SingletonList<TNode>(TNode node) where TNode : SyntaxNode
	{
		return new SyntaxList<TNode>(node);
	}

	public static SyntaxList<TNode> List<TNode>(IEnumerable<TNode> nodes) where TNode : SyntaxNode
	{
		return new SyntaxList<TNode>(nodes);
	}

	public static SyntaxTokenList TokenList()
	{
		return default;
	}

	public static SyntaxTokenList TokenList(SyntaxToken token)
	{
		return new SyntaxTokenList(token);
	}

	public static SyntaxTokenList TokenList(params SyntaxToken[] tokens)
	{
		return new SyntaxTokenList(tokens);
	}

	public static SyntaxTokenList TokenList(IEnumerable<SyntaxToken> tokens)
	{
		return new SyntaxTokenList(tokens);
	}

	public static SyntaxTrivia Trivia(Microsoft.CodeAnalysis.CSharp.Syntax.StructuredTriviaSyntax node)
	{
		return new SyntaxTrivia(default(SyntaxToken), node.Green, 0, 0);
	}

	public static SyntaxTriviaList TriviaList()
	{
		return default;
	}

	public static SyntaxTriviaList TriviaList(SyntaxTrivia trivia)
	{
		return new SyntaxTriviaList(trivia);
	}

	public static SyntaxTriviaList TriviaList(params SyntaxTrivia[] trivias)
	{
		return new SyntaxTriviaList(trivias);
	}

	public static SyntaxTriviaList TriviaList(IEnumerable<SyntaxTrivia> trivias)
	{
		return new SyntaxTriviaList(trivias);
	}

	public static SeparatedSyntaxList<TNode> SeparatedList<TNode>() where TNode : SyntaxNode
	{
		return default;
	}

	public static SeparatedSyntaxList<TNode> SingletonSeparatedList<TNode>(TNode node) where TNode : SyntaxNode
	{
		return new SeparatedSyntaxList<TNode>(new SyntaxNodeOrTokenList(node, 0));
	}

	public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IEnumerable<TNode> nodes) where TNode : SyntaxNode
	{
		if (nodes == null)
		{
			return default;
		}
		ICollection<TNode> collection = nodes as ICollection<TNode>;
		if (collection != null && collection.Count == 0)
		{
			return default;
		}
		using IEnumerator<TNode> enumerator = nodes.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return default;
		}
		TNode current = enumerator.Current;
		if (!enumerator.MoveNext())
		{
			return SingletonSeparatedList(current);
		}
		Microsoft.CodeAnalysis.Syntax.SeparatedSyntaxListBuilder<TNode> separatedSyntaxListBuilder = new Microsoft.CodeAnalysis.Syntax.SeparatedSyntaxListBuilder<TNode>(collection?.Count ?? 3);
		separatedSyntaxListBuilder.Add(current);
		SyntaxToken separatorToken = Token(SyntaxKind.CommaToken);
		do
		{
			separatedSyntaxListBuilder.AddSeparator(in separatorToken);
			separatedSyntaxListBuilder.Add(enumerator.Current);
		}
		while (enumerator.MoveNext());
		return separatedSyntaxListBuilder.ToList();
	}

	public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IEnumerable<TNode> nodes, IEnumerable<SyntaxToken> separators) where TNode : SyntaxNode
	{
		if (nodes != null)
		{
			IEnumerator<TNode> enumerator = nodes.GetEnumerator();
			Microsoft.CodeAnalysis.Syntax.SeparatedSyntaxListBuilder<TNode> separatedSyntaxListBuilder = Microsoft.CodeAnalysis.Syntax.SeparatedSyntaxListBuilder<TNode>.Create();
			if (separators != null)
			{
				foreach (SyntaxToken separator in separators)
				{
					SyntaxToken separatorToken = separator;
					if (!enumerator.MoveNext())
					{
						throw new ArgumentException("nodes must not be empty.", "nodes");
					}
					separatedSyntaxListBuilder.Add(enumerator.Current);
					separatedSyntaxListBuilder.AddSeparator(in separatorToken);
				}
			}
			if (enumerator.MoveNext())
			{
				separatedSyntaxListBuilder.Add(enumerator.Current);
				if (enumerator.MoveNext())
				{
					throw new ArgumentException("separators must have 1 fewer element than nodes", "separators");
				}
			}
			return separatedSyntaxListBuilder.ToList();
		}
		if (separators != null)
		{
			throw new ArgumentException("When nodes is null, separators must also be null.", "separators");
		}
		return default;
	}

	public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IEnumerable<SyntaxNodeOrToken> nodesAndTokens) where TNode : SyntaxNode
	{
		return SeparatedList<TNode>(NodeOrTokenList(nodesAndTokens));
	}

	public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(SyntaxNodeOrTokenList nodesAndTokens) where TNode : SyntaxNode
	{
		if (!HasSeparatedNodeTokenPattern(nodesAndTokens))
		{
			throw new ArgumentException(CodeAnalysisResources.NodeOrTokenOutOfSequence);
		}
		if (!NodesAreCorrectType<TNode>(nodesAndTokens))
		{
			throw new ArgumentException(CodeAnalysisResources.UnexpectedTypeOfNodeInList);
		}
		return new SeparatedSyntaxList<TNode>(nodesAndTokens);
	}

	private static bool NodesAreCorrectType<TNode>(SyntaxNodeOrTokenList list)
	{
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			SyntaxNodeOrToken syntaxNodeOrToken = list[i];
			if (syntaxNodeOrToken.IsNode && !(syntaxNodeOrToken.AsNode() is TNode))
			{
				return false;
			}
		}
		return true;
	}

	private static bool HasSeparatedNodeTokenPattern(SyntaxNodeOrTokenList list)
	{
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (list[i].IsToken == ((i & 1) == 0))
			{
				return false;
			}
		}
		return true;
	}

	public static SyntaxNodeOrTokenList NodeOrTokenList()
	{
		return default;
	}

	public static SyntaxNodeOrTokenList NodeOrTokenList(IEnumerable<SyntaxNodeOrToken> nodesAndTokens)
	{
		return new SyntaxNodeOrTokenList(nodesAndTokens);
	}

	public static SyntaxNodeOrTokenList NodeOrTokenList(params SyntaxNodeOrToken[] nodesAndTokens)
	{
		return new SyntaxNodeOrTokenList(nodesAndTokens);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax IdentifierName(string name)
	{
		return IdentifierName(Identifier(name));
	}

	public static SyntaxTree SyntaxTree(SyntaxNode root, ParseOptions options = null, string path = "", Encoding encoding = null)
	{
		return CSharpSyntaxTree.Create((CSharpSyntaxNode)root, (CSharpParseOptions)options, path, encoding);
	}

	public static SyntaxTree ParseSyntaxTree(string text, ParseOptions options = null, string path = "", Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ParseSyntaxTree(SourceText.From(text, encoding), options, path, cancellationToken);
	}

	public static SyntaxTree ParseSyntaxTree(SourceText text, ParseOptions options = null, string path = "", CancellationToken cancellationToken = default(CancellationToken))
	{
		return CSharpSyntaxTree.ParseText(text, (CSharpParseOptions)options, path, cancellationToken);
	}

	public static SyntaxTriviaList ParseLeadingTrivia(string text, int offset = 0)
	{
		return ParseLeadingTrivia(text, CSharpParseOptions.Default, offset);
	}

	internal static SyntaxTriviaList ParseLeadingTrivia(string text, CSharpParseOptions options, int offset = 0)
	{
		using Lexer lexer = new Lexer(MakeSourceText(text, offset), options);
		return lexer.LexSyntaxLeadingTrivia();
	}

	public static SyntaxTriviaList ParseTrailingTrivia(string text, int offset = 0)
	{
		using Lexer lexer = new Lexer(MakeSourceText(text, offset), CSharpParseOptions.Default);
		return lexer.LexSyntaxTrailingTrivia();
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax ParseCref(string text)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.XmlAttributeSyntax xmlAttributeSyntax = ((Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax)((Microsoft.CodeAnalysis.CSharp.Syntax.DocumentationCommentTriviaSyntax)ParseLeadingTrivia($"/// <see cref=\"{text}\"/>", CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Diagnose)).First().GetStructure()).Content[1]).Attributes[0];
		if (xmlAttributeSyntax.Kind() != SyntaxKind.XmlCrefAttribute)
		{
			return null;
		}
		return ((Microsoft.CodeAnalysis.CSharp.Syntax.XmlCrefAttributeSyntax)xmlAttributeSyntax).Cref;
	}

	public static SyntaxToken ParseToken(string text, int offset = 0)
	{
		using Lexer lexer = new Lexer(MakeSourceText(text, offset), CSharpParseOptions.Default);
		return new SyntaxToken(lexer.Lex(LexerMode.Syntax));
	}

	public static IEnumerable<SyntaxToken> ParseTokens(string text, int offset = 0, int initialTokenPosition = 0, CSharpParseOptions options = null)
	{
		using Lexer lexer = new Lexer(MakeSourceText(text, offset), options ?? CSharpParseOptions.Default);
		int position = initialTokenPosition;
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken token;
		do
		{
			token = lexer.Lex(LexerMode.Syntax);
			yield return new SyntaxToken(null, token, position, 0);
			position += token.FullWidth;
		}
		while (token.Kind != SyntaxKind.EndOfFileToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax ParseName(string text, int offset = 0, bool consumeFullText = true)
	{
		using Lexer lexer = MakeLexer(text, offset);
		using LanguageParser languageParser = MakeParser(lexer);
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameSyntax nameSyntax = languageParser.ParseName();
		if (consumeFullText)
		{
			nameSyntax = languageParser.ConsumeUnexpectedTokens(nameSyntax);
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax)nameSyntax.CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax ParseTypeName(string text, int offset = 0, bool consumeFullText = true)
	{
		using Lexer lexer = MakeLexer(text, offset);
		using LanguageParser languageParser = MakeParser(lexer);
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax typeSyntax = languageParser.ParseTypeName();
		if (consumeFullText)
		{
			typeSyntax = languageParser.ConsumeUnexpectedTokens(typeSyntax);
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax)typeSyntax.CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax ParseExpression(string text, int offset = 0, ParseOptions options = null, bool consumeFullText = true)
	{
		using Lexer lexer = MakeLexer(text, offset, (CSharpParseOptions)options);
		using LanguageParser languageParser = MakeParser(lexer);
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionSyntax expressionSyntax = languageParser.ParseExpression();
		if (consumeFullText)
		{
			expressionSyntax = languageParser.ConsumeUnexpectedTokens(expressionSyntax);
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax)expressionSyntax.CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax ParseStatement(string text, int offset = 0, ParseOptions options = null, bool consumeFullText = true)
	{
		using Lexer lexer = MakeLexer(text, offset, (CSharpParseOptions)options);
		using LanguageParser languageParser = MakeParser(lexer);
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StatementSyntax statementSyntax = languageParser.ParseStatement();
		if (consumeFullText)
		{
			statementSyntax = languageParser.ConsumeUnexpectedTokens(statementSyntax);
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax)statementSyntax.CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax ParseCompilationUnit(string text, int offset = 0, CSharpParseOptions options = null)
	{
		using Lexer lexer = MakeLexer(text, offset, options);
		using LanguageParser languageParser = MakeParser(lexer);
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)languageParser.ParseCompilationUnit().CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax ParseParameterList(string text, int offset = 0, ParseOptions options = null, bool consumeFullText = true)
	{
		using Lexer lexer = MakeLexer(text, offset, (CSharpParseOptions)options);
		using LanguageParser languageParser = MakeParser(lexer);
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax parameterListSyntax = languageParser.ParseParenthesizedParameterList();
		if (consumeFullText)
		{
			parameterListSyntax = languageParser.ConsumeUnexpectedTokens(parameterListSyntax);
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax)parameterListSyntax.CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BracketedParameterListSyntax ParseBracketedParameterList(string text, int offset = 0, ParseOptions options = null, bool consumeFullText = true)
	{
		using Lexer lexer = MakeLexer(text, offset, (CSharpParseOptions)options);
		using LanguageParser languageParser = MakeParser(lexer);
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BracketedParameterListSyntax bracketedParameterListSyntax = languageParser.ParseBracketedParameterList();
		if (consumeFullText)
		{
			bracketedParameterListSyntax = languageParser.ConsumeUnexpectedTokens(bracketedParameterListSyntax);
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.BracketedParameterListSyntax)bracketedParameterListSyntax.CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax ParseArgumentList(string text, int offset = 0, ParseOptions options = null, bool consumeFullText = true)
	{
		using Lexer lexer = MakeLexer(text, offset, (CSharpParseOptions)options);
		using LanguageParser languageParser = MakeParser(lexer);
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArgumentListSyntax argumentListSyntax = languageParser.ParseParenthesizedArgumentList();
		if (consumeFullText)
		{
			argumentListSyntax = languageParser.ConsumeUnexpectedTokens(argumentListSyntax);
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax)argumentListSyntax.CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BracketedArgumentListSyntax ParseBracketedArgumentList(string text, int offset = 0, ParseOptions options = null, bool consumeFullText = true)
	{
		using Lexer lexer = MakeLexer(text, offset, (CSharpParseOptions)options);
		using LanguageParser languageParser = MakeParser(lexer);
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BracketedArgumentListSyntax bracketedArgumentListSyntax = languageParser.ParseBracketedArgumentList();
		if (consumeFullText)
		{
			bracketedArgumentListSyntax = languageParser.ConsumeUnexpectedTokens(bracketedArgumentListSyntax);
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.BracketedArgumentListSyntax)bracketedArgumentListSyntax.CreateRed();
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentListSyntax ParseAttributeArgumentList(string text, int offset = 0, ParseOptions options = null, bool consumeFullText = true)
	{
		using Lexer lexer = MakeLexer(text, offset, (CSharpParseOptions)options);
		using LanguageParser languageParser = MakeParser(lexer);
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeArgumentListSyntax attributeArgumentListSyntax = languageParser.ParseAttributeArgumentList();
		if (consumeFullText)
		{
			attributeArgumentListSyntax = languageParser.ConsumeUnexpectedTokens(attributeArgumentListSyntax);
		}
		return (Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentListSyntax)attributeArgumentListSyntax.CreateRed();
	}

	private static SourceText MakeSourceText(string text, int offset)
	{
		return SourceText.From(text, Encoding.UTF8).GetSubText(offset);
	}

	private static Lexer MakeLexer(string text, int offset, CSharpParseOptions options = null)
	{
		return new Lexer(MakeSourceText(text, offset), options ?? CSharpParseOptions.Default);
	}

	private static LanguageParser MakeParser(Lexer lexer)
	{
		return new LanguageParser(lexer, null, null);
	}

	public static bool AreEquivalent(SyntaxTree oldTree, SyntaxTree newTree, bool topLevel)
	{
		if (oldTree == null && newTree == null)
		{
			return true;
		}
		if (oldTree == null || newTree == null)
		{
			return false;
		}
		return SyntaxEquivalence.AreEquivalent(oldTree, newTree, null, topLevel);
	}

	public static bool AreEquivalent(SyntaxNode oldNode, SyntaxNode newNode, bool topLevel)
	{
		return SyntaxEquivalence.AreEquivalent(oldNode, newNode, null, topLevel);
	}

	public static bool AreEquivalent(SyntaxNode oldNode, SyntaxNode newNode, Func<SyntaxKind, bool> ignoreChildNode = null)
	{
		return SyntaxEquivalence.AreEquivalent(oldNode, newNode, ignoreChildNode, topLevel: false);
	}

	public static bool AreEquivalent(SyntaxToken oldToken, SyntaxToken newToken)
	{
		return SyntaxEquivalence.AreEquivalent(oldToken, newToken);
	}

	public static bool AreEquivalent(SyntaxTokenList oldList, SyntaxTokenList newList)
	{
		return SyntaxEquivalence.AreEquivalent(oldList, newList);
	}

	public static bool AreEquivalent<TNode>(SyntaxList<TNode> oldList, SyntaxList<TNode> newList, bool topLevel) where TNode : CSharpSyntaxNode
	{
		return SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, null, topLevel);
	}

	public static bool AreEquivalent<TNode>(SyntaxList<TNode> oldList, SyntaxList<TNode> newList, Func<SyntaxKind, bool> ignoreChildNode = null) where TNode : SyntaxNode
	{
		return SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, ignoreChildNode, topLevel: false);
	}

	public static bool AreEquivalent<TNode>(SeparatedSyntaxList<TNode> oldList, SeparatedSyntaxList<TNode> newList, bool topLevel) where TNode : SyntaxNode
	{
		return SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, null, topLevel);
	}

	public static bool AreEquivalent<TNode>(SeparatedSyntaxList<TNode> oldList, SeparatedSyntaxList<TNode> newList, Func<SyntaxKind, bool> ignoreChildNode = null) where TNode : SyntaxNode
	{
		return SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, ignoreChildNode, topLevel: false);
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax GetStandaloneType(Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax node)
	{
		if (node != null && node.Parent is Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax && (node.Kind() == SyntaxKind.IdentifierName || node.Kind() == SyntaxKind.GenericName))
		{
			switch (expressionSyntax.Kind())
			{
			case SyntaxKind.QualifiedName:
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax qualifiedNameSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax)expressionSyntax;
				if (qualifiedNameSyntax.Right == node)
				{
					return qualifiedNameSyntax;
				}
				break;
			}
			case SyntaxKind.AliasQualifiedName:
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax aliasQualifiedNameSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax)expressionSyntax;
				if (aliasQualifiedNameSyntax.Name == node)
				{
					return aliasQualifiedNameSyntax;
				}
				break;
			}
			}
		}
		return node;
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax GetStandaloneExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return (GetStandaloneNode(expression) as Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax) ?? expression;
	}

	internal static CSharpSyntaxNode GetStandaloneNode(CSharpSyntaxNode node)
	{
		if (node == null || (!(node is Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax) && !(node is Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax)))
		{
			return node;
		}
		switch (node.Kind())
		{
		default:
			return node;
		case SyntaxKind.NameMemberCref:
		case SyntaxKind.IndexerMemberCref:
		case SyntaxKind.OperatorMemberCref:
		case SyntaxKind.ConversionOperatorMemberCref:
		case SyntaxKind.IdentifierName:
		case SyntaxKind.GenericName:
		case SyntaxKind.ArrayType:
		case SyntaxKind.NullableType:
		{
			CSharpSyntaxNode parent = node.Parent;
			if (parent == null)
			{
				return node;
			}
			switch (parent.Kind())
			{
			case SyntaxKind.QualifiedName:
				if (((Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax)parent).Right == node)
				{
					return parent;
				}
				break;
			case SyntaxKind.AliasQualifiedName:
				if (((Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax)parent).Name == node)
				{
					return parent;
				}
				break;
			case SyntaxKind.SimpleMemberAccessExpression:
			case SyntaxKind.PointerMemberAccessExpression:
				if (((Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax)parent).Name == node)
				{
					return parent;
				}
				break;
			case SyntaxKind.MemberBindingExpression:
				if (((Microsoft.CodeAnalysis.CSharp.Syntax.MemberBindingExpressionSyntax)parent).Name == node)
				{
					return parent;
				}
				break;
			case SyntaxKind.NameMemberCref:
				if (((Microsoft.CodeAnalysis.CSharp.Syntax.NameMemberCrefSyntax)parent).Name == node)
				{
					CSharpSyntaxNode parent2 = parent.Parent;
					if (parent2 == null || parent2.Kind() != SyntaxKind.QualifiedCref)
					{
						return parent;
					}
					return parent2;
				}
				break;
			case SyntaxKind.QualifiedCref:
				if (((Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedCrefSyntax)parent).Member == node)
				{
					return parent;
				}
				break;
			case SyntaxKind.ArrayCreationExpression:
				if (((Microsoft.CodeAnalysis.CSharp.Syntax.ArrayCreationExpressionSyntax)parent).Type == node)
				{
					return parent;
				}
				break;
			case SyntaxKind.ObjectCreationExpression:
				if (node.Kind() == SyntaxKind.NullableType && ((Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax)parent).Type == node)
				{
					return parent;
				}
				break;
			case SyntaxKind.StackAllocArrayCreationExpression:
				if (((Microsoft.CodeAnalysis.CSharp.Syntax.StackAllocArrayCreationExpressionSyntax)parent).Type == node)
				{
					return parent;
				}
				break;
			}
			return node;
		}
		}
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalAccessExpressionSyntax FindConditionalAccessNodeForBinding(CSharpSyntaxNode node)
	{
		CSharpSyntaxNode cSharpSyntaxNode = node;
		while (cSharpSyntaxNode != null)
		{
			cSharpSyntaxNode = cSharpSyntaxNode.Parent;
			if (cSharpSyntaxNode.Kind() == SyntaxKind.ConditionalAccessExpression)
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalAccessExpressionSyntax conditionalAccessExpressionSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalAccessExpressionSyntax)cSharpSyntaxNode;
				if (conditionalAccessExpressionSyntax.OperatorToken.EndPosition == node.Position)
				{
					return conditionalAccessExpressionSyntax;
				}
			}
		}
		return null;
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax GetNonGenericExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		if (expression != null)
		{
			switch (expression.Kind())
			{
			case SyntaxKind.SimpleMemberAccessExpression:
			case SyntaxKind.PointerMemberAccessExpression:
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax memberAccessExpressionSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax)expression;
				if (memberAccessExpressionSyntax.Name.Kind() == SyntaxKind.GenericName)
				{
					Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax genericNameSyntax2 = (Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax)memberAccessExpressionSyntax.Name;
					return BinaryExpression(expression.Kind(), memberAccessExpressionSyntax.Expression, memberAccessExpressionSyntax.OperatorToken, IdentifierName(genericNameSyntax2.Identifier));
				}
				break;
			}
			case SyntaxKind.QualifiedName:
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax qualifiedNameSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax)expression;
				if (qualifiedNameSyntax.Right.Kind() == SyntaxKind.GenericName)
				{
					Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax genericNameSyntax3 = (Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax)qualifiedNameSyntax.Right;
					return QualifiedName(qualifiedNameSyntax.Left, qualifiedNameSyntax.DotToken, IdentifierName(genericNameSyntax3.Identifier));
				}
				break;
			}
			case SyntaxKind.AliasQualifiedName:
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax aliasQualifiedNameSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax)expression;
				if (aliasQualifiedNameSyntax.Name.Kind() == SyntaxKind.GenericName)
				{
					Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax genericNameSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax)aliasQualifiedNameSyntax.Name;
					return AliasQualifiedName(aliasQualifiedNameSyntax.Alias, aliasQualifiedNameSyntax.ColonColonToken, IdentifierName(genericNameSyntax.Identifier));
				}
				break;
			}
			}
		}
		return expression;
	}

	public static bool IsCompleteSubmission(SyntaxTree tree)
	{
		if (tree == null)
		{
			throw new ArgumentNullException("tree");
		}
		if (tree.Options.Kind != SourceCodeKind.Script)
		{
			throw new ArgumentException(CSharpResources.SyntaxTreeIsNotASubmission);
		}
		if (!tree.HasCompilationUnitRoot)
		{
			return false;
		}
		Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax compilationUnitSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)tree.GetRoot();
		if (!compilationUnitSyntax.HasErrors)
		{
			return true;
		}
		foreach (Diagnostic diagnostic in compilationUnitSyntax.EndOfFileToken.GetDiagnostics())
		{
			ErrorCode code = (ErrorCode)diagnostic.Code;
			if (code == ErrorCode.ERR_EndifDirectiveExpected || code == ErrorCode.ERR_OpenEndedComment || code == ErrorCode.ERR_EndRegionDirectiveExpected)
			{
				return false;
			}
		}
		SyntaxNode syntaxNode = compilationUnitSyntax.ChildNodes().LastOrDefault();
		if (syntaxNode == null)
		{
			return true;
		}
		if (syntaxNode.HasTrailingTrivia && syntaxNode.ContainsDiagnostics && HasUnterminatedMultiLineComment(syntaxNode.GetTrailingTrivia()))
		{
			return false;
		}
		if (syntaxNode.IsKind(SyntaxKind.IncompleteMember))
		{
			return false;
		}
		if (!syntaxNode.IsKind(SyntaxKind.GlobalStatement))
		{
			return !syntaxNode.GetLastToken(includeZeroWidth: true, includeSkipped: true, includeDirectives: true, includeDocumentationComments: true).IsMissing;
		}
		Microsoft.CodeAnalysis.CSharp.Syntax.GlobalStatementSyntax globalStatementSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.GlobalStatementSyntax)syntaxNode;
		SyntaxToken token = syntaxNode.GetLastToken(includeZeroWidth: true, includeSkipped: true, includeDirectives: true, includeDocumentationComments: true);
		if (token.IsMissing)
		{
			if (tree.Options.Kind == SourceCodeKind.Regular || !globalStatementSyntax.Statement.IsKind(SyntaxKind.ExpressionStatement) || !token.IsKind(SyntaxKind.SemicolonToken))
			{
				return false;
			}
			token = token.GetPreviousToken(SyntaxToken.Any, Microsoft.CodeAnalysis.SyntaxTrivia.Any);
			if (token.IsMissing)
			{
				return false;
			}
		}
		foreach (Diagnostic diagnostic2 in token.GetDiagnostics())
		{
			switch ((ErrorCode)diagnostic2.Code)
			{
			case ErrorCode.ERR_NewlineInConst:
			case ErrorCode.ERR_EOFExpected:
			case ErrorCode.ERR_UnterminatedStringLit:
			case ErrorCode.ERR_GlobalDefinitionOrStatementExpected:
				return false;
			}
		}
		return true;
	}

	private static bool HasUnterminatedMultiLineComment(SyntaxTriviaList triviaList)
	{
		foreach (SyntaxTrivia item in triviaList)
		{
			if (item.ContainsDiagnostics && item.Kind() == SyntaxKind.MultiLineCommentTrivia)
			{
				return true;
			}
		}
		return false;
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CaseSwitchLabelSyntax CaseSwitchLabel(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax value)
	{
		return CaseSwitchLabel(Token(SyntaxKind.CaseKeyword), value, Token(SyntaxKind.ColonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DefaultSwitchLabelSyntax DefaultSwitchLabel()
	{
		return DefaultSwitchLabel(Token(SyntaxKind.DefaultKeyword), Token(SyntaxKind.ColonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax Block(params Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax[] statements)
	{
		return Block(List(statements));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax Block(IEnumerable<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statements)
	{
		return Block(List(statements));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax PropertyDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.AccessorListSyntax accessorList)
	{
		return PropertyDeclaration(attributeLists, modifiers, type, explicitInterfaceSpecifier, identifier, accessorList, null, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.ConversionOperatorDeclarationSyntax ConversionOperatorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, SyntaxToken semicolonToken)
	{
		return ConversionOperatorDeclaration(attributeLists, modifiers, implicitOrExplicitKeyword, operatorKeyword, type, parameterList, body, null, semicolonToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.OperatorDeclarationSyntax OperatorDeclaration(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax returnType, SyntaxToken operatorKeyword, SyntaxToken operatorToken, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax parameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, SyntaxToken semicolonToken)
	{
		return OperatorDeclaration(attributeLists, modifiers, returnType, operatorKeyword, operatorToken, parameterList, body, null, semicolonToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax UsingDirective(Microsoft.CodeAnalysis.CSharp.Syntax.NameEqualsSyntax alias, Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name)
	{
		return UsingDirective(Token(SyntaxKind.UsingKeyword), default, alias, name, Token(SyntaxKind.SemicolonToken));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body)
	{
		return AccessorDeclaration(kind, attributeLists, modifiers, body, null);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken keyword, Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax body, SyntaxToken semicolonToken)
	{
		return AccessorDeclaration(kind, attributeLists, modifiers, keyword, body, null, semicolonToken);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody)
	{
		return AccessorDeclaration(kind, attributeLists, modifiers, null, expressionBody);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken keyword, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		return AccessorDeclaration(kind, attributeLists, modifiers, keyword, null, expressionBody, semicolonToken);
	}

	internal static SyntaxKind GetTypeDeclarationKeywordKind(DeclarationKind kind)
	{
		return kind switch
		{
			DeclarationKind.Class => SyntaxKind.ClassKeyword, 
			DeclarationKind.Struct => SyntaxKind.StructKeyword, 
			DeclarationKind.Interface => SyntaxKind.InterfaceKeyword, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	private static SyntaxKind GetTypeDeclarationKeywordKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.ClassDeclaration => SyntaxKind.ClassKeyword, 
			SyntaxKind.StructDeclaration => SyntaxKind.StructKeyword, 
			SyntaxKind.InterfaceDeclaration => SyntaxKind.InterfaceKeyword, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax TypeDeclaration(SyntaxKind kind, SyntaxToken identifier)
	{
		return TypeDeclaration(kind, default, default, Token(GetTypeDeclarationKeywordKind(kind)), identifier, null, null, default, Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken), default);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax TypeDeclaration(SyntaxKind kind, string identifier)
	{
		return TypeDeclaration(kind, Identifier(identifier));
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax TypeDeclaration(SyntaxKind kind, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributes, SyntaxTokenList modifiers, SyntaxToken keyword, SyntaxToken identifier, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameterList, Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax baseList, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		return kind switch
		{
			SyntaxKind.ClassDeclaration => (Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax)ClassDeclaration(attributes, modifiers, keyword, identifier, typeParameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken), 
			SyntaxKind.StructDeclaration => StructDeclaration(attributes, modifiers, keyword, identifier, typeParameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken), 
			SyntaxKind.InterfaceDeclaration => InterfaceDeclaration(attributes, modifiers, keyword, identifier, typeParameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken), 
			_ => throw new ArgumentException("kind"), 
		};
	}
}
