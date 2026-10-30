using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseRelationalCaseClause : CaseClause, IRelationalCaseClauseOperation, ICaseClauseOperation, IOperation
{
	public sealed override ILabelSymbol Label => null;

	public BinaryOperatorKind Relation { get; }

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

	public BaseRelationalCaseClause(BinaryOperatorKind relation, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(CaseKind.Relational, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Relation = relation;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitRelationalCaseClause(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitRelationalCaseClause(this, argument);
	}
}
