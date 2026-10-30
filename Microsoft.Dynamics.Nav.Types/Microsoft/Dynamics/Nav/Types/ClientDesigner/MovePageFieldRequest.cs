using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MovePageFieldRequest : MovePageControlRequestBase
{
	public MovePageFieldRequest(int pageId, string controlName, string parentControlName, string relativeControlName)
		: base(pageId, controlName, parentControlName, relativeControlName)
	{
	}
}
