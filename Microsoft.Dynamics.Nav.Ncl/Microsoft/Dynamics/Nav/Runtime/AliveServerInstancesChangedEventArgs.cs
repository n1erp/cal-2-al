using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AliveServerInstancesChangedEventArgs : EventArgs
{
	public bool AnotherServerHasCrashed { get; private set; }

	public bool AnotherServerRemoved { get; private set; }

	public AliveServerInstancesChangedEventArgs(bool anotherServerHasCrashed, bool anotherServerRemoved)
	{
		AnotherServerHasCrashed = anotherServerHasCrashed;
		AnotherServerRemoved = anotherServerRemoved;
	}
}
