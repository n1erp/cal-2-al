using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct AliasAndUsingDirective(AliasSymbol alias, UsingDirectiveSyntax usingDirective)
{
	public readonly AliasSymbol Alias = alias;

	public readonly UsingDirectiveSyntax UsingDirective = usingDirective;
}
