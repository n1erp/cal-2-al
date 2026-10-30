using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal class NavAppOperationRequestReference
{
	public NavAppTenantOperationFields.NavAppOperationType OperationType { get; set; }

	public NavAppTenantOperationFields.NavAppOperationStatus Status { get; set; }

	public string Details { get; set; }

	public NavAppOperationMetadataVersion MetadataVersion { get; set; }

	public NavAppOperationMetadata Metadata { get; set; }

	public string Description { get; set; }
}
