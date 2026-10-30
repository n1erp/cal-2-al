using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Table;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class TableExtensionExporter : TableSectionsExporter
{
	private readonly AlExportOptions options;

	public TableExtensionExporter(AlExportOptions options)
		: base(new NullExporter(), options)
	{
		this.options = options;
	}

	public override void Export(IElement element, IAlWriter writer)
	{
		IEnumerable<IElement> tableChanges = GetTableChanges(element);
		IEnumerable<IElement> fieldsChanges = GetFieldsChanges(element);
		IEnumerable<IElement> keysChanges = GetKeysChanges(element);
		IEnumerable<IElement> variablesChanges = GetVariablesChanges(element);
		IEnumerable<IElement> proceduresChanges = GetProceduresChanges(element);
		IEnumerable<IElement> triggersChanges = GetTriggersChanges(element);
		IEnumerable<IElement> documentationChanges = GetDocumentationChanges(element);
		IEnumerable<IElement> unsupportedChanges = GetUnsupportedChanges(element, tableChanges, fieldsChanges, keysChanges, variablesChanges, proceduresChanges, triggersChanges, documentationChanges);
		ChangeContentExporter.ExportDocumentationChanges(documentationChanges, writer);
		ExportObjectPropertyChanges(tableChanges, writer, options);
		ExportChanges(fieldsChanges, writer, ExporterLocator, ElementType.Field, options);
		ExportChanges(keysChanges, writer, ExporterLocator, ElementType.Key, options);
		ChangeContentExporter.ExportTriggers(triggersChanges, writer);
		ChangeContentExporter.ExportProcedures(proceduresChanges, writer);
		ChangeContentExporter.ExportUnsupportedChanges(unsupportedChanges, writer);
		ChangeContentExporter.ExportVariables(variablesChanges, writer);
	}

	private static void ExportObjectPropertyChanges(IEnumerable<IElement> changes, IAlWriter writer, AlExportOptions options)
	{
		IEnumerable<IElement> enumerable = (changes as IElement[]) ?? changes.ToArray();
		if (enumerable.Any())
		{
			PropertyChangeExporter propertyChangeExporter = new PropertyChangeExporter(new ModificationPropertyConverter(options));
			string stringProperty = enumerable.First().GetStringProperty(PropertyType.Target);
			propertyChangeExporter.Export(enumerable, stringProperty, writer);
		}
	}

	private static void ExportChanges(IEnumerable<IElement> changes, IAlWriter writer, IExporterLocator exporterLocator, ElementType elementType, AlExportOptions options)
	{
		IEnumerable<IElement> source = (changes as IElement[]) ?? changes.ToArray();
		if (!source.Any())
		{
			return;
		}
		List<IGrouping<string, IElement>> propertyChanges = (from @group in source
			where ChangeContentExporter.GetChangeType(@group) == ChangeType.PropertyChange
			group @group by @group.GetStringProperty(PropertyType.Target)).ToList();
		List<IElement> otherChanges = source.Where((IElement change) => ChangeContentExporter.GetChangeType(change) != ChangeType.PropertyChange).ToList();
		writer?.WriteSection(ChangeContentExporter.GetSectionHeader(elementType), () =>
		{
			if (elementType == ElementType.Key)
			{
				propertyChanges.ForEach((IGrouping<string, IElement> group) =>
				{
					group.ForEach((IElement change) =>
					{
						new WarningExporter().Export(change, writer);
					});
				});
			}
			else
			{
				propertyChanges.ForEach((IGrouping<string, IElement> group) =>
				{
					ExportPropertyChangeList(group.GetElements().ToList(), group.Key, writer, options);
				});
			}
			otherChanges.ForEach((IElement change) =>
			{
				ExportChangeSection(change, writer, exporterLocator, elementType);
			});
		});
	}

	private static void ExportPropertyChangeList(IEnumerable<IElement> propChanges, string targetElementKey, IAlWriter writer, AlExportOptions options)
	{
		new PropertyChangeExporter(new ModificationPropertyConverter(options))?.Export(propChanges, targetElementKey, writer);
	}

	private static void ExportChangeSection(IElement changeSection, IAlWriter writer, IExporterLocator exporterLocator, ElementType targetElementType)
	{
		INestedElementExporter subElementExporter = new ChangeElementExporter(exporterLocator);
		IElementExporter elementExporter;
		switch (ChangeContentExporter.GetChangeType(changeSection))
		{
		case ChangeType.InsertAfter:
		case ChangeType.CollectionInsertion:
			elementExporter = new FlatCollectionInsertionExporter(subElementExporter, targetElementType);
			break;
		case ChangeType.CodeModification:
			elementExporter = new CommentedCodeDeltaExporter();
			break;
		default:
			elementExporter = new WarningExporter();
			break;
		}
		elementExporter?.Export(changeSection, writer);
	}

	private static IEnumerable<IElement> GetTableChanges(IElement element)
	{
		IEnumerable<IElement> enumerable = from e in ChangeContentExporter.GetChangesOfTypeAndTarget(element, ElementType.Unknown, ChangeTypeKind.PropertyInsertion, ChangeTypeKind.PropertyModification)
			where GetPropertyType(e) != PropertyType.VersionList
			select e;
		foreach (IElement item in enumerable)
		{
			item.SetStringProperty(PropertyType.Target, element.Name);
		}
		return enumerable;
	}

	private static PropertyType GetPropertyType(IElement element)
	{
		return EnumNameLookup<PropertyType>.StringToEnum(element.GetStringProperty(PropertyType.Property));
	}

	private static IEnumerable<IElement> GetFieldsChanges(IElement element)
	{
		return ChangeContentExporter.GetChangesOfType(element, ElementType.Field);
	}

	private static IEnumerable<IElement> GetKeysChanges(IElement element)
	{
		return ChangeContentExporter.GetChangesOfType(element, ElementType.Key);
	}

	private static IEnumerable<IElement> GetVariablesChanges(IElement element)
	{
		return ChangeContentExporter.GetVariablesChanges(element);
	}

	private static IEnumerable<IElement> GetProceduresChanges(IElement element)
	{
		List<IElement> list = ChangeContentExporter.GetChangesOfType(element, ElementType.Event).ToList();
		list.AddRange(ChangeContentExporter.GetChangesOfType(element, ElementType.Procedure));
		return list;
	}

	private static IEnumerable<IElement> GetTriggersChanges(IElement element)
	{
		return ChangeContentExporter.GetTriggerChanges(element);
	}

	private static IEnumerable<IElement> GetDocumentationChanges(IElement element)
	{
		return ChangeContentExporter.GetChangesOfType(element, ElementType.Documentation);
	}

	private static IEnumerable<IElement> GetUnsupportedChanges(IElement element, params IEnumerable<IElement>[] changes)
	{
		IEnumerable<IElement> second = changes.SelectMany((IEnumerable<IElement> i) => i);
		return element.GetSubElements().Except(second).ToList();
	}
}
