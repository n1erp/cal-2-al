using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface ICacheSynchronizationController : IDisposable
{
	bool EmitTableDataChangesEvents { get; }

	void StartCacheSynchronization();

	void StopCacheSynchronization();

	void OnTenantDataChanged(NavTenant tenant);
}
