using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class LaunchDebuggerResponse : ServerResponse
{
	private object[] args;

	private object returnValue;

	[DataMember]
	public object[] Args
	{
		get
		{
			return args;
		}
		set
		{
			args = value;
		}
	}

	[DataMember]
	public object ReturnValue
	{
		get
		{
			return returnValue;
		}
		set
		{
			returnValue = value;
		}
	}
}
