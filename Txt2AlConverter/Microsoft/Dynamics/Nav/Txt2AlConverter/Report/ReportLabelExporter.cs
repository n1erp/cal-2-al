using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Report;

internal sealed class ReportLabelExporter : INestedElementExporter
{
	private readonly PropertyType[] inlineProperties;

	private readonly IPropertiesConverter propertiesConverter;

	public ReportLabelExporter(IPropertiesConverter propertiesConverter)
	{
		inlineProperties = new PropertyType[2]
		{
			PropertyType.Name,
			PropertyType.CaptionML
		};
		this.propertiesConverter = propertiesConverter;
	}

	public void Export(ElementTreeNode elementTreeNode, IAlWriter writer)
	{
		IElement element = elementTreeNode.Element;
		PropertyTypeInfo propertyInfo = elementTreeNode.Element.GetPropertyInfo(PropertyType.CaptionML);
		if (AlUtilities.IsMultilanguageProperty(element, propertyInfo.PropertyType))
		{
			TranslationUtilities.WriteReportLabel(writer, element);
			return;
		}
		writer.Write(AlUtilities.GetSectionTypeName(element).ToLowerInvariant());
		writer.Write('(');
		AlUtilities.WriteElementHeaderContent(writer, element, inlineProperties, propertiesConverter);
		writer.WriteLine(')');
		writer.PendingEmptyLine = false;
	}
}
