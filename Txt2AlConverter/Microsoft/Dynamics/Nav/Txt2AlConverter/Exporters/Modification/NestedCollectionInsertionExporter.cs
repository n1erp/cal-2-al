using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class NestedCollectionInsertionExporter : IElementExporter
{
	private readonly ElementType targetElementType;

	private readonly ExporterLocator serviceLocator;

	internal NestedCollectionInsertionExporter(ElementType targetElementType, AlExportOptions options)
	{
		this.targetElementType = targetElementType;
		serviceLocator = new ExporterLocator(SectionsExporterHelper.GetElementTypeName);
		serviceLocator.AddService("container", SectionsExporterHelper.GetControlCollectionExporter(serviceLocator, options));
		serviceLocator.AddService("actioncontainer", SectionsExporterHelper.GetActionCollectionExporter(serviceLocator, options));
		serviceLocator.AddService("field", SectionsExporterHelper.GetFieldExporter(serviceLocator, options));
		serviceLocator.AddService("usercontrol", SectionsExporterHelper.GetUserControlExporter(serviceLocator));
		serviceLocator.AddService("group", SectionsExporterHelper.GetFieldGroupExporter(serviceLocator, options));
		serviceLocator.AddService("repeater", SectionsExporterHelper.GetFieldGroupExporter(serviceLocator, options));
		serviceLocator.AddService("part", SectionsExporterHelper.GetPagePartExporter(options));
		serviceLocator.AddService("action", SectionsExporterHelper.GetActionExporter(serviceLocator, options));
		serviceLocator.AddService("actiongroup", SectionsExporterHelper.GetActionGroupExporter(serviceLocator, options));
		serviceLocator.AddService("separator", SectionsExporterHelper.GetActionExporter(serviceLocator, options));
	}

	public void Export(IElement element, IAlWriter writer)
	{
		ExportSection(element, writer, targetElementType);
	}

	private void ExportSection(IElement element, IAlWriter writer, ElementType targetElementType)
	{
		if (!element.GetSubElements(targetElementType).Any())
		{
			return;
		}
		if (writer.PendingEmptyLine)
		{
			writer.WriteLine();
		}
		writer.PendingEmptyLine = false;
		foreach (ElementTreeNode child in ElementTreeNode.Create(element, targetElementType, forceNesting: true).Children)
		{
			string elementKey = serviceLocator.GetElementKey(child.Element);
			serviceLocator.GetService<INestedElementExporter>(elementKey).Export(child, writer);
		}
		writer.PendingEmptyLine = true;
	}
}
