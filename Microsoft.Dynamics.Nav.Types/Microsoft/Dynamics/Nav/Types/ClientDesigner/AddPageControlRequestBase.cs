using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class AddPageControlRequestBase : NavPageDesignerRequest
{
	[DataMember]
	public string ParentControlName { get; set; }

	[DataMember]
	public string RelativeControlName { get; set; }

	public AddPageControlRequestBase(int pageId, string parentControlName, string relativeControlName)
		: base(pageId)
	{
		ParentControlName = parentControlName;
		RelativeControlName = relativeControlName;
	}
}
