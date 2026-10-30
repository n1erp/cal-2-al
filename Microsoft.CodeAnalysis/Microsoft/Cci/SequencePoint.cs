using System.Diagnostics;
using Roslyn.Utilities;

namespace Microsoft.Cci;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal struct SequencePoint(DebugSourceDocument document, int offset, int startLine, int startColumn, int endLine, int endColumn)
{
	public readonly int Offset = offset;

	public readonly int StartLine = startLine;

	public readonly int StartColumn = startColumn;

	public readonly int EndLine = endLine;

	public readonly int EndColumn = endColumn;

	public readonly DebugSourceDocument Document = document;

	public bool IsHidden => StartLine == 16707566;

	public override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override bool Equals(object obj)
	{
		throw ExceptionUtilities.Unreachable;
	}

	private string GetDebuggerDisplay()
	{
		if (!IsHidden)
		{
			return $"{Offset}: ({StartLine}, {StartColumn}) - ({EndLine}, {EndColumn})";
		}
		return "<hidden>";
	}
}
