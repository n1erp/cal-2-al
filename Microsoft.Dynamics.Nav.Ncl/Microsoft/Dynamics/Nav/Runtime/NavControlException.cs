using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavControlException : Exception
{
	private NavControlStatement ctrlStatement;

	internal NavControlStatement ControlStatement => ctrlStatement;

	internal NavControlException(NavControlStatement controlStatement)
		: base("An unimplemented method was called.")
	{
		ctrlStatement = controlStatement;
	}
}
