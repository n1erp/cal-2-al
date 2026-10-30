using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseWithStatement : Operation, IWithOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Value != null)
			{
				yield return Value;
			}
			if (Body != null)
			{
				yield return Body;
			}
		}
	}

	public abstract IOperation Body { get; }

	public abstract IOperation Value { get; }

	protected BaseWithStatement(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.None, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitWith(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitWith(this, argument);
	}
}
