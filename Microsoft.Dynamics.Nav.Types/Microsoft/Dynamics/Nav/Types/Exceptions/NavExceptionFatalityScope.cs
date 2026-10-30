using System;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public enum NavExceptionFatalityScope
{
	None,
	Call,
	Page
}
