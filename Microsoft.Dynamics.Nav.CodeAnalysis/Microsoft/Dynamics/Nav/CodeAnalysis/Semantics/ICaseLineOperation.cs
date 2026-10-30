using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface ICaseLineOperation : IOperation
{
	ImmutableArray<IOperation> Expressions { get; }

	IOperation Statement { get; }
}
