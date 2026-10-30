using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLRecursiveEventCallException : NavNCLException
{
	public NavNCLRecursiveEventCallException()
	{
	}

	public NavNCLRecursiveEventCallException(string message)
		: base(message)
	{
	}

	public NavNCLRecursiveEventCallException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLRecursiveEventCallException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLRecursiveEventCallException Create(string functionName, NavTriggerEventType eventType, ObjectType objectType, string objectName, string fieldName, int fieldId)
	{
		if (string.IsNullOrEmpty(fieldName) && fieldId == 0)
		{
			return new NavNCLRecursiveEventCallException(string.Format(CultureInfo.CurrentCulture, Lang.RecursionInEventCall, functionName, eventType, objectType, objectName));
		}
		return new NavNCLRecursiveEventCallException(string.Format(CultureInfo.CurrentCulture, Lang.RecursionInTableEventCall, functionName, eventType, objectType, objectName, (fieldId != 0) ? fieldId.ToString(CultureInfo.InvariantCulture) : fieldName));
	}
}
