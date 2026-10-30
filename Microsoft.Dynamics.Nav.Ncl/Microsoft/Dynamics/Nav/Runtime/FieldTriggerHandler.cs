using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class FieldTriggerHandler<T> where T : class
{
	public Type HandlerType { get; private set; }

	public Action<T> HandlerAction { get; private set; }

	public FieldTriggerHandler(Type handlerType, Action<T> handlerAction)
	{
		HandlerType = handlerType;
		HandlerAction = handlerAction;
	}
}
