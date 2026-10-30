using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Types;

public interface IEventLogEntryWriter
{
	void WriteEntry(string eventSourceName, string messageToWrite, EventLogEntryType entryType, int eventId, short taskCategory, byte[] rawData);
}
