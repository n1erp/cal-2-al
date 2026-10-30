using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface ICaseStatement : IOperation
{
	IOperation Value { get; }

	ImmutableArray<ICaseLineOperation> CaseLines { get; }

	IOperation ElseStatement { get; }
}
