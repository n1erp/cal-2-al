using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Report;

internal sealed class RdlcLayoutExporter : IElementExporter
{
	public void Export(IElement element, IAlWriter writer)
	{
		bool flag = true;
		foreach (string rdlDataLine in element.GetRdlDataLines())
		{
			if (flag)
			{
				writer.WriteLine(rdlDataLine.TrimStart());
				flag = false;
			}
			else
			{
				writer.WriteLine(rdlDataLine);
			}
		}
	}
}
