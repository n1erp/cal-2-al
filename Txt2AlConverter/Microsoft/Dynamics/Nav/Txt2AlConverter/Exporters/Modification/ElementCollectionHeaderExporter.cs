using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal sealed class ElementCollectionHeaderExporter : IElementExporter
{
	public void Export(IElement element, IAlWriter writer)
	{
		writer.Write("addfirst");
		writer.Write('(');
		writer.Write(GetSectionName(element));
		writer.WriteLine(')');
		writer.PendingEmptyLine = false;
	}

	private static string GetSectionName(IElement element)
	{
		string text = string.Empty;
		if (element.GetPropertySetState(PropertyType.ContainerType))
		{
			text = AlUtilities.GetControlNameFromControlContainerType(element.GetStringProperty(PropertyType.ContainerType));
		}
		else if (element.GetPropertySetState(PropertyType.ActionContainerType))
		{
			text = AlUtilities.GetActionNameFromActionContainerType(element.GetStringProperty(PropertyType.ActionContainerType));
		}
		return text.ToLowerInvariant();
	}
}
