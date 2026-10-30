using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class EndTransactionEventArgs : EventArgs
{
	private readonly bool commit;

	public bool Commit => commit;

	public EndTransactionEventArgs(bool commit)
	{
		this.commit = commit;
	}
}
