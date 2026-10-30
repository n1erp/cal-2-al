namespace Microsoft.Dynamics.Nav.Runtime;

public class ReportSaveAsWordRenderer : ReportSaveAsXmlTreeRenderer
{
	public NavOutStream WordOutStream { get; private set; }

	public ReportSaveAsWordRenderer(NavReport report, string fileName, NavOutStream wordOutStream)
		: base(report, fileName, null)
	{
		WordOutStream = wordOutStream;
	}
}
