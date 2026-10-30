using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Emit;

public sealed class EmitDifferenceResult : EmitResult
{
	public EmitBaseline Baseline { get; }

	internal EmitDifferenceResult(bool success, ImmutableArray<Diagnostic> diagnostics, EmitBaseline baseline)
		: base(success, diagnostics)
	{
		Baseline = baseline;
	}
}
