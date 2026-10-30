namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class PseudoVariableExpressions
{
	internal abstract BoundExpression GetValue(BoundPseudoVariable variable, DiagnosticBag diagnostics);

	internal abstract BoundExpression GetAddress(BoundPseudoVariable variable);
}
