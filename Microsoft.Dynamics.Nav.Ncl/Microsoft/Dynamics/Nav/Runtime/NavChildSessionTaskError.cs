using System;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavChildSessionTaskError
{
	public string ErrorCode { get; } = string.Empty;

	public string ErrorText { get; } = string.Empty;

	public string ErrorCallstack { get; } = string.Empty;

	public NavBaseException Exception { get; }

	public NavChildSessionTaskError(NavSession childSession, NavBaseException exception)
	{
		if (childSession == null)
		{
			throw new ArgumentNullException("childSession");
		}
		Exception = exception ?? throw new ArgumentNullException("exception");
		ErrorCode = childSession.GetLastErrorCode;
		ErrorText = childSession.GetLastErrorText;
		ErrorCallstack = childSession.GetLastErrorCallstack();
	}
}
