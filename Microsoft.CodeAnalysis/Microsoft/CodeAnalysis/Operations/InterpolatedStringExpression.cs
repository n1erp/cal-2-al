using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class InterpolatedStringExpression : BaseInterpolatedStringExpression, IInterpolatedStringOperation, IOperation
{
	public override ImmutableArray<IInterpolatedStringContentOperation> Parts { get; }

	public InterpolatedStringExpression(ImmutableArray<IInterpolatedStringContentOperation> parts, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Parts = Operation.SetParentOperation(parts, this);
	}
}
