using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal class NavAppTenantOperationManagement : INavAppTenantOperationManagement
{
	private readonly INavAppTenantOperationRepository tenantRepository;

	private readonly INavAppOperationInvoker operationInvoker;

	private readonly IEnvironmentInfoProvider infoProvider;

	public NavAppTenantOperationManagement(INavAppTenantOperationRepository tenantRepository, INavAppOperationInvoker operationInvoker, IEnvironmentInfoProvider infoProvider)
	{
		this.tenantRepository = tenantRepository ?? throw new ArgumentNullException("tenantRepository");
		this.operationInvoker = operationInvoker ?? throw new ArgumentNullException("operationInvoker");
		this.infoProvider = infoProvider ?? throw new ArgumentNullException("infoProvider");
	}

	public void RefreshOperationStatus(Guid operationId)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Refreshing Status Operation -- OperationId:{0};", operationId);
		using (DiagnosticsResolver.CreateTraceScope("00001RF", DiagnosticParameter.SystemMetadata(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			NavAppTenantOperationRecord operationForId = tenantRepository.GetOperationForId(operationId);
			if (operationForId != null)
			{
				try
				{
					RefreshOperationResponse refreshOperationResponse = operationInvoker.RefreshOperation(operationForId);
					tenantRepository.UpdateOperationStatus(operationId, refreshOperationResponse.Status, refreshOperationResponse.Details);
					return;
				}
				catch (Exception ex)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001XZ", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "Package deploy operation status refresh failed -- OperationId:{0}, JobId:{1}, Exception message : {2}", operationForId.OperationId, operationForId.Metadata?.JobId, ex.Message);
					throw;
				}
			}
		}
	}

	public NavAppDeployOperationMetadata GetDeployOperationMetadata(Guid operationId)
	{
		NavAppDeployOperationMetadata navAppDeployOperationMetadata = null;
		string content = string.Format(CultureInfo.InvariantCulture, "Retriving deploy operation metadata-- OperationId:{0};", operationId);
		using (DiagnosticsResolver.CreateTraceScope("00001Y8", DiagnosticParameter.SystemMetadata(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			try
			{
				navAppDeployOperationMetadata = tenantRepository.GetOperationForId(operationId)?.Metadata as NavAppDeployOperationMetadata;
			}
			catch (Exception ex)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001YD", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "Package deploy operation metadta retrival failed -- OperationId:{0}, exception message:{1}", operationId, ex.Message);
			}
		}
		if (navAppDeployOperationMetadata == null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001YE", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "Package deploy operation metadta retrival failed -- OperationId:{0}", operationId);
		}
		return navAppDeployOperationMetadata;
	}

	public string GetOperationDetailedStatusMessage(Guid operationId)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Retriving Operation Detailed status -- OperationId:{0};", operationId);
		using (DiagnosticsResolver.CreateTraceScope("00001Y1", DiagnosticParameter.SystemMetadata(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			NavAppTenantOperationRecord navAppTenantOperationRecord = tenantRepository.GetOperationForId(operationId) ?? throw new NavAppException(Lang.NoNavAppOperationRecordFoundWithGivenId.FormatWithResolvedCulture(operationId));
			try
			{
				RefreshOperationDetailsResponse detailedOperationStatus = operationInvoker.GetDetailedOperationStatus(navAppTenantOperationRecord);
				return detailedOperationStatus.Details;
			}
			catch (Exception ex)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001Y0", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "Package deploy operation Detailed status retrival failed -- OperationId:{0}, JobId:{1}, Exception message : {2}", navAppTenantOperationRecord.OperationId, navAppTenantOperationRecord.Metadata?.JobId, ex.Message);
				throw;
			}
		}
	}

	public Guid UploadPackage(string language, INavAppReference appInfo, DeploymentSchedule deploymentSchedule, Stream packageStream)
	{
		string tenantId = tenantRepository.TenantId;
		string content = string.Format(CultureInfo.InvariantCulture, "Uploading Package Operation -- Tenant:{0}; Schedule:{1}; Language:{2}; AppId:{3}, Version:{4}", tenantId, deploymentSchedule, language, (appInfo != null) ? appInfo.AppId : null, (appInfo != null) ? appInfo.Version : null);
		using (DiagnosticsResolver.CreateTraceScope("00001RG", DiagnosticParameter.Oii(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			UploadPackageOperationResponse uploadPackageOperationResponse = operationInvoker.UploadPackage(tenantId, infoProvider.GetInfoForTenant(tenantId), deploymentSchedule, language, packageStream);
			Guid guid = tenantRepository.UpdateOrInsert(NavAppOperationHelper.CreateDeployPackageReference(uploadPackageOperationResponse.Status, uploadPackageOperationResponse.Details, appInfo.UploadDescription(), new NavAppDeployPackageOperationMetadata
			{
				AppId = ((WrappedValue<Guid>)(object)appInfo.AppId).Value,
				Version = appInfo.Version,
				JobId = uploadPackageOperationResponse.JobId,
				TenantId = tenantId,
				Language = language,
				AppName = appInfo.Name,
				AppPublisher = appInfo.Publisher,
				Schedule = deploymentSchedule.DisplayString()
			}));
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001RB", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Created upload package operation entry -- OperationId:{0}, JobId:{1}", guid, uploadPackageOperationResponse.JobId);
			return guid;
		}
	}

	public Guid DeployTarget(string language, Guid appId)
	{
		string tenantId = tenantRepository.TenantId;
		string content = string.Format(CultureInfo.InvariantCulture, "Publish Target Operation -- Tenant:{0}; AppId:{1}", tenantId, appId);
		using (DiagnosticsResolver.CreateTraceScope("00001RH", DiagnosticParameter.Oii(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			DeployTargetOperationResponse deployTargetOperationResponse = operationInvoker.DeployTarget(tenantId, appId, infoProvider.GetInfoForTenant(tenantId), language);
			string description = ((string.IsNullOrEmpty(deployTargetOperationResponse.AppName) || string.IsNullOrEmpty(deployTargetOperationResponse.AppPublisher)) ? appId.DeployTargetDescription() : Lang.NavAppDeployTargetOperationFriendlyDescription.FormatWithResolvedCulture(deployTargetOperationResponse.AppName, deployTargetOperationResponse.AppPublisher));
			Guid guid = tenantRepository.UpdateOrInsert(NavAppOperationHelper.CreateDeployTargetReference(deployTargetOperationResponse.Status, deployTargetOperationResponse.Details, description, new NavAppDeployTargetOperationMetadata
			{
				AppId = appId,
				JobId = deployTargetOperationResponse.JobId,
				TenantId = tenantId,
				Language = language,
				AppName = deployTargetOperationResponse.AppName,
				AppPublisher = deployTargetOperationResponse.AppPublisher
			}));
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001RC", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Created publish target operation entry -- OperationId:{0}, JobId:{1}", guid, deployTargetOperationResponse.JobId);
			return guid;
		}
	}

	public void NotifyExtensionInstallOperation(NavAppOperationMetadataResult installResult)
	{
		try
		{
			PerformForRelatedOperation(installResult, "Install", UpdateTenantOperationRecord, UpdateTenantOperationRecord);
		}
		catch (Exception exception)
		{
			string content = string.Format(CultureInfo.InvariantCulture, "Extension install notification failed -- AppId:{0}; TenantId:{1}", installResult?.AppId, installResult?.TenantId);
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001RA", Category.Extensions, exception, Verbosity.Error, DiagnosticParameter.Oii(content));
		}
	}

	private void PerformForRelatedOperation<T>(T operationResult, string nstOperationType, Action<NavAppTenantOperationRecord, T> onSuccess = null, Action<NavAppTenantOperationRecord, T> onFailure = null) where T : NavAppOperationResult
	{
		NavAppTenantOperationRecord operationForResult = tenantRepository.GetOperationForResult(operationResult);
		if (operationForResult != null)
		{
			switch (operationResult.ResultStatus)
			{
			case NavAppOperationResultStatus.Failure:
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001RE", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Operation failure -- Operation:{0}; Type:{1}; Status:{2}; ResultKey:{3}", operationForResult.OperationId, nstOperationType, operationResult.ResultStatus, operationResult.Key);
				onFailure?.Invoke(operationForResult, operationResult);
				break;
			case NavAppOperationResultStatus.Success:
				onSuccess?.Invoke(operationForResult, operationResult);
				break;
			}
		}
	}

	private void UpdateTenantOperationRecord(NavAppTenantOperationRecord record, NavAppOperationResult result)
	{
		tenantRepository.UpdateOperationStatus(record.OperationId, result.ResultStatus.ToOperationStatus(), result.Details);
	}
}
