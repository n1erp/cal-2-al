using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Types;

[ServiceContract(SessionMode = SessionMode.Required, Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public interface INCLAdminService
{
	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataTable GetNavDataEntries([TelemetrySystemMetadata] int tableId, [TelemetrySystemMetadata] NavFilterGroup[] filterGroups);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	Version GetReleaseVersion();

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse ImportLicense([TelemetryAccountData] byte[] license, [TelemetrySystemMetadata] LicenseDatabase licenseDatabase);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse NewUser([TelemetryEuii] string userName, [TelemetryEuii] string fullName, [TelemetrySystemMetadata] NavUserState state, [TelemetrySystemMetadata] DateTime expiryDate, [TelemetryEupi] string sid, [TelemetrySystemMetadata] bool changePassword, [TelemetrySystemMetadata] LicenseType licenseType, [TelemetrySuppress] string passwordHash, [TelemetrySuppress] string authenticationKey, [TelemetrySuppress] bool webServicesKey, [TelemetrySuppress] DateTime webServicesKeyExpiryDate, [TelemetryEuii] string authenticationEmail, [TelemetryEuii] string contactEmail, [TelemetrySystemMetadata] string profileId, [TelemetrySystemMetadata] Guid applicationId, [TelemetryOii] string company = null, [TelemetrySystemMetadata] string languageId = null);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	RemoveUserResponse RemoveUser([TelemetryEuii] string userName, [TelemetryEupi] string sid);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse RenameUser([TelemetryEuii] string currentUserName, [TelemetryEuii] string newUserName, [TelemetryEuii] string fullName, [TelemetrySystemMetadata] NavUserState state, [TelemetrySystemMetadata] DateTime expiryDate, [TelemetryEupi] string currentSid, [TelemetryEupi] string newSid, [TelemetrySystemMetadata] bool changePassword, [TelemetrySystemMetadata] LicenseType licenseType, [TelemetrySystemMetadata] bool setLicenseType, [TelemetrySuppress] string passwordHash, [TelemetrySuppress] string authenticationKey, [TelemetrySuppress] bool webServicesKey, [TelemetrySuppress] DateTime webServicesKeyExpiryDate, [TelemetryEuii] string authenticationEmail, [TelemetryEuii] string contactEmail, [TelemetrySystemMetadata] string profileId, [TelemetrySystemMetadata] Guid applicationId, [TelemetryEupi] string authenticationObjectId, [TelemetryOii] string company = null, [TelemetrySystemMetadata] string languageId = null);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ConvertUserPersonalizationResponse ConvertUserPersonalization(ConvertUserPersonalizationRequest request);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ConvertProfileResponse ConvertProfileConfiguration(ConvertProfileRequest request);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse NewUserRole([TelemetryEuii] string userName, [TelemetryEupi] string sid, [TelemetrySystemMetadata] string roleId, [TelemetrySystemMetadata] PermissionScope scope, [TelemetrySystemMetadata] string appName, [TelemetrySystemMetadata] string publisher, [TelemetryOii] string companyName);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse RemoveUserRole([TelemetryEuii] string userName, [TelemetryEupi] string sid, [TelemetrySystemMetadata] string roleId, [TelemetrySystemMetadata] PermissionScope scope, [TelemetrySystemMetadata] string appName, [TelemetrySystemMetadata] string publisher, [TelemetryOii] string companyName);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse NewRole([TelemetrySystemMetadata] string roleId, [TelemetrySystemMetadata] string roleName);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse RemoveRole([TelemetrySystemMetadata] string roleId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse RenameRole([TelemetrySystemMetadata] string currentRoleId, [TelemetrySystemMetadata] string newRoleName);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse NewPermission([TelemetrySystemMetadata] string roleId, [TelemetrySystemMetadata] ObjectType objectType, [TelemetrySystemMetadata] int objectId, [TelemetrySystemMetadata] int permissionMask, [TelemetrySystemMetadata] string securityFilter);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse RemovePermission([TelemetrySystemMetadata] string roleId, [TelemetrySystemMetadata] ObjectType objectType, [TelemetrySystemMetadata] int objectId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse SetPermission([TelemetrySystemMetadata] string roleId, [TelemetrySystemMetadata] ObjectType objectType, [TelemetrySystemMetadata] int objectId, [TelemetrySystemMetadata] int permissionMask, [TelemetrySystemMetadata] string securityFilter, [TelemetrySystemMetadata] int permissionModifiedMask);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void NewCompany([TelemetryOii] string name, [TelemetryOii] string displayName = null);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void NewEvaluationCompany([TelemetryOii] string name, [TelemetryOii] string displayName = null);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void RemoveCompany([TelemetryOii] string name, [TelemetrySystemMetadata] bool forceImmediateDataDeletion = false);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void RenameCompany([TelemetryOii] string name, [TelemetryOii] string newName);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void CopyCompany([TelemetryOii] string sourceName, [TelemetryOii] string destinationName);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataTable GetCompanies([TelemetrySystemMetadata] bool includePartiallyDeletedCompanies = false);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void SetCompany(SetCompanyArguments setCompanyArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse InvokeCodeunit(ConnectionRequest connectionRequest, [TelemetrySystemMetadata] int codeunitId, [TelemetrySystemMetadata] string methodName, [TelemetryCustomerContent] string argument, [TelemetryOii] string companyName, [TelemetrySystemMetadata] InvokeCodeunitTimeZone invokeTimeZone);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse NewAddIn([TelemetrySystemMetadata] string addInName, [TelemetrySystemMetadata] string publicKeyToken, [TelemetrySystemMetadata] string version, [TelemetrySystemMetadata] string description, [TelemetrySystemMetadata] AddInCategory category, [TelemetrySuppress] byte[] resource);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataTable GetAddIn([TelemetrySystemMetadata] string addInName, [TelemetrySystemMetadata] string publicKeyToken, [TelemetrySystemMetadata] string version, [TelemetrySystemMetadata] int category, [TelemetrySystemMetadata] bool includeResource);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse SetAddIn([TelemetrySystemMetadata] string addInName, [TelemetrySystemMetadata] string publicKeyToken, [TelemetrySystemMetadata] string version, [TelemetrySystemMetadata] string description, [TelemetrySystemMetadata] int category, [TelemetrySystemMetadata] byte[] resource);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse RemoveAddIn([TelemetrySystemMetadata] string addInName, [TelemetrySystemMetadata] string publicKeyToken, [TelemetrySystemMetadata] string version);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse NewWebService([TelemetrySystemMetadata] ObjectType objectType, [TelemetrySystemMetadata] int objectId, [TelemetrySystemMetadata] string name, [TelemetrySystemMetadata] bool published);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataTable GetWebService();

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse RemoveWebService([TelemetrySystemMetadata] ObjectType objectType, [TelemetrySystemMetadata] string serviceName);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	IList<TableConnectionSettingsBase> GetTableConnection([TelemetrySystemMetadata] TableConnectionType tableType, [TelemetrySystemMetadata] string connectionId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void AddTableConnection(TableConnectionSettingsBase connectionSettings);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void RemoveTableConnection([TelemetrySystemMetadata] TableConnectionType tableType, [TelemetrySystemMetadata] string connectionId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	UnpublishNavAppResponse UnpublishNavApp(UnpublishNavAppArguments arguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	PublishNavAppResponse PublishNavApp(PublishNavAppArguments arguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	RepairNavAppResponse RepairNavApp(RepairNavAppArguments arguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ChangeNavAppStateResponse StartNavAppDataUpgrade(StartNavAppDataUpgradeArguments startNavAppDataUpgradeArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ChangeNavAppStateResponse UninstallNavApp(UninstallNavAppArguments uninstallNavAppArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ChangeNavAppStateResponse InstallNavApp(InstallNavAppArguments installNavAppArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	Guid InstallNavAppAsync(InstallNavAppArguments installNavAppArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ChangeNavAppStateResponse CheckInstallNavAppStatus([TelemetryOii] string tenantId, [TelemetrySystemMetadata] Guid taskId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	IEnumerable<NavAppInfoResponse> GetNavAppInfo(GetNavAppInfoArguments getNavAppInfoArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	IEnumerable<NavAppInfoResponse> GetInstalledNavAppInfo([TelemetryOii] string tenantId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	NavTenantRuntimeSettingsList GetTenantsWithAppInstalled(GetNavAppTenantArguments getNavAppTenantArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	NavAppTableDataExportResult ExportNavAppTableData([TelemetryEuii] string folderPath, [TelemetrySystemMetadata] int tableId, [TelemetrySystemMetadata] bool force);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	GetNavAppRuntimePackageResponse GetNavAppRuntimePackage(GetNavAppRuntimePackageArguments arguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse ImportConfigurationPackageFile([TelemetrySuppress] byte[] packageData, [TelemetrySystemMetadata] string setupType, [TelemetrySystemMetadata] int? processingOrder, [TelemetrySystemMetadata] bool force);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse RemoveConfigurationPackageFile([TelemetrySystemMetadata] string code);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse ImportMembershipEntitlement([TelemetrySuppress] byte[] entitlementsFile, [TelemetrySystemMetadata] string type, [TelemetrySystemMetadata] string id, [TelemetrySystemMetadata] string name, [TelemetrySystemMetadata] string entitlementSetId, [TelemetrySystemMetadata] string entitlementSetName, [TelemetrySystemMetadata] bool isEvaluation, [TelemetrySystemMetadata] bool force);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse SandboxOutboundConfigCleanUp(ConnectionRequest connectionRequest);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void RemoveAllClientConnections();
}
