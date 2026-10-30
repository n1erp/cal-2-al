using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyDeconstructionAssignmentExpression : BaseDeconstructionAssignmentExpression, IDeconstructionAssignmentOperation, IAssignmentOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyTarget;

	private readonly Lazy<IOperation> _lazyValue;

	public override IOperation Target => Operation.SetParentOperation(_lazyTarget.Value, this);

	public override IOperation Value => Operation.SetParentOperation(_lazyValue.Value, this);

	public LazyDeconstructionAssignmentExpression(Lazy<IOperation> target, Lazy<IOperation> value, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyTarget = target ?? throw new ArgumentNullException("target");
		_lazyValue = value ?? throw new ArgumentNullException("value");
	}
}
