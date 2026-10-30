using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyInterpolatedStringExpression : BaseInterpolatedStringExpression, IInterpolatedStringOperation, IOperation
{
	private readonly Lazy<ImmutableArray<IInterpolatedStringContentOperation>> _lazyParts;

	public override ImmutableArray<IInterpolatedStringContentOperation> Parts => Operation.SetParentOperation(_lazyParts.Value, this);

	public LazyInterpolatedStringExpression(Lazy<ImmutableArray<IInterpolatedStringContentOperation>> parts, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyParts = parts;
	}
}
