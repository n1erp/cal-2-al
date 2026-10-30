using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.PreviewReference;

internal class ReferenceSymbolToSyntaxTableExtension : ReferenceSymbolToSyntax
{
	protected override ApplicationObjectSyntax ConvertApplicationSymbol(ISymbol symbol)
	{
		return ReferenceSymbolToSyntax.ConvertApplicationSymbol<TableExtensionTypeSymbol>(symbol, ConvertTableExtension);
	}

	private TableExtensionSyntax ConvertTableExtension(TableExtensionTypeSymbol tableExtension)
	{
		TableExtensionSyntax tableExtensionSyntax = SyntaxFactory.TableExtension(SyntaxFactory.ObjectId(tableExtension.Id), tableExtension.Name, SyntaxFactory.ObjectNameOrId(SyntaxFactory.IdentifierName(tableExtension.Target.Name)));
		ImmutableArray<Symbol> members = tableExtension.GetMembers();
		if (!tableExtension.Properties.IsDefaultOrEmpty)
		{
			TableExtensionSyntax tableExtensionSyntax2 = tableExtensionSyntax;
			PropertySyntaxOrEmpty[] items = ReferenceSymbolToSyntax.ConvertProperties(tableExtension.Properties, tableExtension);
			tableExtensionSyntax = tableExtensionSyntax2.AddPropertyListProperties(items);
		}
		SyntaxList<FieldBaseSyntax> fields = ConvertTableExtensionFields(members.Where((Symbol m) => m.Kind == SymbolKind.Field).OfType<FieldSymbol>());
		if (fields.Count != 0)
		{
			tableExtensionSyntax = tableExtensionSyntax.WithFields(SyntaxFactory.FieldExtensionList(fields));
		}
		SyntaxList<KeySyntax> keys = ReferenceSymbolToSyntax.ConvertTableKeys(members.Where((Symbol m) => m.Kind == SymbolKind.Key).OfType<FieldNameListSymbol>());
		if (keys.Count != 0)
		{
			tableExtensionSyntax = tableExtensionSyntax.WithKeys(SyntaxFactory.KeyList(keys));
		}
		if (members != null && !members.IsEmpty)
		{
			tableExtensionSyntax = tableExtensionSyntax.WithMembers(ConvertMembers(members));
		}
		return tableExtensionSyntax;
	}

	private static SyntaxList<FieldBaseSyntax> ConvertTableExtensionFields(IEnumerable<FieldSymbol> fields)
	{
		SyntaxListBuilder<FieldBaseSyntax> syntaxListBuilder = SyntaxListBuilder<FieldBaseSyntax>.Create();
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
