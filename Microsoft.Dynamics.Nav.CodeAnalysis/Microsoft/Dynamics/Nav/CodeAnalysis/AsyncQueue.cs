using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class AsyncQueue<TElement>
{
	private readonly Queue<TElement> data = new Queue<TElement>();

	private readonly TaskCompletionSource<bool> whenCompleted = new TaskCompletionSource<bool>();

	private bool disallowEnqueue;

	private bool completed;

	private Queue<TaskCompletionSource<TElement>> waiters;

	private object SyncObject => data;

	public int Count
	{
		get
		{
			lock (SyncObject)
			{
				return data.Count;
			}
		}
	}

	public bool IsCompleted
	{
		get
		{
			lock (SyncObject)
			{
				return completed;
			}
		}
	}

	public Task WhenCompletedTask => whenCompleted.Task;

	public void Enqueue(TElement value)
	{
		if (!EnqueueCore(value))
		{
			throw new InvalidOperationException("Cannot call {nameof(Enqueue)} when the queue is already completed.");
		}
	}

	public bool TryEnqueue(TElement value)
	{
		return EnqueueCore(value);
	}

	private bool EnqueueCore(TElement value)
	{
		if (disallowEnqueue)
		{
			throw new InvalidOperationException("Cannot enqueue data after PromiseNotToEnqueue.");
		}
		TaskCompletionSource<TElement> taskCompletionSource;
		lock (SyncObject)
		{
			if (completed)
			{
				return false;
			}
			if (waiters == null || waiters.Count == 0)
			{
				data.Enqueue(value);
				return true;
			}
			DebugAssertHelper.Assert(data.Count == 0);
			taskCompletionSource = waiters.Dequeue();
		}
		taskCompletionSource.SetResult(value);
		return true;
	}

	public bool TryDequeue(out TElement d)
	{
		lock (SyncObject)
		{
			if (data.Count == 0)
			{
				d = default;
				return false;
			}
			d = data.Dequeue();
			return true;
		}
	}

	public void Complete()
	{
		if (!CompleteCore())
		{
			throw new InvalidOperationException("Cannot call {nameof(Complete)} when the queue is already completed.");
		}
	}

	public bool TryComplete()
	{
		return CompleteCore();
	}

	private bool CompleteCore()
	{
		Queue<TaskCompletionSource<TElement>> existingWaiters;
		lock (SyncObject)
		{
			if (completed)
			{
				return false;
			}
			existingWaiters = waiters;
			completed = true;
			waiters = null;
		}
		Task.Run(() =>
		{
			if (existingWaiters != null)
			{
				foreach (TaskCompletionSource<TElement> item in existingWaiters)
				{
					item.SetCanceled();
				}
			}
			whenCompleted.SetResult(result: true);
		});
		return true;
	}

	public Task<TElement> DequeueAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return WithCancellation(DequeueAsyncCore(), cancellationToken);
	}

	private static Task<T> WithCancellation<T>(Task<T> task, CancellationToken cancellationToken)
	{
		if (task.IsCompleted || !cancellationToken.CanBeCanceled)
		{
			return task;
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return new Task<T>(() => default, cancellationToken);
		}
		return task.ContinueWith((Task<T> t) => t, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).Unwrap();
	}

	private Task<TElement> DequeueAsyncCore()
	{
		lock (SyncObject)
		{
			if (data.Count > 0)
			{
				return Task.FromResult(data.Dequeue());
			}
			if (completed)
			{
				TaskCompletionSource<TElement> taskCompletionSource = new TaskCompletionSource<TElement>();
				taskCompletionSource.SetCanceled();
				return taskCompletionSource.Task;
			}
			if (waiters == null)
			{
				waiters = new Queue<TaskCompletionSource<TElement>>();
			}
			TaskCompletionSource<TElement> taskCompletionSource2 = new TaskCompletionSource<TElement>();
			waiters.Enqueue(taskCompletionSource2);
			return taskCompletionSource2.Task;
		}
	}

	public void PromiseNotToEnqueue()
	{
		disallowEnqueue = true;
	}
}
