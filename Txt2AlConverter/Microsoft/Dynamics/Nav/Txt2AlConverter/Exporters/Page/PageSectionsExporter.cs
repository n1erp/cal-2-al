using System;
using Microsoft.Dynamics.Nav.Model.IO.Txt;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Page;

internal class PageSectionsExporter : ElementWithSectionsExporter
{
	private readonly ExporterLocator serviceLocator;

	protected override IExporterLocator ExporterLocator => serviceLocator;

	public PageSectionsExporter(IElementExporter nextExporter, AlExportOptions options)
		: base(nextExporter)
	{
		serviceLocator = new ExporterLocator(SectionsExporterHelper.GetElementTypeName);
		serviceLocator.AddService("field", SectionsExporterHelper.GetFieldExporter(serviceLocator, options));
		serviceLocator.AddService("label", SectionsExporterHelper.GetLabelExporter(serviceLocator, options));
		serviceLocator.AddService("usercontrol", SectionsExporterHelper.GetUserControlExporter(serviceLocator));
		serviceLocator.AddService("container", SectionsExporterHelper.GetFieldContainerExporter(serviceLocator, options));
		serviceLocator.AddService("group", SectionsExporterHelper.GetFieldGroupExporter(serviceLocator, options));
		serviceLocator.AddService("repeater", SectionsExporterHelper.GetFieldGroupExporter(serviceLocator, options));
		serviceLocator.AddService("part", SectionsExporterHelper.GetPagePartExporter(options));
		serviceLocator.AddService("actioncontainer", SectionsExporterHelper.GetActionContainerExporter(serviceLocator, options));
		serviceLocator.AddService("action", SectionsExporterHelper.GetActionExporter(serviceLocator, options));
		serviceLocator.AddService("actiongroup", SectionsExporterHelper.GetActionGroupExporter(serviceLocator, options));
		serviceLocator.AddService("separator", SectionsExporterHelper.GetActionExporter(serviceLocator, options));
	}

	protected override string GetSectionName(ElementRelationExport section)
	{
		string text = section.ContainerName.ToLowerInvariant();
		if (string.Equals(text, "controls", StringComparison.OrdinalIgnoreCase))
		{
			text = "layout";
		}
		return text;
	}
}
