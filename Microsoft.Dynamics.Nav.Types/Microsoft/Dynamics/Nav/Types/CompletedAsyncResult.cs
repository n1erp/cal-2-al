using System;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Types;

public class CompletedAsyncResult : IAsyncResult
{
	private readonly object state;

	private readonly object result;

	public object Result => result;

	public bool IsCompleted => true;

	public WaitHandle AsyncWaitHandle
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public object AsyncState => state;

	public bool CompletedSynchronously => true;

	public static CompletedAsyncResult FromResult(object result)
	{
		return new CompletedAsyncResult(result, null);
	}

	public static CompletedAsyncResult WithState(object state)
	{
		return new CompletedAsyncResult(null, state);
	}

	public CompletedAsyncResult(object result, object state)
	{
		this.result = result;
		this.state = state;
	}
}
