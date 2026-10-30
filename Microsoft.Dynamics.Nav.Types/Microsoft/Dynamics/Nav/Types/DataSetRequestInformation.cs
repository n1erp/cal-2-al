using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[KnownType(typeof(ReportRequestInformation))]
[KnownType(typeof(QueryRequestInformation))]
public abstract class DataSetRequestInformation
{
	[DataMember(Name = "Id")]
	public int Id { get; set; }

	[DataMember(Name = "Caption")]
	public string Caption { get; set; }

	[DataMember(Name = "Type")]
	public DataSetRequestType RequestType { get; set; }

	protected DataSetRequestInformation(DataSetRequestType type)
	{
		RequestType = type;
	}
}
