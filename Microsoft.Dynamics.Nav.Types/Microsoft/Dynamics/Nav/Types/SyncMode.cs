using System;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public enum SyncMode
{
	ForceSync = 0,
	Sync = 2,
	CheckOnly = 3
}
