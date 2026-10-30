using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyDynamicObjectCreationExpression : BaseDynamicObjectCreationExpression, IDynamicObjectCreationOperation, IOperation
{
	private readonly Lazy<IObjectOrCollectionInitializerOperation> _lazyInitializer;

	private readonly Lazy<ImmutableArray<IOperation>> _lazyArguments;

	public override ImmutableArray<IOperation> Arguments => Operation.SetParentOperation(_lazyArguments.Value, this);

	public override IObjectOrCollectionInitializerOperation Initializer => Operation.SetParentOperation(_lazyInitializer.Value, this);

	public LazyDynamicObjectCreationExpression(Lazy<ImmutableArray<IOperation>> arguments, ImmutableArray<string> argumentNames, ImmutableArray<RefKind> argumentRefKinds, Lazy<IObjectOrCollectionInitializerOperation> initializer, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(argumentNames, argumentRefKinds, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyArguments = arguments ?? throw new ArgumentNullException("arguments");
		_lazyInitializer = initializer ?? throw new ArgumentNullException("initializer");
	}
}
