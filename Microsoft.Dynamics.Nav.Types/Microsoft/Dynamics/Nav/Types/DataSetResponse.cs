using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Data;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class DataSetResponse : ServerResponse
{
	[DataMember(Order = 1)]
	private readonly NavDataSet dataSet;

	public NavDataSet DataSet => dataSet;

	[DataMember(Order = 2)]
	public NavRecordState State { get; set; }

	public DataSetResponse(NavDataSet dataSet, NavRecordState state)
	{
		if (dataSet == null)
		{
			throw new ArgumentNullException("dataSet");
		}
		this.dataSet = dataSet;
		State = state;
	}
}
