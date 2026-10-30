using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public abstract class AbstractPageExtensionSyntax : ApplicationObjectExtensionSyntax
{
	public abstract SyntaxToken OpenBraceToken { get; }

	public abstract PageExtensionLayoutSyntax Layout { get; }

	public abstract PageExtensionActionListSyntax Actions { get; }

	public abstract PageExtensionViewListSyntax Views { get; }

	public abstract SyntaxToken CloseBraceToken { get; }

	internal AbstractPageExtensionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}
}
