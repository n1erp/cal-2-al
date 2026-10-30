namespace Microsoft.CodeAnalysis.CSharp.CodeGen;

internal enum ExprContext
{
	None,
	Sideeffects,
	Value,
	Address,
	AssignmentTarget,
	Box
}
