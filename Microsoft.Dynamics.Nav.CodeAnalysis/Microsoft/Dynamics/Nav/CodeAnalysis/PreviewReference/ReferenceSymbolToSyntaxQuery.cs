using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.PreviewReference;

internal class ReferenceSymbolToSyntaxQuery : ReferenceSymbolToSyntax
{
	protected override ApplicationObjectSyntax ConvertApplicationSymbol(ISymbol symbol)
	{
		return ReferenceSymbolToSyntax.ConvertApplicationSymbol<QueryTypeSymbol>(symbol, ConvertQuery);
	}

	private QuerySyntax ConvertQuery(QueryTypeSymbol query)
	{
		QuerySyntax querySyntax = SyntaxFactory.Query(SyntaxFactory.ObjectId(query.Id), query.Name);
		ImmutableArray<Symbol> members = query.GetMembers();
		if (!query.Properties.IsDefaultOrEmpty)
		{
			QuerySyntax querySyntax2 = querySyntax;
			PropertySyntaxOrEmpty[] items = ReferenceSymbolToSyntax.ConvertProperties(query.Properties, query);
			querySyntax = querySyntax2.AddPropertyListProperties(items);
		}
		if (members != null && !members.IsEmpty)
		{
			querySyntax = querySyntax.WithMembers(ConvertMembers(members));
		}
		return querySyntax;
	}
}
