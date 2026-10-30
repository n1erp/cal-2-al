using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class TextSpanExtensions
{
	internal static bool IsLocationInSpan(this TextSpan span, Location location, SyntaxTree syntaxTree)
	{
		DebugAssertHelper.Assert(syntaxTree != null);
		if (location.SourceTree == syntaxTree)
		{
			return span.Contains(location.SourceSpan);
		}
		return false;
	}
}
