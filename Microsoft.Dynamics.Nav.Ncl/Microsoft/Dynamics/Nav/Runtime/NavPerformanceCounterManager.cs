using System;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavPerformanceCounterManager : IDisposable
{
	private PerformanceCounter[] performanceCounters;

	private readonly string performanceCounterInstanceName;

	public NavPerformanceCounterManager(string performanceCounterInstanceName)
	{
		this.performanceCounterInstanceName = performanceCounterInstanceName;
	}

	public virtual bool Initialize()
	{
		NavPerformanceCounterEntry[] array = (NavPerformanceCounterEntry[])Enum.GetValues(typeof(NavPerformanceCounterEntry));
		PerformanceCounter[] array2 = new PerformanceCounter[array.Length];
		int num = 0;
		FieldInfo[] fields = typeof(NavPerformanceCounterEntry).GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo element in fields)
		{
			NavPerformanceCounterMappingAttribute navPerformanceCounterMappingAttribute = (NavPerformanceCounterMappingAttribute)Attribute.GetCustomAttribute(element, typeof(NavPerformanceCounterMappingAttribute));
			PerformanceCounter performanceCounter = new PerformanceCounter
			{
				CategoryName = navPerformanceCounterMappingAttribute.CategoryName,
				CounterName = navPerformanceCounterMappingAttribute.CounterName,
				InstanceLifetime = PerformanceCounterInstanceLifetime.Global,
				InstanceName = performanceCounterInstanceName,
				ReadOnly = false
			};
			array2[num] = performanceCounter;
			num++;
		}
		try
		{
			PerformanceCounter[] array3 = array2;
			foreach (PerformanceCounter performanceCounter2 in array3)
			{
				performanceCounter2.RawValue = 0L;
			}
			performanceCounters = array2;
			return true;
		}
		catch (InvalidOperationException)
		{
			return false;
		}
	}

	public virtual void IncrementCounter(NavPerformanceCounterEntry counter)
	{
		if (performanceCounters != null)
		{
			performanceCounters[(int)counter].Increment();
		}
	}

	public virtual void DecrementCounter(NavPerformanceCounterEntry counter)
	{
		if (performanceCounters != null)
		{
			performanceCounters[(int)counter].Decrement();
		}
	}

	public virtual void IncrementCounterBy(NavPerformanceCounterEntry counter, long value)
	{
		if (performanceCounters != null)
		{
			performanceCounters[(int)counter].IncrementBy(value);
		}
	}

	public virtual void SetCounter(long newValue, NavPerformanceCounterEntry counter)
	{
		if (performanceCounters != null)
		{
			performanceCounters[(int)counter].RawValue = newValue;
		}
	}

	public virtual long GetCounterRawValue(NavPerformanceCounterEntry counter)
	{
		if (performanceCounters != null)
		{
			return performanceCounters[(int)counter].RawValue;
		}
		throw new InvalidOperationException("Performance countersare not initialized");
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (!disposing || performanceCounters == null)
		{
			return;
		}
		PerformanceCounter[] array = performanceCounters;
		foreach (PerformanceCounter performanceCounter in array)
		{
			if (performanceCounter != null)
			{
				performanceCounter.RemoveInstance();
				performanceCounter.Dispose();
			}
		}
	}
}
