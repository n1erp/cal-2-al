using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

public abstract class CSharpSyntaxRewriter : CSharpSyntaxVisitor<SyntaxNode>
{
	private readonly bool _visitIntoStructuredTrivia;

	private int _recursionDepth;

	public virtual bool VisitIntoStructuredTrivia => _visitIntoStructuredTrivia;

	public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
	{
		SyntaxToken identifier = VisitToken(node.Identifier);
		return node.Update(identifier);
	}

	public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node)
	{
		NameSyntax left = (NameSyntax)Visit(node.Left);
		SyntaxToken dotToken = VisitToken(node.DotToken);
		SimpleNameSyntax right = (SimpleNameSyntax)Visit(node.Right);
		return node.Update(left, dotToken, right);
	}

	public override SyntaxNode VisitGenericName(GenericNameSyntax node)
	{
		SyntaxToken identifier = VisitToken(node.Identifier);
		TypeArgumentListSyntax typeArgumentList = (TypeArgumentListSyntax)Visit(node.TypeArgumentList);
		return node.Update(identifier, typeArgumentList);
	}

	public override SyntaxNode VisitTypeArgumentList(TypeArgumentListSyntax node)
	{
		SyntaxToken lessThanToken = VisitToken(node.LessThanToken);
		SeparatedSyntaxList<TypeSyntax> arguments = VisitList(node.Arguments);
		SyntaxToken greaterThanToken = VisitToken(node.GreaterThanToken);
		return node.Update(lessThanToken, arguments, greaterThanToken);
	}

	public override SyntaxNode VisitAliasQualifiedName(AliasQualifiedNameSyntax node)
	{
		IdentifierNameSyntax alias = (IdentifierNameSyntax)Visit(node.Alias);
		SyntaxToken colonColonToken = VisitToken(node.ColonColonToken);
		SimpleNameSyntax name = (SimpleNameSyntax)Visit(node.Name);
		return node.Update(alias, colonColonToken, name);
	}

	public override SyntaxNode VisitPredefinedType(PredefinedTypeSyntax node)
	{
		SyntaxToken keyword = VisitToken(node.Keyword);
		return node.Update(keyword);
	}

	public override SyntaxNode VisitArrayType(ArrayTypeSyntax node)
	{
		TypeSyntax elementType = (TypeSyntax)Visit(node.ElementType);
		SyntaxList<ArrayRankSpecifierSyntax> rankSpecifiers = VisitList(node.RankSpecifiers);
		return node.Update(elementType, rankSpecifiers);
	}

	public override SyntaxNode VisitArrayRankSpecifier(ArrayRankSpecifierSyntax node)
	{
		SyntaxToken openBracketToken = VisitToken(node.OpenBracketToken);
		SeparatedSyntaxList<ExpressionSyntax> sizes = VisitList(node.Sizes);
		SyntaxToken closeBracketToken = VisitToken(node.CloseBracketToken);
		return node.Update(openBracketToken, sizes, closeBracketToken);
	}

	public override SyntaxNode VisitPointerType(PointerTypeSyntax node)
	{
		TypeSyntax elementType = (TypeSyntax)Visit(node.ElementType);
		SyntaxToken asteriskToken = VisitToken(node.AsteriskToken);
		return node.Update(elementType, asteriskToken);
	}

	public override SyntaxNode VisitNullableType(NullableTypeSyntax node)
	{
		TypeSyntax elementType = (TypeSyntax)Visit(node.ElementType);
		SyntaxToken questionToken = VisitToken(node.QuestionToken);
		return node.Update(elementType, questionToken);
	}

	public override SyntaxNode VisitTupleType(TupleTypeSyntax node)
	{
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		SeparatedSyntaxList<TupleElementSyntax> elements = VisitList(node.Elements);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(openParenToken, elements, closeParenToken);
	}

	public override SyntaxNode VisitTupleElement(TupleElementSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken identifier = VisitToken(node.Identifier);
		return node.Update(type, identifier);
	}

	public override SyntaxNode VisitOmittedTypeArgument(OmittedTypeArgumentSyntax node)
	{
		SyntaxToken omittedTypeArgumentToken = VisitToken(node.OmittedTypeArgumentToken);
		return node.Update(omittedTypeArgumentToken);
	}

	public override SyntaxNode VisitRefType(RefTypeSyntax node)
	{
		SyntaxToken refKeyword = VisitToken(node.RefKeyword);
		SyntaxToken readOnlyKeyword = VisitToken(node.ReadOnlyKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		return node.Update(refKeyword, readOnlyKeyword, type);
	}

	public override SyntaxNode VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
	{
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(openParenToken, expression, closeParenToken);
	}

	public override SyntaxNode VisitTupleExpression(TupleExpressionSyntax node)
	{
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		SeparatedSyntaxList<ArgumentSyntax> arguments = VisitList(node.Arguments);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(openParenToken, arguments, closeParenToken);
	}

	public override SyntaxNode VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
	{
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		ExpressionSyntax operand = (ExpressionSyntax)Visit(node.Operand);
		return node.Update(operatorToken, operand);
	}

	public override SyntaxNode VisitAwaitExpression(AwaitExpressionSyntax node)
	{
		SyntaxToken awaitKeyword = VisitToken(node.AwaitKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(awaitKeyword, expression);
	}

	public override SyntaxNode VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
	{
		ExpressionSyntax operand = (ExpressionSyntax)Visit(node.Operand);
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		return node.Update(operand, operatorToken);
	}

	public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		SimpleNameSyntax name = (SimpleNameSyntax)Visit(node.Name);
		return node.Update(expression, operatorToken, name);
	}

	public override SyntaxNode VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		ExpressionSyntax whenNotNull = (ExpressionSyntax)Visit(node.WhenNotNull);
		return node.Update(expression, operatorToken, whenNotNull);
	}

	public override SyntaxNode VisitMemberBindingExpression(MemberBindingExpressionSyntax node)
	{
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		SimpleNameSyntax name = (SimpleNameSyntax)Visit(node.Name);
		return node.Update(operatorToken, name);
	}

	public override SyntaxNode VisitElementBindingExpression(ElementBindingExpressionSyntax node)
	{
		BracketedArgumentListSyntax argumentList = (BracketedArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(argumentList);
	}

	public override SyntaxNode VisitImplicitElementAccess(ImplicitElementAccessSyntax node)
	{
		BracketedArgumentListSyntax argumentList = (BracketedArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(argumentList);
	}

	public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
	{
		ExpressionSyntax left = (ExpressionSyntax)Visit(node.Left);
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		ExpressionSyntax right = (ExpressionSyntax)Visit(node.Right);
		return node.Update(left, operatorToken, right);
	}

	public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
	{
		ExpressionSyntax left = (ExpressionSyntax)Visit(node.Left);
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		ExpressionSyntax right = (ExpressionSyntax)Visit(node.Right);
		return node.Update(left, operatorToken, right);
	}

	public override SyntaxNode VisitConditionalExpression(ConditionalExpressionSyntax node)
	{
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken questionToken = VisitToken(node.QuestionToken);
		ExpressionSyntax whenTrue = (ExpressionSyntax)Visit(node.WhenTrue);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		ExpressionSyntax whenFalse = (ExpressionSyntax)Visit(node.WhenFalse);
		return node.Update(condition, questionToken, whenTrue, colonToken, whenFalse);
	}

	public override SyntaxNode VisitThisExpression(ThisExpressionSyntax node)
	{
		SyntaxToken token = VisitToken(node.Token);
		return node.Update(token);
	}

	public override SyntaxNode VisitBaseExpression(BaseExpressionSyntax node)
	{
		SyntaxToken token = VisitToken(node.Token);
		return node.Update(token);
	}

	public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
	{
		SyntaxToken token = VisitToken(node.Token);
		return node.Update(token);
	}

	public override SyntaxNode VisitMakeRefExpression(MakeRefExpressionSyntax node)
	{
		SyntaxToken keyword = VisitToken(node.Keyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(keyword, openParenToken, expression, closeParenToken);
	}

	public override SyntaxNode VisitRefTypeExpression(RefTypeExpressionSyntax node)
	{
		SyntaxToken keyword = VisitToken(node.Keyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(keyword, openParenToken, expression, closeParenToken);
	}

	public override SyntaxNode VisitRefValueExpression(RefValueExpressionSyntax node)
	{
		SyntaxToken keyword = VisitToken(node.Keyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken comma = VisitToken(node.Comma);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(keyword, openParenToken, expression, comma, type, closeParenToken);
	}

	public override SyntaxNode VisitCheckedExpression(CheckedExpressionSyntax node)
	{
		SyntaxToken keyword = VisitToken(node.Keyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(keyword, openParenToken, expression, closeParenToken);
	}

	public override SyntaxNode VisitDefaultExpression(DefaultExpressionSyntax node)
	{
		SyntaxToken keyword = VisitToken(node.Keyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(keyword, openParenToken, type, closeParenToken);
	}

	public override SyntaxNode VisitTypeOfExpression(TypeOfExpressionSyntax node)
	{
		SyntaxToken keyword = VisitToken(node.Keyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(keyword, openParenToken, type, closeParenToken);
	}

	public override SyntaxNode VisitSizeOfExpression(SizeOfExpressionSyntax node)
	{
		SyntaxToken keyword = VisitToken(node.Keyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(keyword, openParenToken, type, closeParenToken);
	}

	public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		ArgumentListSyntax argumentList = (ArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(expression, argumentList);
	}

	public override SyntaxNode VisitElementAccessExpression(ElementAccessExpressionSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		BracketedArgumentListSyntax argumentList = (BracketedArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(expression, argumentList);
	}

	public override SyntaxNode VisitArgumentList(ArgumentListSyntax node)
	{
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		SeparatedSyntaxList<ArgumentSyntax> arguments = VisitList(node.Arguments);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(openParenToken, arguments, closeParenToken);
	}

	public override SyntaxNode VisitBracketedArgumentList(BracketedArgumentListSyntax node)
	{
		SyntaxToken openBracketToken = VisitToken(node.OpenBracketToken);
		SeparatedSyntaxList<ArgumentSyntax> arguments = VisitList(node.Arguments);
		SyntaxToken closeBracketToken = VisitToken(node.CloseBracketToken);
		return node.Update(openBracketToken, arguments, closeBracketToken);
	}

	public override SyntaxNode VisitArgument(ArgumentSyntax node)
	{
		NameColonSyntax nameColon = (NameColonSyntax)Visit(node.NameColon);
		SyntaxToken refKindKeyword = VisitToken(node.RefKindKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(nameColon, refKindKeyword, expression);
	}

	public override SyntaxNode VisitNameColon(NameColonSyntax node)
	{
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		return node.Update(name, colonToken);
	}

	public override SyntaxNode VisitDeclarationExpression(DeclarationExpressionSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		VariableDesignationSyntax designation = (VariableDesignationSyntax)Visit(node.Designation);
		return node.Update(type, designation);
	}

	public override SyntaxNode VisitCastExpression(CastExpressionSyntax node)
	{
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(openParenToken, type, closeParenToken, expression);
	}

	public override SyntaxNode VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
	{
		SyntaxToken asyncKeyword = VisitToken(node.AsyncKeyword);
		SyntaxToken delegateKeyword = VisitToken(node.DelegateKeyword);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		CSharpSyntaxNode body = (CSharpSyntaxNode)Visit(node.Body);
		return node.Update(asyncKeyword, delegateKeyword, parameterList, body);
	}

	public override SyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
	{
		SyntaxToken asyncKeyword = VisitToken(node.AsyncKeyword);
		ParameterSyntax parameter = (ParameterSyntax)Visit(node.Parameter);
		SyntaxToken arrowToken = VisitToken(node.ArrowToken);
		CSharpSyntaxNode body = (CSharpSyntaxNode)Visit(node.Body);
		return node.Update(asyncKeyword, parameter, arrowToken, body);
	}

	public override SyntaxNode VisitRefExpression(RefExpressionSyntax node)
	{
		SyntaxToken refKeyword = VisitToken(node.RefKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(refKeyword, expression);
	}

	public override SyntaxNode VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
	{
		SyntaxToken asyncKeyword = VisitToken(node.AsyncKeyword);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		SyntaxToken arrowToken = VisitToken(node.ArrowToken);
		CSharpSyntaxNode body = (CSharpSyntaxNode)Visit(node.Body);
		return node.Update(asyncKeyword, parameterList, arrowToken, body);
	}

	public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
	{
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SeparatedSyntaxList<ExpressionSyntax> expressions = VisitList(node.Expressions);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(openBraceToken, expressions, closeBraceToken);
	}

	public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
	{
		SyntaxToken newKeyword = VisitToken(node.NewKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		ArgumentListSyntax argumentList = (ArgumentListSyntax)Visit(node.ArgumentList);
		InitializerExpressionSyntax initializer = (InitializerExpressionSyntax)Visit(node.Initializer);
		return node.Update(newKeyword, type, argumentList, initializer);
	}

	public override SyntaxNode VisitAnonymousObjectMemberDeclarator(AnonymousObjectMemberDeclaratorSyntax node)
	{
		NameEqualsSyntax nameEquals = (NameEqualsSyntax)Visit(node.NameEquals);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(nameEquals, expression);
	}

	public override SyntaxNode VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
	{
		SyntaxToken newKeyword = VisitToken(node.NewKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax> initializers = VisitList(node.Initializers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(newKeyword, openBraceToken, initializers, closeBraceToken);
	}

	public override SyntaxNode VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
	{
		SyntaxToken newKeyword = VisitToken(node.NewKeyword);
		ArrayTypeSyntax type = (ArrayTypeSyntax)Visit(node.Type);
		InitializerExpressionSyntax initializer = (InitializerExpressionSyntax)Visit(node.Initializer);
		return node.Update(newKeyword, type, initializer);
	}

	public override SyntaxNode VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
	{
		SyntaxToken newKeyword = VisitToken(node.NewKeyword);
		SyntaxToken openBracketToken = VisitToken(node.OpenBracketToken);
		SyntaxTokenList commas = VisitList(node.Commas);
		SyntaxToken closeBracketToken = VisitToken(node.CloseBracketToken);
		InitializerExpressionSyntax initializer = (InitializerExpressionSyntax)Visit(node.Initializer);
		return node.Update(newKeyword, openBracketToken, commas, closeBracketToken, initializer);
	}

	public override SyntaxNode VisitStackAllocArrayCreationExpression(StackAllocArrayCreationExpressionSyntax node)
	{
		SyntaxToken stackAllocKeyword = VisitToken(node.StackAllocKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		InitializerExpressionSyntax initializer = (InitializerExpressionSyntax)Visit(node.Initializer);
		return node.Update(stackAllocKeyword, type, initializer);
	}

	public override SyntaxNode VisitImplicitStackAllocArrayCreationExpression(ImplicitStackAllocArrayCreationExpressionSyntax node)
	{
		SyntaxToken stackAllocKeyword = VisitToken(node.StackAllocKeyword);
		SyntaxToken openBracketToken = VisitToken(node.OpenBracketToken);
		SyntaxToken closeBracketToken = VisitToken(node.CloseBracketToken);
		InitializerExpressionSyntax initializer = (InitializerExpressionSyntax)Visit(node.Initializer);
		return node.Update(stackAllocKeyword, openBracketToken, closeBracketToken, initializer);
	}

	public override SyntaxNode VisitQueryExpression(QueryExpressionSyntax node)
	{
		FromClauseSyntax fromClause = (FromClauseSyntax)Visit(node.FromClause);
		QueryBodySyntax body = (QueryBodySyntax)Visit(node.Body);
		return node.Update(fromClause, body);
	}

	public override SyntaxNode VisitQueryBody(QueryBodySyntax node)
	{
		SyntaxList<QueryClauseSyntax> clauses = VisitList(node.Clauses);
		SelectOrGroupClauseSyntax selectOrGroup = (SelectOrGroupClauseSyntax)Visit(node.SelectOrGroup);
		QueryContinuationSyntax continuation = (QueryContinuationSyntax)Visit(node.Continuation);
		return node.Update(clauses, selectOrGroup, continuation);
	}

	public override SyntaxNode VisitFromClause(FromClauseSyntax node)
	{
		SyntaxToken fromKeyword = VisitToken(node.FromKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken identifier = VisitToken(node.Identifier);
		SyntaxToken inKeyword = VisitToken(node.InKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(fromKeyword, type, identifier, inKeyword, expression);
	}

	public override SyntaxNode VisitLetClause(LetClauseSyntax node)
	{
		SyntaxToken letKeyword = VisitToken(node.LetKeyword);
		SyntaxToken identifier = VisitToken(node.Identifier);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(letKeyword, identifier, equalsToken, expression);
	}

	public override SyntaxNode VisitJoinClause(JoinClauseSyntax node)
	{
		SyntaxToken joinKeyword = VisitToken(node.JoinKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken identifier = VisitToken(node.Identifier);
		SyntaxToken inKeyword = VisitToken(node.InKeyword);
		ExpressionSyntax inExpression = (ExpressionSyntax)Visit(node.InExpression);
		SyntaxToken onKeyword = VisitToken(node.OnKeyword);
		ExpressionSyntax leftExpression = (ExpressionSyntax)Visit(node.LeftExpression);
		SyntaxToken equalsKeyword = VisitToken(node.EqualsKeyword);
		ExpressionSyntax rightExpression = (ExpressionSyntax)Visit(node.RightExpression);
		JoinIntoClauseSyntax joinIntoClauseSyntax = (JoinIntoClauseSyntax)Visit(node.Into);
		return node.Update(joinKeyword, type, identifier, inKeyword, inExpression, onKeyword, leftExpression, equalsKeyword, rightExpression, joinIntoClauseSyntax);
	}

	public override SyntaxNode VisitJoinIntoClause(JoinIntoClauseSyntax node)
	{
		SyntaxToken intoKeyword = VisitToken(node.IntoKeyword);
		SyntaxToken identifier = VisitToken(node.Identifier);
		return node.Update(intoKeyword, identifier);
	}

	public override SyntaxNode VisitWhereClause(WhereClauseSyntax node)
	{
		SyntaxToken whereKeyword = VisitToken(node.WhereKeyword);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		return node.Update(whereKeyword, condition);
	}

	public override SyntaxNode VisitOrderByClause(OrderByClauseSyntax node)
	{
		SyntaxToken orderByKeyword = VisitToken(node.OrderByKeyword);
		SeparatedSyntaxList<OrderingSyntax> orderings = VisitList(node.Orderings);
		return node.Update(orderByKeyword, orderings);
	}

	public override SyntaxNode VisitOrdering(OrderingSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken ascendingOrDescendingKeyword = VisitToken(node.AscendingOrDescendingKeyword);
		return node.Update(expression, ascendingOrDescendingKeyword);
	}

	public override SyntaxNode VisitSelectClause(SelectClauseSyntax node)
	{
		SyntaxToken selectKeyword = VisitToken(node.SelectKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(selectKeyword, expression);
	}

	public override SyntaxNode VisitGroupClause(GroupClauseSyntax node)
	{
		SyntaxToken groupKeyword = VisitToken(node.GroupKeyword);
		ExpressionSyntax groupExpression = (ExpressionSyntax)Visit(node.GroupExpression);
		SyntaxToken byKeyword = VisitToken(node.ByKeyword);
		ExpressionSyntax byExpression = (ExpressionSyntax)Visit(node.ByExpression);
		return node.Update(groupKeyword, groupExpression, byKeyword, byExpression);
	}

	public override SyntaxNode VisitQueryContinuation(QueryContinuationSyntax node)
	{
		SyntaxToken intoKeyword = VisitToken(node.IntoKeyword);
		SyntaxToken identifier = VisitToken(node.Identifier);
		QueryBodySyntax body = (QueryBodySyntax)Visit(node.Body);
		return node.Update(intoKeyword, identifier, body);
	}

	public override SyntaxNode VisitOmittedArraySizeExpression(OmittedArraySizeExpressionSyntax node)
	{
		SyntaxToken omittedArraySizeExpressionToken = VisitToken(node.OmittedArraySizeExpressionToken);
		return node.Update(omittedArraySizeExpressionToken);
	}

	public override SyntaxNode VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
	{
		SyntaxToken stringStartToken = VisitToken(node.StringStartToken);
		SyntaxList<InterpolatedStringContentSyntax> contents = VisitList(node.Contents);
		SyntaxToken stringEndToken = VisitToken(node.StringEndToken);
		return node.Update(stringStartToken, contents, stringEndToken);
	}

	public override SyntaxNode VisitIsPatternExpression(IsPatternExpressionSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken isKeyword = VisitToken(node.IsKeyword);
		PatternSyntax pattern = (PatternSyntax)Visit(node.Pattern);
		return node.Update(expression, isKeyword, pattern);
	}

	public override SyntaxNode VisitThrowExpression(ThrowExpressionSyntax node)
	{
		SyntaxToken throwKeyword = VisitToken(node.ThrowKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(throwKeyword, expression);
	}

	public override SyntaxNode VisitWhenClause(WhenClauseSyntax node)
	{
		SyntaxToken whenKeyword = VisitToken(node.WhenKeyword);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		return node.Update(whenKeyword, condition);
	}

	public override SyntaxNode VisitDeclarationPattern(DeclarationPatternSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		VariableDesignationSyntax designation = (VariableDesignationSyntax)Visit(node.Designation);
		return node.Update(type, designation);
	}

	public override SyntaxNode VisitConstantPattern(ConstantPatternSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(expression);
	}

	public override SyntaxNode VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
	{
		SyntaxToken textToken = VisitToken(node.TextToken);
		return node.Update(textToken);
	}

	public override SyntaxNode VisitInterpolation(InterpolationSyntax node)
	{
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		InterpolationAlignmentClauseSyntax alignmentClause = (InterpolationAlignmentClauseSyntax)Visit(node.AlignmentClause);
		InterpolationFormatClauseSyntax formatClause = (InterpolationFormatClauseSyntax)Visit(node.FormatClause);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(openBraceToken, expression, alignmentClause, formatClause, closeBraceToken);
	}

	public override SyntaxNode VisitInterpolationAlignmentClause(InterpolationAlignmentClauseSyntax node)
	{
		SyntaxToken commaToken = VisitToken(node.CommaToken);
		ExpressionSyntax value = (ExpressionSyntax)Visit(node.Value);
		return node.Update(commaToken, value);
	}

	public override SyntaxNode VisitInterpolationFormatClause(InterpolationFormatClauseSyntax node)
	{
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		SyntaxToken formatStringToken = VisitToken(node.FormatStringToken);
		return node.Update(colonToken, formatStringToken);
	}

	public override SyntaxNode VisitGlobalStatement(GlobalStatementSyntax node)
	{
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(statement);
	}

	public override SyntaxNode VisitBlock(BlockSyntax node)
	{
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<StatementSyntax> statements = VisitList(node.Statements);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(openBraceToken, statements, closeBraceToken);
	}

	public override SyntaxNode VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
	{
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		TypeSyntax returnType = (TypeSyntax)Visit(node.ReturnType);
		SyntaxToken identifier = VisitToken(node.Identifier);
		TypeParameterListSyntax typeParameterList = (TypeParameterListSyntax)Visit(node.TypeParameterList);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = VisitList(node.ConstraintClauses);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(modifiers, returnType, identifier, typeParameterList, parameterList, constraintClauses, body, expressionBody, semicolonToken);
	}

	public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
	{
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)Visit(node.Declaration);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(modifiers, declaration, semicolonToken);
	}

	public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SeparatedSyntaxList<VariableDeclaratorSyntax> variables = VisitList(node.Variables);
		return node.Update(type, variables);
	}

	public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
	{
		SyntaxToken identifier = VisitToken(node.Identifier);
		BracketedArgumentListSyntax argumentList = (BracketedArgumentListSyntax)Visit(node.ArgumentList);
		EqualsValueClauseSyntax initializer = (EqualsValueClauseSyntax)Visit(node.Initializer);
		return node.Update(identifier, argumentList, initializer);
	}

	public override SyntaxNode VisitEqualsValueClause(EqualsValueClauseSyntax node)
	{
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		ExpressionSyntax value = (ExpressionSyntax)Visit(node.Value);
		return node.Update(equalsToken, value);
	}

	public override SyntaxNode VisitSingleVariableDesignation(SingleVariableDesignationSyntax node)
	{
		SyntaxToken identifier = VisitToken(node.Identifier);
		return node.Update(identifier);
	}

	public override SyntaxNode VisitDiscardDesignation(DiscardDesignationSyntax node)
	{
		SyntaxToken underscoreToken = VisitToken(node.UnderscoreToken);
		return node.Update(underscoreToken);
	}

	public override SyntaxNode VisitParenthesizedVariableDesignation(ParenthesizedVariableDesignationSyntax node)
	{
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		SeparatedSyntaxList<VariableDesignationSyntax> variables = VisitList(node.Variables);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(openParenToken, variables, closeParenToken);
	}

	public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(expression, semicolonToken);
	}

	public override SyntaxNode VisitEmptyStatement(EmptyStatementSyntax node)
	{
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(semicolonToken);
	}

	public override SyntaxNode VisitLabeledStatement(LabeledStatementSyntax node)
	{
		SyntaxToken identifier = VisitToken(node.Identifier);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(identifier, colonToken, statement);
	}

	public override SyntaxNode VisitGotoStatement(GotoStatementSyntax node)
	{
		SyntaxToken gotoKeyword = VisitToken(node.GotoKeyword);
		SyntaxToken caseOrDefaultKeyword = VisitToken(node.CaseOrDefaultKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(gotoKeyword, caseOrDefaultKeyword, expression, semicolonToken);
	}

	public override SyntaxNode VisitBreakStatement(BreakStatementSyntax node)
	{
		SyntaxToken breakKeyword = VisitToken(node.BreakKeyword);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(breakKeyword, semicolonToken);
	}

	public override SyntaxNode VisitContinueStatement(ContinueStatementSyntax node)
	{
		SyntaxToken continueKeyword = VisitToken(node.ContinueKeyword);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(continueKeyword, semicolonToken);
	}

	public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
	{
		SyntaxToken returnKeyword = VisitToken(node.ReturnKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(returnKeyword, expression, semicolonToken);
	}

	public override SyntaxNode VisitThrowStatement(ThrowStatementSyntax node)
	{
		SyntaxToken throwKeyword = VisitToken(node.ThrowKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(throwKeyword, expression, semicolonToken);
	}

	public override SyntaxNode VisitYieldStatement(YieldStatementSyntax node)
	{
		SyntaxToken yieldKeyword = VisitToken(node.YieldKeyword);
		SyntaxToken returnOrBreakKeyword = VisitToken(node.ReturnOrBreakKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(yieldKeyword, returnOrBreakKeyword, expression, semicolonToken);
	}

	public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
	{
		SyntaxToken whileKeyword = VisitToken(node.WhileKeyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(whileKeyword, openParenToken, condition, closeParenToken, statement);
	}

	public override SyntaxNode VisitDoStatement(DoStatementSyntax node)
	{
		SyntaxToken doKeyword = VisitToken(node.DoKeyword);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		SyntaxToken whileKeyword = VisitToken(node.WhileKeyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(doKeyword, statement, whileKeyword, openParenToken, condition, closeParenToken, semicolonToken);
	}

	public override SyntaxNode VisitForStatement(ForStatementSyntax node)
	{
		SyntaxToken forKeyword = VisitToken(node.ForKeyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)Visit(node.Declaration);
		SeparatedSyntaxList<ExpressionSyntax> initializers = VisitList(node.Initializers);
		SyntaxToken firstSemicolonToken = VisitToken(node.FirstSemicolonToken);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken secondSemicolonToken = VisitToken(node.SecondSemicolonToken);
		SeparatedSyntaxList<ExpressionSyntax> incrementors = VisitList(node.Incrementors);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(forKeyword, openParenToken, declaration, initializers, firstSemicolonToken, condition, secondSemicolonToken, incrementors, closeParenToken, statement);
	}

	public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
	{
		SyntaxToken forEachKeyword = VisitToken(node.ForEachKeyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken identifier = VisitToken(node.Identifier);
		SyntaxToken inKeyword = VisitToken(node.InKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(forEachKeyword, openParenToken, type, identifier, inKeyword, expression, closeParenToken, statement);
	}

	public override SyntaxNode VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
	{
		SyntaxToken forEachKeyword = VisitToken(node.ForEachKeyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		ExpressionSyntax variable = (ExpressionSyntax)Visit(node.Variable);
		SyntaxToken inKeyword = VisitToken(node.InKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(forEachKeyword, openParenToken, variable, inKeyword, expression, closeParenToken, statement);
	}

	public override SyntaxNode VisitUsingStatement(UsingStatementSyntax node)
	{
		SyntaxToken usingKeyword = VisitToken(node.UsingKeyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)Visit(node.Declaration);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(usingKeyword, openParenToken, declaration, expression, closeParenToken, statement);
	}

	public override SyntaxNode VisitFixedStatement(FixedStatementSyntax node)
	{
		SyntaxToken fixedKeyword = VisitToken(node.FixedKeyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)Visit(node.Declaration);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(fixedKeyword, openParenToken, declaration, closeParenToken, statement);
	}

	public override SyntaxNode VisitCheckedStatement(CheckedStatementSyntax node)
	{
		SyntaxToken keyword = VisitToken(node.Keyword);
		BlockSyntax block = (BlockSyntax)Visit(node.Block);
		return node.Update(keyword, block);
	}

	public override SyntaxNode VisitUnsafeStatement(UnsafeStatementSyntax node)
	{
		SyntaxToken unsafeKeyword = VisitToken(node.UnsafeKeyword);
		BlockSyntax block = (BlockSyntax)Visit(node.Block);
		return node.Update(unsafeKeyword, block);
	}

	public override SyntaxNode VisitLockStatement(LockStatementSyntax node)
	{
		SyntaxToken lockKeyword = VisitToken(node.LockKeyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(lockKeyword, openParenToken, expression, closeParenToken, statement);
	}

	public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
	{
		SyntaxToken ifKeyword = VisitToken(node.IfKeyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		ElseClauseSyntax elseClauseSyntax = (ElseClauseSyntax)Visit(node.Else);
		return node.Update(ifKeyword, openParenToken, condition, closeParenToken, statement, elseClauseSyntax);
	}

	public override SyntaxNode VisitElseClause(ElseClauseSyntax node)
	{
		SyntaxToken elseKeyword = VisitToken(node.ElseKeyword);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(elseKeyword, statement);
	}

	public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
	{
		SyntaxToken switchKeyword = VisitToken(node.SwitchKeyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<SwitchSectionSyntax> sections = VisitList(node.Sections);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(switchKeyword, openParenToken, expression, closeParenToken, openBraceToken, sections, closeBraceToken);
	}

	public override SyntaxNode VisitSwitchSection(SwitchSectionSyntax node)
	{
		SyntaxList<SwitchLabelSyntax> labels = VisitList(node.Labels);
		SyntaxList<StatementSyntax> statements = VisitList(node.Statements);
		return node.Update(labels, statements);
	}

	public override SyntaxNode VisitCasePatternSwitchLabel(CasePatternSwitchLabelSyntax node)
	{
		SyntaxToken keyword = VisitToken(node.Keyword);
		PatternSyntax pattern = (PatternSyntax)Visit(node.Pattern);
		WhenClauseSyntax whenClause = (WhenClauseSyntax)Visit(node.WhenClause);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		return node.Update(keyword, pattern, whenClause, colonToken);
	}

	public override SyntaxNode VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
	{
		SyntaxToken keyword = VisitToken(node.Keyword);
		ExpressionSyntax value = (ExpressionSyntax)Visit(node.Value);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		return node.Update(keyword, value, colonToken);
	}

	public override SyntaxNode VisitDefaultSwitchLabel(DefaultSwitchLabelSyntax node)
	{
		SyntaxToken keyword = VisitToken(node.Keyword);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		return node.Update(keyword, colonToken);
	}

	public override SyntaxNode VisitTryStatement(TryStatementSyntax node)
	{
		SyntaxToken tryKeyword = VisitToken(node.TryKeyword);
		BlockSyntax block = (BlockSyntax)Visit(node.Block);
		SyntaxList<CatchClauseSyntax> catches = VisitList(node.Catches);
		FinallyClauseSyntax finallyClauseSyntax = (FinallyClauseSyntax)Visit(node.Finally);
		return node.Update(tryKeyword, block, catches, finallyClauseSyntax);
	}

	public override SyntaxNode VisitCatchClause(CatchClauseSyntax node)
	{
		SyntaxToken catchKeyword = VisitToken(node.CatchKeyword);
		CatchDeclarationSyntax declaration = (CatchDeclarationSyntax)Visit(node.Declaration);
		CatchFilterClauseSyntax filter = (CatchFilterClauseSyntax)Visit(node.Filter);
		BlockSyntax block = (BlockSyntax)Visit(node.Block);
		return node.Update(catchKeyword, declaration, filter, block);
	}

	public override SyntaxNode VisitCatchDeclaration(CatchDeclarationSyntax node)
	{
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken identifier = VisitToken(node.Identifier);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(openParenToken, type, identifier, closeParenToken);
	}

	public override SyntaxNode VisitCatchFilterClause(CatchFilterClauseSyntax node)
	{
		SyntaxToken whenKeyword = VisitToken(node.WhenKeyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		ExpressionSyntax filterExpression = (ExpressionSyntax)Visit(node.FilterExpression);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(whenKeyword, openParenToken, filterExpression, closeParenToken);
	}

	public override SyntaxNode VisitFinallyClause(FinallyClauseSyntax node)
	{
		SyntaxToken finallyKeyword = VisitToken(node.FinallyKeyword);
		BlockSyntax block = (BlockSyntax)Visit(node.Block);
		return node.Update(finallyKeyword, block);
	}

	public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
	{
		SyntaxList<ExternAliasDirectiveSyntax> externs = VisitList(node.Externs);
		SyntaxList<UsingDirectiveSyntax> usings = VisitList(node.Usings);
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxList<MemberDeclarationSyntax> members = VisitList(node.Members);
		SyntaxToken endOfFileToken = VisitToken(node.EndOfFileToken);
		return node.Update(externs, usings, attributeLists, members, endOfFileToken);
	}

	public override SyntaxNode VisitExternAliasDirective(ExternAliasDirectiveSyntax node)
	{
		SyntaxToken externKeyword = VisitToken(node.ExternKeyword);
		SyntaxToken aliasKeyword = VisitToken(node.AliasKeyword);
		SyntaxToken identifier = VisitToken(node.Identifier);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(externKeyword, aliasKeyword, identifier, semicolonToken);
	}

	public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
	{
		SyntaxToken usingKeyword = VisitToken(node.UsingKeyword);
		SyntaxToken staticKeyword = VisitToken(node.StaticKeyword);
		NameEqualsSyntax alias = (NameEqualsSyntax)Visit(node.Alias);
		NameSyntax name = (NameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(usingKeyword, staticKeyword, alias, name, semicolonToken);
	}

	public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
	{
		SyntaxToken namespaceKeyword = VisitToken(node.NamespaceKeyword);
		NameSyntax name = (NameSyntax)Visit(node.Name);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<ExternAliasDirectiveSyntax> externs = VisitList(node.Externs);
		SyntaxList<UsingDirectiveSyntax> usings = VisitList(node.Usings);
		SyntaxList<MemberDeclarationSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(namespaceKeyword, name, openBraceToken, externs, usings, members, closeBraceToken, semicolonToken);
	}

	public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
	{
		SyntaxToken openBracketToken = VisitToken(node.OpenBracketToken);
		AttributeTargetSpecifierSyntax target = (AttributeTargetSpecifierSyntax)Visit(node.Target);
		SeparatedSyntaxList<AttributeSyntax> attributes = VisitList(node.Attributes);
		SyntaxToken closeBracketToken = VisitToken(node.CloseBracketToken);
		return node.Update(openBracketToken, target, attributes, closeBracketToken);
	}

	public override SyntaxNode VisitAttributeTargetSpecifier(AttributeTargetSpecifierSyntax node)
	{
		SyntaxToken identifier = VisitToken(node.Identifier);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		return node.Update(identifier, colonToken);
	}

	public override SyntaxNode VisitAttribute(AttributeSyntax node)
	{
		NameSyntax name = (NameSyntax)Visit(node.Name);
		AttributeArgumentListSyntax argumentList = (AttributeArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(name, argumentList);
	}

	public override SyntaxNode VisitAttributeArgumentList(AttributeArgumentListSyntax node)
	{
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		SeparatedSyntaxList<AttributeArgumentSyntax> arguments = VisitList(node.Arguments);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(openParenToken, arguments, closeParenToken);
	}

	public override SyntaxNode VisitAttributeArgument(AttributeArgumentSyntax node)
	{
		NameEqualsSyntax nameEquals = (NameEqualsSyntax)Visit(node.NameEquals);
		NameColonSyntax nameColon = (NameColonSyntax)Visit(node.NameColon);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(nameEquals, nameColon, expression);
	}

	public override SyntaxNode VisitNameEquals(NameEqualsSyntax node)
	{
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		return node.Update(name, equalsToken);
	}

	public override SyntaxNode VisitTypeParameterList(TypeParameterListSyntax node)
	{
		SyntaxToken lessThanToken = VisitToken(node.LessThanToken);
		SeparatedSyntaxList<TypeParameterSyntax> parameters = VisitList(node.Parameters);
		SyntaxToken greaterThanToken = VisitToken(node.GreaterThanToken);
		return node.Update(lessThanToken, parameters, greaterThanToken);
	}

	public override SyntaxNode VisitTypeParameter(TypeParameterSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxToken varianceKeyword = VisitToken(node.VarianceKeyword);
		SyntaxToken identifier = VisitToken(node.Identifier);
		return node.Update(attributeLists, varianceKeyword, identifier);
	}

	public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		SyntaxToken keyword = VisitToken(node.Keyword);
		SyntaxToken identifier = VisitToken(node.Identifier);
		TypeParameterListSyntax typeParameterList = (TypeParameterListSyntax)Visit(node.TypeParameterList);
		BaseListSyntax baseList = (BaseListSyntax)Visit(node.BaseList);
		SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = VisitList(node.ConstraintClauses);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<MemberDeclarationSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, keyword, identifier, typeParameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
	}

	public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		SyntaxToken keyword = VisitToken(node.Keyword);
		SyntaxToken identifier = VisitToken(node.Identifier);
		TypeParameterListSyntax typeParameterList = (TypeParameterListSyntax)Visit(node.TypeParameterList);
		BaseListSyntax baseList = (BaseListSyntax)Visit(node.BaseList);
		SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = VisitList(node.ConstraintClauses);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<MemberDeclarationSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, keyword, identifier, typeParameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
	}

	public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		SyntaxToken keyword = VisitToken(node.Keyword);
		SyntaxToken identifier = VisitToken(node.Identifier);
		TypeParameterListSyntax typeParameterList = (TypeParameterListSyntax)Visit(node.TypeParameterList);
		BaseListSyntax baseList = (BaseListSyntax)Visit(node.BaseList);
		SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = VisitList(node.ConstraintClauses);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<MemberDeclarationSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, keyword, identifier, typeParameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
	}

	public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		SyntaxToken enumKeyword = VisitToken(node.EnumKeyword);
		SyntaxToken identifier = VisitToken(node.Identifier);
		BaseListSyntax baseList = (BaseListSyntax)Visit(node.BaseList);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, enumKeyword, identifier, baseList, openBraceToken, members, closeBraceToken, semicolonToken);
	}

	public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		SyntaxToken delegateKeyword = VisitToken(node.DelegateKeyword);
		TypeSyntax returnType = (TypeSyntax)Visit(node.ReturnType);
		SyntaxToken identifier = VisitToken(node.Identifier);
		TypeParameterListSyntax typeParameterList = (TypeParameterListSyntax)Visit(node.TypeParameterList);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = VisitList(node.ConstraintClauses);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, delegateKeyword, returnType, identifier, typeParameterList, parameterList, constraintClauses, semicolonToken);
	}

	public override SyntaxNode VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxToken identifier = VisitToken(node.Identifier);
		EqualsValueClauseSyntax equalsValue = (EqualsValueClauseSyntax)Visit(node.EqualsValue);
		return node.Update(attributeLists, identifier, equalsValue);
	}

	public override SyntaxNode VisitBaseList(BaseListSyntax node)
	{
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		SeparatedSyntaxList<BaseTypeSyntax> types = VisitList(node.Types);
		return node.Update(colonToken, types);
	}

	public override SyntaxNode VisitSimpleBaseType(SimpleBaseTypeSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		return node.Update(type);
	}

	public override SyntaxNode VisitTypeParameterConstraintClause(TypeParameterConstraintClauseSyntax node)
	{
		SyntaxToken whereKeyword = VisitToken(node.WhereKeyword);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints = VisitList(node.Constraints);
		return node.Update(whereKeyword, name, colonToken, constraints);
	}

	public override SyntaxNode VisitConstructorConstraint(ConstructorConstraintSyntax node)
	{
		SyntaxToken newKeyword = VisitToken(node.NewKeyword);
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(newKeyword, openParenToken, closeParenToken);
	}

	public override SyntaxNode VisitClassOrStructConstraint(ClassOrStructConstraintSyntax node)
	{
		SyntaxToken classOrStructKeyword = VisitToken(node.ClassOrStructKeyword);
		return node.Update(classOrStructKeyword);
	}

	public override SyntaxNode VisitTypeConstraint(TypeConstraintSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		return node.Update(type);
	}

	public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)Visit(node.Declaration);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, declaration, semicolonToken);
	}

	public override SyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		SyntaxToken eventKeyword = VisitToken(node.EventKeyword);
		VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)Visit(node.Declaration);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, eventKeyword, declaration, semicolonToken);
	}

	public override SyntaxNode VisitExplicitInterfaceSpecifier(ExplicitInterfaceSpecifierSyntax node)
	{
		NameSyntax name = (NameSyntax)Visit(node.Name);
		SyntaxToken dotToken = VisitToken(node.DotToken);
		return node.Update(name, dotToken);
	}

	public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		TypeSyntax returnType = (TypeSyntax)Visit(node.ReturnType);
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = (ExplicitInterfaceSpecifierSyntax)Visit(node.ExplicitInterfaceSpecifier);
		SyntaxToken identifier = VisitToken(node.Identifier);
		TypeParameterListSyntax typeParameterList = (TypeParameterListSyntax)Visit(node.TypeParameterList);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = VisitList(node.ConstraintClauses);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, returnType, explicitInterfaceSpecifier, identifier, typeParameterList, parameterList, constraintClauses, body, expressionBody, semicolonToken);
	}

	public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		TypeSyntax returnType = (TypeSyntax)Visit(node.ReturnType);
		SyntaxToken operatorKeyword = VisitToken(node.OperatorKeyword);
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, returnType, operatorKeyword, operatorToken, parameterList, body, expressionBody, semicolonToken);
	}

	public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		SyntaxToken implicitOrExplicitKeyword = VisitToken(node.ImplicitOrExplicitKeyword);
		SyntaxToken operatorKeyword = VisitToken(node.OperatorKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, implicitOrExplicitKeyword, operatorKeyword, type, parameterList, body, expressionBody, semicolonToken);
	}

	public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		SyntaxToken identifier = VisitToken(node.Identifier);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		ConstructorInitializerSyntax initializer = (ConstructorInitializerSyntax)Visit(node.Initializer);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, identifier, parameterList, initializer, body, expressionBody, semicolonToken);
	}

	public override SyntaxNode VisitConstructorInitializer(ConstructorInitializerSyntax node)
	{
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		SyntaxToken thisOrBaseKeyword = VisitToken(node.ThisOrBaseKeyword);
		ArgumentListSyntax argumentList = (ArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(colonToken, thisOrBaseKeyword, argumentList);
	}

	public override SyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		SyntaxToken tildeToken = VisitToken(node.TildeToken);
		SyntaxToken identifier = VisitToken(node.Identifier);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, tildeToken, identifier, parameterList, body, expressionBody, semicolonToken);
	}

	public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = (ExplicitInterfaceSpecifierSyntax)Visit(node.ExplicitInterfaceSpecifier);
		SyntaxToken identifier = VisitToken(node.Identifier);
		AccessorListSyntax accessorList = (AccessorListSyntax)Visit(node.AccessorList);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		EqualsValueClauseSyntax initializer = (EqualsValueClauseSyntax)Visit(node.Initializer);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, type, explicitInterfaceSpecifier, identifier, accessorList, expressionBody, initializer, semicolonToken);
	}

	public override SyntaxNode VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
	{
		SyntaxToken arrowToken = VisitToken(node.ArrowToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(arrowToken, expression);
	}

	public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		SyntaxToken eventKeyword = VisitToken(node.EventKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = (ExplicitInterfaceSpecifierSyntax)Visit(node.ExplicitInterfaceSpecifier);
		SyntaxToken identifier = VisitToken(node.Identifier);
		AccessorListSyntax accessorList = (AccessorListSyntax)Visit(node.AccessorList);
		return node.Update(attributeLists, modifiers, eventKeyword, type, explicitInterfaceSpecifier, identifier, accessorList);
	}

	public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = (ExplicitInterfaceSpecifierSyntax)Visit(node.ExplicitInterfaceSpecifier);
		SyntaxToken thisKeyword = VisitToken(node.ThisKeyword);
		BracketedParameterListSyntax parameterList = (BracketedParameterListSyntax)Visit(node.ParameterList);
		AccessorListSyntax accessorList = (AccessorListSyntax)Visit(node.AccessorList);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, type, explicitInterfaceSpecifier, thisKeyword, parameterList, accessorList, expressionBody, semicolonToken);
	}

	public override SyntaxNode VisitAccessorList(AccessorListSyntax node)
	{
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<AccessorDeclarationSyntax> accessors = VisitList(node.Accessors);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(openBraceToken, accessors, closeBraceToken);
	}

	public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		SyntaxToken keyword = VisitToken(node.Keyword);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, keyword, body, expressionBody, semicolonToken);
	}

	public override SyntaxNode VisitParameterList(ParameterListSyntax node)
	{
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		SeparatedSyntaxList<ParameterSyntax> parameters = VisitList(node.Parameters);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(openParenToken, parameters, closeParenToken);
	}

	public override SyntaxNode VisitBracketedParameterList(BracketedParameterListSyntax node)
	{
		SyntaxToken openBracketToken = VisitToken(node.OpenBracketToken);
		SeparatedSyntaxList<ParameterSyntax> parameters = VisitList(node.Parameters);
		SyntaxToken closeBracketToken = VisitToken(node.CloseBracketToken);
		return node.Update(openBracketToken, parameters, closeBracketToken);
	}

	public override SyntaxNode VisitParameter(ParameterSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken identifier = VisitToken(node.Identifier);
		EqualsValueClauseSyntax equalsValueClauseSyntax = (EqualsValueClauseSyntax)Visit(node.Default);
		return node.Update(attributeLists, modifiers, type, identifier, equalsValueClauseSyntax);
	}

	public override SyntaxNode VisitIncompleteMember(IncompleteMemberSyntax node)
	{
		SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxTokenList modifiers = VisitList(node.Modifiers);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		return node.Update(attributeLists, modifiers, type);
	}

	public override SyntaxNode VisitSkippedTokensTrivia(SkippedTokensTriviaSyntax node)
	{
		SyntaxTokenList tokens = VisitList(node.Tokens);
		return node.Update(tokens);
	}

	public override SyntaxNode VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node)
	{
		SyntaxList<XmlNodeSyntax> content = VisitList(node.Content);
		SyntaxToken endOfComment = VisitToken(node.EndOfComment);
		return node.Update(content, endOfComment);
	}

	public override SyntaxNode VisitTypeCref(TypeCrefSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		return node.Update(type);
	}

	public override SyntaxNode VisitQualifiedCref(QualifiedCrefSyntax node)
	{
		TypeSyntax container = (TypeSyntax)Visit(node.Container);
		SyntaxToken dotToken = VisitToken(node.DotToken);
		MemberCrefSyntax member = (MemberCrefSyntax)Visit(node.Member);
		return node.Update(container, dotToken, member);
	}

	public override SyntaxNode VisitNameMemberCref(NameMemberCrefSyntax node)
	{
		TypeSyntax name = (TypeSyntax)Visit(node.Name);
		CrefParameterListSyntax parameters = (CrefParameterListSyntax)Visit(node.Parameters);
		return node.Update(name, parameters);
	}

	public override SyntaxNode VisitIndexerMemberCref(IndexerMemberCrefSyntax node)
	{
		SyntaxToken thisKeyword = VisitToken(node.ThisKeyword);
		CrefBracketedParameterListSyntax parameters = (CrefBracketedParameterListSyntax)Visit(node.Parameters);
		return node.Update(thisKeyword, parameters);
	}

	public override SyntaxNode VisitOperatorMemberCref(OperatorMemberCrefSyntax node)
	{
		SyntaxToken operatorKeyword = VisitToken(node.OperatorKeyword);
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		CrefParameterListSyntax parameters = (CrefParameterListSyntax)Visit(node.Parameters);
		return node.Update(operatorKeyword, operatorToken, parameters);
	}

	public override SyntaxNode VisitConversionOperatorMemberCref(ConversionOperatorMemberCrefSyntax node)
	{
		SyntaxToken implicitOrExplicitKeyword = VisitToken(node.ImplicitOrExplicitKeyword);
		SyntaxToken operatorKeyword = VisitToken(node.OperatorKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		CrefParameterListSyntax parameters = (CrefParameterListSyntax)Visit(node.Parameters);
		return node.Update(implicitOrExplicitKeyword, operatorKeyword, type, parameters);
	}

	public override SyntaxNode VisitCrefParameterList(CrefParameterListSyntax node)
	{
		SyntaxToken openParenToken = VisitToken(node.OpenParenToken);
		SeparatedSyntaxList<CrefParameterSyntax> parameters = VisitList(node.Parameters);
		SyntaxToken closeParenToken = VisitToken(node.CloseParenToken);
		return node.Update(openParenToken, parameters, closeParenToken);
	}

	public override SyntaxNode VisitCrefBracketedParameterList(CrefBracketedParameterListSyntax node)
	{
		SyntaxToken openBracketToken = VisitToken(node.OpenBracketToken);
		SeparatedSyntaxList<CrefParameterSyntax> parameters = VisitList(node.Parameters);
		SyntaxToken closeBracketToken = VisitToken(node.CloseBracketToken);
		return node.Update(openBracketToken, parameters, closeBracketToken);
	}

	public override SyntaxNode VisitCrefParameter(CrefParameterSyntax node)
	{
		SyntaxToken refKindKeyword = VisitToken(node.RefKindKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		return node.Update(refKindKeyword, type);
	}

	public override SyntaxNode VisitXmlElement(XmlElementSyntax node)
	{
		XmlElementStartTagSyntax startTag = (XmlElementStartTagSyntax)Visit(node.StartTag);
		SyntaxList<XmlNodeSyntax> content = VisitList(node.Content);
		XmlElementEndTagSyntax endTag = (XmlElementEndTagSyntax)Visit(node.EndTag);
		return node.Update(startTag, content, endTag);
	}

	public override SyntaxNode VisitXmlElementStartTag(XmlElementStartTagSyntax node)
	{
		SyntaxToken lessThanToken = VisitToken(node.LessThanToken);
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		SyntaxList<XmlAttributeSyntax> attributes = VisitList(node.Attributes);
		SyntaxToken greaterThanToken = VisitToken(node.GreaterThanToken);
		return node.Update(lessThanToken, name, attributes, greaterThanToken);
	}

	public override SyntaxNode VisitXmlElementEndTag(XmlElementEndTagSyntax node)
	{
		SyntaxToken lessThanSlashToken = VisitToken(node.LessThanSlashToken);
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		SyntaxToken greaterThanToken = VisitToken(node.GreaterThanToken);
		return node.Update(lessThanSlashToken, name, greaterThanToken);
	}

	public override SyntaxNode VisitXmlEmptyElement(XmlEmptyElementSyntax node)
	{
		SyntaxToken lessThanToken = VisitToken(node.LessThanToken);
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		SyntaxList<XmlAttributeSyntax> attributes = VisitList(node.Attributes);
		SyntaxToken slashGreaterThanToken = VisitToken(node.SlashGreaterThanToken);
		return node.Update(lessThanToken, name, attributes, slashGreaterThanToken);
	}

	public override SyntaxNode VisitXmlName(XmlNameSyntax node)
	{
		XmlPrefixSyntax prefix = (XmlPrefixSyntax)Visit(node.Prefix);
		SyntaxToken localName = VisitToken(node.LocalName);
		return node.Update(prefix, localName);
	}

	public override SyntaxNode VisitXmlPrefix(XmlPrefixSyntax node)
	{
		SyntaxToken prefix = VisitToken(node.Prefix);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		return node.Update(prefix, colonToken);
	}

	public override SyntaxNode VisitXmlTextAttribute(XmlTextAttributeSyntax node)
	{
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		SyntaxToken startQuoteToken = VisitToken(node.StartQuoteToken);
		SyntaxTokenList textTokens = VisitList(node.TextTokens);
		SyntaxToken endQuoteToken = VisitToken(node.EndQuoteToken);
		return node.Update(name, equalsToken, startQuoteToken, textTokens, endQuoteToken);
	}

	public override SyntaxNode VisitXmlCrefAttribute(XmlCrefAttributeSyntax node)
	{
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		SyntaxToken startQuoteToken = VisitToken(node.StartQuoteToken);
		CrefSyntax cref = (CrefSyntax)Visit(node.Cref);
		SyntaxToken endQuoteToken = VisitToken(node.EndQuoteToken);
		return node.Update(name, equalsToken, startQuoteToken, cref, endQuoteToken);
	}

	public override SyntaxNode VisitXmlNameAttribute(XmlNameAttributeSyntax node)
	{
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		SyntaxToken startQuoteToken = VisitToken(node.StartQuoteToken);
		IdentifierNameSyntax identifier = (IdentifierNameSyntax)Visit(node.Identifier);
		SyntaxToken endQuoteToken = VisitToken(node.EndQuoteToken);
		return node.Update(name, equalsToken, startQuoteToken, identifier, endQuoteToken);
	}

	public override SyntaxNode VisitXmlText(XmlTextSyntax node)
	{
		SyntaxTokenList textTokens = VisitList(node.TextTokens);
		return node.Update(textTokens);
	}

	public override SyntaxNode VisitXmlCDataSection(XmlCDataSectionSyntax node)
	{
		SyntaxToken startCDataToken = VisitToken(node.StartCDataToken);
		SyntaxTokenList textTokens = VisitList(node.TextTokens);
		SyntaxToken endCDataToken = VisitToken(node.EndCDataToken);
		return node.Update(startCDataToken, textTokens, endCDataToken);
	}

	public override SyntaxNode VisitXmlProcessingInstruction(XmlProcessingInstructionSyntax node)
	{
		SyntaxToken startProcessingInstructionToken = VisitToken(node.StartProcessingInstructionToken);
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		SyntaxTokenList textTokens = VisitList(node.TextTokens);
		SyntaxToken endProcessingInstructionToken = VisitToken(node.EndProcessingInstructionToken);
		return node.Update(startProcessingInstructionToken, name, textTokens, endProcessingInstructionToken);
	}

	public override SyntaxNode VisitXmlComment(XmlCommentSyntax node)
	{
		SyntaxToken lessThanExclamationMinusMinusToken = VisitToken(node.LessThanExclamationMinusMinusToken);
		SyntaxTokenList textTokens = VisitList(node.TextTokens);
		SyntaxToken minusMinusGreaterThanToken = VisitToken(node.MinusMinusGreaterThanToken);
		return node.Update(lessThanExclamationMinusMinusToken, textTokens, minusMinusGreaterThanToken);
	}

	public override SyntaxNode VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken ifKeyword = VisitToken(node.IfKeyword);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, ifKeyword, condition, endOfDirectiveToken, node.IsActive, node.BranchTaken, node.ConditionValue);
	}

	public override SyntaxNode VisitElifDirectiveTrivia(ElifDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken elifKeyword = VisitToken(node.ElifKeyword);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, elifKeyword, condition, endOfDirectiveToken, node.IsActive, node.BranchTaken, node.ConditionValue);
	}

	public override SyntaxNode VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken elseKeyword = VisitToken(node.ElseKeyword);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, elseKeyword, endOfDirectiveToken, node.IsActive, node.BranchTaken);
	}

	public override SyntaxNode VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken endIfKeyword = VisitToken(node.EndIfKeyword);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, endIfKeyword, endOfDirectiveToken, node.IsActive);
	}

	public override SyntaxNode VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken regionKeyword = VisitToken(node.RegionKeyword);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, regionKeyword, endOfDirectiveToken, node.IsActive);
	}

	public override SyntaxNode VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken endRegionKeyword = VisitToken(node.EndRegionKeyword);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, endRegionKeyword, endOfDirectiveToken, node.IsActive);
	}

	public override SyntaxNode VisitErrorDirectiveTrivia(ErrorDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken errorKeyword = VisitToken(node.ErrorKeyword);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, errorKeyword, endOfDirectiveToken, node.IsActive);
	}

	public override SyntaxNode VisitWarningDirectiveTrivia(WarningDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken warningKeyword = VisitToken(node.WarningKeyword);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, warningKeyword, endOfDirectiveToken, node.IsActive);
	}

	public override SyntaxNode VisitBadDirectiveTrivia(BadDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken identifier = VisitToken(node.Identifier);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, identifier, endOfDirectiveToken, node.IsActive);
	}

	public override SyntaxNode VisitDefineDirectiveTrivia(DefineDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken defineKeyword = VisitToken(node.DefineKeyword);
		SyntaxToken name = VisitToken(node.Name);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, defineKeyword, name, endOfDirectiveToken, node.IsActive);
	}

	public override SyntaxNode VisitUndefDirectiveTrivia(UndefDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken undefKeyword = VisitToken(node.UndefKeyword);
		SyntaxToken name = VisitToken(node.Name);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, undefKeyword, name, endOfDirectiveToken, node.IsActive);
	}

	public override SyntaxNode VisitLineDirectiveTrivia(LineDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken lineKeyword = VisitToken(node.LineKeyword);
		SyntaxToken line = VisitToken(node.Line);
		SyntaxToken file = VisitToken(node.File);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, lineKeyword, line, file, endOfDirectiveToken, node.IsActive);
	}

	public override SyntaxNode VisitPragmaWarningDirectiveTrivia(PragmaWarningDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken pragmaKeyword = VisitToken(node.PragmaKeyword);
		SyntaxToken warningKeyword = VisitToken(node.WarningKeyword);
		SyntaxToken disableOrRestoreKeyword = VisitToken(node.DisableOrRestoreKeyword);
		SeparatedSyntaxList<ExpressionSyntax> errorCodes = VisitList(node.ErrorCodes);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, pragmaKeyword, warningKeyword, disableOrRestoreKeyword, errorCodes, endOfDirectiveToken, node.IsActive);
	}

	public override SyntaxNode VisitPragmaChecksumDirectiveTrivia(PragmaChecksumDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken pragmaKeyword = VisitToken(node.PragmaKeyword);
		SyntaxToken checksumKeyword = VisitToken(node.ChecksumKeyword);
		SyntaxToken file = VisitToken(node.File);
		SyntaxToken guid = VisitToken(node.Guid);
		SyntaxToken bytes = VisitToken(node.Bytes);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, pragmaKeyword, checksumKeyword, file, guid, bytes, endOfDirectiveToken, node.IsActive);
	}

	public override SyntaxNode VisitReferenceDirectiveTrivia(ReferenceDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken referenceKeyword = VisitToken(node.ReferenceKeyword);
		SyntaxToken file = VisitToken(node.File);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, referenceKeyword, file, endOfDirectiveToken, node.IsActive);
	}

	public override SyntaxNode VisitLoadDirectiveTrivia(LoadDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken loadKeyword = VisitToken(node.LoadKeyword);
		SyntaxToken file = VisitToken(node.File);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, loadKeyword, file, endOfDirectiveToken, node.IsActive);
	}

	public override SyntaxNode VisitShebangDirectiveTrivia(ShebangDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = VisitToken(node.HashToken);
		SyntaxToken exclamationToken = VisitToken(node.ExclamationToken);
		SyntaxToken endOfDirectiveToken = VisitToken(node.EndOfDirectiveToken);
		return node.Update(hashToken, exclamationToken, endOfDirectiveToken, node.IsActive);
	}

	public CSharpSyntaxRewriter(bool visitIntoStructuredTrivia = false)
	{
		_visitIntoStructuredTrivia = visitIntoStructuredTrivia;
	}

	public override SyntaxNode Visit(SyntaxNode node)
	{
		if (node != null)
		{
			_recursionDepth++;
			StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
			SyntaxNode result = ((CSharpSyntaxNode)node).Accept(this);
			_recursionDepth--;
			return result;
		}
		return null;
	}

	public virtual SyntaxToken VisitToken(SyntaxToken token)
	{
		GreenNode node = token.Node;
		if (node == null)
		{
			return token;
		}
		GreenNode leadingTriviaCore = node.GetLeadingTriviaCore();
		GreenNode trailingTriviaCore = node.GetTrailingTriviaCore();
		if (leadingTriviaCore != null)
		{
			SyntaxTriviaList trivia = VisitList(new SyntaxTriviaList(in token, leadingTriviaCore));
			if (trailingTriviaCore != null)
			{
				int index = ((!leadingTriviaCore.IsList) ? 1 : leadingTriviaCore.SlotCount);
				SyntaxTriviaList trivia2 = VisitList(new SyntaxTriviaList(in token, trailingTriviaCore, token.Position + node.FullWidth - trailingTriviaCore.FullWidth, index));
				if (trivia.Node != leadingTriviaCore)
				{
					token = token.WithLeadingTrivia(trivia);
				}
				if (trivia2.Node == trailingTriviaCore)
				{
					return token;
				}
				return token.WithTrailingTrivia(trivia2);
			}
			if (trivia.Node == leadingTriviaCore)
			{
				return token;
			}
			return token.WithLeadingTrivia(trivia);
		}
		if (trailingTriviaCore != null)
		{
			SyntaxTriviaList trivia3 = VisitList(new SyntaxTriviaList(in token, trailingTriviaCore, token.Position + node.FullWidth - trailingTriviaCore.FullWidth));
			if (trivia3.Node == trailingTriviaCore)
			{
				return token;
			}
			return token.WithTrailingTrivia(trivia3);
		}
		return token;
	}

	public virtual SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
	{
		if (VisitIntoStructuredTrivia && trivia.HasStructure)
		{
			CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)trivia.GetStructure();
			StructuredTriviaSyntax structuredTriviaSyntax = (StructuredTriviaSyntax)Visit(cSharpSyntaxNode);
			if (structuredTriviaSyntax != cSharpSyntaxNode)
			{
				if (structuredTriviaSyntax != null)
				{
					return SyntaxFactory.Trivia(structuredTriviaSyntax);
				}
				return default;
			}
		}
		return trivia;
	}

	public virtual SyntaxList<TNode> VisitList<TNode>(SyntaxList<TNode> list) where TNode : SyntaxNode
	{
		SyntaxListBuilder syntaxListBuilder = null;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			TNode val = list[i];
			TNode val2 = VisitListElement(val);
			if (val != val2 && syntaxListBuilder == null)
			{
				syntaxListBuilder = new SyntaxListBuilder(count);
				syntaxListBuilder.AddRange(list, 0, i);
			}
			if (syntaxListBuilder != null && val2 != null && !val2.IsKind(SyntaxKind.None))
			{
				syntaxListBuilder.Add(val2);
			}
		}
		if (syntaxListBuilder != null)
		{
			return syntaxListBuilder.ToList();
		}
		return list;
	}

	public virtual TNode VisitListElement<TNode>(TNode node) where TNode : SyntaxNode
	{
		return (TNode)Visit(node);
	}

	public virtual SeparatedSyntaxList<TNode> VisitList<TNode>(SeparatedSyntaxList<TNode> list) where TNode : SyntaxNode
	{
		int count = list.Count;
		int separatorCount = list.SeparatorCount;
		SeparatedSyntaxListBuilder<TNode> separatedSyntaxListBuilder = default;
		int i;
		for (i = 0; i < separatorCount; i++)
		{
			TNode val = list[i];
			TNode val2 = VisitListElement(val);
			SyntaxToken separator = list.GetSeparator(i);
			SyntaxToken separatorToken = VisitListSeparator(separator);
			if (separatedSyntaxListBuilder.IsNull && (val != val2 || separator != separatorToken))
			{
				separatedSyntaxListBuilder = new SeparatedSyntaxListBuilder<TNode>(count);
				separatedSyntaxListBuilder.AddRange(in list, i);
			}
			if (separatedSyntaxListBuilder.IsNull)
			{
				continue;
			}
			if (val2 != null)
			{
				separatedSyntaxListBuilder.Add(val2);
				if (separatorToken.RawKind == 0)
				{
					throw new InvalidOperationException(CodeAnalysisResources.SeparatorIsExpected);
				}
				separatedSyntaxListBuilder.AddSeparator(in separatorToken);
			}
			else if (val2 == null)
			{
				throw new InvalidOperationException(CodeAnalysisResources.ElementIsExpected);
			}
		}
		if (i < count)
		{
			TNode val3 = list[i];
			TNode val4 = VisitListElement(val3);
			if (separatedSyntaxListBuilder.IsNull && val3 != val4)
			{
				separatedSyntaxListBuilder = new SeparatedSyntaxListBuilder<TNode>(count);
				separatedSyntaxListBuilder.AddRange(in list, i);
			}
			if (!separatedSyntaxListBuilder.IsNull && val4 != null)
			{
				separatedSyntaxListBuilder.Add(val4);
			}
		}
		if (!separatedSyntaxListBuilder.IsNull)
		{
			return separatedSyntaxListBuilder.ToList();
		}
		return list;
	}

	public virtual SyntaxToken VisitListSeparator(SyntaxToken separator)
	{
		return VisitToken(separator);
	}

	public virtual SyntaxTokenList VisitList(SyntaxTokenList list)
	{
		SyntaxTokenListBuilder syntaxTokenListBuilder = null;
		int count = list.Count;
		int num = -1;
		foreach (SyntaxToken item in list)
		{
			num++;
			SyntaxToken syntaxToken = VisitToken(item);
			if (item != syntaxToken && syntaxTokenListBuilder == null)
			{
				syntaxTokenListBuilder = new SyntaxTokenListBuilder(count);
				syntaxTokenListBuilder.Add(list, 0, num);
			}
			if (syntaxTokenListBuilder != null && syntaxToken.Kind() != SyntaxKind.None)
			{
				syntaxTokenListBuilder.Add(syntaxToken);
			}
		}
		return syntaxTokenListBuilder?.ToList() ?? list;
	}

	public virtual SyntaxTriviaList VisitList(SyntaxTriviaList list)
	{
		int count = list.Count;
		if (count != 0)
		{
			SyntaxTriviaListBuilder syntaxTriviaListBuilder = null;
			int num = -1;
			foreach (SyntaxTrivia item in list)
			{
				num++;
				SyntaxTrivia syntaxTrivia = VisitListElement(item);
				if (syntaxTrivia != item && syntaxTriviaListBuilder == null)
				{
					syntaxTriviaListBuilder = new SyntaxTriviaListBuilder(count);
					syntaxTriviaListBuilder.Add(in list, 0, num);
				}
				if (syntaxTriviaListBuilder != null && syntaxTrivia.Kind() != SyntaxKind.None)
				{
					syntaxTriviaListBuilder.Add(syntaxTrivia);
				}
			}
			if (syntaxTriviaListBuilder != null)
			{
				return syntaxTriviaListBuilder.ToList();
			}
		}
		return list;
	}

	public virtual SyntaxTrivia VisitListElement(SyntaxTrivia element)
	{
		return VisitTrivia(element);
	}
}
