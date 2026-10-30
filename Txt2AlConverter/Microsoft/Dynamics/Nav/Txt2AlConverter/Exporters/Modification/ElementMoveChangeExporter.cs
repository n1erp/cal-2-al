using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class ElementMoveChangeExporter : IElementExporter
{
	public void Export(IElement element, IAlWriter writer)
	{
		ExportSection(element, writer);
	}

	private static void ExportSection(IElement element, IAlWriter writer)
	{
		element.TryGetStringProperty(PropertyType.Target, out var value);
		element.TryGetStringProperty(PropertyType.InsertAfter, out var value2);
		value = ModificationContentExporter.WrapInQuotesIfNeeded(ModificationContentExporter.GetALNameFromElementIdentifier(value));
		value2 = ModificationContentExporter.WrapInQuotesIfNeeded(ModificationContentExporter.GetALNameFromElementIdentifier(value2));
		writer.WriteLine(SyntaxKind.MoveAfterKeyword.GetText() + "(" + value2 + ";" + value + ")");
	}
}
