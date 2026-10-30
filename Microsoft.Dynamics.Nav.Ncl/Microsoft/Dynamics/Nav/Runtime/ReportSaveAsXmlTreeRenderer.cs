namespace Microsoft.Dynamics.Nav.Runtime;

public class ReportSaveAsXmlTreeRenderer : SaveAsXmlTreeRenderer
{
	private readonly NavReport report;

	public ReportSaveAsXmlTreeRenderer(NavReport report, string fileName, NavOutStream outStream)
		: base(GenerateFileName(report.Caption, fileName), report, outStream)
	{
		this.report = report;
		this.report.IsSaveAsXmlTreeProcessor = true;
	}

	protected override void Render()
	{
		Close();
	}

	private static string GenerateFileName(string report, string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return report + ".xml";
		}
		return fileName;
	}
}
