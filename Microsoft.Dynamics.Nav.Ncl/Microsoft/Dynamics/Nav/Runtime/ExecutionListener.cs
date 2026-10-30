using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class ExecutionListener
{
	private class CompoundExecutionListener : IExecutionListener
	{
		private readonly IExecutionListener[] listeners;

		internal IExecutionListener[] Listeners => listeners;

		public CompoundExecutionListener(IEnumerable<IExecutionListener> listeners)
		{
			this.listeners = listeners.ToArray();
		}

		public void ProcessExit(NavMethodScope methodScope)
		{
			for (int i = 0; i < listeners.Length; i++)
			{
				listeners[i].ProcessExit(methodScope);
			}
		}

		public void ProcessStatementHit(NavMethodScope methodScope)
		{
			for (int i = 0; i < listeners.Length; i++)
			{
				listeners[i].ProcessStatementHit(methodScope);
			}
		}

		public void ProcessException(NavMethodScope methodScope, Exception exception)
		{
			for (int i = 0; i < listeners.Length; i++)
			{
				listeners[i].ProcessException(methodScope, exception);
			}
		}

		public bool CanCoexistWithOtherListener(IExecutionListener otherListener)
		{
			throw new NotSupportedException();
		}
	}

	private static readonly object syncRoot = new object();

	public static IExecutionListener Instance { get; private set; }

	public static IEnumerable<IExecutionListener> AllRegisteredListeners => Instance.AsEnumerableOfListeners();

	public static bool AddListener(IExecutionListener newListener)
	{
		if (newListener == null)
		{
			throw new ArgumentNullException("newListener");
		}
		lock (syncRoot)
		{
			List<IExecutionListener> list = Instance.AsEnumerableOfListeners().ToList();
			if (list.Any((IExecutionListener existingListener) => !existingListener.CanCoexistWithOtherListener(newListener)))
			{
				return false;
			}
			list.Add(newListener);
			Instance = list.AsSingleInstanceOrNull();
			return true;
		}
	}

	public static void RemoveListener(IExecutionListener listener)
	{
		lock (syncRoot)
		{
			Instance = (from l in Instance.AsEnumerableOfListeners()
				where l != listener
				select l).AsSingleInstanceOrNull();
		}
	}

	private static IEnumerable<IExecutionListener> AsEnumerableOfListeners(this IExecutionListener executionListener)
	{
		if (executionListener == null)
		{
			return new IExecutionListener[0];
		}
		if (executionListener is CompoundExecutionListener compoundExecutionListener)
		{
			return compoundExecutionListener.Listeners.SelectMany(AsEnumerableOfListeners);
		}
		return new IExecutionListener[1] { executionListener };
	}

	private static IExecutionListener AsSingleInstanceOrNull(this IEnumerable<IExecutionListener> listeners)
	{
		return listeners.Count() switch
		{
			0 => null, 
			1 => listeners.First(), 
			_ => new CompoundExecutionListener(listeners), 
		};
	}
}
