using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseFieldReferenceExpression : MemberReferenceExpression, IFieldReferenceOperation, IMemberReferenceOperation, IOperation
{
	public IFieldSymbol Field => (IFieldSymbol)Member;

	public bool IsDeclaration { get; }

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

	public BaseFieldReferenceExpression(IFieldSymbol field, bool isDeclaration, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(field, OperationKind.FieldReference, semanticModel, syntax, type, constantValue, isImplicit)
	{
		IsDeclaration = isDeclaration;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitFieldReference(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitFieldReference(this, argument);
	}
}
