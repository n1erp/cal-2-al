using System;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Query;

internal sealed class QuerySectionsExporter : ElementWithSectionsExporter
{
	private readonly ExporterLocator serviceLocator;

	protected override IExporterLocator ExporterLocator => serviceLocator;

	public QuerySectionsExporter(IElementExporter nextExporter, AlExportOptions options)
		: base(nextExporter)
	{
		serviceLocator = new ExporterLocator(GetQueryElementExporterKey);
		serviceLocator.AddService("dataitem", GetDataItemExporter(options));
		serviceLocator.AddService("column", GetColumnExporter());
		serviceLocator.AddService("filter", GetColumnExporter());
		serviceLocator.AddService("columnWithoutDataSource", GetColumnWithoutDataSourceExporter());
	}

	private static string GetQueryElementExporterKey(IElement dataItem)
	{
		string text = dataItem.GetStringProperty(PropertyType.QueryElementType).ToLowerInvariant();
		switch (text)
		{
		case "dataitem":
		case "filter":
			return text;
		case "column":
			return GetColumnKey(dataItem);
		default:
			throw new ArgumentOutOfRangeException("queryElementType", text);
		}
	}

	private static string GetColumnKey(IElement column)
	{
		string value = null;
		column.TryGetStringProperty(PropertyType.DataSource, out value);
		if (string.IsNullOrWhiteSpace(value))
		{
			return "columnWithoutDataSource";
		}
		return "column";
	}

	private static INestedElementExporter GetColumnExporter()
	{
		PropertyType[] inlineProperties = new PropertyType[2]
		{
			PropertyType.Name,
			PropertyType.DataSource
		};
		QueryPropertiesConverter propertyExporter = new QueryPropertiesConverter();
		ElementWithPropertiesExporter bodyExporter = new ElementWithPropertiesExporter(propertyExporter, new NullExporter());
		return new TerminallNestedElementExporter(new HeaderExporter(propertyExporter, inlineProperties, bodyExporter));
	}

	private static INestedElementExporter GetColumnWithoutDataSourceExporter()
	{
		PropertyType[] inlineProperties = new PropertyType[1] { PropertyType.Name };
		QueryPropertiesConverter propertyExporter = new QueryPropertiesConverter();
		ElementWithPropertiesExporter bodyExporter = new ElementWithPropertiesExporter(propertyExporter, new NullExporter());
		return new TerminallNestedElementExporter(new HeaderExporter(propertyExporter, inlineProperties, bodyExporter));
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
			PreChildrenParentExporter = new ElementWithPropertiesExporter(new QueryPropertiesConverter(), new NullExporter()),
			PostChildrenParentExporter = new ElementWithTriggersExporter(new NullExporter(), options),
			HeaderExporter = new ElementHeaderExporter(new QueryPropertiesConverter(), inlineProperties)
		};
	}
}
