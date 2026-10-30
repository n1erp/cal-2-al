using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundCollectionInitializerExpression : BoundObjectInitializerExpressionBase
{
	public BoundCollectionInitializerExpression(SyntaxNode syntax, ImmutableArray<BoundExpression> initializers, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.CollectionInitializerExpression, syntax, initializers, type, hasErrors || initializers.HasErrors())
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCollectionInitializerExpression(this);
	}

	public BoundCollectionInitializerExpression Update(ImmutableArray<BoundExpression> initializers, TypeSymbol type)
	{
		if (initializers != Initializers || type != Type)
		{
			return new BoundCollectionInitializerExpression(Syntax, initializers, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
