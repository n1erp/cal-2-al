using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.ClientDesigner;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class DesignerRequest : IExtensibleDataObject
{
	private ExtensionDataObject unknownDataBag;

	public virtual ExtensionDataObject ExtensionData
	{
		get
		{
			return unknownDataBag;
		}
		set
		{
			unknownDataBag = value;
		}
	}

	[DataMember]
	public NavDesignerRequest Action { get; set; }

	public DesignerRequest(NavDesignerRequest action)
	{
		Action = action;
	}
}
