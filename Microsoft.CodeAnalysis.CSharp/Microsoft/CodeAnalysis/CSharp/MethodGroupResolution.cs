using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

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

	public bool IsExtensionMethodGroup
	{
		get
		{
			if (MethodGroup != null)
			{
				return MethodGroup.IsExtensionMethodGroup;
			}
			return false;
		}
	}

	public bool IsLocalFunctionInvocation
	{
		get
		{
			MethodGroup methodGroup = MethodGroup;
			if (methodGroup != null && methodGroup.Methods.Count == 1)
			{
				return MethodGroup.Methods[0].MethodKind == MethodKind.LocalFunction;
			}
			return false;
		}
	}

	public MethodGroupResolution(MethodGroup methodGroup, ImmutableArray<Diagnostic> diagnostics)
		: this(methodGroup, null, null, null, methodGroup.ResultKind, diagnostics)
	{
	}

	public MethodGroupResolution(MethodGroup methodGroup, OverloadResolutionResult<MethodSymbol> overloadResolutionResult, AnalyzedArguments analyzedArguments, ImmutableArray<Diagnostic> diagnostics)
		: this(methodGroup, null, overloadResolutionResult, analyzedArguments, methodGroup.ResultKind, diagnostics)
	{
	}

	public MethodGroupResolution(Symbol otherSymbol, LookupResultKind resultKind, ImmutableArray<Diagnostic> diagnostics)
		: this(null, otherSymbol, null, null, resultKind, diagnostics)
	{
	}

	public MethodGroupResolution(MethodGroup methodGroup, Symbol otherSymbol, OverloadResolutionResult<MethodSymbol> overloadResolutionResult, AnalyzedArguments analyzedArguments, LookupResultKind resultKind, ImmutableArray<Diagnostic> diagnostics)
	{
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
			if (MethodGroup.IsExtensionMethodGroup && AnalyzedArguments != null)
			{
				AnalyzedArguments.Free();
			}
			MethodGroup.Free();
		}
		if (OverloadResolutionResult != null)
		{
			OverloadResolutionResult.Free();
		}
	}
}
