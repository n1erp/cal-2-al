using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseNoPiaObjectCreationOperation : Operation, INoPiaObjectCreationOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Initializer != null)
			{
				yield return Initializer;
			}
		}
	}

	public abstract IObjectOrCollectionInitializerOperation Initializer { get; }

	public BaseNoPiaObjectCreationOperation(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.None, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitNoPiaObjectCreation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitNoPiaObjectCreation(this, argument);
	}
}
