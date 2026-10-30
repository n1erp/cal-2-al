using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.PreviewReference;

internal class ReferenceSymbolToSyntaxTable : ReferenceSymbolToSyntax
{
	protected override ApplicationObjectSyntax ConvertApplicationSymbol(ISymbol symbol)
	{
		return ReferenceSymbolToSyntax.ConvertApplicationSymbol<TableTypeSymbol>(symbol, ConvertTable);
	}

	private TableSyntax ConvertTable(TableTypeSymbol table)
	{
		TableSyntax tableSyntax = SyntaxFactory.Table(SyntaxFactory.ObjectId(table.Id), table.Name);
		ImmutableArray<Symbol> members = table.GetMembers();
		if (!table.Properties.IsDefaultOrEmpty)
		{
			TableSyntax tableSyntax2 = tableSyntax;
			PropertySyntaxOrEmpty[] items = ReferenceSymbolToSyntax.ConvertProperties(table.Properties, table);
			tableSyntax = tableSyntax2.AddPropertyListProperties(items);
		}
		SyntaxList<FieldSyntax> syntaxList = ConvertTableFields(table.GetFields());
		if (!syntaxList.IsEmpty())
		{
			tableSyntax = tableSyntax.WithFields(SyntaxFactory.FieldList(syntaxList));
		}
		SyntaxList<KeySyntax> syntaxList2 = ReferenceSymbolToSyntax.ConvertTableKeys(table.GetKeys());
		if (!syntaxList2.IsEmpty())
		{
			tableSyntax = tableSyntax.WithKeys(SyntaxFactory.KeyList(syntaxList2));
		}
		SyntaxList<FieldGroupSyntax> syntaxList3 = ReferenceSymbolToSyntax.ConvertTableFieldGroups(table.GetFieldGroups());
		if (!syntaxList3.IsEmpty())
		{
			tableSyntax = tableSyntax.WithFieldGroups(SyntaxFactory.FieldGroupList(syntaxList3));
		}
		if (members != null && !members.IsEmpty)
		{
			tableSyntax = tableSyntax.WithMembers(ConvertMembers(members));
		}
		return tableSyntax;
	}

	private static SyntaxList<FieldSyntax> ConvertTableFields(IEnumerable<FieldSymbol> fields)
	{
		SyntaxListBuilder<FieldSyntax> syntaxListBuilder = SyntaxListBuilder<FieldSyntax>.Create();
		foreach (FieldSymbol field in fields)
		{
			FieldSyntax fieldSyntax = SyntaxFactory.Field(SyntaxFactory.Literal(field.Id), SyntaxFactory.IdentifierName(field.Name), ReferenceSymbolToSyntax.GetDataTypeSyntax(field.Type));
			FieldSyntax fieldSyntax2 = fieldSyntax;
			PropertySyntaxOrEmpty[] items = ReferenceSymbolToSyntax.ConvertProperties(field.Properties, field);
			fieldSyntax = fieldSyntax2.AddPropertyListProperties(items);
			syntaxListBuilder.Add(fieldSyntax);
		}
		return syntaxListBuilder.ToList();
	}
}
