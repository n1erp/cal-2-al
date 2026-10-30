namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal enum ArgumentAnalysisResultKind : byte
{
	Normal = 0,
	NoCorrespondingParameter = 1,
	FirstInvalid = NoCorrespondingParameter,
	RequiredParameterMissing = 2
}
