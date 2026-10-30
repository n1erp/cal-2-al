using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyArrayInitializer : BaseArrayInitializer, IArrayInitializerOperation, IOperation
{
	private readonly Lazy<ImmutableArray<IOperation>> _lazyElementValues;

	public override ImmutableArray<IOperation> ElementValues => Operation.SetParentOperation(_lazyElementValues.Value, this);

	public LazyArrayInitializer(Lazy<ImmutableArray<IOperation>> elementValues, SemanticModel semanticModel, SyntaxNode syntax, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, constantValue, isImplicit)
	{
		_lazyElementValues = elementValues;
	}
}
