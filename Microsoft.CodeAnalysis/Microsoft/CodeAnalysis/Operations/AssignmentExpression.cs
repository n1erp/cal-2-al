using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class AssignmentExpression : Operation, IAssignmentOperation, IOperation
{
	public sealed override IEnumerable<IOperation> Children
	{
		get
		{
			if (Target != null)
			{
				yield return Target;
			}
			if (Value != null)
			{
				yield return Value;
			}
		}
	}

	public abstract IOperation Target { get; }

	public abstract IOperation Value { get; }

	protected AssignmentExpression(OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}
}
