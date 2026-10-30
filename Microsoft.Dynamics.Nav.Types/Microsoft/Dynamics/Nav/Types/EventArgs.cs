using System;

namespace Microsoft.Dynamics.Nav.Types;

public class EventArgs<T> : EventArgs
{
	private T data;

	public T Data => data;

	public EventArgs(T data)
	{
		this.data = data;
	}
}
