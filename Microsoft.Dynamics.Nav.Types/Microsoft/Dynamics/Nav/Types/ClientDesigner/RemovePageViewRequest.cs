using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RemovePageViewRequest : RemovePageControlRequestBase
{
	public RemovePageViewRequest(int pageId, string viewName)
		: base(pageId, viewName)
	{
	}
}
