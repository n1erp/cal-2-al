using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalMethodOrTriggerDeclarationSyntax : InternalMemberSyntax
{
	public abstract InternalSyntaxList<InternalMemberAttributeSyntax> Attributes { get; }

	public abstract InternalIdentifierNameSyntax Name { get; }

	public abstract InternalParameterListSyntax ParameterList { get; }

	public abstract InternalReturnValueSyntax ReturnValue { get; }

	public abstract InternalSyntaxToken SemicolonToken { get; }

	public abstract InternalVarSectionSyntax Variables { get; }

	public abstract InternalBlockSyntax Body { get; }

	internal InternalMethodOrTriggerDeclarationSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalMethodOrTriggerDeclarationSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalMethodOrTriggerDeclarationSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
