using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SetActionPropertyRequest : SetControlPropertyRequestBase
{
	public SetActionPropertyRequest(int pageId, string actionName, string name, object value)
		: base(pageId, actionName, name, value)
	{
	}
}
