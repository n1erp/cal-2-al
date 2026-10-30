using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Dynamics.Nav.Diagnostic.EventSource;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class EventEntryAdaptor
{
	internal string Message { get; set; }

	internal EventLogEntryType EntryType { get; set; }

	internal int EventId { get; set; }

	internal short TaskCategory { get; set; }

	internal static EventEntryAdaptor CreateInstance(EventWrittenEventArgs eventArgs)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (eventArgs == null)
		{
			return null;
		}
		if ((object)eventArgs.EventSource != NavServerEventSource.Log && (object)eventArgs.EventSource != NavDiagnosticsEventSource.Log)
		{
			return null;
		}
		if (!NavServerEventSource.Keywords.Compare(eventArgs.Keywords, (EventKeywords)1))
		{
			return null;
		}
		return new EventEntryAdaptor
		{
			EntryType = LevelToEventLogEntryType(eventArgs.Level),
			EventId = eventArgs.EventId,
			Message = GetMessageFromPayLoad(eventArgs.Payload, eventArgs.Message),
			TaskCategory = (short)eventArgs.Task
		};
	}

	private static string GetMessageFromPayLoad(ReadOnlyCollection<object> payLoad, string messageFormat)
	{
		return string.Format(CultureInfo.InvariantCulture, messageFormat, payLoad.ToArray());
	}

	private static EventLogEntryType LevelToEventLogEntryType(EventLevel level)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected I4, but got Unknown
		switch ((int)level)
		{
		case 1:
		case 2:
			return EventLogEntryType.Error;
		case 3:
			return EventLogEntryType.Warning;
		case 0:
		case 4:
		case 5:
			return EventLogEntryType.Information;
		default:
			return EventLogEntryType.Error;
		}
	}
}
