using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class InitializeAppGroupProfilerSession
{
	private readonly Dictionary<ObjectType, InitializeAppGroupProfilerEntry> entriesByType = new Dictionary<ObjectType, InitializeAppGroupProfilerEntry>();

	public IEnumerable<InitializeAppGroupProfilerEntry> Entries => entriesByType.Values;

	public TimeSpan TotalTime { get; private set; }

	public InitializeAppGroupProfilerSession()
	{
		ObjectType[] supportedTypes = InitializeAppGroupProfiler.SupportedTypes;
		foreach (ObjectType objectType in supportedTypes)
		{
			entriesByType.Add(objectType, new InitializeAppGroupProfilerEntry(objectType));
		}
	}

	public void RecordObjectTiming(ObjectType type, bool constructedNew, TimeSpan elapsed)
	{
		if (entriesByType.TryGetValue(type, out var value))
		{
			value.RecordResult(constructedNew, elapsed);
		}
	}

	public void RecordTotalTime(TimeSpan elapsed)
	{
		TotalTime = elapsed;
	}
}
