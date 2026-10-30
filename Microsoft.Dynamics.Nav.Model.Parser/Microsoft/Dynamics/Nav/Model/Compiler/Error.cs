namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Error : AstNode
{
	public string ErrorMessage { get; set; }

	public Error()
	{
	}

	public Error(string message, TextPosition position = null)
		: base(AstNodeType.ErrorNode, position)
	{
		ErrorMessage = message;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.VisitErrorNode(this);
	}
}
