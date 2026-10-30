using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyUsingStatement : BaseUsingStatement, IUsingOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyResources;

	private readonly Lazy<IOperation> _lazyBody;

	public override IOperation Resources => Operation.SetParentOperation(_lazyResources.Value, this);

	public override IOperation Body => Operation.SetParentOperation(_lazyBody.Value, this);

	public LazyUsingStatement(Lazy<IOperation> resources, Lazy<IOperation> body, ImmutableArray<ILocalSymbol> locals, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyResources = resources ?? throw new ArgumentNullException("resources");
		_lazyBody = body ?? throw new ArgumentNullException("body");
	}
}
