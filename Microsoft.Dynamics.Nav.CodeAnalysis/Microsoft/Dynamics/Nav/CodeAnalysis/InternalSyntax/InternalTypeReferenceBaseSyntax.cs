using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalTypeReferenceBaseSyntax : InternalSyntaxNode
{
	public abstract InternalArraySyntax Array { get; }

	public abstract InternalDataTypeSyntax DataType { get; }

	internal InternalTypeReferenceBaseSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalTypeReferenceBaseSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalTypeReferenceBaseSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
