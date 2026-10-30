using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseVariableDeclarator : Operation, IVariableDeclaratorOperation, IOperation
{
	public ILocalSymbol Symbol { get; }

	public abstract IVariableInitializerOperation Initializer { get; }

	public abstract ImmutableArray<IOperation> IgnoredArguments { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			foreach (IOperation ignoredArgument in IgnoredArguments)
			{
				yield return ignoredArgument;
			}
			if (Initializer != null)
			{
				yield return Initializer;
			}
		}
	}

	protected BaseVariableDeclarator(ILocalSymbol symbol, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.VariableDeclarator, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Symbol = symbol;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitVariableDeclarator(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitVariableDeclarator(this, argument);
	}
}
