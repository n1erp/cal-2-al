using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseInterpolatedStringText : Operation, IInterpolatedStringTextOperation, IInterpolatedStringContentOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Text != null)
			{
				yield return Text;
			}
		}
	}

	public abstract IOperation Text { get; }

	protected BaseInterpolatedStringText(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.InterpolatedStringText, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInterpolatedStringText(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitInterpolatedStringText(this, argument);
	}
}
