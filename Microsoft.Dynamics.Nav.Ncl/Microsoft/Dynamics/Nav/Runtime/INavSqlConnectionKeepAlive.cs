using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface INavSqlConnectionKeepAlive : IDisposable
{
	event NavSqlConnectionKeepAlive.KeepAliveEventHandler OnSuccess;

	void Reset();
}
