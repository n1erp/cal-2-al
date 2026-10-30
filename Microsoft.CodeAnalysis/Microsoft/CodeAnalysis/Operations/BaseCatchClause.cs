using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseCatchClause : Operation, ICatchClauseOperation, IOperation
{
	public ITypeSymbol ExceptionType { get; }

	public ImmutableArray<ILocalSymbol> Locals { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (ExceptionDeclarationOrExpression != null)
			{
				yield return ExceptionDeclarationOrExpression;
			}
			if (Filter != null)
			{
				yield return Filter;
			}
			if (Handler != null)
			{
				yield return Handler;
			}
		}
	}

	public abstract IOperation ExceptionDeclarationOrExpression { get; }

	public abstract IOperation Filter { get; }

	public abstract IBlockOperation Handler { get; }

	protected BaseCatchClause(ITypeSymbol exceptionType, ImmutableArray<ILocalSymbol> locals, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.CatchClause, semanticModel, syntax, type, constantValue, isImplicit)
	{
		ExceptionType = exceptionType;
		Locals = locals;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitCatchClause(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitCatchClause(this, argument);
	}
}
