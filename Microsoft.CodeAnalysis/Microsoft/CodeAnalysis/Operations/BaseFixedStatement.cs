using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseFixedStatement : Operation, IFixedOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Variables != null)
			{
				yield return Variables;
			}
			if (Body != null)
			{
				yield return Body;
			}
		}
	}

	public ImmutableArray<ILocalSymbol> Locals { get; }

	public abstract IVariableDeclarationGroupOperation Variables { get; }

	public abstract IOperation Body { get; }

	protected BaseFixedStatement(ImmutableArray<ILocalSymbol> locals, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.None, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Locals = locals;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitFixed(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitFixed(this, argument);
	}
}
