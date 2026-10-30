using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlCData : NavXmlText
{
	public new static NavXmlCData Default => new NavXmlCData(null);

	public override NavNclType NclType => NavNclType.NavXmlCData;

	public new static NavXmlCData ALCreate(string value)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() => new NavXmlCData(new XCData(value)));
	}

	internal NavXmlCData(XCData value)
		: base(value)
	{
	}
}
