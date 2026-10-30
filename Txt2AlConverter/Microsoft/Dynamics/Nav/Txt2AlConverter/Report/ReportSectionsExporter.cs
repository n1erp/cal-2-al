using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Page;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Report;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Report;

internal sealed class ReportSectionsExporter : ElementWithSectionsExporter
{
	private readonly ExporterLocator serviceLocator;

	protected override IExporterLocator ExporterLocator => serviceLocator;

	public ReportSectionsExporter(IElementExporter nextExporter, AlExportOptions options)
		: base(nextExporter)
	{
		serviceLocator = new ExporterLocator(GetChildExporterKey);
		serviceLocator.AddService("dataitem", GetDataItemExporter(options));
		serviceLocator.AddService("column", GetColumnExporter());
		serviceLocator.AddService("filter", GetColumnExporter());
		serviceLocator.AddService("label", new ReportLabelExporter(new ReportPropertiesConverter()));
		serviceLocator.AddService("requestpage", GetRequestPageExporter(options));
	}

	private static string GetChildExporterKey(IElement child)
	{
		if (child.ElementTypeInfo.ElementType == ElementType.DataItem)
		{
			return GetDataItemTypeName(child);
		}
		return child.ElementTypeInfo.Name.ToLowerInvariant();
	}

	private static string GetDataItemTypeName(IElement dataItem)
	{
		return dataItem.GetStringProperty(PropertyType.ReportDataItemDataType).ToLowerInvariant();
	}

	private static INestedElementExporter GetColumnExporter()
	{
		PropertyType[] inlineProperties = new PropertyType[2]
		{
			PropertyType.Name,
			PropertyType.SourceExpression
		};
		ReportPropertiesConverter propertyExporter = new ReportPropertiesConverter();
		ElementWithPropertiesExporter bodyExporter = new ElementWithPropertiesExporter(propertyExporter, new NullExporter());
		return new TerminallNestedElementExporter(new HeaderExporter(propertyExporter, inlineProperties, bodyExporter));
	}

	private static INestedElementExporter GetRequestPageExporter(AlExportOptions options)
	{
		return new TerminallNestedElementExporter(new PageContentExporter(options));
	}

	private INestedElementExporter GetDataItemExporter(AlExportOptions options)
	{
		PropertyType[] inlineProperties = new PropertyType[2]
		{
			PropertyType.Name,
			PropertyType.DataItemTable
		};
		return new NestedElementExporter(ExporterLocator)
		{
			PreChildrenParentExporter = new ElementWithPropertiesExporter(new ReportPropertiesConverter(), new NullExporter()),
			PostChildrenParentExporter = new ElementWithTriggersExporter(new NullExporter(), options),
			HeaderExporter = new ElementHeaderExporter(new ReportPropertiesConverter(), inlineProperties)
		};
	}
}
