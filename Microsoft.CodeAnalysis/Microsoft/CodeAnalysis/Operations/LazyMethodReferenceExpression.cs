using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyMethodReferenceExpression : BaseMethodReferenceExpression, IMethodReferenceOperation, IMemberReferenceOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyInstance;

	public override IOperation Instance => Operation.SetParentOperation(_lazyInstance.Value, this);

	public LazyMethodReferenceExpression(IMethodSymbol method, bool isVirtual, Lazy<IOperation> instance, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(method, isVirtual, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyInstance = instance ?? throw new ArgumentNullException("instance");
	}
}
