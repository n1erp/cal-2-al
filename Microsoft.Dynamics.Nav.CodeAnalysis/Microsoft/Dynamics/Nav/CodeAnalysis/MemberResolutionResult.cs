using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[SuppressMessage("Performance", "RS0008", Justification = "Equality not actually implemented")]
[DebuggerDisplay("{Result.GetDebuggerDisplay()} {Member}")]
internal struct MemberResolutionResult<TMember> where TMember : Symbol
{
	private readonly TMember member;

	private readonly MemberAnalysisResult result;

	internal bool IsNull => (object)member == null;

	internal bool IsNotNull => (object)member != null;

	public TMember Member => member;

	public MemberResolutionKind Resolution => Result.Kind;

	public bool IsValid => Result.IsValid;

	public bool IsApplicable => Result.IsApplicable;

	internal bool HasUseSiteDiagnosticToReport => result.HasUseSiteDiagnosticToReportFor(member);

	internal MemberAnalysisResult Result => result;

	internal MemberResolutionResult(TMember member, MemberAnalysisResult result)
	{
		this.member = member;
		this.result = result;
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
