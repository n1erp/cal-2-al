using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundLoopStatement : BoundStatement
{
	public GeneratedLabelSymbol BreakLabel { get; }

	public GeneratedLabelSymbol ContinueLabel { get; }

	protected BoundLoopStatement(BoundKind kind, SyntaxNode syntax, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel, bool hasErrors)
		: base(kind, syntax, hasErrors)
	{
		BreakLabel = breakLabel;
		ContinueLabel = continueLabel;
	}

	protected BoundLoopStatement(BoundKind kind, SyntaxNode syntax, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel)
		: base(kind, syntax)
	{
		BreakLabel = breakLabel;
		ContinueLabel = continueLabel;
	}
}
