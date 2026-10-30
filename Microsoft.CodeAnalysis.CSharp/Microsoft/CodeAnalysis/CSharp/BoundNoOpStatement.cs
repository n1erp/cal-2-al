namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundNoOpStatement : BoundStatement
{
	public NoOpStatementFlavor Flavor { get; }

	public BoundNoOpStatement(SyntaxNode syntax, NoOpStatementFlavor flavor, bool hasErrors)
		: base(BoundKind.NoOpStatement, syntax, hasErrors)
	{
		Flavor = flavor;
	}

	public BoundNoOpStatement(SyntaxNode syntax, NoOpStatementFlavor flavor)
		: base(BoundKind.NoOpStatement, syntax)
	{
		Flavor = flavor;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNoOpStatement(this);
	}

	public BoundNoOpStatement Update(NoOpStatementFlavor flavor)
	{
		if (flavor != Flavor)
		{
			return new BoundNoOpStatement(Syntax, flavor, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
