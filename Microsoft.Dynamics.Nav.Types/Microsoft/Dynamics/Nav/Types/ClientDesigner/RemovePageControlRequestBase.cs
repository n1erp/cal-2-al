using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RemovePageControlRequestBase : NavPageDesignerRequest
{
	[DataMember]
	public string ControlName { get; set; }

	public RemovePageControlRequestBase(int pageId, string controlName)
		: base(pageId)
	{
		ControlName = controlName;
	}
}
