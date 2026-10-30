using System;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.IO.Txt;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Page;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.XmlPort;

internal sealed class XmlPortSectionsExporter : ElementWithSectionsExporter
{
	private readonly ExporterLocator serviceLocator;

	protected override IExporterLocator ExporterLocator => serviceLocator;

	public XmlPortSectionsExporter(IElementExporter nextExporter, AlExportOptions options)
		: base(nextExporter)
	{
		serviceLocator = new ExporterLocator(GetChildExporterKey);
		serviceLocator.AddService("requestpage", GetRequestPageExporter(options));
		serviceLocator.AddService("tableelement", GetXmlPortElementExporter(options));
		serviceLocator.AddService("fieldelement", GetXmlPortElementExporter(options));
		serviceLocator.AddService("textelement", GetXmlPortElementExporter(options));
		serviceLocator.AddService("fieldattribute", GetXmlPortElementExporter(options));
		serviceLocator.AddService("textattribute", GetXmlPortElementExporter(options));
	}

	protected override string GetSectionName(ElementRelationExport section)
	{
		string text = section.ContainerName.ToLowerInvariant();
		if (string.Equals(text, "elements", StringComparison.OrdinalIgnoreCase))
		{
			text = "schema";
		}
		return text;
	}

	private static string GetChildExporterKey(IElement child)
	{
		if (child.ElementTypeInfo.ElementType == ElementType.XmlPortElement)
		{
			string stringProperty = child.GetStringProperty(PropertyType.SourceType);
			return string.Concat(str1: child.GetStringProperty(PropertyType.NodeType).ToLowerInvariant(), str0: stringProperty.ToLowerInvariant());
		}
		return child.ElementTypeInfo.Name.ToLowerInvariant();
	}

	private static INestedElementExporter GetRequestPageExporter(AlExportOptions options)
	{
		return new TerminallNestedElementExporter(new PageContentExporter(options));
	}

	private INestedElementExporter GetXmlPortElementExporter(AlExportOptions options)
	{
		return new NestedElementExporter(ExporterLocator)
		{
			PreChildrenParentExporter = new ElementWithPropertiesExporter(new XmlPortPropertiesConverter(), new NullExporter()),
			PostChildrenParentExporter = new ElementWithTriggersExporter(new NullExporter(), options),
			HeaderExporter = new XmlPortElementHeaderExporter()
		};
	}
}
