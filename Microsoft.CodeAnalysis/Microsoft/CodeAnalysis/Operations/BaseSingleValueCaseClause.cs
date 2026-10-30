using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseSingleValueCaseClause : CaseClauseWithLabel, ISingleValueCaseClauseOperation, ICaseClauseOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Value != null)
			{
				yield return Value;
			}
		}
	}

	public abstract IOperation Value { get; }

	public BaseSingleValueCaseClause(ILabelSymbol label, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(label, CaseKind.SingleValue, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitSingleValueCaseClause(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitSingleValueCaseClause(this, argument);
	}
}
