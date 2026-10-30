using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.PreviewReference;

internal class ReferenceSymbolToSyntaxCodeunit : ReferenceSymbolToSyntax
{
	protected override ApplicationObjectSyntax ConvertApplicationSymbol(ISymbol symbol)
	{
		return ReferenceSymbolToSyntax.ConvertApplicationSymbol<CodeunitTypeSymbol>(symbol, ConvertCodeunit);
	}

	private CodeunitSyntax ConvertCodeunit(CodeunitTypeSymbol codeunit)
	{
		CodeunitSyntax codeunitSyntax = SyntaxFactory.Codeunit(SyntaxFactory.ObjectId(codeunit.Id), codeunit.Name);
		ImmutableArray<Symbol> members = codeunit.GetMembers();
		if (!codeunit.Properties.IsDefaultOrEmpty)
		{
			CodeunitSyntax codeunitSyntax2 = codeunitSyntax;
			PropertySyntaxOrEmpty[] items = ReferenceSymbolToSyntax.ConvertProperties(codeunit.Properties, codeunit);
			codeunitSyntax = codeunitSyntax2.AddPropertyListProperties(items);
		}
		if (members != null && !members.IsEmpty)
		{
			codeunitSyntax = codeunitSyntax.WithMembers(ConvertMembers(members));
		}
		return codeunitSyntax;
	}
}
