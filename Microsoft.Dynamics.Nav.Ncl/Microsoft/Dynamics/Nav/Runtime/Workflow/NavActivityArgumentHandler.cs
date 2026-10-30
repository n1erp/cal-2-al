using System;
using System.Activities;

namespace Microsoft.Dynamics.Nav.Runtime.Workflow;

public class NavActivityArgumentHandler : INavActivityArgumentHandler
{
	public T LoadInArgument<T>(ActivityContext context, InArgument<T> argument)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (argument == null)
		{
			throw new ArgumentNullException("argument");
		}
		return context.GetValue(argument);
	}

	public void SetOutArgument<T>(ActivityContext context, OutArgument<T> argument, T value)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (argument == null)
		{
			throw new ArgumentNullException("argument");
		}
	}

	public void CheckForNull(object value, string name)
	{
		if (value is string value2 && string.IsNullOrEmpty(value2))
		{
			throw new ArgumentException("Empty string is not allowed", name);
		}
		if (value == null)
		{
			throw new ArgumentNullException(name);
		}
	}
}
