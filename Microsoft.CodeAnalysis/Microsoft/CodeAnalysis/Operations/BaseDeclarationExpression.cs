using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseDeclarationExpression : Operation, IDeclarationExpressionOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Expression != null)
			{
				yield return Expression;
			}
		}
	}

	public abstract IOperation Expression { get; }

	public BaseDeclarationExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.DeclarationExpression, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitDeclarationExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitDeclarationExpression(this, argument);
	}
}
