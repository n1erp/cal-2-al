using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class LookupResultKindExtensions
{
	public static CandidateReason ToCandidateReason(this LookupResultKind resultKind)
	{
		switch (resultKind)
		{
		case LookupResultKind.Empty:
			return CandidateReason.None;
		case LookupResultKind.NotATypeOrNamespace:
			return CandidateReason.NotATypeOrNamespace;
		case LookupResultKind.NotAnAttributeType:
			return CandidateReason.NotAnAttributeType;
		case LookupResultKind.Inaccessible:
			return CandidateReason.Inaccessible;
		case LookupResultKind.NotCreatable:
			return CandidateReason.NotCreatable;
		case LookupResultKind.NotReferencable:
			return CandidateReason.NotReferencable;
		case LookupResultKind.NotAValue:
			return CandidateReason.NotAValue;
		case LookupResultKind.NotAVariable:
			return CandidateReason.NotAVariable;
		case LookupResultKind.NotInvocable:
			return CandidateReason.NotInvocable;
		case LookupResultKind.StaticInstanceMismatch:
			return CandidateReason.StaticInstanceMismatch;
		case LookupResultKind.OverloadResolutionFailure:
			return CandidateReason.OverloadResolutionFailure;
		case LookupResultKind.Ambiguous:
			return CandidateReason.Ambiguous;
		case LookupResultKind.MemberGroup:
			return CandidateReason.MemberGroup;
		case LookupResultKind.NotAnOption:
			return CandidateReason.NotAnOption;
		case LookupResultKind.NotAPageElement:
			return CandidateReason.NotAPageElement;
		case LookupResultKind.Viable:
			DebugAssertHelper.Assert(condition: false, "Should not call this on LookupResultKind.Viable");
			return CandidateReason.None;
		default:
			throw new InvalidOperationException(resultKind.ToString());
		}
	}

	public static LookupResultKind WorseResultKind(this LookupResultKind resultKind1, LookupResultKind resultKind2)
	{
		if (resultKind1 == LookupResultKind.Empty)
		{
			return resultKind2;
		}
		if (resultKind2 == LookupResultKind.Empty)
		{
			return resultKind1;
		}
		if ((int)resultKind1 < (int)resultKind2)
		{
			return resultKind1;
		}
		return resultKind2;
	}
}
