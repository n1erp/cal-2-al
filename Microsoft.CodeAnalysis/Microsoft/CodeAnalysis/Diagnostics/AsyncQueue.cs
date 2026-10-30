using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class AsyncQueue<TElement>
{
	private readonly TaskCompletionSource<bool> _whenCompleted = new TaskCompletionSource<bool>();

	private readonly Queue<TElement> _data = new Queue<TElement>();

	private Queue<TaskCompletionSource<TElement>> _waiters;

	private bool _completed;

	private bool _disallowEnqueue;

	private object SyncObject => _data;

	public int Count
	{
		get
		{
			lock (SyncObject)
			{
				return _data.Count;
			}
		}
	}

	public bool IsCompleted
	{
		get
		{
			lock (SyncObject)
			{
				return _completed;
			}
		}
	}

	public Task WhenCompletedTask => _whenCompleted.Task;

	public void Enqueue(TElement value)
	{
		if (!EnqueueCore(value))
		{
			throw new InvalidOperationException("Cannot call Enqueue when the queue is already completed.");
		}
	}

	public bool TryEnqueue(TElement value)
	{
		return EnqueueCore(value);
	}

	private bool EnqueueCore(TElement value)
	{
		if (_disallowEnqueue)
		{
			throw new InvalidOperationException("Cannot enqueue data after PromiseNotToEnqueue.");
		}
		TaskCompletionSource<TElement> waiter;
		lock (SyncObject)
		{
			if (_completed)
			{
				return false;
			}
			if (_waiters == null || _waiters.Count == 0)
			{
				_data.Enqueue(value);
				return true;
			}
			waiter = _waiters.Dequeue();
		}
		Task.Run(() =>
		{
			waiter.SetResult(value);
		});
		return true;
	}

	public bool TryDequeue(out TElement d)
	{
		lock (SyncObject)
		{
			if (_data.Count == 0)
			{
				d = default;
				return false;
			}
			d = _data.Dequeue();
			return true;
		}
	}

	public void Complete()
	{
		if (!CompleteCore())
		{
			throw new InvalidOperationException("Cannot call Complete when the queue is already completed.");
		}
	}

	public void PromiseNotToEnqueue()
	{
		_disallowEnqueue = true;
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
			if (_completed)
			{
				return false;
			}
			_completed = true;
			existingWaiters = _waiters;
			_waiters = null;
		}
		Task.Run(() =>
		{
			Queue<TaskCompletionSource<TElement>> queue = existingWaiters;
			if (queue != null && queue.Count > 0)
			{
				foreach (TaskCompletionSource<TElement> item in existingWaiters)
				{
					item.SetCanceled();
				}
			}
			_whenCompleted.SetResult(result: true);
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
			if (_data.Count > 0)
			{
				return Task.FromResult(_data.Dequeue());
			}
			if (_completed)
			{
				TaskCompletionSource<TElement> taskCompletionSource = new TaskCompletionSource<TElement>();
				taskCompletionSource.SetCanceled();
				return taskCompletionSource.Task;
			}
			if (_waiters == null)
			{
				_waiters = new Queue<TaskCompletionSource<TElement>>();
			}
			TaskCompletionSource<TElement> taskCompletionSource2 = new TaskCompletionSource<TElement>();
			_waiters.Enqueue(taskCompletionSource2);
			return taskCompletionSource2.Task;
		}
	}
}
