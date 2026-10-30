using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyPatternCaseClause : BasePatternCaseClause, IPatternCaseClauseOperation, ICaseClauseOperation, IOperation
{
	private readonly Lazy<IPatternOperation> _lazyPattern;

	private readonly Lazy<IOperation> _lazyGuardExpression;

	public override IPatternOperation Pattern => Operation.SetParentOperation(_lazyPattern.Value, this);

	public override IOperation Guard => Operation.SetParentOperation(_lazyGuardExpression.Value, this);

	public LazyPatternCaseClause(ILabelSymbol label, Lazy<IPatternOperation> lazyPattern, Lazy<IOperation> lazyGuardExpression, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(label, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyPattern = lazyPattern ?? throw new ArgumentNullException("lazyPattern");
		_lazyGuardExpression = lazyGuardExpression ?? throw new ArgumentNullException("lazyGuardExpression");
	}
}
