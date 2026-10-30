using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SetViewPropertyRequest : SetControlPropertyRequestBase
{
	[DataMember]
	public DesignerLevels? DesignerLevel { get; set; }

	public SetViewPropertyRequest(DesignerLevels? designerLevel, int pageId, string viewName, string name, object value)
		: base(pageId, viewName, name, value)
	{
		DesignerLevel = designerLevel;
	}
}
