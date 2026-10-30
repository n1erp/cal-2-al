using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseTryStatement : Operation, ITryOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Body != null)
			{
				yield return Body;
			}
			foreach (ICatchClauseOperation @catch in Catches)
			{
				if (@catch != null)
				{
					yield return @catch;
				}
			}
			if (Finally != null)
			{
				yield return Finally;
			}
		}
	}

	public ILabelSymbol ExitLabel { get; }

	public abstract IBlockOperation Body { get; }

	public abstract ImmutableArray<ICatchClauseOperation> Catches { get; }

	public abstract IBlockOperation Finally { get; }

	protected BaseTryStatement(ILabelSymbol exitLabel, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Try, semanticModel, syntax, type, constantValue, isImplicit)
	{
		ExitLabel = exitLabel;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTry(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTry(this, argument);
	}
}
