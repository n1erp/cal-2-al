using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class UpdatePageViewRequest : ViewRequestBase
{
	public UpdatePageViewRequest(DesignerLevels designerLevel, int pageId, string viewName, ViewInformation information)
		: base(designerLevel, pageId, viewName, information)
	{
	}
}
