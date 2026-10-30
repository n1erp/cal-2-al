using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[Flags]
public enum NCLMetaFilterModes
{
	None = 0,
	OnlyMaxLimit = 1,
	ValueIsFilter = 2
}
