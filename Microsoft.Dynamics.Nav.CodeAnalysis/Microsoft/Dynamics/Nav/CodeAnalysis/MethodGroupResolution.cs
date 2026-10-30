using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal struct MethodGroupResolution
{
	public readonly MethodGroup MethodGroup;

	public readonly Symbol OtherSymbol;

	public readonly OverloadResolutionResult<MethodSymbol> OverloadResolutionResult;

	public readonly AnalyzedArguments AnalyzedArguments;

	public readonly ImmutableArray<Diagnostic> Diagnostics;

	public readonly LookupResultKind ResultKind;

	public bool IsEmpty
	{
		get
		{
			if (MethodGroup == null)
			{
				return (object)OtherSymbol == null;
			}
			return false;
		}
	}

	public bool HasAnyErrors => Diagnostics.HasAnyErrors();

	public bool HasAnyApplicableMethod
	{
		get
		{
			if (MethodGroup != null && ResultKind == LookupResultKind.Viable)
			{
				if (OverloadResolutionResult != null)
				{
					return OverloadResolutionResult.HasAnyApplicableMember;
				}
				return true;
			}
			return false;
		}
	}

	public MethodGroupResolution(MethodGroup methodGroup, ImmutableArray<Diagnostic> diagnostics)
		: this(methodGroup, null, null, null, methodGroup.ResultKind, diagnostics)
	{
	}

	public MethodGroupResolution(Symbol otherSymbol, LookupResultKind resultKind, ImmutableArray<Diagnostic> diagnostics)
		: this(null, otherSymbol, null, null, resultKind, diagnostics)
	{
	}

	public MethodGroupResolution(MethodGroup methodGroup, Symbol otherSymbol, OverloadResolutionResult<MethodSymbol> overloadResolutionResult, AnalyzedArguments analyzedArguments, LookupResultKind resultKind, ImmutableArray<Diagnostic> diagnostics)
	{
		DebugAssertHelper.Assert(methodGroup == null || methodGroup.Methods.Count > 0);
		DebugAssertHelper.Assert(methodGroup == null || (object)otherSymbol == null);
		DebugAssertHelper.Assert((object)otherSymbol == null || otherSymbol.Kind != SymbolKind.Method);
		DebugAssertHelper.Assert(resultKind != LookupResultKind.Ambiguous);
		DebugAssertHelper.Assert(!diagnostics.IsDefault);
		MethodGroup = methodGroup;
		OtherSymbol = otherSymbol;
		OverloadResolutionResult = overloadResolutionResult;
		AnalyzedArguments = analyzedArguments;
		ResultKind = resultKind;
		Diagnostics = diagnostics;
	}

	public void Free()
	{
		if (MethodGroup != null)
		{
			MethodGroup.Free();
		}
		if (OverloadResolutionResult != null)
		{
			OverloadResolutionResult.Free();
		}
	}
}
