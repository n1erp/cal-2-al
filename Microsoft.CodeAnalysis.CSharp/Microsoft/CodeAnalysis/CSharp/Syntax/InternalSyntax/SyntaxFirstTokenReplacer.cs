namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class SyntaxFirstTokenReplacer : CSharpSyntaxRewriter
{
	private readonly SyntaxToken _oldToken;

	private readonly SyntaxToken _newToken;

	private readonly int _diagnosticOffsetDelta;

	private bool _foundOldToken;

	private SyntaxFirstTokenReplacer(SyntaxToken oldToken, SyntaxToken newToken, int diagnosticOffsetDelta)
	{
		_oldToken = oldToken;
		_newToken = newToken;
		_diagnosticOffsetDelta = diagnosticOffsetDelta;
		_foundOldToken = false;
	}

	internal static TRoot Replace<TRoot>(TRoot root, SyntaxToken oldToken, SyntaxToken newToken, int diagnosticOffsetDelta) where TRoot : CSharpSyntaxNode
	{
		return (TRoot)new SyntaxFirstTokenReplacer(oldToken, newToken, diagnosticOffsetDelta).Visit(root);
	}

	public override CSharpSyntaxNode Visit(CSharpSyntaxNode node)
	{
		if (node != null && !_foundOldToken)
		{
			if (node is SyntaxToken)
			{
				_foundOldToken = true;
				return _newToken;
			}
			return UpdateDiagnosticOffset(base.Visit(node), _diagnosticOffsetDelta);
		}
		return node;
	}

	private static TSyntax UpdateDiagnosticOffset<TSyntax>(TSyntax node, int diagnosticOffsetDelta) where TSyntax : CSharpSyntaxNode
	{
		DiagnosticInfo[] diagnostics = node.GetDiagnostics();
		if (diagnostics == null || diagnostics.Length == 0)
		{
			return node;
		}
		int num = diagnostics.Length;
		DiagnosticInfo[] array = new DiagnosticInfo[num];
		for (int i = 0; i < num; i++)
		{
			DiagnosticInfo diagnosticInfo = diagnostics[i];
			SyntaxDiagnosticInfo syntaxDiagnosticInfo = diagnosticInfo as SyntaxDiagnosticInfo;
			array[i] = ((syntaxDiagnosticInfo == null) ? diagnosticInfo : new SyntaxDiagnosticInfo(syntaxDiagnosticInfo.Offset + diagnosticOffsetDelta, syntaxDiagnosticInfo.Width, (ErrorCode)syntaxDiagnosticInfo.Code, syntaxDiagnosticInfo.Arguments));
		}
		return node.WithDiagnosticsGreen(array);
	}
}
