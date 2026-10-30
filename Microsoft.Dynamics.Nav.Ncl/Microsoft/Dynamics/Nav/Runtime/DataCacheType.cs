using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[Flags]
internal enum DataCacheType
{
	None = 0,
	Private = 1,
	NonTransactional = 2,
	Global = Private | NonTransactional
}
