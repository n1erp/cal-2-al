namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundGlobalStatementInitializer : BoundInitializer
{
	public BoundStatement Statement { get; }

	public BoundGlobalStatementInitializer(SyntaxNode syntax, BoundStatement statement, bool hasErrors = false)
		: base(BoundKind.GlobalStatementInitializer, syntax, hasErrors || statement.HasErrors())
	{
		Statement = statement;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitGlobalStatementInitializer(this);
	}

	public BoundGlobalStatementInitializer Update(BoundStatement statement)
	{
		if (statement != Statement)
		{
			return new BoundGlobalStatementInitializer(Syntax, statement, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
