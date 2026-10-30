using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseConstantPattern : Operation, IConstantPatternOperation, IPatternOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Value != null)
			{
				yield return Value;
			}
		}
	}

	public abstract IOperation Value { get; }

	protected BaseConstantPattern(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.ConstantPattern, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitConstantPattern(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitConstantPattern(this, argument);
	}
}
