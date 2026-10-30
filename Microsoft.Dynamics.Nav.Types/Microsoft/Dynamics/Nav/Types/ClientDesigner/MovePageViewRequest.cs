using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MovePageViewRequest : MovePageControlRequestBase
{
	public MovePageViewRequest(int pageId, string viewName, string relativeViewName)
		: base(pageId, viewName, null, relativeViewName)
	{
	}
}
