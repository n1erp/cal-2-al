using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyParenthesizedExpression : BaseParenthesizedExpression, IParenthesizedOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyOperand;

	public override IOperation Operand => Operation.SetParentOperation(_lazyOperand.Value, this);

	public LazyParenthesizedExpression(Lazy<IOperation> operand, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyOperand = operand ?? throw new ArgumentNullException("operand");
	}
}
