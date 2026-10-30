using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class EvaluatedConstant
{
	public readonly ConstantValue Value;

	public readonly ImmutableArray<Diagnostic> Diagnostics;

	public EvaluatedConstant(ConstantValue value, ImmutableArray<Diagnostic> diagnostics)
	{
		Value = value;
		Diagnostics = diagnostics.NullToEmpty();
	}
}
