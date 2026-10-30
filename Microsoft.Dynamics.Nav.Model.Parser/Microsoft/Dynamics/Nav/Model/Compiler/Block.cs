using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Block : Statement
{
	public List<Statement> Stmts { get; set; }

	public Block(List<Statement> stmts, TextPosition position = null)
		: base(AstNodeType.BlockNode, position)
	{
		Stmts = stmts;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptStatements(Stmts, visitor);
		visitor.VisitBlockNode(this);
	}
}
