using System;
using System.IO;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.ExtensionProxySvc.Client;
using Microsoft.Dynamics.Nav.ExtensionProxySvc.Client.Exceptions;
using Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;
using Microsoft.Dynamics.Nav.Runtime.Utility;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.ExternalDeploy.ProxySvc;

internal sealed class ProxySvcExternalDeployer : INavAppExternalDeployer, IDisposable
{
	private readonly IProxySvcClient client;

	public ProxySvcExternalDeployer(IProxySvcClient client)
	{
		this.client = client ?? throw new ArgumentNullException("client");
	}

	public UploadPackageOperationResponse UploadPackage(Stream packageStream, string tenantId, Version releaseVersion, string language, DeploymentSchedule schedule)
	{
		return HandleProxyOperation(() =>
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			return client.UploadPackage(packageStream, schedule.ToClientType(), InstallLanguage.Create(language), tenantId, releaseVersion);
		}, new UploadPackageOperationResponse(), (IDeploymentJobResponse uploadResult, UploadPackageOperationResponse response) =>
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			response.JobId = uploadResult.JobId;
			response.Status = ((IDeploymentStatusResponse)uploadResult).Status.ToOperationStatus();
			response.Details = uploadResult.GetJobResponseDetails();
		});
	}

	public DeployTargetOperationResponse DeployTarget(AppId appId, string tenantId, Version releaseVersion, string language)
	{
		return HandleProxyOperation(() => client.DeployTarget(((WrappedValue<Guid>)(object)appId).Value, InstallLanguage.Create(language), tenantId, releaseVersion), new DeployTargetOperationResponse(), (IDeployTargetJobResponse deployResult, DeployTargetOperationResponse response) =>
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			response.JobId = ((IDeploymentJobResponse)deployResult).JobId;
			response.Status = ((IDeploymentStatusResponse)deployResult).Status.ToOperationStatus();
			response.Details = ((IDeploymentJobResponse)(object)deployResult).GetJobResponseDetails();
			response.AppName = deployResult.AppName;
			response.AppPublisher = deployResult.AppPublisher;
		});
	}

	public RefreshOperationResponse RefreshDeploymentStatus(Guid jobId)
	{
		return HandleProxyOperation(() => client.GetDeploymentStatus(jobId), new RefreshOperationResponse(), (IDeploymentStatusResponse refreshResult, RefreshOperationResponse response) =>
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			response.Status = refreshResult.Status.ToOperationStatus();
			response.Details = refreshResult.GetRefreshResponseDetails();
		});
	}

	public RefreshOperationDetailsResponse RefreshDetailedDeploymentStatus(Guid jobId)
	{
		return HandleProxyOperation(() => client.GetDeploymentDetailedStatus(jobId), new RefreshOperationDetailsResponse(), (IDeploymentDetailedStatusResponse detailsResult, RefreshOperationDetailsResponse response) =>
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			response.Status = detailsResult.ErrorCode.ToOperationStatus();
			response.Details = detailsResult.Details;
		});
	}

	private static TOperation HandleProxyOperation<TOperation, TClient>(Func<TClient> clientOperation, TOperation mutableResponse, Action<TClient, TOperation> onSuccess) where TOperation : OperationResponse where TClient : class
	{
		//IL_0018: Expected Obj, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected Obj, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected Obj, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected Obj, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		RequestStatusCode resultStatus;
		string details;
		try
		{
			TClient arg = clientOperation();
			onSuccess(arg, mutableResponse);
			return mutableResponse;
		}
		catch (ProxyServiceBadContentException ex)
		{
			ProxyServiceBadContentException ex2 = ex;
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001XU", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "Extension Proxy Service bad content exception. StatusCode -- {0}; Error Message -- {1}", ((ProxyServiceException)ex2).StatusCode, ((Exception)(object)ex2).Message);
			resultStatus = ((ProxyServiceException)ex2).StatusCode;
			details = Lang.NavAppExternalDeployMalformedResponse.FormatWithResolvedCulture();
		}
		catch (ProxyServiceHttpException ex3)
		{
			ProxyServiceHttpException ex4 = ex3;
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001XV", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "Extension Proxy Service Http Exception. StatusCode -- {0}; HttpCode -- {1}; Error Message -- {2}", ((ProxyServiceException)ex4).StatusCode, ex4.OriginalErrorCode, ((Exception)(object)ex4).Message);
			resultStatus = ((ProxyServiceException)ex4).StatusCode;
			details = Lang.NavAppExternalDeployUnexpectedHttpError.FormatWithResolvedCulture(ex4.OriginalErrorCode);
		}
		catch (ProxyServiceTimeoutException ex5)
		{
			ProxyServiceTimeoutException ex6 = ex5;
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001XW", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "Extension Proxy Service Timeout Exception. StatusCode -- {0}; Error Message -- {1}", ((ProxyServiceException)ex6).StatusCode, ((Exception)(object)ex6).Message);
			resultStatus = ((ProxyServiceException)ex6).StatusCode;
			details = Lang.NavAppExternalDeployOperationTimeout.FormatWithResolvedCulture();
		}
		catch (ProxyServiceException ex7)
		{
			ProxyServiceException ex8 = ex7;
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001XX", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "Extension Proxy Service Generic Proxy Exception. StatusCode -- {0}; Error Message -- {1}", ex8.StatusCode, ((Exception)(object)ex8).Message);
			resultStatus = ex8.StatusCode;
			details = Lang.NavAppExternalDeployUnexpectedError.FormatWithResolvedCulture(((Exception)(object)ex8).Message);
		}
		catch (Exception ex9)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001XY", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "Extension Proxy Service Unhandled exception during operation execution. Error Message -- {0}", ex9.Message);
			resultStatus = (RequestStatusCode)0;
			details = Lang.NavAppExternalDeployUnexpectedError.FormatWithResolvedCulture(ex9.Message);
		}
		mutableResponse.Status = resultStatus.ToOperationStatus();
		mutableResponse.Details = details;
		return mutableResponse;
	}

	public void Dispose()
	{
		((IDisposable)client)?.Dispose();
	}
}
