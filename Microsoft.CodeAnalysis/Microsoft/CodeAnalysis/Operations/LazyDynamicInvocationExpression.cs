using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyDynamicInvocationExpression : BaseDynamicInvocationExpression, IDynamicInvocationOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyOperation;

	private readonly Lazy<ImmutableArray<IOperation>> _lazyArguments;

	public override IOperation Operation => Microsoft.CodeAnalysis.Operation.SetParentOperation(_lazyOperation.Value, this);

	public override ImmutableArray<IOperation> Arguments => Microsoft.CodeAnalysis.Operation.SetParentOperation(_lazyArguments.Value, this);

	public LazyDynamicInvocationExpression(Lazy<IOperation> operation, Lazy<ImmutableArray<IOperation>> arguments, ImmutableArray<string> argumentNames, ImmutableArray<RefKind> argumentRefKinds, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(argumentNames, argumentRefKinds, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyOperation = operation ?? throw new ArgumentNullException("operation");
		_lazyArguments = arguments ?? throw new ArgumentNullException("arguments");
	}
}
