using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseInvocationExpression : Operation, IInvocationOperation, IOperation
{
	public IMethodSymbol TargetMethod { get; }

	public bool IsVirtual { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Instance != null)
			{
				yield return Instance;
			}
			foreach (IArgumentOperation argument in Arguments)
			{
				if (argument != null)
				{
					yield return argument;
				}
			}
		}
	}

	public abstract IOperation Instance { get; }

	public abstract ImmutableArray<IArgumentOperation> Arguments { get; }

	protected BaseInvocationExpression(IMethodSymbol targetMethod, bool isVirtual, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Invocation, semanticModel, syntax, type, constantValue, isImplicit)
	{
		TargetMethod = targetMethod;
		IsVirtual = isVirtual;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInvocation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitInvocation(this, argument);
	}
}
