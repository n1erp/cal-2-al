using System;

namespace Microsoft.CodeAnalysis;

internal class InvalidRuleSetException : Exception
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
