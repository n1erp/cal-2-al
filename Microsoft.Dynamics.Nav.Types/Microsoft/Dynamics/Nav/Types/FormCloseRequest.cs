using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class FormCloseRequest : ServerResponse
{
	private NavRecordState state;

	[DataMember]
	public NavRecordState State
	{
		get
		{
			return state;
		}
		set
		{
			state = value;
		}
	}
}
