using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class NavPerformanceCounterMappingAttribute : Attribute
{
	public string CategoryName { get; private set; }

	public string CounterName { get; private set; }

	public PerformanceCounterType CounterType { get; private set; }

	public NavPerformanceCounterMappingAttribute(string categoryName, string counterName, PerformanceCounterType counterType)
	{
		CategoryName = categoryName;
		CounterName = counterName;
		CounterType = counterType;
	}
}
