namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundLockStatement : BoundStatement
{
	public BoundExpression Argument { get; }

	public BoundStatement Body { get; }

	public BoundLockStatement(SyntaxNode syntax, BoundExpression argument, BoundStatement body, bool hasErrors = false)
		: base(BoundKind.LockStatement, syntax, hasErrors || argument.HasErrors() || body.HasErrors())
	{
		Argument = argument;
		Body = body;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLockStatement(this);
	}

	public BoundLockStatement Update(BoundExpression argument, BoundStatement body)
	{
		if (argument != Argument || body != Body)
		{
			return new BoundLockStatement(Syntax, argument, body, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
