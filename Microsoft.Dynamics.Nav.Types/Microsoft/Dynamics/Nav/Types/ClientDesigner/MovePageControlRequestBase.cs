using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MovePageControlRequestBase : NavPageDesignerRequest
{
	[DataMember]
	public string ControlName { get; set; }

	[DataMember]
	public string ParentControlName { get; set; }

	[DataMember]
	public string RelativeControlName { get; set; }

	public MovePageControlRequestBase(int pageId, string controlName, string parentControlName, string relativeControlName)
		: base(pageId)
	{
		ControlName = controlName;
		ParentControlName = parentControlName;
		RelativeControlName = relativeControlName;
	}
}
