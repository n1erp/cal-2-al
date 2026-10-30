using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public abstract class ObjectSyntax : SyntaxNode
{
	public abstract IdentifierNameSyntax Name { get; }

	public abstract PropertyListSyntax PropertyList { get; }

	public abstract SyntaxList<MemberSyntax> Members { get; }

	internal ObjectSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}
}
