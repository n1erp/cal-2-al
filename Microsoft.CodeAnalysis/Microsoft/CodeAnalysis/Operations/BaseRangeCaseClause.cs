using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseRangeCaseClause : CaseClause, IRangeCaseClauseOperation, ICaseClauseOperation, IOperation
{
	public sealed override ILabelSymbol Label => null;

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (MinimumValue != null)
			{
				yield return MinimumValue;
			}
			if (MaximumValue != null)
			{
				yield return MaximumValue;
			}
		}
	}

	public abstract IOperation MinimumValue { get; }

	public abstract IOperation MaximumValue { get; }

	public BaseRangeCaseClause(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(CaseKind.Range, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitRangeCaseClause(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitRangeCaseClause(this, argument);
	}
}
