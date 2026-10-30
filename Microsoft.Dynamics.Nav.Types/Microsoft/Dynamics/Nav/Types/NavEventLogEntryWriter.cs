using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Types;

public class NavEventLogEntryWriter : IEventLogEntryWriter
{
	public void WriteEntry(string eventSourceName, string messageToWrite, EventLogEntryType entryType, int eventId = 0, short taskCategory = 0, byte[] rawData = null)
	{
		EventLog.WriteEntry(eventSourceName, messageToWrite, entryType, eventId, taskCategory, rawData);
	}
}
