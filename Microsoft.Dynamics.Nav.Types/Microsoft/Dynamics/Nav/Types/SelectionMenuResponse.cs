using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SelectionMenuResponse : IExtensibleDataObject
{
	private int selectedMenuIndex;

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
	public int SelectedMenuIndex
	{
		get
		{
			return selectedMenuIndex;
		}
		set
		{
			selectedMenuIndex = value;
		}
	}
}
