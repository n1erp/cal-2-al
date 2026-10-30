namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class AssertErrorStatement : Statement
{
	public Statement Stmt { get; set; }

	public AssertErrorStatement(Statement stmt, TextPosition position = null)
		: base(AstNodeType.AssertStatementNode, position)
	{
		Stmt = stmt;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(Stmt, visitor);
		visitor.VisitAssertStatementNode(this);
	}
}
