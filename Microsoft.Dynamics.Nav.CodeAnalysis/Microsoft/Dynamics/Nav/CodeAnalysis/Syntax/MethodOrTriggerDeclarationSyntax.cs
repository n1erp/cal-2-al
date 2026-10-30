using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public abstract class MethodOrTriggerDeclarationSyntax : MemberSyntax
{
	public abstract SyntaxList<MemberAttributeSyntax> Attributes { get; }

	public abstract IdentifierNameSyntax Name { get; }

	public abstract ParameterListSyntax ParameterList { get; }

	public abstract ReturnValueSyntax ReturnValue { get; }

	public abstract SyntaxToken SemicolonToken { get; }

	public abstract VarSectionSyntax Variables { get; }

	public abstract BlockSyntax Body { get; }

	internal MethodOrTriggerDeclarationSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}
}
