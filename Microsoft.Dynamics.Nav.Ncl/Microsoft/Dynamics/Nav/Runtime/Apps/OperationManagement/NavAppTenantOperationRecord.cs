using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal class NavAppTenantOperationRecord : NavAppOperationRequestReference
{
	public Guid OperationId { get; set; }

	public DateTime StartedOn { get; set; }

	public string MetdataKey => Metadata?.Key;
}
