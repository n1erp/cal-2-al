using System.Security.Principal;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavAdministratorValidator
{
	bool Validate(WindowsIdentity windowsIdentity, IIdentity primaryIdentity);
}
