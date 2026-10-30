using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseInterpolatedStringExpression : Operation, IInterpolatedStringOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			foreach (IInterpolatedStringContentOperation part in Parts)
			{
				if (part != null)
				{
					yield return part;
				}
			}
		}
	}

	public abstract ImmutableArray<IInterpolatedStringContentOperation> Parts { get; }

	protected BaseInterpolatedStringExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.InterpolatedString, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInterpolatedString(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitInterpolatedString(this, argument);
	}
}
