using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyReturnStatement : BaseReturnStatement, IReturnOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyReturnedValue;

	public override IOperation ReturnedValue => Operation.SetParentOperation(_lazyReturnedValue.Value, this);

	public LazyReturnStatement(OperationKind kind, Lazy<IOperation> returnedValue, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyReturnedValue = returnedValue ?? throw new ArgumentNullException("returnedValue");
	}
}
