namespace Microsoft.CodeAnalysis;

public enum CandidateReason
{
	None,
	NotATypeOrNamespace,
	NotAnEvent,
	NotAWithEventsMember,
	NotAnAttributeType,
	WrongArity,
	NotCreatable,
	NotReferencable,
	Inaccessible,
	NotAValue,
	NotAVariable,
	NotInvocable,
	StaticInstanceMismatch,
	OverloadResolutionFailure,
	LateBound,
	Ambiguous,
	MemberGroup
}
