using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseConditionalAccessExpression : Operation, IConditionalAccessOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Operation != null)
			{
				yield return Operation;
			}
			if (WhenNotNull != null)
			{
				yield return WhenNotNull;
			}
		}
	}

	public abstract IOperation Operation { get; }

	public abstract IOperation WhenNotNull { get; }

	protected BaseConditionalAccessExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.ConditionalAccess, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitConditionalAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitConditionalAccess(this, argument);
	}
}
