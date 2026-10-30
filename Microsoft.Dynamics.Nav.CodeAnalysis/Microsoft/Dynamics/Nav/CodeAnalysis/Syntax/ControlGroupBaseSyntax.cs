using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public abstract class ControlGroupBaseSyntax : ControlBaseSyntax
{
	public abstract SyntaxList<ControlBaseSyntax> Controls { get; }

	internal ControlGroupBaseSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}
}
