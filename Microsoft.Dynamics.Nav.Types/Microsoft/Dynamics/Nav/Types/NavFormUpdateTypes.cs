using System;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[Flags]
public enum NavFormUpdateTypes
{
	None = 0,
	Update = 1,
	RecordSaved = 2,
	UpdateParent = 4
}
