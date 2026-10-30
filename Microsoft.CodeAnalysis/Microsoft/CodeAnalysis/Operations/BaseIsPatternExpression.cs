using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseIsPatternExpression : Operation, IIsPatternOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Value != null)
			{
				yield return Value;
			}
			if (Pattern != null)
			{
				yield return Pattern;
			}
		}
	}

	public abstract IOperation Value { get; }

	public abstract IPatternOperation Pattern { get; }

	protected BaseIsPatternExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.IsPattern, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitIsPattern(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitIsPattern(this, argument);
	}
}
