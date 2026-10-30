using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class AttributeArgumentListSyntaxExtensions
{
	private const char separator = ',';

	public static int CalculateAttributeArgumentPosition(this AttributeArgumentListSyntax syntax, int cursorPosition)
	{
		int num = 0;
		foreach (AttributeArgumentSyntax argument in syntax.Arguments)
		{
			if (argument.Span.IntersectsWith(cursorPosition) || argument.EndPosition >= cursorPosition)
			{
				break;
			}
			num++;
		}
		return num + ConsiderSkippedEmptyArguments(syntax, num, cursorPosition);
	}

	private static int ConsiderSkippedEmptyArguments(AttributeArgumentListSyntax syntax, int argumentPosition, int cursorPosition)
	{
		return CountCommaOnParenthesisTrivias(syntax, cursorPosition) + CountCommaOnListSeparators(syntax, argumentPosition, cursorPosition);
	}

	private static int CountCommaOnParenthesisTrivias(AttributeArgumentListSyntax syntax, int cursorPosition)
	{
		int num = 0;
		if (syntax.OpenParenthesisToken.HasTrailingTrivia)
		{
			foreach (SyntaxTrivia trailingTrivium in syntax.OpenParenthesisToken.TrailingTrivia)
			{
				if (trailingTrivium.Span.End < cursorPosition)
				{
					num += trailingTrivium.ToString().Count((char x) => x == ',');
				}
			}
		}
		return num;
	}

	private static int CountCommaOnListSeparators(AttributeArgumentListSyntax syntax, int argumentPosition, int cursorPosition)
	{
		int num = 0;
		SyntaxToken[] array = syntax.Arguments.GetSeparators().ToArray();
		for (int i = 0; i < argumentPosition; i++)
		{
			if (!array[i].HasTrailingTrivia)
			{
				continue;
			}
			foreach (SyntaxTrivia item in array[i].GetAllTrailingTriviaToNextToken())
			{
				if (item.Span.End < cursorPosition)
				{
					num += item.ToString().Count((char x) => x == ',');
				}
			}
		}
		return num;
	}
}
