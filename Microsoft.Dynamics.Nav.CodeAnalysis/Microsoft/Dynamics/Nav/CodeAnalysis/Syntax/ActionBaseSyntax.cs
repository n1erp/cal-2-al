using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public abstract class ActionBaseSyntax : SyntaxNode
{
	public abstract SyntaxToken ActionKeyword { get; }

	public abstract SyntaxToken OpenParenthesisToken { get; }

	public abstract IdentifierNameSyntax Name { get; }

	public abstract SyntaxToken CloseParenthesisToken { get; }

	public abstract SyntaxToken OpenBraceToken { get; }

	public abstract PropertyListSyntax PropertyList { get; }

	public abstract SyntaxToken CloseBraceToken { get; }

	internal ActionBaseSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}
}
