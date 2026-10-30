using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundValuePlaceholderBase : BoundExpression
{
	protected BoundValuePlaceholderBase(BoundKind kind, SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(kind, syntax, type, hasErrors)
	{
	}

	protected BoundValuePlaceholderBase(BoundKind kind, SyntaxNode syntax, TypeSymbol type)
		: base(kind, syntax, type)
	{
	}
}
