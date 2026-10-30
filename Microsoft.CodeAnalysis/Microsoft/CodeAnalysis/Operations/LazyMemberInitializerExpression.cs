using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyMemberInitializerExpression : BaseMemberInitializerExpression, IMemberInitializerOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyInitializedMember;

	private readonly Lazy<IObjectOrCollectionInitializerOperation> _lazyInitializer;

	public override IOperation InitializedMember => Operation.SetParentOperation(_lazyInitializedMember.Value, this);

	public override IObjectOrCollectionInitializerOperation Initializer => Operation.SetParentOperation(_lazyInitializer.Value, this);

	public LazyMemberInitializerExpression(Lazy<IOperation> initializedMember, Lazy<IObjectOrCollectionInitializerOperation> initializer, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyInitializedMember = initializedMember ?? throw new ArgumentNullException("initializedMember");
		_lazyInitializer = initializer ?? throw new ArgumentNullException("initializer");
	}
}
