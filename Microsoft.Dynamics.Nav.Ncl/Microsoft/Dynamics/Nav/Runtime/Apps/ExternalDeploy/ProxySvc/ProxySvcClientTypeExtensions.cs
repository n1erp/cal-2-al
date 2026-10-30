using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.ExtensionProxySvc.Client;
using Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.ExternalDeploy.ProxySvc;

internal static class ProxySvcClientTypeExtensions
{
	public static string GetRefreshResponseDetails(this IDeploymentStatusResponse response)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		string result = string.Empty;
		switch (response.Status.ToOperationStatus())
		{
		case NavAppTenantOperationFields.NavAppOperationStatus.Completed:
			result = Lang.NavAppExternalPublishCompleted.FormatWithResolvedCulture();
			break;
		case NavAppTenantOperationFields.NavAppOperationStatus.InProgress:
		case NavAppTenantOperationFields.NavAppOperationStatus.Failed:
			result = response.Details.FormatWithResolvedCulture();
			break;
		case NavAppTenantOperationFields.NavAppOperationStatus.NotFound:
			result = Lang.NavAppOperationNotFound.FormatWithResolvedCulture();
			break;
		}
		return result;
	}

	public static string GetJobResponseDetails(this IDeploymentJobResponse response)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		string result = string.Empty;
		switch (((IDeploymentStatusResponse)response).Status.ToOperationStatus())
		{
		case NavAppTenantOperationFields.NavAppOperationStatus.Completed:
			result = Lang.NavAppExternalPublishCompleted.FormatWithResolvedCulture();
			break;
		case NavAppTenantOperationFields.NavAppOperationStatus.InProgress:
		case NavAppTenantOperationFields.NavAppOperationStatus.Failed:
			result = ((IDeploymentStatusResponse)response).Details.FormatWithResolvedCulture();
			break;
		case NavAppTenantOperationFields.NavAppOperationStatus.NotFound:
			result = Lang.NavAppOperationNotFound.FormatWithResolvedCulture();
			break;
		}
		return result;
	}

	public static PackageDeploymentSchedule ToClientType(this DeploymentSchedule schedule)
	{
		return schedule switch
		{
			DeploymentSchedule.NextMajorUpdate => (PackageDeploymentSchedule)1, 
			DeploymentSchedule.NextMinorUpdate => (PackageDeploymentSchedule)2, 
			_ => (PackageDeploymentSchedule)0, 
		};
	}
}
