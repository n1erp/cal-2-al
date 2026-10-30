using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseAnonymousFunctionExpression : Operation, IAnonymousFunctionOperation, IOperation
{
	public IMethodSymbol Symbol { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Body != null)
			{
				yield return Body;
			}
		}
	}

	public abstract IBlockOperation Body { get; }

	protected BaseAnonymousFunctionExpression(IMethodSymbol symbol, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.AnonymousFunction, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Symbol = symbol;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitAnonymousFunction(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitAnonymousFunction(this, argument);
	}
}
