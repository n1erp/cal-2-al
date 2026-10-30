using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class ManualResetGate
{
	private const int Open = 0;

	private const int Closed = 1;

	private int state;

	public bool IsOpen => Volatile.Read(ref state) == 0;

	public bool TryEnter()
	{
		return Interlocked.CompareExchange(ref state, 1, 0) == 0;
	}

	public void Reset()
	{
		Volatile.Write(ref state, 0);
	}
}
