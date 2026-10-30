using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Data;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class DataSetRequest : ServerCallbackRequest
{
	[DataMember(Name = "Cancel")]
	public bool Cancel { get; set; }

	[DataMember(Name = "ExecutionGuid")]
	public Guid ExecutionGuid { get; set; }

	[DataMember(Name = "FinalDataSet")]
	public bool FinalDataSet { get; set; }

	[DataMember(Name = "DataSet")]
	public NavDataSet DataSet { get; set; }

	[DataMember(Name = "DataSetRequestInformation")]
	public DataSetRequestInformation Information { get; set; }

	public override void Process(IClientCallbackHandler callbackHandler)
	{
		if (callbackHandler == null)
		{
			throw new ArgumentNullException("callbackHandler");
		}
		callbackHandler.DataSetPageReady(this);
	}
}
