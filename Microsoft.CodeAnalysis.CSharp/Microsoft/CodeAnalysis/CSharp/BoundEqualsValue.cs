using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundEqualsValue : BoundInitializer
{
	public ImmutableArray<LocalSymbol> Locals { get; }

	public BoundExpression Value { get; }

	protected BoundEqualsValue(BoundKind kind, SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, BoundExpression value, bool hasErrors = false)
		: base(kind, syntax, hasErrors)
	{
		Locals = locals;
		Value = value;
	}
}
