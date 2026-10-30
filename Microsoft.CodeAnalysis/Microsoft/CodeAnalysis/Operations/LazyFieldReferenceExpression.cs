using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyFieldReferenceExpression : BaseFieldReferenceExpression, IFieldReferenceOperation, IMemberReferenceOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyInstance;

	public override IOperation Instance => Operation.SetParentOperation(_lazyInstance.Value, this);

	public LazyFieldReferenceExpression(IFieldSymbol field, bool isDeclaration, Lazy<IOperation> instance, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(field, isDeclaration, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyInstance = instance ?? throw new ArgumentNullException("instance");
	}
}
