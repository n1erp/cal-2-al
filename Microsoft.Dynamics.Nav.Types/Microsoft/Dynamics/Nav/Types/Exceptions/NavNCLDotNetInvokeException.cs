using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLDotNetInvokeException : NavNCLException
{
	public NavNCLDotNetInvokeException()
	{
	}

	public NavNCLDotNetInvokeException(string message)
		: base(message)
	{
	}

	public NavNCLDotNetInvokeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLDotNetInvokeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLDotNetInvokeException Create(CultureInfo culture, string methodName, Exception innerException)
	{
		string arg = string.Empty;
		if (innerException != null)
		{
			arg = innerException.Message;
		}
		return new NavNCLDotNetInvokeException(string.Format(culture, Lang.DotNetInvokeError, methodName, arg), innerException)
		{
			FatalityScope = NavExceptionFatalityScope.Call
		};
	}

	public static NavNCLDotNetInvokeException Create(CultureInfo culture, string methodName, Type type, Exception innerException, string innerExceptionMessage)
	{
		return new NavNCLDotNetInvokeException((type != null) ? string.Format(culture, Lang.DotNetInvokeOnTypeError, methodName, type.ToString(), innerExceptionMessage) : string.Format(culture, Lang.DotNetInvokeError, methodName, innerExceptionMessage), innerException)
		{
			FatalityScope = NavExceptionFatalityScope.Call
		};
	}

	public static NavNCLDotNetInvokeException Create(CultureInfo culture, string methodName, Type type, Exception innerException)
	{
		string innerExceptionMessage = string.Empty;
		if (innerException != null)
		{
			innerExceptionMessage = innerException.Message;
		}
		return Create(culture, methodName, type, innerException, innerExceptionMessage);
	}

	public static NavNCLDotNetInvokeException Create(CultureInfo culture, string methodName, Type type, string message)
	{
		message = ((message != null) ? string.Format(culture, Lang.DotNetInvokeOnTypeError, methodName, type.ToString(), message) : string.Format(culture, Lang.DotNetInvokeError, methodName, message));
		return new NavNCLDotNetInvokeException(message)
		{
			FatalityScope = NavExceptionFatalityScope.Call
		};
	}

	public static NavNCLDotNetInvokeException Create(CultureInfo culture, Type fromType, Type toType, Exception innerException)
	{
		return new NavNCLDotNetInvokeException(string.Format(culture, Lang.DotNetAssignError, fromType.ToString(), toType.ToString()), innerException)
		{
			FatalityScope = NavExceptionFatalityScope.Call
		};
	}

	public static NavNCLDotNetInvokeException CreateNoLanguageSupport(CultureInfo culture, string language)
	{
		return new NavNCLDotNetInvokeException(string.Format(culture, Lang._00095_50169, language))
		{
			FatalityScope = NavExceptionFatalityScope.Call
		};
	}
}
