using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundTypeOf : BoundExpression
{
	public MethodSymbol GetTypeFromHandle { get; }

	protected BoundTypeOf(BoundKind kind, SyntaxNode syntax, MethodSymbol getTypeFromHandle, TypeSymbol type, bool hasErrors)
		: base(kind, syntax, type, hasErrors)
	{
		GetTypeFromHandle = getTypeFromHandle;
	}

	protected BoundTypeOf(BoundKind kind, SyntaxNode syntax, MethodSymbol getTypeFromHandle, TypeSymbol type)
		: base(kind, syntax, type)
	{
		GetTypeFromHandle = getTypeFromHandle;
	}
}
