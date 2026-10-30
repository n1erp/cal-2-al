using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class FormProperties : IExtensibleDataObject
{
	[DataMember]
	public bool InsertAllowed { get; set; }

	[DataMember]
	public bool ModifyAllowed { get; set; }

	[DataMember]
	public bool DeleteAllowed { get; set; }

	[DataMember]
	public bool IsAbleToOverwriteAllowedProperties { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }
}
