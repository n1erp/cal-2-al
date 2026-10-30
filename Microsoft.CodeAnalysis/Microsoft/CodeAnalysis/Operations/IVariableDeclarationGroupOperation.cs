using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IVariableDeclarationGroupOperation : IOperation
{
	ImmutableArray<IVariableDeclarationOperation> Declarations { get; }
}
