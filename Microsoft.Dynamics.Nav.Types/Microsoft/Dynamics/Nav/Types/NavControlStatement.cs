using System;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public enum NavControlStatement
{
	None,
	Break,
	BreakUnbound,
	Skip,
	Quit,
	Cancel,
	ClientCancel
}
