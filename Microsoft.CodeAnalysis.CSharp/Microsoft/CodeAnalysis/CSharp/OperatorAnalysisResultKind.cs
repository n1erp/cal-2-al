namespace Microsoft.CodeAnalysis.CSharp;

internal enum OperatorAnalysisResultKind : byte
{
	Undefined,
	Inapplicable,
	Worse,
	Applicable
}
