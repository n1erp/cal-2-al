using System;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct MemberResolutionResult<TMember> where TMember : Symbol
{
	private readonly TMember _member;

	private readonly TMember _leastOverriddenMember;

	private readonly MemberAnalysisResult _result;

	internal bool IsNull => (object)_member == null;

	internal bool IsNotNull => (object)_member != null;

	public TMember Member => _member;

	internal TMember LeastOverriddenMember => _leastOverriddenMember;

	public MemberResolutionKind Resolution => Result.Kind;

	public bool IsValid => Result.IsValid;

	public bool IsApplicable => Result.IsApplicable;

	internal bool HasUseSiteDiagnosticToReport => _result.HasUseSiteDiagnosticToReportFor(_member);

	internal MemberAnalysisResult Result => _result;

	internal MemberResolutionResult(TMember member, TMember leastOverriddenMember, MemberAnalysisResult result)
	{
		_member = member;
		_leastOverriddenMember = leastOverriddenMember;
		_result = result;
	}

	internal MemberResolutionResult<TMember> Worse()
	{
		return new MemberResolutionResult<TMember>(Member, LeastOverriddenMember, MemberAnalysisResult.Worse());
	}

	internal MemberResolutionResult<TMember> Worst()
	{
		return new MemberResolutionResult<TMember>(Member, LeastOverriddenMember, MemberAnalysisResult.Worst());
	}

	public override bool Equals(object obj)
	{
		throw new NotSupportedException();
	}

	public override int GetHashCode()
	{
		throw new NotSupportedException();
	}
}
