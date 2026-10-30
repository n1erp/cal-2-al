using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class PropertyReferenceExpression : BasePropertyReferenceExpression, IPropertyReferenceOperation, IMemberReferenceOperation, IOperation
{
	public override IOperation Instance { get; }

	public override ImmutableArray<IArgumentOperation> Arguments { get; }

	public PropertyReferenceExpression(IPropertySymbol property, IOperation instance, ImmutableArray<IArgumentOperation> arguments, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(property, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Instance = Operation.SetParentOperation(instance, this);
		Arguments = Operation.SetParentOperation(arguments, this);
	}
}
