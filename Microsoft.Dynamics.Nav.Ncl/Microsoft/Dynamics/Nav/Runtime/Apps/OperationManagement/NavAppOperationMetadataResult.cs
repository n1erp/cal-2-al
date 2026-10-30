using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal abstract class NavAppOperationMetadataResult : NavAppOperationResult
{
	public string TenantId { get; set; }

	public Guid AppId { get; set; }
}
