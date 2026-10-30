namespace Microsoft.CodeAnalysis.CSharp;

internal enum LookupResultKind : byte
{
	Empty,
	NotATypeOrNamespace,
	NotAnAttributeType,
	WrongArity,
	NotCreatable,
	Inaccessible,
	NotReferencable,
	NotAValue,
	NotAVariable,
	NotInvocable,
	NotLabel,
	StaticInstanceMismatch,
	OverloadResolutionFailure,
	Ambiguous,
	MemberGroup,
	Viable
}
