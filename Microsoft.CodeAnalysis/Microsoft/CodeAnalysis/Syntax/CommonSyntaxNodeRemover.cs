namespace Microsoft.CodeAnalysis.Syntax;

internal static class CommonSyntaxNodeRemover
{
	public static void GetSeparatorInfo(SyntaxNodeOrTokenList nodesAndSeparators, int nodeIndex, int endOfLineKind, out bool nextTokenIsSeparator, out bool nextSeparatorBelongsToNode)
	{
		SyntaxNode syntaxNode = nodesAndSeparators[nodeIndex].AsNode();
		nextTokenIsSeparator = nodeIndex + 1 < nodesAndSeparators.Count && nodesAndSeparators[nodeIndex + 1].IsToken;
		int num;
		if (nextTokenIsSeparator)
		{
			SyntaxToken syntaxToken = nodesAndSeparators[nodeIndex + 1].AsToken();
			if (!syntaxToken.HasLeadingTrivia && !ContainsEndOfLine(syntaxNode.GetTrailingTrivia(), endOfLineKind))
			{
				num = (ContainsEndOfLine(syntaxToken.TrailingTrivia, endOfLineKind) ? 1 : 0);
				goto IL_0074;
			}
		}
		num = 0;
		goto IL_0074;
		IL_0074:
		nextSeparatorBelongsToNode = (byte)num != 0;
	}

	private static bool ContainsEndOfLine(SyntaxTriviaList triviaList, int endOfLineKind)
	{
		return triviaList.IndexOf(endOfLineKind) >= 0;
	}
}
