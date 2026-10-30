using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public abstract class AstNode
{
	public AstNodeType NodeType { get; set; }

	public TextPosition Position { get; set; }

	protected AstNode()
	{
		NodeType = AstNodeType.ErrorNode;
	}

	protected AstNode(AstNodeType nodeType, TextPosition position = null)
	{
		NodeType = nodeType;
		Position = position ?? TextPosition.Undefined;
	}

	public virtual void Dump(int indent)
	{
	}

	protected void AcceptStatements(IEnumerable<Statement> statements, AstVisitor visitor)
	{
		if (statements == null)
		{
			return;
		}
		foreach (Statement statement in statements)
		{
			statement.Accept(visitor);
		}
	}

	protected void AcceptExpressions(IEnumerable<Expression> expressions, AstVisitor visitor)
	{
		if (expressions == null)
		{
			return;
		}
		foreach (Expression expression in expressions)
		{
			expression.Accept(visitor);
		}
	}

	protected void AcceptAstNode(AstNode node, AstVisitor visitor)
	{
		node?.Accept(visitor);
	}

	protected internal abstract void Accept(AstVisitor visitor);
}
