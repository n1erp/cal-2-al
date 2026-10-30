using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[Flags]
internal enum NavEnvironmentFlags
{
	None = 0,
	Standalone = 1,
	DevelopmentMode = 2,
	DisableNavApps = 4,
	DisableEvents = 8,
	RemoveAllAppsMode = 0x10,
	DisableBackgroundChangeListener = 0x20
}
