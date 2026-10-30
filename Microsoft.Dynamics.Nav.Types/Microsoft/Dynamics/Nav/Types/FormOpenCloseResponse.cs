using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Data;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class FormOpenCloseResponse : ServerResponse
{
	private NavRecordState[] states;

	private NavDataSet dataSet;

	[DataMember]
	public NavRecordState[] States
	{
		get
		{
			return states;
		}
		set
		{
			states = value;
		}
	}

	[DataMember]
	public NavDataSet DataSet
	{
		get
		{
			return dataSet;
		}
		set
		{
			dataSet = value;
		}
	}
}
