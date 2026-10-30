namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundNonConstructorMethodBody : BoundMethodBodyBase
{
	public BoundNonConstructorMethodBody(SyntaxNode syntax, BoundBlock blockBody, BoundBlock expressionBody, bool hasErrors = false)
		: base(BoundKind.NonConstructorMethodBody, syntax, blockBody, expressionBody, hasErrors || blockBody.HasErrors() || expressionBody.HasErrors())
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNonConstructorMethodBody(this);
	}

	public BoundNonConstructorMethodBody Update(BoundBlock blockBody, BoundBlock expressionBody)
	{
		if (blockBody != BlockBody || expressionBody != ExpressionBody)
		{
			return new BoundNonConstructorMethodBody(Syntax, blockBody, expressionBody, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
