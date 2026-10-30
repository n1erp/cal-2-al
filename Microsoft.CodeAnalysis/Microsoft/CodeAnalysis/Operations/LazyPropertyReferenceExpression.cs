using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyPropertyReferenceExpression : BasePropertyReferenceExpression, IPropertyReferenceOperation, IMemberReferenceOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyInstance;

	private readonly Lazy<ImmutableArray<IArgumentOperation>> _lazyArguments;

	public override IOperation Instance => Operation.SetParentOperation(_lazyInstance.Value, this);

	public override ImmutableArray<IArgumentOperation> Arguments => Operation.SetParentOperation(_lazyArguments.Value, this);

	public LazyPropertyReferenceExpression(IPropertySymbol property, Lazy<IOperation> instance, Lazy<ImmutableArray<IArgumentOperation>> arguments, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(property, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyInstance = instance ?? throw new ArgumentNullException("instance");
		_lazyArguments = arguments ?? throw new ArgumentNullException("arguments");
	}
}
