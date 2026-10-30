using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[Flags]
internal enum CacheClear
{
	DoNotClear = 0,
	ClearPrimaryKeyCache = 1,
	ClearResultSetCache = 2,
	ClearAll = ClearPrimaryKeyCache | ClearResultSetCache
}
