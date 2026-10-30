using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavSqlTransientErrorHelper
{
	private const int DefaultMaxRetryCount = 30;

	private const int DefaultRetryInterval = 10;

	public static T RetryOperationWithTransientError<T>(Func<T> operation, int maxRetryCount = 30, int retryIntervalInSeconds = 10, [CallerMemberName] string caller = null, string additionalContext = null)
	{
		T result = default;
		InternalRetryOperationWithTransientError<Exception>(() =>
		{
			result = operation();
		}, maxRetryCount, retryIntervalInSeconds, caller, additionalContext, expectedType: false);
		return result;
	}

	public static T RetryOperationWithTransientError<T, TException>(Func<T> operation, int maxRetryCount = 30, int retryIntervalInSeconds = 10, [CallerMemberName] string caller = null, string additionalContext = null) where TException : Exception
	{
		T result = default;
		InternalRetryOperationWithTransientError<TException>(() =>
		{
			result = operation();
		}, maxRetryCount, retryIntervalInSeconds, caller, additionalContext, expectedType: true);
		return result;
	}

	public static void RetryOperationWithTransientError(Action operation, int maxRetryCount = 30, int retryIntervalInSeconds = 10, [CallerMemberName] string caller = null, string additionalContext = null)
	{
		InternalRetryOperationWithTransientError<Exception>(operation, maxRetryCount, retryIntervalInSeconds, caller, additionalContext, expectedType: false);
	}

	public static void RetryOperationWithTransientError<T>(Action operation, int maxRetryCount = 30, int retryIntervalInSeconds = 10, [CallerMemberName] string caller = null, string additionalContext = null) where T : Exception
	{
		InternalRetryOperationWithTransientError<T>(operation, maxRetryCount, retryIntervalInSeconds, caller, additionalContext, expectedType: true);
	}

	private static void InternalRetryOperationWithTransientError<T>(Action operation, int maxRetryCount, int retryIntervalInSeconds, string callerName, string additionaInfo, bool expectedType) where T : Exception
	{
		int num = 0;
		while (true)
		{
			try
			{
				operation();
				break;
			}
			catch (NavSqlException ex) when (num < maxRetryCount && NavSqlReturnCode.IsTransientError(ex.ErrorNumber))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000BV", Category.Operations, ex, Verbosity.Normal);
				num++;
				Thread.Sleep(retryIntervalInSeconds * 1000);
				SendContextualTrace(ex, callerName, additionaInfo);
			}
			catch (NavCSideException ex2) when (num < maxRetryCount && ex2.ErrorNumber == 22924076)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000BW", Category.Operations, ex2, Verbosity.Normal);
				num++;
				Thread.Sleep(retryIntervalInSeconds * 1000);
				SendContextualTrace(ex2, callerName, additionaInfo);
			}
			catch (T val) when (expectedType)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000011X", Category.Operations, val, Verbosity.Normal);
				num++;
				Thread.Sleep(retryIntervalInSeconds * 1000);
				SendContextualTrace(val, callerName, additionaInfo);
			}
		}
	}

	private static void SendContextualTrace(Exception e, string callerName, string additionaInfo)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000011Y", Category.Operations, e, Verbosity.Normal, DiagnosticParameter.SystemMetadata("Transient error exception detected. Caller -- " + callerName + "; AdditionalInfo -- " + additionaInfo));
	}
}
