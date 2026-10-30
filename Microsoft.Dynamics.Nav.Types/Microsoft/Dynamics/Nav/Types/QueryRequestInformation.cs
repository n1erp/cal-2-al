using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class QueryRequestInformation : DataSetRequestInformation
{
	[DataMember(Name = "MetadataToken")]
	public long MetadataToken { get; set; }

	public QueryRequestInformation()
		: base(DataSetRequestType.Query)
	{
	}
}
