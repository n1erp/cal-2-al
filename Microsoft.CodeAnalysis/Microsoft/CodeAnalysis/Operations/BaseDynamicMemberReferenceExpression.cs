using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseDynamicMemberReferenceExpression : Operation, IDynamicMemberReferenceOperation, IOperation
{
	public string MemberName { get; }

	public ImmutableArray<ITypeSymbol> TypeArguments { get; }

	public ITypeSymbol ContainingType { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Instance != null)
			{
				yield return Instance;
			}
		}
	}

	public abstract IOperation Instance { get; }

	protected BaseDynamicMemberReferenceExpression(string memberName, ImmutableArray<ITypeSymbol> typeArguments, ITypeSymbol containingType, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.DynamicMemberReference, semanticModel, syntax, type, constantValue, isImplicit)
	{
		MemberName = memberName;
		TypeArguments = typeArguments;
		ContainingType = containingType;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitDynamicMemberReference(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitDynamicMemberReference(this, argument);
	}
}
