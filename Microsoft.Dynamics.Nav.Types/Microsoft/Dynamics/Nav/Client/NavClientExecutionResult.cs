using System;

namespace Microsoft.Dynamics.Nav.Client;

public class NavClientExecutionResult
{
	public bool ShowError { get; set; }

	public int ReturnValue { get; set; }

	public Exception Exception { get; set; }

	public NavClientExecutionResult(Exception exception, int returnValue)
	{
		Exception = exception;
		ReturnValue = returnValue;
		ShowError = true;
	}

	public NavClientExecutionResult(Exception exception, int returnValue, bool showError)
	{
		Exception = exception;
		ReturnValue = returnValue;
		ShowError = showError;
	}
}
