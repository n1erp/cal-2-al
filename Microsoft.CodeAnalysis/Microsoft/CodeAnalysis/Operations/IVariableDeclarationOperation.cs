using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IVariableDeclarationOperation : IOperation
{
	ImmutableArray<IVariableDeclaratorOperation> Declarators { get; }

	IVariableInitializerOperation Initializer { get; }
}
