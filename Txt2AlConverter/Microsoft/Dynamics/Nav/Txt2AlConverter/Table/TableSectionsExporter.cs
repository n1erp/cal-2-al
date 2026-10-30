using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Table;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Table;

internal class TableSectionsExporter : ElementWithSectionsExporter
{
	private readonly ExporterLocator serviceLocator;

	protected override IExporterLocator ExporterLocator => serviceLocator;

	public TableSectionsExporter(IElementExporter nextExporter, AlExportOptions options)
		: base(nextExporter)
	{
		serviceLocator = new ExporterLocator((IElement child) => child.ElementTypeInfo.Name.ToLowerInvariant());
		serviceLocator.AddService("field", GetFieldExporter(options));
		serviceLocator.AddService("key", GetKeyExporter(options));
		serviceLocator.AddService("fieldgroup", GetFieldGroupExporter(options));
	}

	private static INestedElementExporter GetFieldExporter(AlExportOptions options)
	{
		ElementWithTriggersExporter elementWithTriggersExporter = new ElementWithTriggersExporter(new NullExporter(), options);
		PropertyType[] inlineProperties = new PropertyType[3]
		{
			PropertyType.FieldNo,
			PropertyType.Name,
			PropertyType.DataType
		};
		TablePropertiesConverter propertyExporter = new TablePropertiesConverter(options);
		ElementWithPropertiesExporter bodyExporter = new ElementWithPropertiesExporter(propertyExporter, elementWithTriggersExporter);
		return new TerminallNestedElementExporter(new HeaderExporter(propertyExporter, inlineProperties, bodyExporter));
	}

	private static INestedElementExporter GetKeyExporter(AlExportOptions options)
	{
		return new TerminallNestedElementExporter(new TableKeyExporter(options));
	}

	private static INestedElementExporter GetFieldGroupExporter(AlExportOptions options)
	{
		PropertyType[] inlineProperties = new PropertyType[2]
		{
			PropertyType.Name,
			PropertyType.Fields
		};
		TablePropertiesConverter propertyExporter = new TablePropertiesConverter(options);
		ElementWithPropertiesExporter bodyExporter = new ElementWithPropertiesExporter(propertyExporter, new NullExporter());
		return new TerminallNestedElementExporter(new HeaderExporter(propertyExporter, inlineProperties, bodyExporter));
	}
}
