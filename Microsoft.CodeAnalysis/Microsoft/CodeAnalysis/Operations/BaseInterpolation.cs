using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseInterpolation : Operation, IInterpolationOperation, IInterpolatedStringContentOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Expression != null)
			{
				yield return Expression;
			}
			if (Alignment != null)
			{
				yield return Alignment;
			}
			if (FormatString != null)
			{
				yield return FormatString;
			}
		}
	}

	public abstract IOperation Expression { get; }

	public abstract IOperation Alignment { get; }

	public abstract IOperation FormatString { get; }

	protected BaseInterpolation(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Interpolation, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInterpolation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitInterpolation(this, argument);
	}
}
