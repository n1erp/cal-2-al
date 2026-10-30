using System.Collections.Generic;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types;

[ServiceContract(SessionMode = SessionMode.Required, Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public interface ITenantDatabaseManagementService
{
	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	NavTenantDatabaseSettings MountTenantDatabase(MountTenantDatabaseArguments mountTenantDatabaseArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	IList<NavTenantDatabaseSettings> GetTenantDatabase([TelemetryOii] string id, [TelemetrySystemMetadata] bool forceRefresh = false);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void DismountTenantDatabase([TelemetryOii] string id);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	int SynchronizeTenantsDatabase(SynchronizeTenantsDatabaseArguments synchronizeTenantsDatabaseArguments);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	NavTenantDatabaseSettings GetTenantDatabaseSynchronizeStatus([TelemetryOii] string id);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	SyncNavAppResponse SyncNavApp(SyncNavAppArguments syncNavAppArguments);
}
