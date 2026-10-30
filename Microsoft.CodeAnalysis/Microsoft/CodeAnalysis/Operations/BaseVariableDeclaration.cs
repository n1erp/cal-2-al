using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseVariableDeclaration : Operation, IVariableDeclarationOperation, IOperation
{
	public abstract ImmutableArray<IVariableDeclaratorOperation> Declarators { get; }

	public abstract IVariableInitializerOperation Initializer { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			foreach (IVariableDeclaratorOperation declarator in Declarators)
			{
				yield return declarator;
			}
			if (Initializer != null)
			{
				yield return Initializer;
			}
		}
	}

	protected BaseVariableDeclaration(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.VariableDeclaration, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitVariableDeclaration(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitVariableDeclaration(this, argument);
	}
}
