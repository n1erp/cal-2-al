using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class PromotePageActionRequest : NavPageDesignerRequest
{
	[DataMember]
	public string ActionName { get; set; }

	[DataMember]
	public string PromotedCategory { get; set; }

	[DataMember]
	public bool? PromotedOnly { get; set; }

	public PromotePageActionRequest(int pageId, string actionName, string promotedCategory, bool? promotedOnly)
		: base(pageId)
	{
		ActionName = actionName;
		PromotedCategory = promotedCategory;
		PromotedOnly = promotedOnly;
	}
}
