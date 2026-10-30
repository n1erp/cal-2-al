namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundConstantPattern : BoundPattern
{
	public BoundExpression Value { get; }

	public ConstantValue ConstantValue { get; }

	public BoundConstantPattern(SyntaxNode syntax, BoundExpression value, ConstantValue constantValue, bool hasErrors = false)
		: base(BoundKind.ConstantPattern, syntax, hasErrors || value.HasErrors())
	{
		Value = value;
		ConstantValue = constantValue;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConstantPattern(this);
	}

	public BoundConstantPattern Update(BoundExpression value, ConstantValue constantValue)
	{
		if (value != Value || constantValue != ConstantValue)
		{
			return new BoundConstantPattern(Syntax, value, constantValue, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
