using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazySingleValueCaseClause : BaseSingleValueCaseClause, ISingleValueCaseClauseOperation, ICaseClauseOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyValue;

	public override IOperation Value => Operation.SetParentOperation(_lazyValue.Value, this);

	public LazySingleValueCaseClause(ILabelSymbol label, Lazy<IOperation> value, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(label, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyValue = value ?? throw new ArgumentNullException("value");
	}
}
