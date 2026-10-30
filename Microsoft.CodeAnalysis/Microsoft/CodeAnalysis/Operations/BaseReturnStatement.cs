using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseReturnStatement : Operation, IReturnOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (ReturnedValue != null)
			{
				yield return ReturnedValue;
			}
		}
	}

	public abstract IOperation ReturnedValue { get; }

	protected BaseReturnStatement(OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitReturn(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitReturn(this, argument);
	}
}
