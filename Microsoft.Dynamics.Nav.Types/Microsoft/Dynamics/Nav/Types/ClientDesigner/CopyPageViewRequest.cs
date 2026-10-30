using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class CopyPageViewRequest : CreatePageViewRequest
{
	public CopyPageViewRequest(DesignerLevels designerLevel, int pageId, string newViewCaption, ViewInformation information, string originalViewName)
		: base(designerLevel, pageId, newViewCaption, information, originalViewName, softValidation: false)
	{
	}
}
