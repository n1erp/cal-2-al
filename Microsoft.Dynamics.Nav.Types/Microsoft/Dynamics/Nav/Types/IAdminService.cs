using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types;

[ServiceContract(SessionMode = SessionMode.Required, Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[ServiceKnownType(typeof(Dictionary<string, bool>))]
public interface IAdminService : INCLAdminService, ITenantManagementService, ICSideService, IHealthService, IStartupService, IOpenDynamicFileHandlerService, ITenantDatabaseManagementService
{
	[OperationContract]
	int GetAdminServerVersion();

	[OperationContract]
	IDictionary<string, string> GetServerSettings(GetServerSettingsArguments arguments);

	[OperationContract]
	void UpdateServerSetting([TelemetrySystemMetadata] string key, [TelemetrySuppress] string value);

	[OperationContract]
	void UpdateSessionConfiguration([TelemetrySystemMetadata] string key, [TelemetrySuppress] object value);

	[OperationContract(IsInitiating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse OpenAdminConnection(ConnectionRequest connectionRequest);

	[OperationContract(IsTerminating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void CloseAdminConnection();

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void RemoveClientConnection([TelemetrySystemMetadata] int connectionId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataTable ExportLicenseInfo();

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void ExportNavData([TelemetryEuii] string fileName, [TelemetryEuii] string description, [TelemetrySystemMetadata] bool includeApplication, [TelemetrySystemMetadata] bool includeApplicationData, [TelemetrySystemMetadata] bool includeGlobalData, [TelemetrySystemMetadata] bool includeAllCompanies, [TelemetryOii] ReadOnlyCollection<string> companiesToInclude);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void ImportNavData([TelemetryEuii] string fileName, [TelemetrySystemMetadata] bool includeApplicationData, [TelemetrySystemMetadata] bool includeGlobalData, [TelemetrySystemMetadata] bool commitPerTable, [TelemetryOii] IReadOnlyDictionary<string, bool> companyImports);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataTable GetNavDataExportInformation([TelemetryEuii] string fileName);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataTable GetClientConnectionInfo();

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataTable GetUser();

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataTable GetUserRole([TelemetryEuii] string userName, [TelemetryEupi] string sid, [TelemetrySystemMetadata] string roleId, [TelemetrySystemMetadata] string appName, [TelemetrySystemMetadata] string publisher, [TelemetryOii] string companyName);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataTable GetRole();

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataTable GetPermission([TelemetrySystemMetadata] string roleId, [TelemetrySystemMetadata] int objectType, [TelemetrySystemMetadata] int objectId);

	[OperationContract]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void Ping();
}
