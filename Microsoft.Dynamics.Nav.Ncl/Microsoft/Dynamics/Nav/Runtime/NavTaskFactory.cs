using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavTaskFactory
{
	public static Task RunTask(Func<Task> function)
	{
		return Task.Run(() =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				return function();
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		});
	}

	public static Task RunTask(Action action)
	{
		return Task.Run(() =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				action();
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		});
	}

	public static Task<TResult> RunTask<TResult>(Func<Task<TResult>> function)
	{
		return Task.Run(() =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				return function();
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		});
	}

	public static Task<TResult> RunTask<TResult>(Func<TResult> function)
	{
		return Task.Run(() =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				return function();
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		});
	}

	public static Task RunTask(Func<Task> function, CancellationToken cancellationToken)
	{
		return Task.Run(() =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				return function();
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		}, cancellationToken);
	}

	public static Task RunTask(Action action, CancellationToken cancellationToken)
	{
		return Task.Run(() =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				action();
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		}, cancellationToken);
	}

	public static Task<TResult> RunTask<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
	{
		return Task.Run(() =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				return function();
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		}, cancellationToken);
	}

	public static Task<TResult> RunTask<TResult>(Func<TResult> function, CancellationToken cancellationToken)
	{
		return Task.Run(() =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				return function();
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		}, cancellationToken);
	}

	public static Task StartNewTask(Action action)
	{
		return Task.Factory.StartNew(() =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				action();
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		});
	}

	public static Task StartNewTask(Action action, CancellationToken cancellationToken)
	{
		return Task.Factory.StartNew(() =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				action();
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		}, cancellationToken);
	}

	public static Task StartNewTask(Action<object> action, object state)
	{
		return Task.Factory.StartNew((object _) =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				action(null);
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		}, state);
	}

	public static Task StartNewTask(Action action, TaskCreationOptions creationOptions)
	{
		return Task.Factory.StartNew(() =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				action();
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		}, creationOptions);
	}

	public static Task<TResult> StartNewTask<TResult>(Func<TResult> function, CancellationToken cancellationToken)
	{
		return Task.Factory.StartNew(() =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				return function();
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		}, cancellationToken);
	}

	public static Task<TResult> StartNewTask<TResult>(Func<TResult> function, TaskCreationOptions creationOptions)
	{
		return Task.Factory.StartNew(() =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				return function();
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		}, creationOptions);
	}
}
