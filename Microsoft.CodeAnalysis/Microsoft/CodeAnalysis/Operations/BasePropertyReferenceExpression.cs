using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BasePropertyReferenceExpression : MemberReferenceExpression, IPropertyReferenceOperation, IMemberReferenceOperation, IOperation
{
	public IPropertySymbol Property => (IPropertySymbol)Member;

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

	public abstract ImmutableArray<IArgumentOperation> Arguments { get; }

	protected BasePropertyReferenceExpression(IPropertySymbol property, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(property, OperationKind.PropertyReference, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitPropertyReference(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitPropertyReference(this, argument);
	}
}
