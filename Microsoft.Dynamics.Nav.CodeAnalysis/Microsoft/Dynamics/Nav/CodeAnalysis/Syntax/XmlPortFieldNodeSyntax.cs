using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public abstract class XmlPortFieldNodeSyntax : XmlPortNodeSyntax
{
	public abstract SyntaxToken SemicolonToken { get; }

	public abstract CodeExpressionSyntax SourceField { get; }

	internal XmlPortFieldNodeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}
}
