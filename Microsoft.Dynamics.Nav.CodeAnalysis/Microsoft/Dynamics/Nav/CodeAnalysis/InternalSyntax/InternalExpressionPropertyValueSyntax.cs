using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalExpressionPropertyValueSyntax : InternalPropertyValueSyntax
{
	public abstract InternalCodeExpressionSyntax Expression { get; }

	internal InternalExpressionPropertyValueSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalExpressionPropertyValueSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalExpressionPropertyValueSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
