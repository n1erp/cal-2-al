using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class InternalSyntaxFirstTokenReplacer : InternalSyntaxRewriter
{
	private readonly InternalSyntaxToken oldToken;

	private readonly InternalSyntaxToken newToken;

	private readonly int diagnosticOffsetDelta;

	private bool foundOldToken;

	private InternalSyntaxFirstTokenReplacer(InternalSyntaxToken oldToken, InternalSyntaxToken newToken, int diagnosticOffsetDelta)
	{
		this.oldToken = oldToken;
		this.newToken = newToken;
		this.diagnosticOffsetDelta = diagnosticOffsetDelta;
		foundOldToken = false;
	}

	internal static TRoot Replace<TRoot>(TRoot root, InternalSyntaxToken oldToken, InternalSyntaxToken newToken, int diagnosticOffsetDelta) where TRoot : InternalSyntaxNode
	{
		InternalSyntaxFirstTokenReplacer internalSyntaxFirstTokenReplacer = new InternalSyntaxFirstTokenReplacer(oldToken, newToken, diagnosticOffsetDelta);
		TRoot result = (TRoot)internalSyntaxFirstTokenReplacer.Visit(root);
		DebugAssertHelper.Assert(internalSyntaxFirstTokenReplacer.foundOldToken);
		return result;
	}

	public override InternalSyntaxNode Visit(InternalSyntaxNode node)
	{
		if (node != null && !foundOldToken)
		{
			if (node is InternalSyntaxToken internalSyntaxToken)
			{
				DebugAssertHelper.Assert(internalSyntaxToken == oldToken);
				foundOldToken = true;
				return newToken;
			}
			return UpdateDiagnosticOffset(base.Visit(node), diagnosticOffsetDelta);
		}
		return node;
	}

	private static TSyntax UpdateDiagnosticOffset<TSyntax>(TSyntax node, int diagnosticOffsetDelta) where TSyntax : InternalSyntaxNode
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
		return node.WithInternalDiagnostics(array);
	}
}
