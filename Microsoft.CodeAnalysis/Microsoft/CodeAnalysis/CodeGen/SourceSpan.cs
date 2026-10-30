using System.Diagnostics;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal struct SourceSpan(DebugSourceDocument document, int startLine, int startColumn, int endLine, int endColumn)
{
	public readonly int StartLine = startLine;

	public readonly int StartColumn = startColumn;

	public readonly int EndLine = endLine;

	public readonly int EndColumn = endColumn;

	public readonly DebugSourceDocument Document = document;

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
		return $"({StartLine}, {StartColumn}) - ({EndLine}, {EndColumn})";
	}
}
