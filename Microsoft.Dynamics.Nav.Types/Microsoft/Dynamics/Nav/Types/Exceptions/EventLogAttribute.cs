using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class EventLogAttribute : Attribute
{
	public bool WriteEntry { get; set; }

	public EventLogEntryType LogType { get; set; }

	public EventLogAttribute()
	{
		WriteEntry = true;
		LogType = EventLogEntryType.Error;
	}
}
