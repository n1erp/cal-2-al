using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseAddressOfExpression : Operation, IAddressOfOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Reference != null)
			{
				yield return Reference;
			}
		}
	}

	public abstract IOperation Reference { get; }

	protected BaseAddressOfExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.AddressOf, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitAddressOf(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitAddressOf(this, argument);
	}
}
