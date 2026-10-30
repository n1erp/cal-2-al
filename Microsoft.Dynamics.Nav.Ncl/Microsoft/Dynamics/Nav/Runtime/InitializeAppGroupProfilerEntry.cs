using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class InitializeAppGroupProfilerEntry
{
	public ObjectType ObjectType { get; }

	public int Constructed { get; set; }

	public int Referenced { get; set; }

	public TimeSpan ProcessingTime { get; set; }

	public InitializeAppGroupProfilerEntry(ObjectType objectType)
	{
		ObjectType = objectType;
	}

	public void RecordResult(bool constructed, TimeSpan elapsed)
	{
		Constructed += (constructed ? 1 : 0);
		Referenced += ((!constructed) ? 1 : 0);
		ProcessingTime += elapsed;
	}
}
