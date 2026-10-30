using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RemovePageFieldRequest : RemovePageControlRequestBase
{
	public RemovePageFieldRequest(int pageId, string controlName)
		: base(pageId, controlName)
	{
	}
}
