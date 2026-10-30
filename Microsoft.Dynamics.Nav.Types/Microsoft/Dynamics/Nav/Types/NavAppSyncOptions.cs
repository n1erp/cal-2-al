using System;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[Flags]
public enum NavAppSyncOptions
{
	None = 0,
	SaveData = 1,
	LoadData = 2,
	Default = SaveData | LoadData
}
