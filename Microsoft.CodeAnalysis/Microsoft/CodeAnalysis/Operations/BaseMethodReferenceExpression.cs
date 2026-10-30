using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseMethodReferenceExpression : MemberReferenceExpression, IMethodReferenceOperation, IMemberReferenceOperation, IOperation
{
	public IMethodSymbol Method => (IMethodSymbol)Member;

	public bool IsVirtual { get; }

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

	public BaseMethodReferenceExpression(IMethodSymbol method, bool isVirtual, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(method, OperationKind.MethodReference, semanticModel, syntax, type, constantValue, isImplicit)
	{
		IsVirtual = isVirtual;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitMethodReference(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitMethodReference(this, argument);
	}
}
