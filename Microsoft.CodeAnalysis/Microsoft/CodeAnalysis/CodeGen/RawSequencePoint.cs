using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CodeGen;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal struct RawSequencePoint
{
	internal readonly SyntaxTree SyntaxTree;

	internal readonly int ILMarker;

	internal readonly TextSpan Span;

	internal static readonly TextSpan HiddenSequencePointSpan = new TextSpan(int.MaxValue, 0);

	internal RawSequencePoint(SyntaxTree syntaxTree, int ilMarker, TextSpan span)
	{
		SyntaxTree = syntaxTree;
		ILMarker = ilMarker;
		Span = span;
	}

	private string GetDebuggerDisplay()
	{
		return string.Format("#{0}: {1}", ILMarker, (Span == HiddenSequencePointSpan) ? "hidden" : Span.ToString());
	}
}
