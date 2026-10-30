namespace Microsoft.CodeAnalysis.CSharp;

internal enum UserDefinedConversionResultKind : byte
{
	NoApplicableOperators,
	NoBestSourceType,
	NoBestTargetType,
	Ambiguous,
	Valid
}
