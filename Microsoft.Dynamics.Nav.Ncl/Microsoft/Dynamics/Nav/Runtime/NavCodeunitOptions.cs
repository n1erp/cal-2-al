using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[Flags]
public enum NavCodeunitOptions
{
	SingleInstance = 1,
	EventManualBinding = 2
}
