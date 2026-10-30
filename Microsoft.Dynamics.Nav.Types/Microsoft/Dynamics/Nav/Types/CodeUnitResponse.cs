using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Data;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class CodeUnitResponse : ServerResponse
{
	private object[] args;

	private NavRecordState dataSetState;

	private NavDataSet serviceDataSet;

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
	public NavRecordState DataSetState
	{
		get
		{
			return dataSetState;
		}
		set
		{
			dataSetState = value;
		}
	}

	[DataMember]
	public NavDataSet DataSet
	{
		get
		{
			return serviceDataSet;
		}
		set
		{
			serviceDataSet = value;
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
