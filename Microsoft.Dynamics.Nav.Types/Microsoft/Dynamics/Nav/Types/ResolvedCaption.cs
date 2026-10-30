using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ResolvedCaption
{
	[DataMember]
	public int FieldId { get; set; }

	[DataMember]
	public string Caption { get; set; }
}
