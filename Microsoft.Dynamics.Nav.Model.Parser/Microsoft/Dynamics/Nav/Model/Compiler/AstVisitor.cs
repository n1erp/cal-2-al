namespace Microsoft.Dynamics.Nav.Model.Compiler;

public abstract class AstVisitor
{
	public virtual void Visit(AstNode node)
	{
		node?.Accept(this);
	}

	public virtual void DefaultVisit(AstNode symbol)
	{
	}

	public virtual void VisitCastNode(Cast cast)
	{
		DefaultVisit(cast);
	}

	public virtual void VisitDefaultValueNode(DefaultValue defaultValue)
	{
		DefaultVisit(defaultValue);
	}

	public virtual void VisitIdentifierNode(Identifier identifier)
	{
		DefaultVisit(identifier);
	}

	public virtual void VisitInListNode(InList inList)
	{
		DefaultVisit(inList);
	}

	public virtual void VisitOptionAccessNode(OptionAccess optionAccess)
	{
		DefaultVisit(optionAccess);
	}

	public virtual void VisitMemberAccessNode(MemberAccess memberAccess)
	{
		DefaultVisit(memberAccess);
	}

	public virtual void VisitMethodCallNode(MethodCall methodCall)
	{
		DefaultVisit(methodCall);
	}

	public virtual void VisitArrayIndexNode(ArrayIndex arrayIndex)
	{
		DefaultVisit(arrayIndex);
	}

	public virtual void VisitBinaryNode(Binary binary)
	{
		DefaultVisit(binary);
	}

	public virtual void VisitUnaryNode(Unary unary)
	{
		DefaultVisit(unary);
	}

	public virtual void VisitParenthesesExpressionNode(ParenthesesExpr parenthesesExpr)
	{
		DefaultVisit(parenthesesExpr);
	}

	public virtual void VisitLiteralNode(Expression expression)
	{
		DefaultVisit(expression);
	}

	public virtual void VisitTypeNode(TypeIdentifier typeIdentifier)
	{
		DefaultVisit(typeIdentifier);
	}

	public virtual void VisitAssertStatementNode(AssertErrorStatement assertErrorStatement)
	{
		DefaultVisit(assertErrorStatement);
	}

	public virtual void VisitExpressionStatementNode(ExpressionStatement expressionStatement)
	{
		DefaultVisit(expressionStatement);
	}

	public virtual void VisitEmptyStatementNode(EmptyStatement emptyStatement)
	{
		DefaultVisit(emptyStatement);
	}

	public virtual void VisitWithNode(With with)
	{
		DefaultVisit(with);
	}

	public virtual void VisitBlockNode(Block block)
	{
		DefaultVisit(block);
	}

	public virtual void VisitExitNode(Exit exit)
	{
		DefaultVisit(exit);
	}

	public virtual void VisitForEachNode(Foreach foreachNode)
	{
		DefaultVisit(foreachNode);
	}

	public virtual void VisitForNode(For forNode)
	{
		DefaultVisit(forNode);
	}

	public virtual void VisitCaseNode(Case caseNode)
	{
		DefaultVisit(caseNode);
	}

	public virtual void VisitWhileNode(While whileNode)
	{
		DefaultVisit(whileNode);
	}

	public virtual void VisitRepeatNode(Repeat repeatNode)
	{
		DefaultVisit(repeatNode);
	}

	public virtual void VisitIfNode(If ifNode)
	{
		DefaultVisit(ifNode);
	}

	public virtual void VisitAssignNode(Assign assign)
	{
		DefaultVisit(assign);
	}

	public virtual void VisitDeclarationNode(Declaration declarationNode)
	{
		DefaultVisit(declarationNode);
	}

	public virtual void VisitErrorNode(Error error)
	{
		DefaultVisit(error);
	}

	public virtual void VisitMemberNode(Member member)
	{
		DefaultVisit(member);
	}

	public virtual void VisitParameterNode(Parameter parameter)
	{
		DefaultVisit(parameter);
	}

	public virtual void OnAccepting(AstNode node)
	{
		DefaultOnAccepting(node);
	}

	public virtual void DefaultOnAccepting(AstNode node)
	{
	}
}
