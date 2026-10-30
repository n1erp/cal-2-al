using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal interface INavAppDeployOperationKey : INavAppOperationKey
{
	string TenantId { get; set; }

	Guid AppId { get; set; }
}
