using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Page;

internal class PageControlActionsExporter : ElementWithSectionsExporter
{
	private readonly PagePropertiesConverter actionPropertiesExporter = new PagePropertiesConverter();

	private readonly ExporterLocator serviceLocator;

	protected override IExporterLocator ExporterLocator => serviceLocator;

	public PageControlActionsExporter(IElementExporter nextExporter, AlExportOptions options)
		: base(nextExporter)
	{
		serviceLocator = new ExporterLocator(GetElementTypeName);
		serviceLocator.AddService("actiongroup", SectionsExporterHelper.GetActionGroupExporter(serviceLocator, options));
		serviceLocator.AddService("action", SectionsExporterHelper.GetActionExporter(serviceLocator, options));
		serviceLocator.AddService("separator", SectionsExporterHelper.GetActionExporter(serviceLocator, options));
	}

	private static string GetElementTypeName(IElement child)
	{
		return child.GetStringProperty(PropertyType.ActionType).ToLowerInvariant();
	}
}
