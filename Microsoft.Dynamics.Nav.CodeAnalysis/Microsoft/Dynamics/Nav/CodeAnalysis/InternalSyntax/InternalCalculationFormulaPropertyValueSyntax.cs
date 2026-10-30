using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalCalculationFormulaPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal InternalCalculationFormulaPropertyValueSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalCalculationFormulaPropertyValueSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalCalculationFormulaPropertyValueSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
