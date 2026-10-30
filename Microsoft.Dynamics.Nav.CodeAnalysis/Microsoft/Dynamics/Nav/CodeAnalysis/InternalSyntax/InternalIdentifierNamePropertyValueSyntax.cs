using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalIdentifierNamePropertyValueSyntax : InternalPropertyValueSyntax
{
	public abstract InternalIdentifierNameSyntax Value { get; }

	internal InternalIdentifierNamePropertyValueSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalIdentifierNamePropertyValueSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalIdentifierNamePropertyValueSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
