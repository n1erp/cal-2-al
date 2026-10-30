namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal enum LookupResultKind : byte
{
	Empty,
	NotATypeOrNamespace,
	NotAnAttributeType,
	NotCreatable,
	Inaccessible,
	NotReferencable,
	NotAValue,
	NotAVariable,
	NotInvocable,
	NotAnOption,
	NotAPageElement,
	StaticInstanceMismatch,
	OverloadResolutionFailure,
	Ambiguous,
	MemberGroup,
	Viable
}
