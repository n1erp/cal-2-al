using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ViewRequestBase : NavPageDesignerRequest
{
	[DataMember]
	public DesignerLevels DesignerLevel { get; set; }

	[DataMember]
	public string ViewName { get; set; }

	[DataMember]
	public ViewInformation ViewInformation { get; set; }

	public ViewRequestBase(DesignerLevels designerLevel, int pageId, string viewName, ViewInformation information)
		: base(pageId)
	{
		DesignerLevel = designerLevel;
		ViewName = viewName;
		ViewInformation = information;
	}
}
