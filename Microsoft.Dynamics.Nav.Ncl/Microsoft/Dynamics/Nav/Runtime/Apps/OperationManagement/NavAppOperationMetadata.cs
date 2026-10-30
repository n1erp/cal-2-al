using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal abstract class NavAppOperationMetadata : INavAppOperationKey
{
	public abstract string Key { get; }

	public Guid JobId { get; set; }
}
