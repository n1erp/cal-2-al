using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Page;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal static class SectionsExporterHelper
{
	private static readonly PagePropertiesConverter actionPropertiesExporter = new PagePropertiesConverter();

	private static readonly PagePropertiesConverter controlHeaderPropertiesConverter = new PagePropertiesConverter();

	private static readonly PagePropertiesConverter controlBodyPropertiesConverter = new PagePropertiesConverter();

	internal static INestedElementExporter GetPagePartExporter(AlExportOptions options)
	{
		return new TerminallNestedElementExporter(new PagePartExporter(controlHeaderPropertiesConverter, controlBodyPropertiesConverter, options));
	}

	internal static object GetLabelExporter(ExporterLocator exporterLocator, AlExportOptions options)
	{
		PropertyType[] inlineProperties = new PropertyType[1] { PropertyType.Name };
		return new NestedElementExporter(exporterLocator)
		{
			HeaderExporter = new ElementHeaderExporter(controlHeaderPropertiesConverter, inlineProperties),
			PreChildrenParentExporter = new ElementWithPropertiesExporter(controlBodyPropertiesConverter, new NullExporter()),
			PostChildrenParentExporter = new ElementWithTriggersExporter(new NullExporter(), options)
		};
	}

	internal static INestedElementExporter GetFieldGroupExporter(IExporterLocator exporterLocator, AlExportOptions options)
	{
		PropertyType[] inlineProperties = new PropertyType[1] { PropertyType.Name };
		return new NestedElementExporter(exporterLocator)
		{
			HeaderExporter = new ElementHeaderExporter(controlHeaderPropertiesConverter, inlineProperties),
			PreChildrenParentExporter = new ElementWithPropertiesExporter(controlBodyPropertiesConverter, new NullExporter()),
			PostChildrenParentExporter = new ElementWithTriggersExporter(new PageControlActionsExporter(new NullExporter(), options), options)
		};
	}

	internal static INestedElementExporter GetFieldContainerExporter(IExporterLocator exporterLocator, AlExportOptions options)
	{
		PropertyType[] inlineProperties = new PropertyType[1] { PropertyType.ContainerType };
		return new NestedElementExporter(exporterLocator)
		{
			HeaderExporter = new ElementHeaderExporter(controlHeaderPropertiesConverter, inlineProperties),
			PreChildrenParentExporter = new ElementWithPropertiesExporter(controlBodyPropertiesConverter, new NullExporter()),
			PostChildrenParentExporter = new ElementWithTriggersExporter(new NullExporter(), options)
		};
	}

	internal static INestedElementExporter GetFieldExporter(IExporterLocator exporterLocator, AlExportOptions options)
	{
		PropertyType[] inlineProperties = new PropertyType[2]
		{
			PropertyType.Name,
			PropertyType.SourceExpression
		};
		return new NestedElementExporter(exporterLocator)
		{
			HeaderExporter = new ElementHeaderExporter(controlHeaderPropertiesConverter, inlineProperties),
			PreChildrenParentExporter = new ElementWithPropertiesExporter(controlBodyPropertiesConverter, new NullExporter()),
			PostChildrenParentExporter = new ElementWithTriggersExporter(new NullExporter(), options)
		};
	}

	internal static INestedElementExporter GetUserControlExporter(IExporterLocator exporterLocator)
	{
		PropertyType[] inlineProperties = new PropertyType[2]
		{
			PropertyType.Name,
			PropertyType.ControlAddIn
		};
		return new NestedElementExporter(exporterLocator)
		{
			HeaderExporter = new ElementHeaderExporter(controlHeaderPropertiesConverter, inlineProperties),
			PreChildrenParentExporter = new ElementWithPropertiesExporter(controlBodyPropertiesConverter, new NullExporter()),
			PostChildrenParentExporter = new ElementWithEventSubscribersExporter(new NullExporter())
		};
	}

	internal static TerminallNestedElementExporter GetActionExporter(IExporterLocator exporterLocator, AlExportOptions options)
	{
		PropertyType[] inlineProperties = new PropertyType[1] { PropertyType.Name };
		ElementWithTriggersExporter nextExporter = new ElementWithTriggersExporter(new NullExporter(), options);
		ElementWithPropertiesExporter bodyExporter = new ElementWithPropertiesExporter(actionPropertiesExporter, nextExporter);
		return new TerminallNestedElementExporter(new HeaderExporter(actionPropertiesExporter, inlineProperties, bodyExporter));
	}

	internal static INestedElementExporter GetActionContainerExporter(IExporterLocator exporterLocator, AlExportOptions options)
	{
		PropertyType[] inlineProperties = new PropertyType[1] { PropertyType.ActionContainerType };
		return new NestedElementExporter(exporterLocator)
		{
			HeaderExporter = new ElementHeaderExporter(actionPropertiesExporter, inlineProperties),
			PreChildrenParentExporter = new ElementWithPropertiesExporter(actionPropertiesExporter, new NullExporter()),
			PostChildrenParentExporter = new ElementWithTriggersExporter(new NullExporter(), options)
		};
	}

	internal static INestedElementExporter GetActionGroupExporter(IExporterLocator exporterLocator, AlExportOptions options)
	{
		PropertyType[] inlineProperties = new PropertyType[1] { PropertyType.Name };
		return new NestedElementExporter(exporterLocator)
		{
			HeaderExporter = new ElementHeaderExporter(actionPropertiesExporter, inlineProperties),
			PreChildrenParentExporter = new ElementWithPropertiesExporter(actionPropertiesExporter, new NullExporter()),
			PostChildrenParentExporter = new ElementWithTriggersExporter(new NullExporter(), options)
		};
	}

	internal static INestedElementExporter GetActionCollectionExporter(IExporterLocator exporterLocator, AlExportOptions options)
	{
		return new NestedElementExporter(exporterLocator)
		{
			HeaderExporter = new ElementCollectionHeaderExporter(),
			PreChildrenParentExporter = new ElementWithPropertiesExporter(actionPropertiesExporter, new NullExporter()),
			PostChildrenParentExporter = new ElementWithTriggersExporter(new NullExporter(), options)
		};
	}

	internal static INestedElementExporter GetControlCollectionExporter(IExporterLocator exporterLocator, AlExportOptions options)
	{
		return new NestedElementExporter(exporterLocator)
		{
			HeaderExporter = new ElementCollectionHeaderExporter(),
			PreChildrenParentExporter = new ElementWithPropertiesExporter(actionPropertiesExporter, new NullExporter()),
			PostChildrenParentExporter = new ElementWithTriggersExporter(new NullExporter(), options)
		};
	}

	internal static string GetElementTypeName(IElement child)
	{
		if (child.ElementTypeInfo.ElementType == ElementType.Control)
		{
			if (child.IsUserControl())
			{
				return "UserControl".ToLowerInvariant();
			}
			if (child.IsLabelControl())
			{
				return "Label".ToLowerInvariant();
			}
			return child.GetStringProperty(PropertyType.ControlType).ToLowerInvariant();
		}
		return child.GetStringProperty(PropertyType.ActionType).ToLowerInvariant();
	}
}
