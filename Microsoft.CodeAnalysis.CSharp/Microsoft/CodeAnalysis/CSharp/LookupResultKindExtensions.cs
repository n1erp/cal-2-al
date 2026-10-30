using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class LookupResultKindExtensions
{
	public static CandidateReason ToCandidateReason(this LookupResultKind resultKind)
	{
		return resultKind switch
		{
			LookupResultKind.Empty => CandidateReason.None, 
			LookupResultKind.NotATypeOrNamespace => CandidateReason.NotATypeOrNamespace, 
			LookupResultKind.NotAnAttributeType => CandidateReason.NotAnAttributeType, 
			LookupResultKind.WrongArity => CandidateReason.WrongArity, 
			LookupResultKind.Inaccessible => CandidateReason.Inaccessible, 
			LookupResultKind.NotCreatable => CandidateReason.NotCreatable, 
			LookupResultKind.NotReferencable => CandidateReason.NotReferencable, 
			LookupResultKind.NotAValue => CandidateReason.NotAValue, 
			LookupResultKind.NotAVariable => CandidateReason.NotAVariable, 
			LookupResultKind.NotInvocable => CandidateReason.NotInvocable, 
			LookupResultKind.StaticInstanceMismatch => CandidateReason.StaticInstanceMismatch, 
			LookupResultKind.OverloadResolutionFailure => CandidateReason.OverloadResolutionFailure, 
			LookupResultKind.Ambiguous => CandidateReason.Ambiguous, 
			LookupResultKind.MemberGroup => CandidateReason.MemberGroup, 
			LookupResultKind.Viable => CandidateReason.None, 
			_ => throw ExceptionUtilities.UnexpectedValue(resultKind), 
		};
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
