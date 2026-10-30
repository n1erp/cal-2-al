using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[KnownType(typeof(SerializableObjectContract))]
[KnownType(typeof(JsonSerializableObjectContract))]
public class FieldRequest : RecordRequest
{
	[DataMember]
	public int ControlId { get; set; }

	[DataMember]
	public int ControlEventId { get; set; }

	[DataMember]
	public int FieldId { get; set; }

	[DataMember]
	public object[] Args { get; set; }
}
