using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyIsTypeExpression : BaseIsTypeExpression, IIsTypeOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyOperand;

	public override IOperation ValueOperand => Operation.SetParentOperation(_lazyOperand.Value, this);

	public LazyIsTypeExpression(Lazy<IOperation> operand, ITypeSymbol isType, bool isNotTypeExpression, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(isType, isNotTypeExpression, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyOperand = operand ?? throw new ArgumentNullException("operand");
	}
}
