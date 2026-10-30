using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal abstract class NavAppDeployOperationMetadata : NavAppOperationMetadata
{
	public string TenantId { get; set; }

	public Guid AppId { get; set; }

	public string Language { get; set; }

	public string Schedule { get; set; }

	public string AppName { get; set; }

	public string AppPublisher { get; set; }
}
