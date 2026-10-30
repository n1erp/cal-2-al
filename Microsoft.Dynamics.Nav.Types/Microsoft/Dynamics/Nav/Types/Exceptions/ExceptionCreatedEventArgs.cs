using System;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

public class ExceptionCreatedEventArgs : EventArgs
{
	private Exception createdException;

	public Exception CreatedException => createdException;

	public ExceptionCreatedEventArgs(Exception createdException)
	{
		this.createdException = createdException;
	}
}
