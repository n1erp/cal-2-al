using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Diagnostic;

public class MonitorCollection : IReadOnlyList<long>, IReadOnlyCollection<long>, IEnumerable<long>, IEnumerable
{
	private static readonly int monitorTypeCount = Enum.GetValues(typeof(MonitorType)).OfType<MonitorType>().Max((MonitorType mt) => MonitorTypeIndex(mt)) + 1;

	private static string[] monitorTypeNames;

	protected long[] Values { get; } = new long[monitorTypeCount];

	public static int MonitorTypeCount => monitorTypeCount;

	public MonitorCollection Parent { get; protected set; }

	public static IReadOnlyList<string> MonitorTypeNames
	{
		get
		{
			if (monitorTypeNames == null)
			{
				string[] array = new string[MonitorTypeCount];
				foreach (MonitorType item in Enum.GetValues(typeof(MonitorType)).OfType<MonitorType>())
				{
					array[MonitorTypeIndex(item)] = item.ToString();
				}
				monitorTypeNames = array;
			}
			return monitorTypeNames;
		}
	}

	public MonitorType MonitorTypes { get; protected set; }

	public DateTime StartTime { get; protected set; }

	public TimeSpan ElapsedTime { get; protected set; }

	public bool SingleThreadAccess { get; protected set; }

	public long this[MonitorType monitorType] => Values[MonitorTypeIndex(monitorType)];

	public int Count => monitorTypeCount;

	public long this[int index] => Values[index];

	protected static int MonitorTypeIndex(MonitorType monitor)
	{
		return (int)(monitor & (MonitorType)0xFFFF);
	}

	public MonitorCollection(bool singleThreadAccess = false)
	{
		SingleThreadAccess = singleThreadAccess;
		StartTime = DateTime.UtcNow;
	}

	public MonitorCollection(MonitorCollection parent, MonitorType monitorTypes, bool singleThreadAccess = false)
		: this(singleThreadAccess)
	{
		Parent = parent;
		MonitorTypes = (MonitorType)((long)monitorTypes & -65536L);
	}

	private void InternalIncrement(int index, long delta)
	{
		if (SingleThreadAccess)
		{
			Values[index] += delta;
		}
		else
		{
			Interlocked.Add(ref Values[index], delta);
		}
	}

	public void Rollup(MonitorCollection destination)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		ElapsedTime = DateTime.UtcNow - StartTime;
		for (int i = 0; i < Values.Length; i++)
		{
			destination.InternalIncrement(i, Values[i]);
		}
		destination.MonitorTypes |= MonitorTypes;
	}

	public IEnumerator<long> GetEnumerator()
	{
		return ((IEnumerable<long>)Values).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
