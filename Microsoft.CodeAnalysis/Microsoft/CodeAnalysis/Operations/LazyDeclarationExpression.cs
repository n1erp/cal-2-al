using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyDeclarationExpression : BaseDeclarationExpression, IDeclarationExpressionOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyExpression;

	public override IOperation Expression => Operation.SetParentOperation(_lazyExpression.Value, this);

	public LazyDeclarationExpression(Lazy<IOperation> expression, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyExpression = expression ?? throw new ArgumentNullException("expression");
	}
}
