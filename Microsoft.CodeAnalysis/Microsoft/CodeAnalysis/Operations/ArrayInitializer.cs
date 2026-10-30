using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ArrayInitializer : BaseArrayInitializer, IArrayInitializerOperation, IOperation
{
	public override ImmutableArray<IOperation> ElementValues { get; }

	public ArrayInitializer(ImmutableArray<IOperation> elementValues, SemanticModel semanticModel, SyntaxNode syntax, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, constantValue, isImplicit)
	{
		ElementValues = Operation.SetParentOperation(elementValues, this);
	}
}
