using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundSequencePointWithSpan : BoundStatement
{
	public BoundStatement StatementOpt { get; }

	public TextSpan Span { get; }

	public BoundSequencePointWithSpan(SyntaxNode syntax, BoundStatement statementOpt, TextSpan span, bool hasErrors = false)
		: base(BoundKind.SequencePointWithSpan, syntax, hasErrors || statementOpt.HasErrors())
	{
		StatementOpt = statementOpt;
		Span = span;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSequencePointWithSpan(this);
	}

	public BoundSequencePointWithSpan Update(BoundStatement statementOpt, TextSpan span)
	{
		if (statementOpt != StatementOpt || span != Span)
		{
			return new BoundSequencePointWithSpan(Syntax, statementOpt, span, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
