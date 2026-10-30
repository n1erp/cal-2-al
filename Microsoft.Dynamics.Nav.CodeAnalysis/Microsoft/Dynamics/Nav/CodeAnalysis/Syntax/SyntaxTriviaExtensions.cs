using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal static class SyntaxTriviaExtensions
{
	public static bool MatchesKind(this SyntaxTrivia trivia, SyntaxKind kind)
	{
		return trivia.Kind == kind;
	}

	public static bool MatchesKind(this SyntaxTrivia trivia, SyntaxKind kind1, SyntaxKind kind2)
	{
		SyntaxKind kind3 = trivia.Kind;
		if (kind3 != kind1)
		{
			return kind3 == kind2;
		}
		return true;
	}

	public static bool MatchesKind(this SyntaxTrivia trivia, params SyntaxKind[] kinds)
	{
		return kinds.Contains(trivia.Kind);
	}

	public static bool IsRegularComment(this SyntaxTrivia trivia)
	{
		if (!trivia.IsSingleLineComment())
		{
			return trivia.IsMultiLineComment();
		}
		return true;
	}

	public static bool IsSingleLineComment(this SyntaxTrivia trivia)
	{
		return trivia.Kind == SyntaxKind.LineCommentTrivia;
	}

	public static bool IsMultiLineComment(this SyntaxTrivia trivia)
	{
		return trivia.Kind == SyntaxKind.CommentTrivia;
	}

	public static bool IsCompleteMultiLineComment(this SyntaxTrivia trivia)
	{
		if (trivia.Kind != SyntaxKind.CommentTrivia)
		{
			return false;
		}
		string text = trivia.ToFullString();
		if (text.Length >= 2)
		{
			return text[text.Length - 1] == '}';
		}
		return false;
	}

	public static string GetCommentText(this SyntaxTrivia trivia)
	{
		string text = trivia.ToString();
		if (trivia.Kind == SyntaxKind.LineCommentTrivia)
		{
			if (text.StartsWith("//", StringComparison.Ordinal))
			{
				text = text.Substring(2);
			}
			return text.TrimStart(null);
		}
		if (trivia.Kind == SyntaxKind.CommentTrivia && text.Length > 1)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (text.EndsWith("*/", StringComparison.Ordinal))
			{
				text = text.Substring(0, text.Length - 2);
			}
			if (text.StartsWith("/*", StringComparison.Ordinal))
			{
				text = text.Substring(2);
			}
			text = text.Trim();
			string newLine = Environment.NewLine;
			string[] array = text.Split(new string[1] { newLine }, StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				string value = array[i].Trim();
				stringBuilder.AppendLine(value);
			}
			stringBuilder.Remove(stringBuilder.Length - newLine.Length, newLine.Length);
			return stringBuilder.ToString();
		}
		throw new InvalidOperationException();
	}

	public static string AsString(this IEnumerable<SyntaxTrivia> trivia)
	{
		if (trivia == null)
		{
			throw new ArgumentNullException("trivia");
		}
		IList<SyntaxTrivia> source = trivia.ToList();
		if (source.Any())
		{
			StringBuilder sb = new StringBuilder();
			source.Select((SyntaxTrivia t) => t.ToFullString()).Do((string s) =>
			{
				sb.Append(s);
			});
			return sb.ToString();
		}
		return string.Empty;
	}

	public static int GetFullWidth(this IEnumerable<SyntaxTrivia> trivia)
	{
		if (trivia == null)
		{
			throw new ArgumentNullException("trivia");
		}
		return trivia.Sum((SyntaxTrivia t) => t.FullWidth);
	}

	public static bool IsWhitespaceOrEndOfLine(this SyntaxTrivia trivia)
	{
		if (trivia.Kind != SyntaxKind.WhiteSpaceTrivia)
		{
			return trivia.Kind == SyntaxKind.EndOfLineTrivia;
		}
		return true;
	}

	public static SyntaxTrivia GetPreviousTrivia(this SyntaxTrivia trivia, SyntaxTree syntaxTree, CancellationToken cancellationToken, bool findInsideTrivia = false)
	{
		TextSpan fullSpan = trivia.FullSpan;
		if (fullSpan.Start == 0)
		{
			return default;
		}
		return syntaxTree.GetRoot(cancellationToken).FindTrivia(fullSpan.Start - 1, findInsideTrivia);
	}
}
