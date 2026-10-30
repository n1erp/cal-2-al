using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RemovePageActionRequest : RemovePageControlRequestBase
{
	public RemovePageActionRequest(int pageId, string actionName)
		: base(pageId, actionName)
	{
	}
}
