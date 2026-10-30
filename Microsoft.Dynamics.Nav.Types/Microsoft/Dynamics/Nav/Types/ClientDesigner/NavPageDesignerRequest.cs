using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class NavPageDesignerRequest : NavDesignerRequest
{
	[DataMember]
	public int PageId { get; set; }

	protected NavPageDesignerRequest(int pageId)
	{
		PageId = pageId;
	}
}
