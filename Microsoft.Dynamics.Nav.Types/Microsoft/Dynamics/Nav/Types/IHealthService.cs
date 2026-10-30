using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types;

[ServiceContract(SessionMode = SessionMode.Required, Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public interface IHealthService
{
	[OperationContract(Name = "IsSystemHealthy", IsInitiating = true, IsTerminating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	bool IsSystemHealthy();

	[OperationContract(Name = "IsTenantHealthy", IsInitiating = true, IsTerminating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	bool IsTenantHealthy(string tenantId);

	[OperationContract(Name = "IsDatabaseHealthy", IsInitiating = true, IsTerminating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	bool IsDatabaseHealthy(string databaseId);
}
