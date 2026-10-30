using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class SyntaxTriviaExtensions
{
	public static int Width(this SyntaxTrivia trivia)
	{
		return trivia.Span.Length;
	}

	public static int FullWidth(this SyntaxTrivia trivia)
	{
		return trivia.FullSpan.Length;
	}

	public static bool IsElastic(this SyntaxTrivia trivia)
	{
		return trivia.HasAnnotation(SyntaxAnnotation.ElasticAnnotation);
	}

	public static bool IsRegularOrDocComment(this SyntaxTrivia trivia)
	{
		return trivia.IsRegularComment();
	}
}
