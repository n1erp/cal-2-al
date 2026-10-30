using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundObjectInitializerExpressionBase : BoundExpression
{
	public ImmutableArray<BoundExpression> Initializers { get; }

	protected BoundObjectInitializerExpressionBase(BoundKind kind, SyntaxNode syntax, ImmutableArray<BoundExpression> initializers, TypeSymbol type, bool hasErrors = false)
		: base(kind, syntax, type, hasErrors)
	{
		Initializers = initializers;
	}
}
