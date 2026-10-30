using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseMemberInitializerExpression : Operation, IMemberInitializerOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (InitializedMember != null)
			{
				yield return InitializedMember;
			}
			if (Initializer != null)
			{
				yield return Initializer;
			}
		}
	}

	public abstract IOperation InitializedMember { get; }

	public abstract IObjectOrCollectionInitializerOperation Initializer { get; }

	protected BaseMemberInitializerExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.MemberInitializer, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitMemberInitializer(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitMemberInitializer(this, argument);
	}
}
