using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security;

namespace Microsoft.Dynamics.Nav.Types;

internal class EventLogEntryWriterProxy
{
	private static readonly int MaxApplicationLogSize = 25600;

	private readonly IEventLogEntryWriter eventEntryWriter;

	private readonly string eventSourceName;

	private readonly string message;

	private readonly EventLogEntryType errorType;

	private readonly int eventId;

	private readonly short taskCategory;

	private readonly byte[] rawData;

	internal EventLogEntryWriterProxy(string eventSourceName, string message, EventLogEntryType errorType, IEventLogEntryWriter eventEntryWriter)
		: this(eventSourceName, message, errorType, 0, 0, null, eventEntryWriter)
	{
	}

	internal EventLogEntryWriterProxy(string eventSourceName, string message, EventLogEntryType errorType, int eventId, short taskCategory, byte[] rawData, IEventLogEntryWriter eventEntryWriter)
	{
		this.eventSourceName = eventSourceName;
		this.message = message;
		this.errorType = errorType;
		this.taskCategory = taskCategory;
		this.rawData = rawData;
		this.eventId = eventId;
		this.eventEntryWriter = eventEntryWriter;
	}

	internal void WriteEntry()
	{
		string text = message;
		try
		{
			while (text.Length > MaxApplicationLogSize)
			{
				eventEntryWriter.WriteEntry(eventSourceName, text.Substring(0, MaxApplicationLogSize), errorType, eventId, taskCategory, rawData);
				text = text.Substring(MaxApplicationLogSize);
			}
			eventEntryWriter.WriteEntry(eventSourceName, text, errorType, eventId, taskCategory, rawData);
		}
		catch (SecurityException exception)
		{
			if (!ExceptionService.IgnoreSecurityException())
			{
				ExceptionService.FailFast(exception, text);
			}
		}
		catch (InvalidOperationException)
		{
		}
		catch (Win32Exception exception2)
		{
			if (!ExceptionService.IgnoreWin32Exception(exception2))
			{
				ExceptionService.FailFast(exception2, text);
			}
		}
	}
}
