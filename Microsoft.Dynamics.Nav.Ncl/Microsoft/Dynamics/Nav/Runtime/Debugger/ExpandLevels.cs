using System;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

[Flags]
public enum ExpandLevels
{
	None = 0,
	Locals = 1,
	Globals = 2,
	TextConstants = 4,
	SqlStatements = 8
}
