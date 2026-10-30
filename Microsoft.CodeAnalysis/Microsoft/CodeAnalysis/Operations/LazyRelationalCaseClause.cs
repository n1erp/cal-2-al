using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyRelationalCaseClause : BaseRelationalCaseClause, IRelationalCaseClauseOperation, ICaseClauseOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyValue;

	public override IOperation Value => Operation.SetParentOperation(_lazyValue.Value, this);

	public LazyRelationalCaseClause(Lazy<IOperation> value, BinaryOperatorKind relation, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(relation, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyValue = value ?? throw new ArgumentNullException("value");
	}
}
