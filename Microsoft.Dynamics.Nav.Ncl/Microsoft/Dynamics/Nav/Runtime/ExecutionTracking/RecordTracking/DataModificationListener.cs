using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.ExecutionTracking.RecordTracking;

internal static class DataModificationListener
{
	private class CompoundDataModificationListener : IDataModificationListener
	{
		private readonly IDataModificationListener[] listeners;

		internal IDataModificationListener[] Listeners => listeners;

		public int Priority
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public CompoundDataModificationListener(IEnumerable<IDataModificationListener> listeners)
		{
			this.listeners = listeners.ToArray();
		}

		public bool CanCoexistWith(IDataModificationListener other)
		{
			throw new NotSupportedException();
		}

		public void OnBeforeInsert(NavRecord record)
		{
			IDataModificationListener[] array = listeners;
			foreach (IDataModificationListener dataModificationListener in array)
			{
				dataModificationListener.OnBeforeInsert(record);
			}
		}

		public void OnBeforeModify(NavRecord record)
		{
			IDataModificationListener[] array = listeners;
			foreach (IDataModificationListener dataModificationListener in array)
			{
				dataModificationListener.OnBeforeModify(record);
			}
		}

		public void OnBeforeDelete(NavRecord record)
		{
			IDataModificationListener[] array = listeners;
			foreach (IDataModificationListener dataModificationListener in array)
			{
				dataModificationListener.OnBeforeDelete(record);
			}
		}

		public void OnBeforeRename(NavRecord record)
		{
			IDataModificationListener[] array = listeners;
			foreach (IDataModificationListener dataModificationListener in array)
			{
				dataModificationListener.OnBeforeRename(record);
			}
		}

		public void OnBeforeModifyAll(NavRecord record)
		{
			IDataModificationListener[] array = listeners;
			foreach (IDataModificationListener dataModificationListener in array)
			{
				dataModificationListener.OnBeforeModifyAll(record);
			}
		}

		public void OnBeforeDeleteAll(NavRecord record)
		{
			IDataModificationListener[] array = listeners;
			foreach (IDataModificationListener dataModificationListener in array)
			{
				dataModificationListener.OnBeforeDeleteAll(record);
			}
		}
	}

	internal static int HighestPriority = 1000000;

	private static readonly object syncRoot = new object();

	public static IDataModificationListener Instance { get; private set; }

	public static IEnumerable<IDataModificationListener> AllRegisteredListeners => Instance.AsEnumerableOfListeners();

	public static bool AddListener(IDataModificationListener newModificationListener)
	{
		if (newModificationListener == null)
		{
			throw new ArgumentNullException("newModificationListener");
		}
		lock (syncRoot)
		{
			List<IDataModificationListener> list = Instance.AsEnumerableOfListeners().ToList();
			if (list.Any((IDataModificationListener existingListener) => !existingListener.CanCoexistWith(newModificationListener)))
			{
				return false;
			}
			list.Add(newModificationListener);
			Instance = list.OrderByDescending((IDataModificationListener listener) => listener.Priority).AsSingleInstanceOrNull();
			return true;
		}
	}

	public static void RemoveListener(IDataModificationListener modificationListener)
	{
		lock (syncRoot)
		{
			IOrderedEnumerable<IDataModificationListener> listeners = from listener in Instance.AsEnumerableOfListeners()
				where listener != modificationListener
				orderby listener.Priority descending
				select listener;
			Instance = listeners.AsSingleInstanceOrNull();
		}
	}

	private static IEnumerable<IDataModificationListener> AsEnumerableOfListeners(this IDataModificationListener listener)
	{
		if (listener == null)
		{
			return new IDataModificationListener[0];
		}
		if (listener is CompoundDataModificationListener compoundDataModificationListener)
		{
			return compoundDataModificationListener.Listeners.SelectMany(AsEnumerableOfListeners);
		}
		return new IDataModificationListener[1] { listener };
	}

	private static IDataModificationListener AsSingleInstanceOrNull(this IEnumerable<IDataModificationListener> listeners)
	{
		return listeners.Count() switch
		{
			0 => null, 
			1 => listeners.First(), 
			_ => new CompoundDataModificationListener(listeners), 
		};
	}
}
