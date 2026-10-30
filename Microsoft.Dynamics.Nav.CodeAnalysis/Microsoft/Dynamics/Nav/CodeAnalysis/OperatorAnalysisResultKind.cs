namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal enum OperatorAnalysisResultKind : byte
{
	Undefined,
	Inapplicable,
	Worse,
	Applicable
}
