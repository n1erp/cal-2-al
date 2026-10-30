using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseLabeledStatement : Operation, ILabeledOperation, IOperation
{
	public ILabelSymbol Label { get; }

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

	public abstract IOperation Operation { get; }

	protected BaseLabeledStatement(ILabelSymbol label, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Labeled, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Label = label;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitLabeled(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitLabeled(this, argument);
	}
}
