using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RestoreActionDefaultsRequest : NavPageDesignerRequest
{
	public RestoreActionDefaultsRequest(int pageId)
		: base(pageId)
	{
	}
}
