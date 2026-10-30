using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types;

[ServiceContract(SessionMode = SessionMode.Required, Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public interface ICSideService
{
	[OperationContract(Name = "CSideService_GetTenants")]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	IEnumerable<string> GetTenants();

	[OperationContract(Name = "CSideService_StartTenantUpgrade")]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void StartTenantUpgrade([TelemetryOii] string tenantId, [TelemetrySystemMetadata] bool parallelExecutionEnabled, [TelemetrySystemMetadata] bool continueOnErrorEnabled, [TelemetrySystemMetadata] bool skipCompanyInitialization, [TelemetrySystemMetadata] bool skipAppVersionCheck);

	[OperationContract(Name = "CSideService_ResumeTenantUpgrade")]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void ResumeTenantUpgrade([TelemetryOii] string tenantId, [TelemetrySystemMetadata] int codeunitId, [TelemetrySystemMetadata] string methodName, [TelemetryOii] string companyName);

	[OperationContract(Name = "CSideService_StopTenantUpgrade")]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void StopTenantUpgrade([TelemetryOii] string tenantId);

	[OperationContract(Name = "CSideService_GetTenantUpgradeStatus")]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	NavDataUpgradeProcessExecutionDetails GetTenantUpgradeStatus([TelemetryOii] string tenantId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void ValidateTableLayoutChanges([TelemetryOii] string tenantId, [TelemetrySystemMetadata] int tableId, [TelemetrySystemMetadata] string baseMetadata);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void SynchronizeTables([TelemetryOii] string tenantId, [TelemetrySystemMetadata] int[] tableIds, [TelemetrySystemMetadata] bool force);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void SynchronizeAllTables([TelemetryOii] string tenantId, [TelemetrySystemMetadata] bool force);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void SynchronizationCheck([TelemetryOii] string tenantId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	NavTenantRuntimeSettings SynchronizationState([TelemetryOii] string tenantId);

	[OperationContract(IsInitiating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	bool PrepareAddInsDownload([TelemetrySystemMetadata] string assemblyFullName, [TelemetrySystemMetadata] bool clientIs64BitProcess);

	[OperationContract(IsInitiating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	bool PrepareAddInsDatabaseDownload([TelemetrySystemMetadata] string databaseServer, [TelemetryOii] string databaseName, [TelemetryEuii] string userName, [TelemetrySuppress] string password, [TelemetrySystemMetadata] string assemblyFullName);

	[OperationContract(IsTerminating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	StreamTransfer Download();

	[OperationContract(IsInitiating = true, IsTerminating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ReadOnlyCollection<string> GetAddInsAssemblyNames();
}
