using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseTranslatedQueryExpression : Operation, ITranslatedQueryOperation, IOperation
{
	public abstract IOperation Operation { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Operation != null)
			{
				yield return Operation;
			}
		}
	}

	protected BaseTranslatedQueryExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.TranslatedQuery, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTranslatedQuery(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTranslatedQuery(this, argument);
	}
}
