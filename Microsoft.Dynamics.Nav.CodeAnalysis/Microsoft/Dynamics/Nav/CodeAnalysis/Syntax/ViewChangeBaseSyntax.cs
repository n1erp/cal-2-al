using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public abstract class ViewChangeBaseSyntax : SyntaxNode
{
	public abstract SyntaxToken ChangeKeyword { get; }

	public abstract SyntaxToken OpenParenthesisToken { get; }

	public abstract SyntaxToken CloseParenthesisToken { get; }

	internal ViewChangeBaseSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}
}
