using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalXmlPortNodeSyntax : InternalSyntaxNode
{
	public abstract InternalSyntaxToken OpenParenthesisToken { get; }

	public abstract InternalIdentifierNameSyntax Name { get; }

	public abstract InternalSyntaxToken CloseParenthesisToken { get; }

	public abstract InternalSyntaxToken OpenBraceToken { get; }

	public abstract InternalPropertyListSyntax PropertyList { get; }

	public abstract InternalSyntaxList<InternalXmlPortNodeSyntax> Schema { get; }

	public abstract InternalSyntaxList<InternalTriggerDeclarationSyntax> Triggers { get; }

	public abstract InternalSyntaxToken CloseBraceToken { get; }

	internal InternalXmlPortNodeSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalXmlPortNodeSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalXmlPortNodeSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
