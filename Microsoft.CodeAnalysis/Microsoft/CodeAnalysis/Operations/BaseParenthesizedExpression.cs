using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseParenthesizedExpression : Operation, IParenthesizedOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Operand != null)
			{
				yield return Operand;
			}
		}
	}

	public abstract IOperation Operand { get; }

	protected BaseParenthesizedExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Parenthesized, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitParenthesized(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitParenthesized(this, argument);
	}
}
