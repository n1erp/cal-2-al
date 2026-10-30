using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundTypeOrInstanceInitializers : BoundStatementList
{
	public BoundTypeOrInstanceInitializers(SyntaxNode syntax, ImmutableArray<BoundStatement> statements, bool hasErrors = false)
		: base(BoundKind.TypeOrInstanceInitializers, syntax, statements, hasErrors || statements.HasErrors())
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTypeOrInstanceInitializers(this);
	}

	public new BoundTypeOrInstanceInitializers Update(ImmutableArray<BoundStatement> statements)
	{
		if (statements != Statements)
		{
			return new BoundTypeOrInstanceInitializers(Syntax, statements, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
