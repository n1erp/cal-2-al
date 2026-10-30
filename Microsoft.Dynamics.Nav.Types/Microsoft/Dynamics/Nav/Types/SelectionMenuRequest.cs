using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SelectionMenuRequest : IExtensibleDataObject
{
	private string[] options;

	private int defaultSelection;

	private ExtensionDataObject unknownDataBag;

	public virtual ExtensionDataObject ExtensionData
	{
		[DebuggerStepThrough]
		get
		{
			return unknownDataBag;
		}
		[DebuggerStepThrough]
		set
		{
			unknownDataBag = value;
		}
	}

	[DataMember]
	public string[] Options
	{
		[DebuggerStepThrough]
		get
		{
			return options;
		}
		[DebuggerStepThrough]
		set
		{
			options = value;
		}
	}

	[DataMember]
	public int DefaultSelection
	{
		[DebuggerStepThrough]
		get
		{
			return defaultSelection;
		}
		[DebuggerStepThrough]
		set
		{
			defaultSelection = value;
		}
	}
}
