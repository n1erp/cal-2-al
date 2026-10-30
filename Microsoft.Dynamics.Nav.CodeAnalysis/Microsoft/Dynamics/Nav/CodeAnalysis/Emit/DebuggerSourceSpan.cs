using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class DebuggerSourceSpan
{
	public int StartColumn { get; set; }

	public int StartLine { get; set; }

	public int EndColumn { get; set; }

	public int EndLine { get; set; }

	public DebuggerSourceSpan(FileLinePositionSpan value, int lineOffset)
	{
		StartColumn = value.StartLinePosition.Character;
		StartLine = value.StartLinePosition.Line - lineOffset;
		EndColumn = value.EndLinePosition.Character;
		EndLine = value.EndLinePosition.Line - lineOffset;
	}

	[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
	public static explicit operator long(DebuggerSourceSpan span)
	{
		return ((long)span.StartLine << 48) | ((long)span.StartColumn << 32) | ((long)span.EndLine << 16) | span.EndColumn;
	}
}
