using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public abstract class FieldGroupChangeBaseSyntax : SyntaxNode
{
	public abstract SyntaxToken OpenParenthesisToken { get; }

	public abstract IdentifierNameSyntax Anchor { get; }

	public abstract SyntaxToken CloseParenthesisToken { get; }

	public abstract SyntaxToken OpenBraceToken { get; }

	public abstract PropertyListSyntax PropertyList { get; }

	public abstract SyntaxToken CloseBraceToken { get; }

	internal FieldGroupChangeBaseSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}
}
