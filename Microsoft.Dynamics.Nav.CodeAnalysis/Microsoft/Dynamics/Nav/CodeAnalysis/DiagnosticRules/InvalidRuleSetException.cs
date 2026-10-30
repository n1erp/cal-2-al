using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DiagnosticRules;

public class InvalidRuleSetException : Exception
{
	public InvalidRuleSetException()
	{
	}

	public InvalidRuleSetException(string message)
		: base(message)
	{
	}

	public InvalidRuleSetException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
