using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundSequencePoint : BoundStatement
{
	public BoundStatement StatementOpt { get; }

	public static BoundStatement Create(CSharpSyntaxNode syntax, TextSpan? part, BoundStatement statement, bool hasErrors = false)
	{
		if (part.HasValue)
		{
			return new BoundSequencePointWithSpan(syntax, statement, part.Value, hasErrors);
		}
		return new BoundSequencePoint(syntax, statement, hasErrors);
	}

	public BoundSequencePoint(SyntaxNode syntax, BoundStatement statementOpt, bool hasErrors = false)
		: base(BoundKind.SequencePoint, syntax, hasErrors || statementOpt.HasErrors())
	{
		StatementOpt = statementOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSequencePoint(this);
	}

	public BoundSequencePoint Update(BoundStatement statementOpt)
	{
		if (statementOpt != StatementOpt)
		{
			return new BoundSequencePoint(Syntax, statementOpt, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
