using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class FormOpenResponse : FormOpenCloseResponse
{
	[DataMember]
	public TableCaptions[] TableCaptions { get; set; }

	[DataMember]
	public FormProperties[] FormProperties { get; set; }
}
