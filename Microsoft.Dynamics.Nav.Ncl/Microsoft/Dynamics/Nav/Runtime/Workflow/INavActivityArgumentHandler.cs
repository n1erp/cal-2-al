using System.Activities;

namespace Microsoft.Dynamics.Nav.Runtime.Workflow;

public interface INavActivityArgumentHandler
{
	T LoadInArgument<T>(ActivityContext context, InArgument<T> argument);

	void SetOutArgument<T>(ActivityContext context, OutArgument<T> argument, T value);

	void CheckForNull(object value, string name);
}
