namespace Microsoft.CodeAnalysis.CSharp;

internal enum MemberResolutionKind : byte
{
	None,
	ApplicableInNormalForm,
	ApplicableInExpandedForm,
	InaccessibleTypeArgument,
	NoCorrespondingParameter,
	NoCorrespondingNamedParameter,
	DuplicateNamedArgument,
	RequiredParameterMissing,
	NameUsedForPositional,
	BadNonTrailingNamedArgument,
	UseSiteError,
	UnsupportedMetadata,
	BadArgumentConversion,
	TypeInferenceFailed,
	TypeInferenceExtensionInstanceArgument,
	ConstructedParameterFailedConstraintCheck,
	ConstraintFailure,
	StaticInstanceMismatch,
	WrongRefKind,
	WrongReturnType,
	LessDerived,
	Worse,
	Worst
}
