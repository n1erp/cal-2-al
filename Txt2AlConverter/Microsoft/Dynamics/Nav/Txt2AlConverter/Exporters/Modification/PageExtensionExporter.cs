using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Page;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class PageExtensionExporter : PageSectionsExporter
{
	private readonly AlExportOptions options;

	public PageExtensionExporter(AlExportOptions options)
		: base(new NullExporter(), options)
	{
		this.options = options;
	}

	public override void Export(IElement element, IAlWriter writer)
	{
		IEnumerable<IElement> pageChanges = GetPageChanges(element);
		IEnumerable<IElement> layoutChanges = GetLayoutChanges(element);
		IEnumerable<IElement> actionsChanges = GetActionsChanges(element);
		IEnumerable<IElement> variablesChanges = GetVariablesChanges(element);
		IEnumerable<IElement> triggersChanges = GetTriggersChanges(element);
		IEnumerable<IElement> proceduresChanges = GetProceduresChanges(element);
		IEnumerable<IElement> documentationChanges = GetDocumentationChanges(element);
		IEnumerable<IElement> unsupportedChanges = GetUnsupportedChanges(element, pageChanges, layoutChanges, actionsChanges, variablesChanges, proceduresChanges, triggersChanges, documentationChanges);
		ChangeContentExporter.ExportDocumentationChanges(documentationChanges, writer);
		ExportObjectPropertyChanges(pageChanges, writer, options);
		ExportChanges(layoutChanges, writer, ExporterLocator, ElementType.Control, options);
		ExportChanges(actionsChanges, writer, ExporterLocator, ElementType.Action, options);
		ChangeContentExporter.ExportVariables(variablesChanges, writer);
		ChangeContentExporter.ExportTriggers(triggersChanges, writer);
		ChangeContentExporter.ExportProcedures(proceduresChanges, writer);
		ChangeContentExporter.ExportUnsupportedChanges(unsupportedChanges, writer);
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
			propertyChanges.ForEach((IGrouping<string, IElement> group) =>
			{
				ExportPropertyChangeList(group.GetElements().ToList(), group.Key, writer, options);
			});
			otherChanges.ForEach((IElement change) =>
			{
				ExportChangeSection(change, writer, exporterLocator, elementType, options);
			});
		});
	}

	private static void ExportPropertyChangeList(IEnumerable<IElement> layoutPropChanges, string targetElementKey, IAlWriter writer, AlExportOptions options)
	{
		new PropertyChangeExporter(new ModificationPropertyConverter(options))?.Export(layoutPropChanges, targetElementKey, writer);
	}

	private static void ExportChangeSection(IElement changeSection, IAlWriter writer, IExporterLocator exporterLocator, ElementType targetElementType, AlExportOptions options)
	{
		INestedElementExporter subElementExporter = new ChangeElementExporter(exporterLocator);
		(ChangeContentExporter.GetChangeType(changeSection) switch
		{
			ChangeType.InsertAfter => (IElementExporter)new FlatCollectionInsertionExporter(subElementExporter, targetElementType), 
			ChangeType.MoveAfter => new ElementMoveChangeExporter(), 
			ChangeType.Deletion => new DeleteElementExporter(), 
			ChangeType.CodeModification => new CommentedCodeDeltaExporter(), 
			ChangeType.CollectionInsertion => new NestedCollectionInsertionExporter(targetElementType, options), 
			_ => new WarningExporter(), 
		})?.Export(changeSection, writer);
	}

	private static IEnumerable<IElement> GetPageChanges(IElement element)
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

	private static IEnumerable<IElement> GetLayoutChanges(IElement element)
	{
		return ChangeContentExporter.GetChangesOfType(element, ElementType.Control).Where(DoesntHaveActionSubelement);
	}

	private static bool DoesntHaveActionSubelement(IElement element)
	{
		return element.GetSubElements().All((IElement s) => !s.GetPropertySetState(PropertyType.ActionType));
	}

	private static IEnumerable<IElement> GetActionsChanges(IElement element)
	{
		return from c in ChangeContentExporter.GetChangesOfType(element, ElementType.Action)
			where !IsTargetingControl(c)
			select c;
	}

	private static bool IsTargetingControl(IElement element)
	{
		return ChangeContentExporter.GetChangeTargetType(element) == ElementType.Control;
	}

	private static IEnumerable<IElement> GetVariablesChanges(IElement element)
	{
		return ChangeContentExporter.GetVariablesChanges(element);
	}

	private static IEnumerable<IElement> GetTriggersChanges(IElement element)
	{
		return ChangeContentExporter.GetTriggerChanges(element);
	}

	private static IEnumerable<IElement> GetProceduresChanges(IElement element)
	{
		List<IElement> list = ChangeContentExporter.GetChangesOfType(element, ElementType.Event).ToList();
		list.AddRange(ChangeContentExporter.GetChangesOfType(element, ElementType.Procedure));
		return list;
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
