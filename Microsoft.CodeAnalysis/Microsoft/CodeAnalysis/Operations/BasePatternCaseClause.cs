using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BasePatternCaseClause : CaseClauseWithLabel, IPatternCaseClauseOperation, ICaseClauseOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Pattern != null)
			{
				yield return Pattern;
			}
			if (Guard != null)
			{
				yield return Guard;
			}
		}
	}

	public abstract IPatternOperation Pattern { get; }

	public abstract IOperation Guard { get; }

	protected BasePatternCaseClause(ILabelSymbol label, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(label, CaseKind.Pattern, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitPatternCaseClause(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitPatternCaseClause(this, argument);
	}
}
