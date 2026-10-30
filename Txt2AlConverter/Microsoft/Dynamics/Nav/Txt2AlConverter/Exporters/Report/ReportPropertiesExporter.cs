using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Report;

internal class ReportPropertiesExporter : ElementWithPropertiesExporter
{
	public ReportPropertiesExporter(IElementExporter nextExporter)
		: base(new ReportPropertiesConverter(), nextExporter)
	{
	}

	protected override void ExportProperties(IElement element, IAlWriter writer)
	{
		SetLayoutProperties(element, writer);
		base.ExportProperties(element, writer);
	}

	private static void SetLayoutProperties(IElement element, IAlWriter writer)
	{
		bool flag = element.GetPropertySetState(PropertyType.DefaultLayout);
		if (element.HasRdlc())
		{
			if (!flag)
			{
				writer.WriteProperty(PropertyKind.DefaultLayout.ToString(), DefaultLayoutKind.RDLC.ToString());
				flag = true;
			}
			string value = "'./" + writer.State.FileNameExporter.GetRdlcLayoutFileName(element) + "'";
			writer.WriteProperty(PropertyKind.RDLCLayout.ToString(), value);
		}
		if (element.HasWordLayout())
		{
			if (!flag)
			{
				writer.WriteProperty(PropertyKind.DefaultLayout.ToString(), DefaultLayoutKind.Word.ToString());
			}
			string value2 = "'./" + writer.State.FileNameExporter.GetWordLayoutFileName(element) + "'";
			writer.WriteProperty(PropertyKind.WordLayout.ToString(), value2);
		}
	}
}
