using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundObjectInitializerExpression : BoundObjectInitializerExpressionBase
{
	public BoundObjectInitializerExpression(SyntaxNode syntax, ImmutableArray<BoundExpression> initializers, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ObjectInitializerExpression, syntax, initializers, type, hasErrors || initializers.HasErrors())
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitObjectInitializerExpression(this);
	}

	public BoundObjectInitializerExpression Update(ImmutableArray<BoundExpression> initializers, TypeSymbol type)
	{
		if (initializers != Initializers || type != Type)
		{
			return new BoundObjectInitializerExpression(Syntax, initializers, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
