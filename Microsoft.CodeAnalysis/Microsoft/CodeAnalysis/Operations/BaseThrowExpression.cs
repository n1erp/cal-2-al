using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseThrowExpression : Operation, IThrowOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Exception != null)
			{
				yield return Exception;
			}
		}
	}

	public abstract IOperation Exception { get; }

	protected BaseThrowExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Throw, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitThrow(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitThrow(this, argument);
	}
}
