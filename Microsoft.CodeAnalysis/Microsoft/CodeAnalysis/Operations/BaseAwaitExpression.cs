using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseAwaitExpression : Operation, IAwaitOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Operation != null)
			{
				yield return Operation;
			}
		}
	}

	public abstract IOperation Operation { get; }

	protected BaseAwaitExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Await, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitAwait(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitAwait(this, argument);
	}
}
