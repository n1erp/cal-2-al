using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseVariableDeclarationGroupOperation : Operation, IVariableDeclarationGroupOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			foreach (IVariableDeclarationOperation declaration in Declarations)
			{
				if (declaration != null)
				{
					yield return declaration;
				}
			}
		}
	}

	public abstract ImmutableArray<IVariableDeclarationOperation> Declarations { get; }

	protected BaseVariableDeclarationGroupOperation(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.VariableDeclarationGroup, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitVariableDeclarationGroup(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitVariableDeclarationGroup(this, argument);
	}
}
