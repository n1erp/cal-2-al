using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavAutomationHandle
{
	[DataMember]
	private int handle;

	[DataMember]
	private bool isComObject;

	public int Handle
	{
		get
		{
			return handle;
		}
		set
		{
			handle = value;
		}
	}

	public bool IsComObject
	{
		get
		{
			return isComObject;
		}
		set
		{
			isComObject = value;
		}
	}

	public NavAutomationHandle(int handle, bool isComObject)
	{
		this.handle = handle;
		this.isComObject = isComObject;
	}
}
