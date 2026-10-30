using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

internal static class SyntaxTreeExtensions
{
	[Conditional("DEBUG")]
	internal static void VerifySource(this SyntaxTree tree, IEnumerable<TextChangeRange> changes = null)
	{
		SyntaxNode root = tree.GetRoot();
		SourceText text = tree.GetText();
		TextSpan textSpan = new TextSpan(0, text.Length);
		SyntaxNode syntaxNode = null;
		if (changes != null)
		{
			TextSpan change = TextChangeRange.Collapse(changes).Span;
			if (change != textSpan)
			{
				syntaxNode = root.DescendantNodes((SyntaxNode n) => n.FullSpan.Contains(change)).LastOrDefault();
			}
		}
		if (syntaxNode == null)
		{
			syntaxNode = root;
		}
		TextSpan fullSpan = syntaxNode.FullSpan;
		TextSpan? textSpan2 = fullSpan.Intersection(textSpan);
		char c = '\0';
		char c2 = '\0';
		int num;
		if (!textSpan2.HasValue)
		{
			num = 0;
		}
		else
		{
			string text2 = text.ToString(textSpan2.Value);
			string text3 = syntaxNode.ToFullString();
			num = FindFirstDifference(text2, text3);
			if (num >= 0)
			{
				c = text3[num];
				c2 = text2[num];
			}
		}
		if (num >= 0)
		{
			num += fullSpan.Start;
			if (num < text.Length)
			{
				LinePosition linePosition = text.Lines.GetLinePosition(num);
				TextLine textLine = text.Lines[linePosition.Line];
				text.ToString();
				_ = $"Unexpected difference at offset {num}: Line {linePosition.Line + 1}, Column {linePosition.Character + 1} \"{textLine.ToString()}\"  (Found: [{c}] Expected: [{c2}])";
			}
		}
	}

	private static int FindFirstDifference(string s1, string s2)
	{
		int length = s1.Length;
		int length2 = s2.Length;
		int num = Math.Min(length, length2);
		for (int i = 0; i < num; i++)
		{
			if (s1[i] != s2[i])
			{
				return i;
			}
		}
		if (length != length2)
		{
			return num + 1;
		}
		return -1;
	}

	public static bool IsHiddenPosition(this SyntaxTree tree, int position, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!tree.HasHiddenRegions())
		{
			return false;
		}
		LineVisibility lineVisibility = tree.GetLineVisibility(position, cancellationToken);
		if (lineVisibility != LineVisibility.Hidden)
		{
			return lineVisibility == LineVisibility.BeforeFirstLineDirective;
		}
		return true;
	}
}
