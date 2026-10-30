using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public abstract class KeyFieldGroupBaseSyntax : SyntaxNode
{
	public abstract SyntaxToken OpenParenthesisToken { get; }

	public abstract SyntaxToken Id { get; }

	public abstract SyntaxToken SemicolonToken1 { get; }

	public abstract IdentifierNameSyntax Name { get; }

	public abstract SyntaxToken SemicolonToken2 { get; }

	public abstract SeparatedSyntaxList<IdentifierNameSyntax> Fields { get; }

	public abstract SyntaxToken CloseParenthesisToken { get; }

	public abstract SyntaxToken OpenBraceToken { get; }

	public abstract PropertyListSyntax PropertyList { get; }

	public abstract SyntaxToken CloseBraceToken { get; }

	internal KeyFieldGroupBaseSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}
}
