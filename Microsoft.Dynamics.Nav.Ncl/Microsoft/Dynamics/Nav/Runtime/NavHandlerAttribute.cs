using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public sealed class NavHandlerAttribute : Attribute
{
	private NavHandlerType handlerType;

	private bool optional;

	public NavHandlerType HandlerType => handlerType;

	public bool Optional
	{
		get
		{
			if (optional)
			{
				return IsOptionalHandlerSupported(handlerType);
			}
			return false;
		}
	}

	public NavHandlerAttribute(NavHandlerType handlerType, bool optional = false)
	{
		this.handlerType = handlerType;
		this.optional = optional;
	}

	private static bool IsOptionalHandlerSupported(NavHandlerType handlerType)
	{
		if (handlerType != NavHandlerType.RecallNotification)
		{
			return handlerType == NavHandlerType.SendNotification;
		}
		return true;
	}
}
