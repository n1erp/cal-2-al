using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseNameOfExpression : Operation, INameOfOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Argument != null)
			{
				yield return Argument;
			}
		}
	}

	public abstract IOperation Argument { get; }

	protected BaseNameOfExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.NameOf, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitNameOf(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitNameOf(this, argument);
	}
}
