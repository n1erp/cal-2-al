using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MoveViewPageFieldRequest : MovePageFieldRequest
{
	[DataMember]
	public string ViewName { get; set; }

	public MoveViewPageFieldRequest(int pageId, string controlName, string parentControlName, string relativeControlName, string viewName)
		: base(pageId, controlName, parentControlName, relativeControlName)
	{
		ViewName = viewName;
	}
}
