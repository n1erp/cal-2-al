namespace Microsoft.CodeAnalysis.CSharp;

internal enum ArgumentAnalysisResultKind : byte
{
	Normal = 0,
	Expanded = 1,
	NoCorrespondingParameter = 2,
	FirstInvalid = NoCorrespondingParameter,
	NoCorrespondingNamedParameter = 3,
	DuplicateNamedArgument = 4,
	RequiredParameterMissing = 5,
	NameUsedForPositional = 6,
	BadNonTrailingNamedArgument = 7
}
