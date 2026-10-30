using System.IdentityModel.Policy;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace Microsoft.Dynamics.Nav.Types;

public class NoIdentityChecksIdentityVerifier : IdentityVerifier
{
	public override bool CheckAccess(EndpointIdentity identity, AuthorizationContext authContext)
	{
		return true;
	}

	public override bool TryGetIdentity(EndpointAddress reference, out EndpointIdentity identity)
	{
		identity = reference.Identity;
		return true;
	}
}
