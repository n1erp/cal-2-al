using System;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class ALNavAppOperationInvoker
{
	private readonly INavAppTenantOperationManagement tenantOperationManagement;

	private readonly INavAppMetadataRetriever metadataRetriever;

	public ALNavAppOperationInvoker()
		: this(NavGlobal.NavAppTenantOperationManagementProvider.CreateForTenant(NavCurrentThread.Session?.Tenant), NavEnvironment.Instance.NavAppMetadataRetriever)
	{
	}

	internal ALNavAppOperationInvoker(INavAppTenantOperationManagement tenantOperationManagement, INavAppMetadataRetriever metadataRetriever)
	{
		this.tenantOperationManagement = tenantOperationManagement ?? throw new ArgumentNullException("tenantOperationManagement");
		this.metadataRetriever = metadataRetriever ?? throw new ArgumentNullException("metadataRetriever");
	}

	public void DeployTarget(Guid appId, string languageCode)
	{
		NavAppDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			string languageName = GetLanguageName(languageCode);
			tenantOperationManagement.DeployTarget(languageName, appId);
		}, FormatAdditionalInfo(), WrapInALException, "DeployTarget");
	}

	public string GetOperationDetailedStatusMessage(Guid operationId)
	{
		return NavAppDiagnosticSession.SendTraceTagOnFailure(() => tenantOperationManagement.GetOperationDetailedStatusMessage(operationId), FormatAdditionalInfo(), WrapInALException, "GetOperationDetailedStatusMessage");
	}

	public void GetOperationDetailedStatusMessageAsStream(Guid operationId, Stream outStream)
	{
		NavAppDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			string operationDetailedStatusMessage = GetOperationDetailedStatusMessage(operationId);
			StreamWriter streamWriter = new StreamWriter(outStream);
			streamWriter.Write(operationDetailedStatusMessage);
			streamWriter.Flush();
			outStream.Position = 0L;
		}, FormatAdditionalInfo(), WrapInALException, "GetOperationDetailedStatusMessageAsStream");
	}

	public string GetDeployOperationAppName(Guid operationId)
	{
		return GetDeployOperationMetadataProperty(operationId, "AppName");
	}

	public string GetDeployOperationAppPublisher(Guid operationId)
	{
		return GetDeployOperationMetadataProperty(operationId, "AppPublisher");
	}

	public string GetDeployOperationSchedule(Guid operationId)
	{
		return GetDeployOperationMetadataProperty(operationId, "Schedule");
	}

	public string GetDeployOperationAppVersion(Guid operationId)
	{
		return GetDeployOperationMetadataProperty(operationId, "Version");
	}

	public string GetDeployOperationJobId(Guid operationId)
	{
		return GetDeployOperationMetadataProperty(operationId, "JobId");
	}

	public void RefreshOperationStatus(Guid operationId)
	{
		try
		{
			NavAppDiagnosticSession.SendTraceTagOnFailure(() =>
			{
				tenantOperationManagement.RefreshOperationStatus(operationId);
			}, FormatAdditionalInfo(), null, "RefreshOperationStatus");
		}
		catch
		{
		}
	}

	public Guid UploadPackage(Stream packageStream, ALPackageDeploymentSchedule deploymentSchedule, string languageCode)
	{
		return NavAppDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Expected Obj, but got Unknown
			string languageName = GetLanguageName(languageCode);
			INavAppReference appInfo;
			try
			{
				using NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(packageStream, leaveOpen: true);
				if (navAppPackageReader.MetadataVersion < 2)
				{
					throw new NavALException(Lang.NavAppUploadOldPackageFailure.FormatWithResolvedCulture());
				}
				NavAppManifest navAppManifest = navAppPackageReader.ReadNavAppManifest();
				packageStream.Seek(0L, SeekOrigin.Begin);
				NavAppRuntimeMetadata val = metadataRetriever.GetAllMetadata().FirstOrDefault((NavAppRuntimeMetadata metadata) => ((WrappedValue<Guid>)(object)metadata.AppId).Value == navAppManifest.AppId && metadata.Scope == NavAppScope.Global);
				if (val != null)
				{
					throw new NavAppException(Lang.ExtensionAlreadyPublishedInGlobalScope.FormatWithResolvedCulture(val.AppId, val.Name));
				}
				NavAppManifest navAppManifest2 = navAppPackageReader.ReadNavAppManifest();
				appInfo = (INavAppReference)new NavAppReference
				{
					AppId = AppId.op_Implicit(navAppManifest2.AppId),
					Name = navAppManifest2.AppName,
					Publisher = navAppManifest2.AppPublisher,
					Version = navAppManifest2.AppVersion
				};
			}
			finally
			{
				if (packageStream.CanSeek)
				{
					packageStream.Seek(0L, SeekOrigin.Begin);
				}
			}
			return tenantOperationManagement.UploadPackage(languageName, appInfo, GetInternalSchedule(deploymentSchedule), packageStream);
		}, FormatAdditionalInfo(), WrapInALException, "UploadPackage");
	}

	private static DeploymentSchedule GetInternalSchedule(ALPackageDeploymentSchedule schedule)
	{
		DeploymentSchedule result = DeploymentSchedule.Current;
		switch (schedule)
		{
		case ALPackageDeploymentSchedule.Immediate:
			result = DeploymentSchedule.Current;
			break;
		case ALPackageDeploymentSchedule.StageForNextUpdate:
			result = DeploymentSchedule.NextMajorUpdate;
			break;
		case ALPackageDeploymentSchedule.StageForNextMinorUpdate:
			result = DeploymentSchedule.NextMinorUpdate;
			break;
		}
		return result;
	}

	private static string GetLanguageName(string languageCode)
	{
		if (!LanguageSetting.TryParse(languageCode, out var _))
		{
			throw new NavALException(Lang.InvalidLanguageCode.FormatWithResolvedCulture(languageCode));
		}
		LanguageSetting languageSetting = new LanguageSetting(languageCode);
		return languageSetting.CultureInfo.Name;
	}

	private static string FormatAdditionalInfo()
	{
		return "Tenant:" + NavCurrentThread.Session?.Tenant?.Id;
	}

	private static Exception WrapInALException(Exception e)
	{
		return (e as NavALException) ?? new NavALException(e.Message);
	}

	private string GetDeployOperationMetadataProperty(Guid operationId, string property)
	{
		NavAppDeployOperationMetadata deployOperationMetadata = tenantOperationManagement.GetDeployOperationMetadata(operationId);
		if (deployOperationMetadata == null)
		{
			return string.Empty;
		}
		string result = string.Empty;
		switch (property)
		{
		case "AppName":
			result = deployOperationMetadata.AppName ?? string.Empty;
			break;
		case "AppPublisher":
			result = deployOperationMetadata.AppPublisher ?? string.Empty;
			break;
		case "Schedule":
			result = deployOperationMetadata.Schedule ?? string.Empty;
			break;
		case "JobId":
			result = deployOperationMetadata.JobId.ToString();
			break;
		case "Version":
			result = (deployOperationMetadata as NavAppDeployPackageOperationMetadata)?.Version?.ToString() ?? string.Empty;
			break;
		}
		return result;
	}
}
