using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Page;

internal sealed class PagePartExporter : IElementExporter
{
	private readonly IElementExporter defaultPagePartExporter;

	private readonly IElementExporter pagePartExporter;

	private readonly IElementExporter systemPartExporter;

	private readonly IElementExporter chartPartExporter;

	public PagePartExporter(PagePropertiesConverter headerPropertiesExporter, PagePropertiesConverter elementBodyPropertiesExporter, AlExportOptions options)
	{
		pagePartExporter = GetPagePartExporter(headerPropertiesExporter, elementBodyPropertiesExporter, options);
		systemPartExporter = GetSystemPartExporter(headerPropertiesExporter, elementBodyPropertiesExporter, options);
		defaultPagePartExporter = GetDefaultExporter(headerPropertiesExporter, elementBodyPropertiesExporter, options);
		chartPartExporter = GetChartPartExporter(headerPropertiesExporter, elementBodyPropertiesExporter, options);
	}

	public void Export(IElement element, IAlWriter writer)
	{
		GetExporterForElement(element).Export(element, writer);
	}

	private IElementExporter GetPagePartExporter(IPropertiesConverter headerPropertiesExporter, IPropertiesConverter bodyPropertiesExporter, AlExportOptions options)
	{
		PropertyType[] inlineProperties = new PropertyType[2]
		{
			PropertyType.Name,
			PropertyType.PagePartId
		};
		return GetPagePartExporter(headerPropertiesExporter, inlineProperties, bodyPropertiesExporter, options);
	}

	private IElementExporter GetSystemPartExporter(IPropertiesConverter headerPropertiesExporter, IPropertiesConverter bodyPropertiesExporter, AlExportOptions options)
	{
		PropertyType[] inlineProperties = new PropertyType[2]
		{
			PropertyType.Name,
			PropertyType.SystemPartId
		};
		return GetPagePartExporter(headerPropertiesExporter, inlineProperties, bodyPropertiesExporter, options);
	}

	private IElementExporter GetChartPartExporter(IPropertiesConverter headerPropertiesExporter, IPropertiesConverter bodyPropertiesExporter, AlExportOptions options)
	{
		PropertyType[] inlineProperties = new PropertyType[2]
		{
			PropertyType.ChartPartId,
			PropertyType.ChartPartId
		};
		return GetPagePartExporter(headerPropertiesExporter, inlineProperties, bodyPropertiesExporter, options);
	}

	private IElementExporter GetPagePartExporter(IPropertiesConverter headerPropertiesExporter, PropertyType[] inlineProperties, IPropertiesConverter bodyPropertiesExporter, AlExportOptions options)
	{
		ElementWithTriggersExporter nextExporter = new ElementWithTriggersExporter(new NullExporter(), options);
		ElementWithPropertiesExporter bodyExporter = new ElementWithPropertiesExporter(bodyPropertiesExporter, nextExporter);
		return new HeaderExporter(headerPropertiesExporter, inlineProperties, bodyExporter);
	}

	private IElementExporter GetDefaultExporter(IPropertiesConverter headerPropertiesExporter, IPropertiesConverter bodyPropertiesExporter, AlExportOptions options)
	{
		PropertyType[] inlineProperties = new PropertyType[1] { PropertyType.Name };
		return GetPagePartExporter(headerPropertiesExporter, inlineProperties, bodyPropertiesExporter, options);
	}

	private IElementExporter GetExporterForElement(IElement element)
	{
		string text = element.GetStringProperty(PropertyType.PartType)?.ToLowerInvariant();
		string stringProperty = element.GetStringProperty(PropertyType.PagePartId);
		string stringProperty2 = element.GetStringProperty(PropertyType.SystemPartId);
		string stringProperty3 = element.GetStringProperty(PropertyType.ChartPartId);
		if (text == "page" || stringProperty != null)
		{
			return pagePartExporter;
		}
		if (text == "systempart" || stringProperty2 != null)
		{
			return systemPartExporter;
		}
		if (text == "chart" || stringProperty3 != null)
		{
			return chartPartExporter;
		}
		return defaultPagePartExporter;
	}
}
