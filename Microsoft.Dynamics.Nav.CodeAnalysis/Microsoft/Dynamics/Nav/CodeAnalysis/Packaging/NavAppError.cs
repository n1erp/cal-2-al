using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class NavAppError
{
	public string Message { get; }

	public NavAppErrorType NavAppErrorType { get; }

	public IEnumerable<NavAppError> NestedErrors { get; }

	private NavAppError(string message, NavAppErrorType navAppErrorType, IEnumerable<NavAppError> nestedErrors)
	{
		Message = message;
		NavAppErrorType = navAppErrorType;
		NestedErrors = nestedErrors ?? new NavAppError[0];
	}

	public static NavAppError FromMessage(string message, NavAppErrorType navAppErrorType)
	{
		return new NavAppError(message, navAppErrorType, null);
	}

	public static NavAppError FromException(Exception exception)
	{
		IEnumerable<Exception> enumerable2;
		if (!(exception is AggregateException ex))
		{
			IEnumerable<Exception> enumerable = ((exception.InnerException == null) ? new Exception[0] : new Exception[1] { exception.InnerException });
			enumerable2 = enumerable;
		}
		else
		{
			enumerable2 = ex.InnerExceptions.AsEnumerable();
		}
		IEnumerable<Exception> source = enumerable2;
		return new NavAppError(exception.Message, NavAppErrorType.FromExceptionType(exception.GetType()), source.Select(FromException));
	}

	public static NavAppError FromMultipleErrors(string message, IEnumerable<NavAppError> errors)
	{
		return new NavAppError(message, NavAppErrorType.Multiple, errors);
	}
}
