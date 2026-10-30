using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseLocalFunctionStatement : Operation, ILocalFunctionOperation, IOperation
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
			if (IgnoredBody != null)
			{
				yield return IgnoredBody;
			}
		}
	}

	public abstract IBlockOperation Body { get; }

	public abstract IBlockOperation IgnoredBody { get; }

	protected BaseLocalFunctionStatement(IMethodSymbol symbol, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.LocalFunction, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Symbol = symbol;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitLocalFunction(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitLocalFunction(this, argument);
	}
}
