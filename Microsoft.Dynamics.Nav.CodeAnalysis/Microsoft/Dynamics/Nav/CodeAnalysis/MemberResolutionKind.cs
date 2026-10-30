namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal enum MemberResolutionKind : byte
{
	None,
	Applicable,
	NoCorrespondingParameter,
	RequiredParameterMissing,
	UseSiteError,
	BadArguments,
	BadTypeFromFirstArguments,
	MemberRequired,
	Worse,
	Worst
}
