using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class CreatePageViewRequest : ViewRequestBase
{
	[DataMember]
	public string RelativeViewName { get; set; }

	[DataMember]
	public bool SoftValidation { get; set; }

	public CreatePageViewRequest(DesignerLevels designerLevel, int pageId, string newViewCaption, ViewInformation information, string relativeViewName, bool softValidation)
		: base(designerLevel, pageId, newViewCaption, information)
	{
		RelativeViewName = relativeViewName;
		SoftValidation = softValidation;
	}
}
