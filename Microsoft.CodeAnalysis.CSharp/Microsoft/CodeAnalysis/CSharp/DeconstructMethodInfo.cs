using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct DeconstructMethodInfo
{
	internal readonly BoundExpression Invocation;

	internal readonly BoundDeconstructValuePlaceholder InputPlaceholder;

	internal readonly ImmutableArray<BoundDeconstructValuePlaceholder> OutputPlaceholders;

	internal bool IsDefault => Invocation == null;

	internal DeconstructMethodInfo(BoundExpression invocation, BoundDeconstructValuePlaceholder inputPlaceholder, ImmutableArray<BoundDeconstructValuePlaceholder> outputPlaceholders)
	{
		Invocation = invocation;
		InputPlaceholder = inputPlaceholder;
		OutputPlaceholders = outputPlaceholders;
	}
}
