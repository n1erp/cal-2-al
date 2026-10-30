using System.ServiceModel;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

[ServiceContract(SessionMode = SessionMode.Required, Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public interface IStartupService
{
	[OperationContract(Name = "GetHomepageMetadata", IsInitiating = true, IsTerminating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	PageDependencyResult GetHomepageMetadata();

	[OperationContract(Name = "GetHomepageNavigation", IsInitiating = true, IsTerminating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	MasterNavigation GetHomepageNavigation();

	[OperationContract(Name = "GetClientConfiguration", IsInitiating = true, IsTerminating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ClientConfiguration GetClientConfiguration(string tenantId, ClientType clientType, string applicationId);
}
