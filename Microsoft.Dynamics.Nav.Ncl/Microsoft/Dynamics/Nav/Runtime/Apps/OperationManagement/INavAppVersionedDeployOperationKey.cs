using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal interface INavAppVersionedDeployOperationKey : INavAppDeployOperationKey, INavAppOperationKey
{
	Version Version { get; set; }
}
