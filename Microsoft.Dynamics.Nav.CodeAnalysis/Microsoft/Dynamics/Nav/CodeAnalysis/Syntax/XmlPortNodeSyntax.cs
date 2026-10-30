using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public abstract class XmlPortNodeSyntax : SyntaxNode
{
	public abstract SyntaxToken OpenParenthesisToken { get; }

	public abstract IdentifierNameSyntax Name { get; }

	public abstract SyntaxToken CloseParenthesisToken { get; }

	public abstract SyntaxToken OpenBraceToken { get; }

	public abstract PropertyListSyntax PropertyList { get; }

	public abstract SyntaxList<XmlPortNodeSyntax> Schema { get; }

	public abstract SyntaxList<TriggerDeclarationSyntax> Triggers { get; }

	public abstract SyntaxToken CloseBraceToken { get; }

	internal XmlPortNodeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}
}
