using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

[SuppressMessage("ApiDesign", "RS0011", Justification = "Matching TPL Signatures")]
internal static class TaskExtensions
{
	public static T WaitAndGetResult<T>(this Task<T> task, CancellationToken cancellationToken)
	{
		task.Wait(cancellationToken);
		return task.Result;
	}

	public static Task SafeContinueWith(this Task task, Action<Task> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		Func<Task, bool> continuationFunction = (Task antecedent) =>
		{
			continuationAction(antecedent);
			return true;
		};
		return task.SafeContinueWith(continuationFunction, cancellationToken, continuationOptions, scheduler);
	}

	public static Task<TResult> SafeContinueWith<TInput, TResult>(this Task<TInput> task, Func<Task<TInput>, TResult> continuationFunction, CancellationToken cancellationToken, TaskScheduler scheduler)
	{
		return task.SafeContinueWith(continuationFunction, cancellationToken, TaskContinuationOptions.None, scheduler);
	}

	public static Task<TResult> SafeContinueWith<TInput, TResult>(this Task<TInput> task, Func<Task<TInput>, TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		return task.SafeContinueWith((Task antecedent) => continuationFunction((Task<TInput>)antecedent), cancellationToken, continuationOptions, scheduler);
	}

	public static Task SafeContinueWith<TInput>(this Task<TInput> task, Action<Task<TInput>> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		return task.SafeContinueWith((Task antecedent) =>
		{
			continuationAction((Task<TInput>)antecedent);
		}, cancellationToken, continuationOptions, scheduler);
	}

	public static Task<TResult> SafeContinueWith<TResult>(this Task task, Func<Task, TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		Func<Task, TResult> continuationFunction2 = (Task t) =>
		{
			try
			{
				return continuationFunction(t);
			}
			catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
			{
				throw ExceptionUtilities.Unreachable;
			}
		};
		return task.ContinueWith(continuationFunction2, cancellationToken, continuationOptions | TaskContinuationOptions.LazyCancellation, scheduler);
	}

	public static Task<TResult> SafeContinueWith<TResult>(this Task task, Func<Task, TResult> continuationFunction, CancellationToken cancellationToken, TaskScheduler scheduler)
	{
		return task.SafeContinueWith(continuationFunction, cancellationToken, TaskContinuationOptions.None, scheduler);
	}

	public static Task SafeContinueWith(this Task task, Action<Task> continuationAction, TaskScheduler scheduler)
	{
		return task.SafeContinueWith(continuationAction, CancellationToken.None, TaskContinuationOptions.None, scheduler);
	}

	public static Task SafeContinueWith<TInput>(this Task<TInput> task, Action<Task<TInput>> continuationFunction, TaskScheduler scheduler)
	{
		return task.SafeContinueWith(continuationFunction, CancellationToken.None, TaskContinuationOptions.None, scheduler);
	}

	public static Task<TResult> SafeContinueWith<TInput, TResult>(this Task<TInput> task, Func<Task<TInput>, TResult> continuationFunction, TaskScheduler scheduler)
	{
		return task.SafeContinueWith(continuationFunction, CancellationToken.None, TaskContinuationOptions.None, scheduler);
	}

	public static Task SafeContinueWith(this Task task, Action<Task> continuationAction, CancellationToken cancellationToken, TaskScheduler scheduler)
	{
		return task.SafeContinueWith(continuationAction, cancellationToken, TaskContinuationOptions.None, scheduler);
	}

	public static Task<TResult> ContinueWithAfterDelay<TInput, TResult>(this Task<TInput> task, Func<Task<TInput>, TResult> continuationFunction, CancellationToken cancellationToken, int millisecondsDelay, TaskContinuationOptions taskContinuationOptions, TaskScheduler scheduler)
	{
		return task.SafeContinueWith((Task<TInput> t) => Task.Delay(millisecondsDelay, cancellationToken).SafeContinueWith((Task _) => continuationFunction(t), cancellationToken, TaskContinuationOptions.None, scheduler), cancellationToken, taskContinuationOptions, scheduler).Unwrap();
	}

	public static Task<TNResult> ContinueWithAfterDelay<TNResult>(this Task task, Func<Task, TNResult> continuationFunction, CancellationToken cancellationToken, int millisecondsDelay, TaskContinuationOptions taskContinuationOptions, TaskScheduler scheduler)
	{
		return task.SafeContinueWith((Task t) => Task.Delay(millisecondsDelay, cancellationToken).SafeContinueWith((Task _) => continuationFunction(t), cancellationToken, TaskContinuationOptions.None, scheduler), cancellationToken, taskContinuationOptions, scheduler).Unwrap();
	}

	public static Task ContinueWithAfterDelay(this Task task, Action continuationAction, CancellationToken cancellationToken, int millisecondsDelay, TaskContinuationOptions taskContinuationOptions, TaskScheduler scheduler)
	{
		return task.SafeContinueWith((Task t) => Task.Delay(millisecondsDelay, cancellationToken).SafeContinueWith((Task _) =>
		{
			continuationAction();
		}, cancellationToken, TaskContinuationOptions.None, scheduler), cancellationToken, taskContinuationOptions, scheduler).Unwrap();
	}

	public static Task<TResult> SafeContinueWithFromAsync<TInput, TResult>(this Task<TInput> task, Func<Task<TInput>, Task<TResult>> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		return task.SafeContinueWithFromAsync((Task antecedent) => continuationFunction((Task<TInput>)antecedent), cancellationToken, continuationOptions, scheduler);
	}

	public static Task<TResult> SafeContinueWithFromAsync<TResult>(this Task task, Func<Task, Task<TResult>> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		Task<TResult> task2 = task.ContinueWith(continuationFunction, cancellationToken, continuationOptions | TaskContinuationOptions.LazyCancellation, scheduler).Unwrap();
		task2.ContinueWith(ReportFatalError, continuationFunction, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		return task2;
	}

	public static Task SafeContinueWithFromAsync(this Task task, Func<Task, Task> continuationFunction, CancellationToken cancellationToken, TaskScheduler scheduler)
	{
		return task.SafeContinueWithFromAsync(continuationFunction, cancellationToken, TaskContinuationOptions.None, scheduler);
	}

	public static Task SafeContinueWithFromAsync(this Task task, Func<Task, Task> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		Task task2 = task.ContinueWith(continuationFunction, cancellationToken, continuationOptions | TaskContinuationOptions.LazyCancellation, scheduler).Unwrap();
		ReportFatalError(task2, continuationFunction);
		return task2;
	}

	public static Task<TNResult> ContinueWithAfterDelayFromAsync<TNResult>(this Task task, Func<Task, Task<TNResult>> continuationFunction, CancellationToken cancellationToken, int millisecondsDelay, TaskContinuationOptions taskContinuationOptions, TaskScheduler scheduler)
	{
		return task.SafeContinueWith((Task t) => Task.Delay(millisecondsDelay, cancellationToken).SafeContinueWithFromAsync((Task _) => continuationFunction(t), cancellationToken, TaskContinuationOptions.None, scheduler), cancellationToken, taskContinuationOptions, scheduler).Unwrap();
	}

	public static Task ContinueWithAfterDelayFromAsync(this Task task, Func<Task, Task> continuationFunction, CancellationToken cancellationToken, int millisecondsDelay, TaskContinuationOptions taskContinuationOptions, TaskScheduler scheduler)
	{
		return task.SafeContinueWith((Task t) => Task.Delay(millisecondsDelay, cancellationToken).SafeContinueWithFromAsync((Task _) => continuationFunction(t), cancellationToken, TaskContinuationOptions.None, scheduler), cancellationToken, taskContinuationOptions, scheduler).Unwrap();
	}

	internal static void ReportFatalError(Task task, object continuationFunction)
	{
		task.ContinueWith(ReportFatalErrorWorker, continuationFunction, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	private static void ReportFatalErrorWorker(Task task, object continuationFunction)
	{
		AggregateException exception = task.Exception;
		MethodInfo methodInfo = ((Delegate)continuationFunction).GetMethodInfo();
		exception.Data["ContinuationFunction"] = methodInfo.DeclaringType.FullName + "::" + methodInfo.Name;
		FatalError.Report(exception);
	}

	public static T WaitAndGetResult_CanCallOnBackground<T>(this Task<T> task, CancellationToken cancellationToken)
	{
		try
		{
			task.Wait(cancellationToken);
		}
		catch (AggregateException ex)
		{
			ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
		}
		return task.Result;
	}
}
