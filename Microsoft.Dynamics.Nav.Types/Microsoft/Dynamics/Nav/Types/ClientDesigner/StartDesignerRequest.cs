using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class StartDesignerRequest : NavDesignerRequest
{
	[DataMember]
	public DesignerLevels DesignerLevel { get; set; }

	public StartDesignerRequest(DesignerLevels designerLevel)
	{
		DesignerLevel = designerLevel;
	}
}
