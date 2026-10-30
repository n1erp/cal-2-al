using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RestoreViewControlDefaultsRequest : RestoreControlDefaultsRequest
{
	[DataMember]
	public string ViewName { get; set; }

	public RestoreViewControlDefaultsRequest(int pageId, string viewName)
		: base(pageId)
	{
		ViewName = viewName;
	}
}
