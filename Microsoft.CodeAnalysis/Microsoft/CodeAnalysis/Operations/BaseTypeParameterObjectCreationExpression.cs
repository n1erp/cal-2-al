using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseTypeParameterObjectCreationExpression : Operation, ITypeParameterObjectCreationOperation, IOperation
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

	public BaseTypeParameterObjectCreationExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.TypeParameterObjectCreation, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTypeParameterObjectCreation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTypeParameterObjectCreation(this, argument);
	}
}
