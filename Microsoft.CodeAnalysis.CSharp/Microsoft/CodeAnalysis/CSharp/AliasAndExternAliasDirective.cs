using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct AliasAndExternAliasDirective(AliasSymbol alias, ExternAliasDirectiveSyntax externAliasDirective)
{
	public readonly AliasSymbol Alias = alias;

	public readonly ExternAliasDirectiveSyntax ExternAliasDirective = externAliasDirective;
}
