using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseArrayInitializer : Operation, IArrayInitializerOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			foreach (IOperation elementValue in ElementValues)
			{
				if (elementValue != null)
				{
					yield return elementValue;
				}
			}
		}
	}

	public abstract ImmutableArray<IOperation> ElementValues { get; }

	protected BaseArrayInitializer(SemanticModel semanticModel, SyntaxNode syntax, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.ArrayInitializer, semanticModel, syntax, null, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitArrayInitializer(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitArrayInitializer(this, argument);
	}
}
