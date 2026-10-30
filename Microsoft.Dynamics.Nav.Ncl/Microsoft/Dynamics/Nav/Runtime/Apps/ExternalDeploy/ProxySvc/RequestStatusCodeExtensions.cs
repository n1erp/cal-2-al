using Microsoft.Dynamics.Nav.ExtensionProxySvc.Client;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.ExternalDeploy.ProxySvc;

internal static class RequestStatusCodeExtensions
{
	public static NavAppTenantOperationFields.NavAppOperationStatus ToOperationStatus(this RequestStatusCode resultStatus)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected I4, but got Unknown
		if ((int)resultStatus <= 103)
		{
			if ((int)resultStatus == 1)
			{
				return NavAppTenantOperationFields.NavAppOperationStatus.InProgress;
			}
			if ((int)resultStatus == 2)
			{
				return NavAppTenantOperationFields.NavAppOperationStatus.Completed;
			}
			switch (resultStatus - 100)
			{
			case 1:
			case 3:
				break;
			case 0:
				return NavAppTenantOperationFields.NavAppOperationStatus.NotFound;
			default:
				goto IL_004c;
			}
		}
		else if ((int)resultStatus != 200 && resultStatus - 400 > 2 && (int)resultStatus != 500)
		{
			goto IL_004c;
		}
		return NavAppTenantOperationFields.NavAppOperationStatus.Failed;
		IL_004c:
		return NavAppTenantOperationFields.NavAppOperationStatus.Unknown;
	}
}
