using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class BoundLoopStatement : BoundStatement
{
	public GeneratedLabelSymbol BreakLabel { get; private set; }

	protected BoundLoopStatement(BoundKind kind, SyntaxNode syntax, GeneratedLabelSymbol breakLabel, bool hasErrors)
		: base(kind, syntax, hasErrors)
	{
		BreakLabel = breakLabel;
	}

	protected BoundLoopStatement(BoundKind kind, SyntaxNode syntax, GeneratedLabelSymbol breakLabel)
		: base(kind, syntax)
	{
		BreakLabel = breakLabel;
	}
}
