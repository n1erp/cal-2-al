using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal interface IBoundLambdaOrFunction
{
	MethodSymbol Symbol { get; }

	SyntaxNode Syntax { get; }

	BoundBlock Body { get; }

	bool WasCompilerGenerated { get; }
}
