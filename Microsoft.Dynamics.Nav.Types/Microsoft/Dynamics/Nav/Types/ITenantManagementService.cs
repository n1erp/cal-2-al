using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Types;

[ServiceContract(SessionMode = SessionMode.Required, Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public interface ITenantManagementService
{
	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataTable GetApplication();

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void SetApplication([TelemetrySystemMetadata] Version applicationVersion, [TelemetrySystemMetadata] string applicationFamily, [TelemetrySystemMetadata] bool force, [TelemetrySystemMetadata] bool incrementApplicationVersion);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void MountApplication(MountApplicationArguments mountApplicationArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	NavTenantRuntimeSettingsList GetTenants([TelemetryOii] string tenantId, [TelemetrySystemMetadata] bool checkSpecifiedTenant = false);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	string SetTenant(SetTenantArguments arguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	IEnumerable<NavAppTableModificationResponse> GetNavAppTableModifications(GetNavAppInfoArguments getNavAppInfoArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	IEnumerable<NavAppTableModificationResponse> GetNavAppTableModificationsForTenant([TelemetryOii] string tenantId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void NewTenant(NewTenantArguments arguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	int MoveTenant(MoveTenantArguments arguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	string SetTenantExclusiveAccessMode(SetTenantExclusiveAccessModeArguments arguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void ReleaseTenantExclusiveAccessMode(ReleaseTenantExclusiveAccessModeArguments arguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	int GetNumberOfBufferTenants(GetNumberOfBufferTenantsArguments arguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	NavTenantSettings RegisterTenant(RegisterTenantArguments arguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	int CopyTenantData(CopyTenantDataArguments arguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	int RemoveTenant(RemoveTenantArguments arguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	bool MountTenant(MountTenantArguments mountTenantArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	NavTenantSettings DismountTenant(DismountTenantArguments arguments = null, [TelemetryOii] string tenantId = null);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	int SynchronizeTenantDatabase(SynchronizeTenantDatabaseArguments synchronizeTenantDatabaseArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	int ConvertTenantDatabaseToSharedSchema(ConvertTenantDatabaseToSharedSchemaArguments convertTenantDatabaseToSharedSchemaArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	int GetCurrentSynchronizationTaskId([TelemetryOii] string tenantId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	TestTenantDatabaseSchemaResponse TestTenantDatabaseSchema([TelemetryOii] string tenantId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	IEnumerable<TableSynchSetupForDataUpgradeResponse> GetTableSynchSetupForDataUpgradeFromDatabaseInfo(GetTableSynchSetupForDataUpgradeArguments arguments);

	[Obsolete("Replaced by NavDataUpgradeProcessExecutionState RetrieveTenantDataUpgradeStatus(string tenantId)")]
	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	NavDataUpgradeProcessExecutionDetails GetTenantDatabaseUpgradeStatus([TelemetryOii] string tenantId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	NavDataUpgradeProcessExecutionState RetrieveTenantDataUpgradeStatus([TelemetryOii] string tenantId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	NavDataUpgradeProcessExecutionState PollForCompletedTenantDataUpgradeStatus([TelemetryOii] string tenantId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void StartTenantDatabaseUpgrade(StartTenantDatabaseUpgradeArguments startTenantDatabaseUpgradeArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void ResumeTenantDatabaseUpgrade(ResumeTenantDatabaseUpgradeArguments resumeTenantDatabaseUpgradeArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void StopTenantDatabaseUpgrade([TelemetryOii] string tenantId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void UpdateScheduledTaskList([TelemetryOii] string tenantId);
}
