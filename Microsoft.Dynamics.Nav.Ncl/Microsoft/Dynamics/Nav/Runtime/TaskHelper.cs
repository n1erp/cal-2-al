using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class TaskHelper
{
	public static readonly Task CompletedTask = Task.FromResult<object>(null);

	public static Task LogOrFailOnException(this Task task, bool failFast = false, bool ignoreNavBaseExceptionUnlessAttributed = false, Action<string, Exception> failFastHandler = null)
	{
		if (task == null)
		{
			return Task.FromResult(result: true);
		}
		return task.ContinueWith((Task antecedent) =>
		{
			AggregateException ex = antecedent.Exception.Flatten();
			foreach (Exception innerException in ex.InnerExceptions)
			{
				LogWriterHelper.LogExceptionEvent(innerException, ignoreNavBaseExceptionUnlessAttributed);
				if (!(innerException is NavBaseException) && !(innerException is NavServerSessionBaseException))
				{
					NavEnvironment.WriteWatsonLog(innerException);
					if (failFast)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000MN", Category.Runtime, innerException, Verbosity.Critical);
						if (failFastHandler != null)
						{
							failFastHandler(innerException.ToString(), innerException);
						}
						else
						{
							DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001MS", Category.Runtime, innerException, Verbosity.Critical);
							EventLogWriter.StopAndWait();
							Environment.FailFast(innerException.ToString(), innerException);
						}
					}
				}
			}
		}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
	}

	public static Task LogOrFailOnExceptionReturnOriginalTask(this Task task, bool failFast = true, bool ignoreNavBaseExceptionUnlessAttributed = false)
	{
		task.LogOrFailOnException(failFast, ignoreNavBaseExceptionUnlessAttributed);
		return task;
	}

	public static Task TerminateOnException<T>(this Task task) where T : Exception
	{
		return task.ContinueWith((Task antecedent) =>
		{
			AggregateException ex = antecedent.Exception.Flatten();
			Exception ex2 = ex.InnerExceptions.OfType<T>().FirstOrDefault();
			if (ex2 != null)
			{
				NavEnvironment.Terminate(ex2, withGrace: true);
			}
			else
			{
				antecedent.Exception.Rethrow();
			}
		}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
	}

	public static void WaitThrowingOriginalException(this Task task)
	{
		task.GetAwaiter().GetResult();
	}

	public static T WaitThrowingOriginalException<T>(this Task<T> task)
	{
		return task.GetAwaiter().GetResult();
	}

	public static void Rethrow(this Exception exception)
	{
		ExceptionDispatchInfo.Capture(exception).Throw();
	}
}
