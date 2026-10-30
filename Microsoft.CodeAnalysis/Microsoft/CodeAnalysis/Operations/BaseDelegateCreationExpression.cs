using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseDelegateCreationExpression : Operation, IDelegateCreationOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Target != null)
			{
				yield return Target;
			}
		}
	}

	public abstract IOperation Target { get; }

	public BaseDelegateCreationExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.DelegateCreation, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitDelegateCreation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitDelegateCreation(this, argument);
	}
}
