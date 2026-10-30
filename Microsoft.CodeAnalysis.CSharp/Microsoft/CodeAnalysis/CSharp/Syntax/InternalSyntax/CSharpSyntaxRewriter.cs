using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class CSharpSyntaxRewriter : CSharpSyntaxVisitor<CSharpSyntaxNode>
{
	protected readonly bool VisitIntoStructuredTrivia;

	public override CSharpSyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
	{
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override CSharpSyntaxNode VisitQualifiedName(QualifiedNameSyntax node)
	{
		NameSyntax left = (NameSyntax)Visit(node.Left);
		SyntaxToken dotToken = (SyntaxToken)Visit(node.DotToken);
		SimpleNameSyntax right = (SimpleNameSyntax)Visit(node.Right);
		return node.Update(left, dotToken, right);
	}

	public override CSharpSyntaxNode VisitGenericName(GenericNameSyntax node)
	{
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		TypeArgumentListSyntax typeArgumentList = (TypeArgumentListSyntax)Visit(node.TypeArgumentList);
		return node.Update(identifier, typeArgumentList);
	}

	public override CSharpSyntaxNode VisitTypeArgumentList(TypeArgumentListSyntax node)
	{
		SyntaxToken lessThanToken = (SyntaxToken)Visit(node.LessThanToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeSyntax> arguments = VisitList(node.Arguments);
		SyntaxToken greaterThanToken = (SyntaxToken)Visit(node.GreaterThanToken);
		return node.Update(lessThanToken, arguments, greaterThanToken);
	}

	public override CSharpSyntaxNode VisitAliasQualifiedName(AliasQualifiedNameSyntax node)
	{
		IdentifierNameSyntax alias = (IdentifierNameSyntax)Visit(node.Alias);
		SyntaxToken colonColonToken = (SyntaxToken)Visit(node.ColonColonToken);
		SimpleNameSyntax name = (SimpleNameSyntax)Visit(node.Name);
		return node.Update(alias, colonColonToken, name);
	}

	public override CSharpSyntaxNode VisitPredefinedType(PredefinedTypeSyntax node)
	{
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		return node.Update(keyword);
	}

	public override CSharpSyntaxNode VisitArrayType(ArrayTypeSyntax node)
	{
		TypeSyntax elementType = (TypeSyntax)Visit(node.ElementType);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ArrayRankSpecifierSyntax> rankSpecifiers = VisitList(node.RankSpecifiers);
		return node.Update(elementType, rankSpecifiers);
	}

	public override CSharpSyntaxNode VisitArrayRankSpecifier(ArrayRankSpecifierSyntax node)
	{
		SyntaxToken openBracketToken = (SyntaxToken)Visit(node.OpenBracketToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> sizes = VisitList(node.Sizes);
		SyntaxToken closeBracketToken = (SyntaxToken)Visit(node.CloseBracketToken);
		return node.Update(openBracketToken, sizes, closeBracketToken);
	}

	public override CSharpSyntaxNode VisitPointerType(PointerTypeSyntax node)
	{
		TypeSyntax elementType = (TypeSyntax)Visit(node.ElementType);
		SyntaxToken asteriskToken = (SyntaxToken)Visit(node.AsteriskToken);
		return node.Update(elementType, asteriskToken);
	}

	public override CSharpSyntaxNode VisitNullableType(NullableTypeSyntax node)
	{
		TypeSyntax elementType = (TypeSyntax)Visit(node.ElementType);
		SyntaxToken questionToken = (SyntaxToken)Visit(node.QuestionToken);
		return node.Update(elementType, questionToken);
	}

	public override CSharpSyntaxNode VisitTupleType(TupleTypeSyntax node)
	{
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TupleElementSyntax> elements = VisitList(node.Elements);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(openParenToken, elements, closeParenToken);
	}

	public override CSharpSyntaxNode VisitTupleElement(TupleElementSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		return node.Update(type, identifier);
	}

	public override CSharpSyntaxNode VisitOmittedTypeArgument(OmittedTypeArgumentSyntax node)
	{
		SyntaxToken omittedTypeArgumentToken = (SyntaxToken)Visit(node.OmittedTypeArgumentToken);
		return node.Update(omittedTypeArgumentToken);
	}

	public override CSharpSyntaxNode VisitRefType(RefTypeSyntax node)
	{
		SyntaxToken refKeyword = (SyntaxToken)Visit(node.RefKeyword);
		SyntaxToken readOnlyKeyword = (SyntaxToken)Visit(node.ReadOnlyKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		return node.Update(refKeyword, readOnlyKeyword, type);
	}

	public override CSharpSyntaxNode VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
	{
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(openParenToken, expression, closeParenToken);
	}

	public override CSharpSyntaxNode VisitTupleExpression(TupleExpressionSyntax node)
	{
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> arguments = VisitList(node.Arguments);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(openParenToken, arguments, closeParenToken);
	}

	public override CSharpSyntaxNode VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
	{
		SyntaxToken operatorToken = (SyntaxToken)Visit(node.OperatorToken);
		ExpressionSyntax operand = (ExpressionSyntax)Visit(node.Operand);
		return node.Update(operatorToken, operand);
	}

	public override CSharpSyntaxNode VisitAwaitExpression(AwaitExpressionSyntax node)
	{
		SyntaxToken awaitKeyword = (SyntaxToken)Visit(node.AwaitKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(awaitKeyword, expression);
	}

	public override CSharpSyntaxNode VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
	{
		ExpressionSyntax operand = (ExpressionSyntax)Visit(node.Operand);
		SyntaxToken operatorToken = (SyntaxToken)Visit(node.OperatorToken);
		return node.Update(operand, operatorToken);
	}

	public override CSharpSyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken operatorToken = (SyntaxToken)Visit(node.OperatorToken);
		SimpleNameSyntax name = (SimpleNameSyntax)Visit(node.Name);
		return node.Update(expression, operatorToken, name);
	}

	public override CSharpSyntaxNode VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken operatorToken = (SyntaxToken)Visit(node.OperatorToken);
		ExpressionSyntax whenNotNull = (ExpressionSyntax)Visit(node.WhenNotNull);
		return node.Update(expression, operatorToken, whenNotNull);
	}

	public override CSharpSyntaxNode VisitMemberBindingExpression(MemberBindingExpressionSyntax node)
	{
		SyntaxToken operatorToken = (SyntaxToken)Visit(node.OperatorToken);
		SimpleNameSyntax name = (SimpleNameSyntax)Visit(node.Name);
		return node.Update(operatorToken, name);
	}

	public override CSharpSyntaxNode VisitElementBindingExpression(ElementBindingExpressionSyntax node)
	{
		BracketedArgumentListSyntax argumentList = (BracketedArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(argumentList);
	}

	public override CSharpSyntaxNode VisitImplicitElementAccess(ImplicitElementAccessSyntax node)
	{
		BracketedArgumentListSyntax argumentList = (BracketedArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(argumentList);
	}

	public override CSharpSyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
	{
		ExpressionSyntax left = (ExpressionSyntax)Visit(node.Left);
		SyntaxToken operatorToken = (SyntaxToken)Visit(node.OperatorToken);
		ExpressionSyntax right = (ExpressionSyntax)Visit(node.Right);
		return node.Update(left, operatorToken, right);
	}

	public override CSharpSyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
	{
		ExpressionSyntax left = (ExpressionSyntax)Visit(node.Left);
		SyntaxToken operatorToken = (SyntaxToken)Visit(node.OperatorToken);
		ExpressionSyntax right = (ExpressionSyntax)Visit(node.Right);
		return node.Update(left, operatorToken, right);
	}

	public override CSharpSyntaxNode VisitConditionalExpression(ConditionalExpressionSyntax node)
	{
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken questionToken = (SyntaxToken)Visit(node.QuestionToken);
		ExpressionSyntax whenTrue = (ExpressionSyntax)Visit(node.WhenTrue);
		SyntaxToken colonToken = (SyntaxToken)Visit(node.ColonToken);
		ExpressionSyntax whenFalse = (ExpressionSyntax)Visit(node.WhenFalse);
		return node.Update(condition, questionToken, whenTrue, colonToken, whenFalse);
	}

	public override CSharpSyntaxNode VisitThisExpression(ThisExpressionSyntax node)
	{
		SyntaxToken token = (SyntaxToken)Visit(node.Token);
		return node.Update(token);
	}

	public override CSharpSyntaxNode VisitBaseExpression(BaseExpressionSyntax node)
	{
		SyntaxToken token = (SyntaxToken)Visit(node.Token);
		return node.Update(token);
	}

	public override CSharpSyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
	{
		SyntaxToken token = (SyntaxToken)Visit(node.Token);
		return node.Update(token);
	}

	public override CSharpSyntaxNode VisitMakeRefExpression(MakeRefExpressionSyntax node)
	{
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(keyword, openParenToken, expression, closeParenToken);
	}

	public override CSharpSyntaxNode VisitRefTypeExpression(RefTypeExpressionSyntax node)
	{
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(keyword, openParenToken, expression, closeParenToken);
	}

	public override CSharpSyntaxNode VisitRefValueExpression(RefValueExpressionSyntax node)
	{
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken comma = (SyntaxToken)Visit(node.Comma);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(keyword, openParenToken, expression, comma, type, closeParenToken);
	}

	public override CSharpSyntaxNode VisitCheckedExpression(CheckedExpressionSyntax node)
	{
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(keyword, openParenToken, expression, closeParenToken);
	}

	public override CSharpSyntaxNode VisitDefaultExpression(DefaultExpressionSyntax node)
	{
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(keyword, openParenToken, type, closeParenToken);
	}

	public override CSharpSyntaxNode VisitTypeOfExpression(TypeOfExpressionSyntax node)
	{
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(keyword, openParenToken, type, closeParenToken);
	}

	public override CSharpSyntaxNode VisitSizeOfExpression(SizeOfExpressionSyntax node)
	{
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(keyword, openParenToken, type, closeParenToken);
	}

	public override CSharpSyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		ArgumentListSyntax argumentList = (ArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(expression, argumentList);
	}

	public override CSharpSyntaxNode VisitElementAccessExpression(ElementAccessExpressionSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		BracketedArgumentListSyntax argumentList = (BracketedArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(expression, argumentList);
	}

	public override CSharpSyntaxNode VisitArgumentList(ArgumentListSyntax node)
	{
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> arguments = VisitList(node.Arguments);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(openParenToken, arguments, closeParenToken);
	}

	public override CSharpSyntaxNode VisitBracketedArgumentList(BracketedArgumentListSyntax node)
	{
		SyntaxToken openBracketToken = (SyntaxToken)Visit(node.OpenBracketToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> arguments = VisitList(node.Arguments);
		SyntaxToken closeBracketToken = (SyntaxToken)Visit(node.CloseBracketToken);
		return node.Update(openBracketToken, arguments, closeBracketToken);
	}

	public override CSharpSyntaxNode VisitArgument(ArgumentSyntax node)
	{
		NameColonSyntax nameColon = (NameColonSyntax)Visit(node.NameColon);
		SyntaxToken refKindKeyword = (SyntaxToken)Visit(node.RefKindKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(nameColon, refKindKeyword, expression);
	}

	public override CSharpSyntaxNode VisitNameColon(NameColonSyntax node)
	{
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken colonToken = (SyntaxToken)Visit(node.ColonToken);
		return node.Update(name, colonToken);
	}

	public override CSharpSyntaxNode VisitDeclarationExpression(DeclarationExpressionSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		VariableDesignationSyntax designation = (VariableDesignationSyntax)Visit(node.Designation);
		return node.Update(type, designation);
	}

	public override CSharpSyntaxNode VisitCastExpression(CastExpressionSyntax node)
	{
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(openParenToken, type, closeParenToken, expression);
	}

	public override CSharpSyntaxNode VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
	{
		SyntaxToken asyncKeyword = (SyntaxToken)Visit(node.AsyncKeyword);
		SyntaxToken delegateKeyword = (SyntaxToken)Visit(node.DelegateKeyword);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		CSharpSyntaxNode body = Visit(node.Body);
		return node.Update(asyncKeyword, delegateKeyword, parameterList, body);
	}

	public override CSharpSyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
	{
		SyntaxToken asyncKeyword = (SyntaxToken)Visit(node.AsyncKeyword);
		ParameterSyntax parameter = (ParameterSyntax)Visit(node.Parameter);
		SyntaxToken arrowToken = (SyntaxToken)Visit(node.ArrowToken);
		CSharpSyntaxNode body = Visit(node.Body);
		return node.Update(asyncKeyword, parameter, arrowToken, body);
	}

	public override CSharpSyntaxNode VisitRefExpression(RefExpressionSyntax node)
	{
		SyntaxToken refKeyword = (SyntaxToken)Visit(node.RefKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(refKeyword, expression);
	}

	public override CSharpSyntaxNode VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
	{
		SyntaxToken asyncKeyword = (SyntaxToken)Visit(node.AsyncKeyword);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		SyntaxToken arrowToken = (SyntaxToken)Visit(node.ArrowToken);
		CSharpSyntaxNode body = Visit(node.Body);
		return node.Update(asyncKeyword, parameterList, arrowToken, body);
	}

	public override CSharpSyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
	{
		SyntaxToken openBraceToken = (SyntaxToken)Visit(node.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> expressions = VisitList(node.Expressions);
		SyntaxToken closeBraceToken = (SyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(openBraceToken, expressions, closeBraceToken);
	}

	public override CSharpSyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
	{
		SyntaxToken newKeyword = (SyntaxToken)Visit(node.NewKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		ArgumentListSyntax argumentList = (ArgumentListSyntax)Visit(node.ArgumentList);
		InitializerExpressionSyntax initializer = (InitializerExpressionSyntax)Visit(node.Initializer);
		return node.Update(newKeyword, type, argumentList, initializer);
	}

	public override CSharpSyntaxNode VisitAnonymousObjectMemberDeclarator(AnonymousObjectMemberDeclaratorSyntax node)
	{
		NameEqualsSyntax nameEquals = (NameEqualsSyntax)Visit(node.NameEquals);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(nameEquals, expression);
	}

	public override CSharpSyntaxNode VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
	{
		SyntaxToken newKeyword = (SyntaxToken)Visit(node.NewKeyword);
		SyntaxToken openBraceToken = (SyntaxToken)Visit(node.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax> initializers = VisitList(node.Initializers);
		SyntaxToken closeBraceToken = (SyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(newKeyword, openBraceToken, initializers, closeBraceToken);
	}

	public override CSharpSyntaxNode VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
	{
		SyntaxToken newKeyword = (SyntaxToken)Visit(node.NewKeyword);
		ArrayTypeSyntax type = (ArrayTypeSyntax)Visit(node.Type);
		InitializerExpressionSyntax initializer = (InitializerExpressionSyntax)Visit(node.Initializer);
		return node.Update(newKeyword, type, initializer);
	}

	public override CSharpSyntaxNode VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
	{
		SyntaxToken newKeyword = (SyntaxToken)Visit(node.NewKeyword);
		SyntaxToken openBracketToken = (SyntaxToken)Visit(node.OpenBracketToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> commas = VisitList(node.Commas);
		SyntaxToken closeBracketToken = (SyntaxToken)Visit(node.CloseBracketToken);
		InitializerExpressionSyntax initializer = (InitializerExpressionSyntax)Visit(node.Initializer);
		return node.Update(newKeyword, openBracketToken, commas, closeBracketToken, initializer);
	}

	public override CSharpSyntaxNode VisitStackAllocArrayCreationExpression(StackAllocArrayCreationExpressionSyntax node)
	{
		SyntaxToken stackAllocKeyword = (SyntaxToken)Visit(node.StackAllocKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		InitializerExpressionSyntax initializer = (InitializerExpressionSyntax)Visit(node.Initializer);
		return node.Update(stackAllocKeyword, type, initializer);
	}

	public override CSharpSyntaxNode VisitImplicitStackAllocArrayCreationExpression(ImplicitStackAllocArrayCreationExpressionSyntax node)
	{
		SyntaxToken stackAllocKeyword = (SyntaxToken)Visit(node.StackAllocKeyword);
		SyntaxToken openBracketToken = (SyntaxToken)Visit(node.OpenBracketToken);
		SyntaxToken closeBracketToken = (SyntaxToken)Visit(node.CloseBracketToken);
		InitializerExpressionSyntax initializer = (InitializerExpressionSyntax)Visit(node.Initializer);
		return node.Update(stackAllocKeyword, openBracketToken, closeBracketToken, initializer);
	}

	public override CSharpSyntaxNode VisitQueryExpression(QueryExpressionSyntax node)
	{
		FromClauseSyntax fromClause = (FromClauseSyntax)Visit(node.FromClause);
		QueryBodySyntax body = (QueryBodySyntax)Visit(node.Body);
		return node.Update(fromClause, body);
	}

	public override CSharpSyntaxNode VisitQueryBody(QueryBodySyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<QueryClauseSyntax> clauses = VisitList(node.Clauses);
		SelectOrGroupClauseSyntax selectOrGroup = (SelectOrGroupClauseSyntax)Visit(node.SelectOrGroup);
		QueryContinuationSyntax continuation = (QueryContinuationSyntax)Visit(node.Continuation);
		return node.Update(clauses, selectOrGroup, continuation);
	}

	public override CSharpSyntaxNode VisitFromClause(FromClauseSyntax node)
	{
		SyntaxToken fromKeyword = (SyntaxToken)Visit(node.FromKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		SyntaxToken inKeyword = (SyntaxToken)Visit(node.InKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(fromKeyword, type, identifier, inKeyword, expression);
	}

	public override CSharpSyntaxNode VisitLetClause(LetClauseSyntax node)
	{
		SyntaxToken letKeyword = (SyntaxToken)Visit(node.LetKeyword);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		SyntaxToken equalsToken = (SyntaxToken)Visit(node.EqualsToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(letKeyword, identifier, equalsToken, expression);
	}

	public override CSharpSyntaxNode VisitJoinClause(JoinClauseSyntax node)
	{
		SyntaxToken joinKeyword = (SyntaxToken)Visit(node.JoinKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		SyntaxToken inKeyword = (SyntaxToken)Visit(node.InKeyword);
		ExpressionSyntax inExpression = (ExpressionSyntax)Visit(node.InExpression);
		SyntaxToken onKeyword = (SyntaxToken)Visit(node.OnKeyword);
		ExpressionSyntax leftExpression = (ExpressionSyntax)Visit(node.LeftExpression);
		SyntaxToken equalsKeyword = (SyntaxToken)Visit(node.EqualsKeyword);
		ExpressionSyntax rightExpression = (ExpressionSyntax)Visit(node.RightExpression);
		JoinIntoClauseSyntax joinIntoClauseSyntax = (JoinIntoClauseSyntax)Visit(node.Into);
		return node.Update(joinKeyword, type, identifier, inKeyword, inExpression, onKeyword, leftExpression, equalsKeyword, rightExpression, joinIntoClauseSyntax);
	}

	public override CSharpSyntaxNode VisitJoinIntoClause(JoinIntoClauseSyntax node)
	{
		SyntaxToken intoKeyword = (SyntaxToken)Visit(node.IntoKeyword);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		return node.Update(intoKeyword, identifier);
	}

	public override CSharpSyntaxNode VisitWhereClause(WhereClauseSyntax node)
	{
		SyntaxToken whereKeyword = (SyntaxToken)Visit(node.WhereKeyword);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		return node.Update(whereKeyword, condition);
	}

	public override CSharpSyntaxNode VisitOrderByClause(OrderByClauseSyntax node)
	{
		SyntaxToken orderByKeyword = (SyntaxToken)Visit(node.OrderByKeyword);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<OrderingSyntax> orderings = VisitList(node.Orderings);
		return node.Update(orderByKeyword, orderings);
	}

	public override CSharpSyntaxNode VisitOrdering(OrderingSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken ascendingOrDescendingKeyword = (SyntaxToken)Visit(node.AscendingOrDescendingKeyword);
		return node.Update(expression, ascendingOrDescendingKeyword);
	}

	public override CSharpSyntaxNode VisitSelectClause(SelectClauseSyntax node)
	{
		SyntaxToken selectKeyword = (SyntaxToken)Visit(node.SelectKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(selectKeyword, expression);
	}

	public override CSharpSyntaxNode VisitGroupClause(GroupClauseSyntax node)
	{
		SyntaxToken groupKeyword = (SyntaxToken)Visit(node.GroupKeyword);
		ExpressionSyntax groupExpression = (ExpressionSyntax)Visit(node.GroupExpression);
		SyntaxToken byKeyword = (SyntaxToken)Visit(node.ByKeyword);
		ExpressionSyntax byExpression = (ExpressionSyntax)Visit(node.ByExpression);
		return node.Update(groupKeyword, groupExpression, byKeyword, byExpression);
	}

	public override CSharpSyntaxNode VisitQueryContinuation(QueryContinuationSyntax node)
	{
		SyntaxToken intoKeyword = (SyntaxToken)Visit(node.IntoKeyword);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		QueryBodySyntax body = (QueryBodySyntax)Visit(node.Body);
		return node.Update(intoKeyword, identifier, body);
	}

	public override CSharpSyntaxNode VisitOmittedArraySizeExpression(OmittedArraySizeExpressionSyntax node)
	{
		SyntaxToken omittedArraySizeExpressionToken = (SyntaxToken)Visit(node.OmittedArraySizeExpressionToken);
		return node.Update(omittedArraySizeExpressionToken);
	}

	public override CSharpSyntaxNode VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
	{
		SyntaxToken stringStartToken = (SyntaxToken)Visit(node.StringStartToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<InterpolatedStringContentSyntax> contents = VisitList(node.Contents);
		SyntaxToken stringEndToken = (SyntaxToken)Visit(node.StringEndToken);
		return node.Update(stringStartToken, contents, stringEndToken);
	}

	public override CSharpSyntaxNode VisitIsPatternExpression(IsPatternExpressionSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken isKeyword = (SyntaxToken)Visit(node.IsKeyword);
		PatternSyntax pattern = (PatternSyntax)Visit(node.Pattern);
		return node.Update(expression, isKeyword, pattern);
	}

	public override CSharpSyntaxNode VisitThrowExpression(ThrowExpressionSyntax node)
	{
		SyntaxToken throwKeyword = (SyntaxToken)Visit(node.ThrowKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(throwKeyword, expression);
	}

	public override CSharpSyntaxNode VisitWhenClause(WhenClauseSyntax node)
	{
		SyntaxToken whenKeyword = (SyntaxToken)Visit(node.WhenKeyword);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		return node.Update(whenKeyword, condition);
	}

	public override CSharpSyntaxNode VisitDeclarationPattern(DeclarationPatternSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		VariableDesignationSyntax designation = (VariableDesignationSyntax)Visit(node.Designation);
		return node.Update(type, designation);
	}

	public override CSharpSyntaxNode VisitConstantPattern(ConstantPatternSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(expression);
	}

	public override CSharpSyntaxNode VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
	{
		SyntaxToken textToken = (SyntaxToken)Visit(node.TextToken);
		return node.Update(textToken);
	}

	public override CSharpSyntaxNode VisitInterpolation(InterpolationSyntax node)
	{
		SyntaxToken openBraceToken = (SyntaxToken)Visit(node.OpenBraceToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		InterpolationAlignmentClauseSyntax alignmentClause = (InterpolationAlignmentClauseSyntax)Visit(node.AlignmentClause);
		InterpolationFormatClauseSyntax formatClause = (InterpolationFormatClauseSyntax)Visit(node.FormatClause);
		SyntaxToken closeBraceToken = (SyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(openBraceToken, expression, alignmentClause, formatClause, closeBraceToken);
	}

	public override CSharpSyntaxNode VisitInterpolationAlignmentClause(InterpolationAlignmentClauseSyntax node)
	{
		SyntaxToken commaToken = (SyntaxToken)Visit(node.CommaToken);
		ExpressionSyntax value = (ExpressionSyntax)Visit(node.Value);
		return node.Update(commaToken, value);
	}

	public override CSharpSyntaxNode VisitInterpolationFormatClause(InterpolationFormatClauseSyntax node)
	{
		SyntaxToken colonToken = (SyntaxToken)Visit(node.ColonToken);
		SyntaxToken formatStringToken = (SyntaxToken)Visit(node.FormatStringToken);
		return node.Update(colonToken, formatStringToken);
	}

	public override CSharpSyntaxNode VisitGlobalStatement(GlobalStatementSyntax node)
	{
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(statement);
	}

	public override CSharpSyntaxNode VisitBlock(BlockSyntax node)
	{
		SyntaxToken openBraceToken = (SyntaxToken)Visit(node.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax> statements = VisitList(node.Statements);
		SyntaxToken closeBraceToken = (SyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(openBraceToken, statements, closeBraceToken);
	}

	public override CSharpSyntaxNode VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		TypeSyntax returnType = (TypeSyntax)Visit(node.ReturnType);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		TypeParameterListSyntax typeParameterList = (TypeParameterListSyntax)Visit(node.TypeParameterList);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = VisitList(node.ConstraintClauses);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(modifiers, returnType, identifier, typeParameterList, parameterList, constraintClauses, body, expressionBody, semicolonToken);
	}

	public override CSharpSyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)Visit(node.Declaration);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(modifiers, declaration, semicolonToken);
	}

	public override CSharpSyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDeclaratorSyntax> variables = VisitList(node.Variables);
		return node.Update(type, variables);
	}

	public override CSharpSyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
	{
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		BracketedArgumentListSyntax argumentList = (BracketedArgumentListSyntax)Visit(node.ArgumentList);
		EqualsValueClauseSyntax initializer = (EqualsValueClauseSyntax)Visit(node.Initializer);
		return node.Update(identifier, argumentList, initializer);
	}

	public override CSharpSyntaxNode VisitEqualsValueClause(EqualsValueClauseSyntax node)
	{
		SyntaxToken equalsToken = (SyntaxToken)Visit(node.EqualsToken);
		ExpressionSyntax value = (ExpressionSyntax)Visit(node.Value);
		return node.Update(equalsToken, value);
	}

	public override CSharpSyntaxNode VisitSingleVariableDesignation(SingleVariableDesignationSyntax node)
	{
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override CSharpSyntaxNode VisitDiscardDesignation(DiscardDesignationSyntax node)
	{
		SyntaxToken underscoreToken = (SyntaxToken)Visit(node.UnderscoreToken);
		return node.Update(underscoreToken);
	}

	public override CSharpSyntaxNode VisitParenthesizedVariableDesignation(ParenthesizedVariableDesignationSyntax node)
	{
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDesignationSyntax> variables = VisitList(node.Variables);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(openParenToken, variables, closeParenToken);
	}

	public override CSharpSyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
	{
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(expression, semicolonToken);
	}

	public override CSharpSyntaxNode VisitEmptyStatement(EmptyStatementSyntax node)
	{
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(semicolonToken);
	}

	public override CSharpSyntaxNode VisitLabeledStatement(LabeledStatementSyntax node)
	{
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		SyntaxToken colonToken = (SyntaxToken)Visit(node.ColonToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(identifier, colonToken, statement);
	}

	public override CSharpSyntaxNode VisitGotoStatement(GotoStatementSyntax node)
	{
		SyntaxToken gotoKeyword = (SyntaxToken)Visit(node.GotoKeyword);
		SyntaxToken caseOrDefaultKeyword = (SyntaxToken)Visit(node.CaseOrDefaultKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(gotoKeyword, caseOrDefaultKeyword, expression, semicolonToken);
	}

	public override CSharpSyntaxNode VisitBreakStatement(BreakStatementSyntax node)
	{
		SyntaxToken breakKeyword = (SyntaxToken)Visit(node.BreakKeyword);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(breakKeyword, semicolonToken);
	}

	public override CSharpSyntaxNode VisitContinueStatement(ContinueStatementSyntax node)
	{
		SyntaxToken continueKeyword = (SyntaxToken)Visit(node.ContinueKeyword);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(continueKeyword, semicolonToken);
	}

	public override CSharpSyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
	{
		SyntaxToken returnKeyword = (SyntaxToken)Visit(node.ReturnKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(returnKeyword, expression, semicolonToken);
	}

	public override CSharpSyntaxNode VisitThrowStatement(ThrowStatementSyntax node)
	{
		SyntaxToken throwKeyword = (SyntaxToken)Visit(node.ThrowKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(throwKeyword, expression, semicolonToken);
	}

	public override CSharpSyntaxNode VisitYieldStatement(YieldStatementSyntax node)
	{
		SyntaxToken yieldKeyword = (SyntaxToken)Visit(node.YieldKeyword);
		SyntaxToken returnOrBreakKeyword = (SyntaxToken)Visit(node.ReturnOrBreakKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(yieldKeyword, returnOrBreakKeyword, expression, semicolonToken);
	}

	public override CSharpSyntaxNode VisitWhileStatement(WhileStatementSyntax node)
	{
		SyntaxToken whileKeyword = (SyntaxToken)Visit(node.WhileKeyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(whileKeyword, openParenToken, condition, closeParenToken, statement);
	}

	public override CSharpSyntaxNode VisitDoStatement(DoStatementSyntax node)
	{
		SyntaxToken doKeyword = (SyntaxToken)Visit(node.DoKeyword);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		SyntaxToken whileKeyword = (SyntaxToken)Visit(node.WhileKeyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(doKeyword, statement, whileKeyword, openParenToken, condition, closeParenToken, semicolonToken);
	}

	public override CSharpSyntaxNode VisitForStatement(ForStatementSyntax node)
	{
		SyntaxToken forKeyword = (SyntaxToken)Visit(node.ForKeyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)Visit(node.Declaration);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> initializers = VisitList(node.Initializers);
		SyntaxToken firstSemicolonToken = (SyntaxToken)Visit(node.FirstSemicolonToken);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken secondSemicolonToken = (SyntaxToken)Visit(node.SecondSemicolonToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> incrementors = VisitList(node.Incrementors);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(forKeyword, openParenToken, declaration, initializers, firstSemicolonToken, condition, secondSemicolonToken, incrementors, closeParenToken, statement);
	}

	public override CSharpSyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
	{
		SyntaxToken forEachKeyword = (SyntaxToken)Visit(node.ForEachKeyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		SyntaxToken inKeyword = (SyntaxToken)Visit(node.InKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(forEachKeyword, openParenToken, type, identifier, inKeyword, expression, closeParenToken, statement);
	}

	public override CSharpSyntaxNode VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
	{
		SyntaxToken forEachKeyword = (SyntaxToken)Visit(node.ForEachKeyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		ExpressionSyntax variable = (ExpressionSyntax)Visit(node.Variable);
		SyntaxToken inKeyword = (SyntaxToken)Visit(node.InKeyword);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(forEachKeyword, openParenToken, variable, inKeyword, expression, closeParenToken, statement);
	}

	public override CSharpSyntaxNode VisitUsingStatement(UsingStatementSyntax node)
	{
		SyntaxToken usingKeyword = (SyntaxToken)Visit(node.UsingKeyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)Visit(node.Declaration);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(usingKeyword, openParenToken, declaration, expression, closeParenToken, statement);
	}

	public override CSharpSyntaxNode VisitFixedStatement(FixedStatementSyntax node)
	{
		SyntaxToken fixedKeyword = (SyntaxToken)Visit(node.FixedKeyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)Visit(node.Declaration);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(fixedKeyword, openParenToken, declaration, closeParenToken, statement);
	}

	public override CSharpSyntaxNode VisitCheckedStatement(CheckedStatementSyntax node)
	{
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		BlockSyntax block = (BlockSyntax)Visit(node.Block);
		return node.Update(keyword, block);
	}

	public override CSharpSyntaxNode VisitUnsafeStatement(UnsafeStatementSyntax node)
	{
		SyntaxToken unsafeKeyword = (SyntaxToken)Visit(node.UnsafeKeyword);
		BlockSyntax block = (BlockSyntax)Visit(node.Block);
		return node.Update(unsafeKeyword, block);
	}

	public override CSharpSyntaxNode VisitLockStatement(LockStatementSyntax node)
	{
		SyntaxToken lockKeyword = (SyntaxToken)Visit(node.LockKeyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(lockKeyword, openParenToken, expression, closeParenToken, statement);
	}

	public override CSharpSyntaxNode VisitIfStatement(IfStatementSyntax node)
	{
		SyntaxToken ifKeyword = (SyntaxToken)Visit(node.IfKeyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		ElseClauseSyntax elseClauseSyntax = (ElseClauseSyntax)Visit(node.Else);
		return node.Update(ifKeyword, openParenToken, condition, closeParenToken, statement, elseClauseSyntax);
	}

	public override CSharpSyntaxNode VisitElseClause(ElseClauseSyntax node)
	{
		SyntaxToken elseKeyword = (SyntaxToken)Visit(node.ElseKeyword);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(elseKeyword, statement);
	}

	public override CSharpSyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
	{
		SyntaxToken switchKeyword = (SyntaxToken)Visit(node.SwitchKeyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		SyntaxToken openBraceToken = (SyntaxToken)Visit(node.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SwitchSectionSyntax> sections = VisitList(node.Sections);
		SyntaxToken closeBraceToken = (SyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(switchKeyword, openParenToken, expression, closeParenToken, openBraceToken, sections, closeBraceToken);
	}

	public override CSharpSyntaxNode VisitSwitchSection(SwitchSectionSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SwitchLabelSyntax> labels = VisitList(node.Labels);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax> statements = VisitList(node.Statements);
		return node.Update(labels, statements);
	}

	public override CSharpSyntaxNode VisitCasePatternSwitchLabel(CasePatternSwitchLabelSyntax node)
	{
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		PatternSyntax pattern = (PatternSyntax)Visit(node.Pattern);
		WhenClauseSyntax whenClause = (WhenClauseSyntax)Visit(node.WhenClause);
		SyntaxToken colonToken = (SyntaxToken)Visit(node.ColonToken);
		return node.Update(keyword, pattern, whenClause, colonToken);
	}

	public override CSharpSyntaxNode VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
	{
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		ExpressionSyntax value = (ExpressionSyntax)Visit(node.Value);
		SyntaxToken colonToken = (SyntaxToken)Visit(node.ColonToken);
		return node.Update(keyword, value, colonToken);
	}

	public override CSharpSyntaxNode VisitDefaultSwitchLabel(DefaultSwitchLabelSyntax node)
	{
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		SyntaxToken colonToken = (SyntaxToken)Visit(node.ColonToken);
		return node.Update(keyword, colonToken);
	}

	public override CSharpSyntaxNode VisitTryStatement(TryStatementSyntax node)
	{
		SyntaxToken tryKeyword = (SyntaxToken)Visit(node.TryKeyword);
		BlockSyntax block = (BlockSyntax)Visit(node.Block);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CatchClauseSyntax> catches = VisitList(node.Catches);
		FinallyClauseSyntax finallyClauseSyntax = (FinallyClauseSyntax)Visit(node.Finally);
		return node.Update(tryKeyword, block, catches, finallyClauseSyntax);
	}

	public override CSharpSyntaxNode VisitCatchClause(CatchClauseSyntax node)
	{
		SyntaxToken catchKeyword = (SyntaxToken)Visit(node.CatchKeyword);
		CatchDeclarationSyntax declaration = (CatchDeclarationSyntax)Visit(node.Declaration);
		CatchFilterClauseSyntax filter = (CatchFilterClauseSyntax)Visit(node.Filter);
		BlockSyntax block = (BlockSyntax)Visit(node.Block);
		return node.Update(catchKeyword, declaration, filter, block);
	}

	public override CSharpSyntaxNode VisitCatchDeclaration(CatchDeclarationSyntax node)
	{
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(openParenToken, type, identifier, closeParenToken);
	}

	public override CSharpSyntaxNode VisitCatchFilterClause(CatchFilterClauseSyntax node)
	{
		SyntaxToken whenKeyword = (SyntaxToken)Visit(node.WhenKeyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		ExpressionSyntax filterExpression = (ExpressionSyntax)Visit(node.FilterExpression);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(whenKeyword, openParenToken, filterExpression, closeParenToken);
	}

	public override CSharpSyntaxNode VisitFinallyClause(FinallyClauseSyntax node)
	{
		SyntaxToken finallyKeyword = (SyntaxToken)Visit(node.FinallyKeyword);
		BlockSyntax block = (BlockSyntax)Visit(node.Block);
		return node.Update(finallyKeyword, block);
	}

	public override CSharpSyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax> externs = VisitList(node.Externs);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax> usings = VisitList(node.Usings);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members = VisitList(node.Members);
		SyntaxToken endOfFileToken = (SyntaxToken)Visit(node.EndOfFileToken);
		return node.Update(externs, usings, attributeLists, members, endOfFileToken);
	}

	public override CSharpSyntaxNode VisitExternAliasDirective(ExternAliasDirectiveSyntax node)
	{
		SyntaxToken externKeyword = (SyntaxToken)Visit(node.ExternKeyword);
		SyntaxToken aliasKeyword = (SyntaxToken)Visit(node.AliasKeyword);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(externKeyword, aliasKeyword, identifier, semicolonToken);
	}

	public override CSharpSyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
	{
		SyntaxToken usingKeyword = (SyntaxToken)Visit(node.UsingKeyword);
		SyntaxToken staticKeyword = (SyntaxToken)Visit(node.StaticKeyword);
		NameEqualsSyntax alias = (NameEqualsSyntax)Visit(node.Alias);
		NameSyntax name = (NameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(usingKeyword, staticKeyword, alias, name, semicolonToken);
	}

	public override CSharpSyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
	{
		SyntaxToken namespaceKeyword = (SyntaxToken)Visit(node.NamespaceKeyword);
		NameSyntax name = (NameSyntax)Visit(node.Name);
		SyntaxToken openBraceToken = (SyntaxToken)Visit(node.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax> externs = VisitList(node.Externs);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax> usings = VisitList(node.Usings);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = (SyntaxToken)Visit(node.CloseBraceToken);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(namespaceKeyword, name, openBraceToken, externs, usings, members, closeBraceToken, semicolonToken);
	}

	public override CSharpSyntaxNode VisitAttributeList(AttributeListSyntax node)
	{
		SyntaxToken openBracketToken = (SyntaxToken)Visit(node.OpenBracketToken);
		AttributeTargetSpecifierSyntax target = (AttributeTargetSpecifierSyntax)Visit(node.Target);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AttributeSyntax> attributes = VisitList(node.Attributes);
		SyntaxToken closeBracketToken = (SyntaxToken)Visit(node.CloseBracketToken);
		return node.Update(openBracketToken, target, attributes, closeBracketToken);
	}

	public override CSharpSyntaxNode VisitAttributeTargetSpecifier(AttributeTargetSpecifierSyntax node)
	{
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		SyntaxToken colonToken = (SyntaxToken)Visit(node.ColonToken);
		return node.Update(identifier, colonToken);
	}

	public override CSharpSyntaxNode VisitAttribute(AttributeSyntax node)
	{
		NameSyntax name = (NameSyntax)Visit(node.Name);
		AttributeArgumentListSyntax argumentList = (AttributeArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(name, argumentList);
	}

	public override CSharpSyntaxNode VisitAttributeArgumentList(AttributeArgumentListSyntax node)
	{
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AttributeArgumentSyntax> arguments = VisitList(node.Arguments);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(openParenToken, arguments, closeParenToken);
	}

	public override CSharpSyntaxNode VisitAttributeArgument(AttributeArgumentSyntax node)
	{
		NameEqualsSyntax nameEquals = (NameEqualsSyntax)Visit(node.NameEquals);
		NameColonSyntax nameColon = (NameColonSyntax)Visit(node.NameColon);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(nameEquals, nameColon, expression);
	}

	public override CSharpSyntaxNode VisitNameEquals(NameEqualsSyntax node)
	{
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken equalsToken = (SyntaxToken)Visit(node.EqualsToken);
		return node.Update(name, equalsToken);
	}

	public override CSharpSyntaxNode VisitTypeParameterList(TypeParameterListSyntax node)
	{
		SyntaxToken lessThanToken = (SyntaxToken)Visit(node.LessThanToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeParameterSyntax> parameters = VisitList(node.Parameters);
		SyntaxToken greaterThanToken = (SyntaxToken)Visit(node.GreaterThanToken);
		return node.Update(lessThanToken, parameters, greaterThanToken);
	}

	public override CSharpSyntaxNode VisitTypeParameter(TypeParameterSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxToken varianceKeyword = (SyntaxToken)Visit(node.VarianceKeyword);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		return node.Update(attributeLists, varianceKeyword, identifier);
	}

	public override CSharpSyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		TypeParameterListSyntax typeParameterList = (TypeParameterListSyntax)Visit(node.TypeParameterList);
		BaseListSyntax baseList = (BaseListSyntax)Visit(node.BaseList);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = VisitList(node.ConstraintClauses);
		SyntaxToken openBraceToken = (SyntaxToken)Visit(node.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = (SyntaxToken)Visit(node.CloseBraceToken);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, keyword, identifier, typeParameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
	}

	public override CSharpSyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		TypeParameterListSyntax typeParameterList = (TypeParameterListSyntax)Visit(node.TypeParameterList);
		BaseListSyntax baseList = (BaseListSyntax)Visit(node.BaseList);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = VisitList(node.ConstraintClauses);
		SyntaxToken openBraceToken = (SyntaxToken)Visit(node.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = (SyntaxToken)Visit(node.CloseBraceToken);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, keyword, identifier, typeParameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
	}

	public override CSharpSyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		TypeParameterListSyntax typeParameterList = (TypeParameterListSyntax)Visit(node.TypeParameterList);
		BaseListSyntax baseList = (BaseListSyntax)Visit(node.BaseList);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = VisitList(node.ConstraintClauses);
		SyntaxToken openBraceToken = (SyntaxToken)Visit(node.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = (SyntaxToken)Visit(node.CloseBraceToken);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, keyword, identifier, typeParameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
	}

	public override CSharpSyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		SyntaxToken enumKeyword = (SyntaxToken)Visit(node.EnumKeyword);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		BaseListSyntax baseList = (BaseListSyntax)Visit(node.BaseList);
		SyntaxToken openBraceToken = (SyntaxToken)Visit(node.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = (SyntaxToken)Visit(node.CloseBraceToken);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, enumKeyword, identifier, baseList, openBraceToken, members, closeBraceToken, semicolonToken);
	}

	public override CSharpSyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		SyntaxToken delegateKeyword = (SyntaxToken)Visit(node.DelegateKeyword);
		TypeSyntax returnType = (TypeSyntax)Visit(node.ReturnType);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		TypeParameterListSyntax typeParameterList = (TypeParameterListSyntax)Visit(node.TypeParameterList);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = VisitList(node.ConstraintClauses);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, delegateKeyword, returnType, identifier, typeParameterList, parameterList, constraintClauses, semicolonToken);
	}

	public override CSharpSyntaxNode VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		EqualsValueClauseSyntax equalsValue = (EqualsValueClauseSyntax)Visit(node.EqualsValue);
		return node.Update(attributeLists, identifier, equalsValue);
	}

	public override CSharpSyntaxNode VisitBaseList(BaseListSyntax node)
	{
		SyntaxToken colonToken = (SyntaxToken)Visit(node.ColonToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<BaseTypeSyntax> types = VisitList(node.Types);
		return node.Update(colonToken, types);
	}

	public override CSharpSyntaxNode VisitSimpleBaseType(SimpleBaseTypeSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		return node.Update(type);
	}

	public override CSharpSyntaxNode VisitTypeParameterConstraintClause(TypeParameterConstraintClauseSyntax node)
	{
		SyntaxToken whereKeyword = (SyntaxToken)Visit(node.WhereKeyword);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken colonToken = (SyntaxToken)Visit(node.ColonToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints = VisitList(node.Constraints);
		return node.Update(whereKeyword, name, colonToken, constraints);
	}

	public override CSharpSyntaxNode VisitConstructorConstraint(ConstructorConstraintSyntax node)
	{
		SyntaxToken newKeyword = (SyntaxToken)Visit(node.NewKeyword);
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(newKeyword, openParenToken, closeParenToken);
	}

	public override CSharpSyntaxNode VisitClassOrStructConstraint(ClassOrStructConstraintSyntax node)
	{
		SyntaxToken classOrStructKeyword = (SyntaxToken)Visit(node.ClassOrStructKeyword);
		return node.Update(classOrStructKeyword);
	}

	public override CSharpSyntaxNode VisitTypeConstraint(TypeConstraintSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		return node.Update(type);
	}

	public override CSharpSyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)Visit(node.Declaration);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, declaration, semicolonToken);
	}

	public override CSharpSyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		SyntaxToken eventKeyword = (SyntaxToken)Visit(node.EventKeyword);
		VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)Visit(node.Declaration);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, eventKeyword, declaration, semicolonToken);
	}

	public override CSharpSyntaxNode VisitExplicitInterfaceSpecifier(ExplicitInterfaceSpecifierSyntax node)
	{
		NameSyntax name = (NameSyntax)Visit(node.Name);
		SyntaxToken dotToken = (SyntaxToken)Visit(node.DotToken);
		return node.Update(name, dotToken);
	}

	public override CSharpSyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		TypeSyntax returnType = (TypeSyntax)Visit(node.ReturnType);
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = (ExplicitInterfaceSpecifierSyntax)Visit(node.ExplicitInterfaceSpecifier);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		TypeParameterListSyntax typeParameterList = (TypeParameterListSyntax)Visit(node.TypeParameterList);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = VisitList(node.ConstraintClauses);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, returnType, explicitInterfaceSpecifier, identifier, typeParameterList, parameterList, constraintClauses, body, expressionBody, semicolonToken);
	}

	public override CSharpSyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		TypeSyntax returnType = (TypeSyntax)Visit(node.ReturnType);
		SyntaxToken operatorKeyword = (SyntaxToken)Visit(node.OperatorKeyword);
		SyntaxToken operatorToken = (SyntaxToken)Visit(node.OperatorToken);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, returnType, operatorKeyword, operatorToken, parameterList, body, expressionBody, semicolonToken);
	}

	public override CSharpSyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		SyntaxToken implicitOrExplicitKeyword = (SyntaxToken)Visit(node.ImplicitOrExplicitKeyword);
		SyntaxToken operatorKeyword = (SyntaxToken)Visit(node.OperatorKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, implicitOrExplicitKeyword, operatorKeyword, type, parameterList, body, expressionBody, semicolonToken);
	}

	public override CSharpSyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		ConstructorInitializerSyntax initializer = (ConstructorInitializerSyntax)Visit(node.Initializer);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, identifier, parameterList, initializer, body, expressionBody, semicolonToken);
	}

	public override CSharpSyntaxNode VisitConstructorInitializer(ConstructorInitializerSyntax node)
	{
		SyntaxToken colonToken = (SyntaxToken)Visit(node.ColonToken);
		SyntaxToken thisOrBaseKeyword = (SyntaxToken)Visit(node.ThisOrBaseKeyword);
		ArgumentListSyntax argumentList = (ArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(colonToken, thisOrBaseKeyword, argumentList);
	}

	public override CSharpSyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		SyntaxToken tildeToken = (SyntaxToken)Visit(node.TildeToken);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, tildeToken, identifier, parameterList, body, expressionBody, semicolonToken);
	}

	public override CSharpSyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = (ExplicitInterfaceSpecifierSyntax)Visit(node.ExplicitInterfaceSpecifier);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		AccessorListSyntax accessorList = (AccessorListSyntax)Visit(node.AccessorList);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		EqualsValueClauseSyntax initializer = (EqualsValueClauseSyntax)Visit(node.Initializer);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, type, explicitInterfaceSpecifier, identifier, accessorList, expressionBody, initializer, semicolonToken);
	}

	public override CSharpSyntaxNode VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
	{
		SyntaxToken arrowToken = (SyntaxToken)Visit(node.ArrowToken);
		ExpressionSyntax expression = (ExpressionSyntax)Visit(node.Expression);
		return node.Update(arrowToken, expression);
	}

	public override CSharpSyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		SyntaxToken eventKeyword = (SyntaxToken)Visit(node.EventKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = (ExplicitInterfaceSpecifierSyntax)Visit(node.ExplicitInterfaceSpecifier);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		AccessorListSyntax accessorList = (AccessorListSyntax)Visit(node.AccessorList);
		return node.Update(attributeLists, modifiers, eventKeyword, type, explicitInterfaceSpecifier, identifier, accessorList);
	}

	public override CSharpSyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = (ExplicitInterfaceSpecifierSyntax)Visit(node.ExplicitInterfaceSpecifier);
		SyntaxToken thisKeyword = (SyntaxToken)Visit(node.ThisKeyword);
		BracketedParameterListSyntax parameterList = (BracketedParameterListSyntax)Visit(node.ParameterList);
		AccessorListSyntax accessorList = (AccessorListSyntax)Visit(node.AccessorList);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, type, explicitInterfaceSpecifier, thisKeyword, parameterList, accessorList, expressionBody, semicolonToken);
	}

	public override CSharpSyntaxNode VisitAccessorList(AccessorListSyntax node)
	{
		SyntaxToken openBraceToken = (SyntaxToken)Visit(node.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AccessorDeclarationSyntax> accessors = VisitList(node.Accessors);
		SyntaxToken closeBraceToken = (SyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(openBraceToken, accessors, closeBraceToken);
	}

	public override CSharpSyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		SyntaxToken keyword = (SyntaxToken)Visit(node.Keyword);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		ArrowExpressionClauseSyntax expressionBody = (ArrowExpressionClauseSyntax)Visit(node.ExpressionBody);
		SyntaxToken semicolonToken = (SyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributeLists, modifiers, keyword, body, expressionBody, semicolonToken);
	}

	public override CSharpSyntaxNode VisitParameterList(ParameterListSyntax node)
	{
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> parameters = VisitList(node.Parameters);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(openParenToken, parameters, closeParenToken);
	}

	public override CSharpSyntaxNode VisitBracketedParameterList(BracketedParameterListSyntax node)
	{
		SyntaxToken openBracketToken = (SyntaxToken)Visit(node.OpenBracketToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> parameters = VisitList(node.Parameters);
		SyntaxToken closeBracketToken = (SyntaxToken)Visit(node.CloseBracketToken);
		return node.Update(openBracketToken, parameters, closeBracketToken);
	}

	public override CSharpSyntaxNode VisitParameter(ParameterSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		EqualsValueClauseSyntax equalsValueClauseSyntax = (EqualsValueClauseSyntax)Visit(node.Default);
		return node.Update(attributeLists, modifiers, type, identifier, equalsValueClauseSyntax);
	}

	public override CSharpSyntaxNode VisitIncompleteMember(IncompleteMemberSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = VisitList(node.AttributeLists);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = VisitList(node.Modifiers);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		return node.Update(attributeLists, modifiers, type);
	}

	public override CSharpSyntaxNode VisitSkippedTokensTrivia(SkippedTokensTriviaSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> tokens = VisitList(node.Tokens);
		return node.Update(tokens);
	}

	public override CSharpSyntaxNode VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlNodeSyntax> content = VisitList(node.Content);
		SyntaxToken endOfComment = (SyntaxToken)Visit(node.EndOfComment);
		return node.Update(content, endOfComment);
	}

	public override CSharpSyntaxNode VisitTypeCref(TypeCrefSyntax node)
	{
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		return node.Update(type);
	}

	public override CSharpSyntaxNode VisitQualifiedCref(QualifiedCrefSyntax node)
	{
		TypeSyntax container = (TypeSyntax)Visit(node.Container);
		SyntaxToken dotToken = (SyntaxToken)Visit(node.DotToken);
		MemberCrefSyntax member = (MemberCrefSyntax)Visit(node.Member);
		return node.Update(container, dotToken, member);
	}

	public override CSharpSyntaxNode VisitNameMemberCref(NameMemberCrefSyntax node)
	{
		TypeSyntax name = (TypeSyntax)Visit(node.Name);
		CrefParameterListSyntax parameters = (CrefParameterListSyntax)Visit(node.Parameters);
		return node.Update(name, parameters);
	}

	public override CSharpSyntaxNode VisitIndexerMemberCref(IndexerMemberCrefSyntax node)
	{
		SyntaxToken thisKeyword = (SyntaxToken)Visit(node.ThisKeyword);
		CrefBracketedParameterListSyntax parameters = (CrefBracketedParameterListSyntax)Visit(node.Parameters);
		return node.Update(thisKeyword, parameters);
	}

	public override CSharpSyntaxNode VisitOperatorMemberCref(OperatorMemberCrefSyntax node)
	{
		SyntaxToken operatorKeyword = (SyntaxToken)Visit(node.OperatorKeyword);
		SyntaxToken operatorToken = (SyntaxToken)Visit(node.OperatorToken);
		CrefParameterListSyntax parameters = (CrefParameterListSyntax)Visit(node.Parameters);
		return node.Update(operatorKeyword, operatorToken, parameters);
	}

	public override CSharpSyntaxNode VisitConversionOperatorMemberCref(ConversionOperatorMemberCrefSyntax node)
	{
		SyntaxToken implicitOrExplicitKeyword = (SyntaxToken)Visit(node.ImplicitOrExplicitKeyword);
		SyntaxToken operatorKeyword = (SyntaxToken)Visit(node.OperatorKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		CrefParameterListSyntax parameters = (CrefParameterListSyntax)Visit(node.Parameters);
		return node.Update(implicitOrExplicitKeyword, operatorKeyword, type, parameters);
	}

	public override CSharpSyntaxNode VisitCrefParameterList(CrefParameterListSyntax node)
	{
		SyntaxToken openParenToken = (SyntaxToken)Visit(node.OpenParenToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CrefParameterSyntax> parameters = VisitList(node.Parameters);
		SyntaxToken closeParenToken = (SyntaxToken)Visit(node.CloseParenToken);
		return node.Update(openParenToken, parameters, closeParenToken);
	}

	public override CSharpSyntaxNode VisitCrefBracketedParameterList(CrefBracketedParameterListSyntax node)
	{
		SyntaxToken openBracketToken = (SyntaxToken)Visit(node.OpenBracketToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CrefParameterSyntax> parameters = VisitList(node.Parameters);
		SyntaxToken closeBracketToken = (SyntaxToken)Visit(node.CloseBracketToken);
		return node.Update(openBracketToken, parameters, closeBracketToken);
	}

	public override CSharpSyntaxNode VisitCrefParameter(CrefParameterSyntax node)
	{
		SyntaxToken refKindKeyword = (SyntaxToken)Visit(node.RefKindKeyword);
		TypeSyntax type = (TypeSyntax)Visit(node.Type);
		return node.Update(refKindKeyword, type);
	}

	public override CSharpSyntaxNode VisitXmlElement(XmlElementSyntax node)
	{
		XmlElementStartTagSyntax startTag = (XmlElementStartTagSyntax)Visit(node.StartTag);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlNodeSyntax> content = VisitList(node.Content);
		XmlElementEndTagSyntax endTag = (XmlElementEndTagSyntax)Visit(node.EndTag);
		return node.Update(startTag, content, endTag);
	}

	public override CSharpSyntaxNode VisitXmlElementStartTag(XmlElementStartTagSyntax node)
	{
		SyntaxToken lessThanToken = (SyntaxToken)Visit(node.LessThanToken);
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlAttributeSyntax> attributes = VisitList(node.Attributes);
		SyntaxToken greaterThanToken = (SyntaxToken)Visit(node.GreaterThanToken);
		return node.Update(lessThanToken, name, attributes, greaterThanToken);
	}

	public override CSharpSyntaxNode VisitXmlElementEndTag(XmlElementEndTagSyntax node)
	{
		SyntaxToken lessThanSlashToken = (SyntaxToken)Visit(node.LessThanSlashToken);
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		SyntaxToken greaterThanToken = (SyntaxToken)Visit(node.GreaterThanToken);
		return node.Update(lessThanSlashToken, name, greaterThanToken);
	}

	public override CSharpSyntaxNode VisitXmlEmptyElement(XmlEmptyElementSyntax node)
	{
		SyntaxToken lessThanToken = (SyntaxToken)Visit(node.LessThanToken);
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlAttributeSyntax> attributes = VisitList(node.Attributes);
		SyntaxToken slashGreaterThanToken = (SyntaxToken)Visit(node.SlashGreaterThanToken);
		return node.Update(lessThanToken, name, attributes, slashGreaterThanToken);
	}

	public override CSharpSyntaxNode VisitXmlName(XmlNameSyntax node)
	{
		XmlPrefixSyntax prefix = (XmlPrefixSyntax)Visit(node.Prefix);
		SyntaxToken localName = (SyntaxToken)Visit(node.LocalName);
		return node.Update(prefix, localName);
	}

	public override CSharpSyntaxNode VisitXmlPrefix(XmlPrefixSyntax node)
	{
		SyntaxToken prefix = (SyntaxToken)Visit(node.Prefix);
		SyntaxToken colonToken = (SyntaxToken)Visit(node.ColonToken);
		return node.Update(prefix, colonToken);
	}

	public override CSharpSyntaxNode VisitXmlTextAttribute(XmlTextAttributeSyntax node)
	{
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		SyntaxToken equalsToken = (SyntaxToken)Visit(node.EqualsToken);
		SyntaxToken startQuoteToken = (SyntaxToken)Visit(node.StartQuoteToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens = VisitList(node.TextTokens);
		SyntaxToken endQuoteToken = (SyntaxToken)Visit(node.EndQuoteToken);
		return node.Update(name, equalsToken, startQuoteToken, textTokens, endQuoteToken);
	}

	public override CSharpSyntaxNode VisitXmlCrefAttribute(XmlCrefAttributeSyntax node)
	{
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		SyntaxToken equalsToken = (SyntaxToken)Visit(node.EqualsToken);
		SyntaxToken startQuoteToken = (SyntaxToken)Visit(node.StartQuoteToken);
		CrefSyntax cref = (CrefSyntax)Visit(node.Cref);
		SyntaxToken endQuoteToken = (SyntaxToken)Visit(node.EndQuoteToken);
		return node.Update(name, equalsToken, startQuoteToken, cref, endQuoteToken);
	}

	public override CSharpSyntaxNode VisitXmlNameAttribute(XmlNameAttributeSyntax node)
	{
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		SyntaxToken equalsToken = (SyntaxToken)Visit(node.EqualsToken);
		SyntaxToken startQuoteToken = (SyntaxToken)Visit(node.StartQuoteToken);
		IdentifierNameSyntax identifier = (IdentifierNameSyntax)Visit(node.Identifier);
		SyntaxToken endQuoteToken = (SyntaxToken)Visit(node.EndQuoteToken);
		return node.Update(name, equalsToken, startQuoteToken, identifier, endQuoteToken);
	}

	public override CSharpSyntaxNode VisitXmlText(XmlTextSyntax node)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens = VisitList(node.TextTokens);
		return node.Update(textTokens);
	}

	public override CSharpSyntaxNode VisitXmlCDataSection(XmlCDataSectionSyntax node)
	{
		SyntaxToken startCDataToken = (SyntaxToken)Visit(node.StartCDataToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens = VisitList(node.TextTokens);
		SyntaxToken endCDataToken = (SyntaxToken)Visit(node.EndCDataToken);
		return node.Update(startCDataToken, textTokens, endCDataToken);
	}

	public override CSharpSyntaxNode VisitXmlProcessingInstruction(XmlProcessingInstructionSyntax node)
	{
		SyntaxToken startProcessingInstructionToken = (SyntaxToken)Visit(node.StartProcessingInstructionToken);
		XmlNameSyntax name = (XmlNameSyntax)Visit(node.Name);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens = VisitList(node.TextTokens);
		SyntaxToken endProcessingInstructionToken = (SyntaxToken)Visit(node.EndProcessingInstructionToken);
		return node.Update(startProcessingInstructionToken, name, textTokens, endProcessingInstructionToken);
	}

	public override CSharpSyntaxNode VisitXmlComment(XmlCommentSyntax node)
	{
		SyntaxToken lessThanExclamationMinusMinusToken = (SyntaxToken)Visit(node.LessThanExclamationMinusMinusToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens = VisitList(node.TextTokens);
		SyntaxToken minusMinusGreaterThanToken = (SyntaxToken)Visit(node.MinusMinusGreaterThanToken);
		return node.Update(lessThanExclamationMinusMinusToken, textTokens, minusMinusGreaterThanToken);
	}

	public override CSharpSyntaxNode VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken ifKeyword = (SyntaxToken)Visit(node.IfKeyword);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, ifKeyword, condition, endOfDirectiveToken, node.IsActive, node.BranchTaken, node.ConditionValue);
	}

	public override CSharpSyntaxNode VisitElifDirectiveTrivia(ElifDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken elifKeyword = (SyntaxToken)Visit(node.ElifKeyword);
		ExpressionSyntax condition = (ExpressionSyntax)Visit(node.Condition);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, elifKeyword, condition, endOfDirectiveToken, node.IsActive, node.BranchTaken, node.ConditionValue);
	}

	public override CSharpSyntaxNode VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken elseKeyword = (SyntaxToken)Visit(node.ElseKeyword);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, elseKeyword, endOfDirectiveToken, node.IsActive, node.BranchTaken);
	}

	public override CSharpSyntaxNode VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken endIfKeyword = (SyntaxToken)Visit(node.EndIfKeyword);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, endIfKeyword, endOfDirectiveToken, node.IsActive);
	}

	public override CSharpSyntaxNode VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken regionKeyword = (SyntaxToken)Visit(node.RegionKeyword);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, regionKeyword, endOfDirectiveToken, node.IsActive);
	}

	public override CSharpSyntaxNode VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken endRegionKeyword = (SyntaxToken)Visit(node.EndRegionKeyword);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, endRegionKeyword, endOfDirectiveToken, node.IsActive);
	}

	public override CSharpSyntaxNode VisitErrorDirectiveTrivia(ErrorDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken errorKeyword = (SyntaxToken)Visit(node.ErrorKeyword);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, errorKeyword, endOfDirectiveToken, node.IsActive);
	}

	public override CSharpSyntaxNode VisitWarningDirectiveTrivia(WarningDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken warningKeyword = (SyntaxToken)Visit(node.WarningKeyword);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, warningKeyword, endOfDirectiveToken, node.IsActive);
	}

	public override CSharpSyntaxNode VisitBadDirectiveTrivia(BadDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken identifier = (SyntaxToken)Visit(node.Identifier);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, identifier, endOfDirectiveToken, node.IsActive);
	}

	public override CSharpSyntaxNode VisitDefineDirectiveTrivia(DefineDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken defineKeyword = (SyntaxToken)Visit(node.DefineKeyword);
		SyntaxToken name = (SyntaxToken)Visit(node.Name);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, defineKeyword, name, endOfDirectiveToken, node.IsActive);
	}

	public override CSharpSyntaxNode VisitUndefDirectiveTrivia(UndefDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken undefKeyword = (SyntaxToken)Visit(node.UndefKeyword);
		SyntaxToken name = (SyntaxToken)Visit(node.Name);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, undefKeyword, name, endOfDirectiveToken, node.IsActive);
	}

	public override CSharpSyntaxNode VisitLineDirectiveTrivia(LineDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken lineKeyword = (SyntaxToken)Visit(node.LineKeyword);
		SyntaxToken line = (SyntaxToken)Visit(node.Line);
		SyntaxToken file = (SyntaxToken)Visit(node.File);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, lineKeyword, line, file, endOfDirectiveToken, node.IsActive);
	}

	public override CSharpSyntaxNode VisitPragmaWarningDirectiveTrivia(PragmaWarningDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken pragmaKeyword = (SyntaxToken)Visit(node.PragmaKeyword);
		SyntaxToken warningKeyword = (SyntaxToken)Visit(node.WarningKeyword);
		SyntaxToken disableOrRestoreKeyword = (SyntaxToken)Visit(node.DisableOrRestoreKeyword);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> errorCodes = VisitList(node.ErrorCodes);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, pragmaKeyword, warningKeyword, disableOrRestoreKeyword, errorCodes, endOfDirectiveToken, node.IsActive);
	}

	public override CSharpSyntaxNode VisitPragmaChecksumDirectiveTrivia(PragmaChecksumDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken pragmaKeyword = (SyntaxToken)Visit(node.PragmaKeyword);
		SyntaxToken checksumKeyword = (SyntaxToken)Visit(node.ChecksumKeyword);
		SyntaxToken file = (SyntaxToken)Visit(node.File);
		SyntaxToken guid = (SyntaxToken)Visit(node.Guid);
		SyntaxToken bytes = (SyntaxToken)Visit(node.Bytes);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, pragmaKeyword, checksumKeyword, file, guid, bytes, endOfDirectiveToken, node.IsActive);
	}

	public override CSharpSyntaxNode VisitReferenceDirectiveTrivia(ReferenceDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken referenceKeyword = (SyntaxToken)Visit(node.ReferenceKeyword);
		SyntaxToken file = (SyntaxToken)Visit(node.File);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, referenceKeyword, file, endOfDirectiveToken, node.IsActive);
	}

	public override CSharpSyntaxNode VisitLoadDirectiveTrivia(LoadDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken loadKeyword = (SyntaxToken)Visit(node.LoadKeyword);
		SyntaxToken file = (SyntaxToken)Visit(node.File);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, loadKeyword, file, endOfDirectiveToken, node.IsActive);
	}

	public override CSharpSyntaxNode VisitShebangDirectiveTrivia(ShebangDirectiveTriviaSyntax node)
	{
		SyntaxToken hashToken = (SyntaxToken)Visit(node.HashToken);
		SyntaxToken exclamationToken = (SyntaxToken)Visit(node.ExclamationToken);
		SyntaxToken endOfDirectiveToken = (SyntaxToken)Visit(node.EndOfDirectiveToken);
		return node.Update(hashToken, exclamationToken, endOfDirectiveToken, node.IsActive);
	}

	public CSharpSyntaxRewriter(bool visitIntoStructuredTrivia = false)
	{
		VisitIntoStructuredTrivia = visitIntoStructuredTrivia;
	}

	public override CSharpSyntaxNode VisitToken(SyntaxToken token)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode> syntaxList = VisitList(token.LeadingTrivia);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode> syntaxList2 = VisitList(token.TrailingTrivia);
		if (syntaxList != token.LeadingTrivia || syntaxList2 != token.TrailingTrivia)
		{
			if (syntaxList != token.LeadingTrivia)
			{
				token = token.TokenWithLeadingTrivia(syntaxList.Node);
			}
			if (syntaxList2 != token.TrailingTrivia)
			{
				token = token.TokenWithTrailingTrivia(syntaxList2.Node);
			}
		}
		return token;
	}

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode> VisitList<TNode>(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode> list) where TNode : CSharpSyntaxNode
	{
		SyntaxListBuilder syntaxListBuilder = null;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			TNode val = list[i];
			CSharpSyntaxNode cSharpSyntaxNode = Visit(val);
			if (val != cSharpSyntaxNode && syntaxListBuilder == null)
			{
				syntaxListBuilder = new SyntaxListBuilder(count);
				syntaxListBuilder.AddRange(list, 0, i);
			}
			syntaxListBuilder?.Add(cSharpSyntaxNode);
		}
		if (syntaxListBuilder != null)
		{
			return syntaxListBuilder.ToList();
		}
		return list;
	}

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode> VisitList<TNode>(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode> list) where TNode : CSharpSyntaxNode
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode> syntaxList = list.GetWithSeparators();
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode> syntaxList2 = VisitList(syntaxList);
		if (syntaxList2 != syntaxList)
		{
			return syntaxList2.AsSeparatedList<TNode>();
		}
		return list;
	}
}
