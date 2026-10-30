using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface IExpiringValue
{
	DateTime ExpirationTime { get; }

	bool Expired { get; }
}
