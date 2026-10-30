using System;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[Flags]
public enum DesignerLevels
{
	None = 0,
	Personalization = 1,
	Configuration = 2,
	Development = 4,
	Inspector = 8,
	All = Personalization | Configuration | Development | Inspector
}
