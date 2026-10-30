using System.Diagnostics;
using Microsoft.Dynamics.Nav.Apps.Utility;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class LogWriterHelperServerLogger : IServerLogger
{
	public void LogEvent(EventLogEntryType entryType, string message, bool isExceptionEvent = false)
	{
		LogWriterHelper.LogEvent(entryType, message, isExceptionEvent);
	}
}
