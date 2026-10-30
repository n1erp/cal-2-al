using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Dynamics.Nav.Diagnostic.EventSource;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavServerEventListener : EventListener
{
	private static readonly Lazy<NavServerEventListener> instance = new Lazy<NavServerEventListener>(() => new NavServerEventListener());

	private readonly IEventLogEntryWriter eventLogWriter;

	public static NavServerEventListener Instance => instance.Value;

	public string EventSourceName { get; set; }

	public NavServerEventListener(IEventLogEntryWriter eventLogWriterParameter = null)
	{
		eventLogWriter = eventLogWriterParameter;
	}

	public void InitializeEvents()
	{
		if (ServerUserSettings.Instance.EnableApplicationChannelLog.Value)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string> { 
			{
				"IgnoreOnEventCommand",
				true.ToString()
			} };
			((EventListener)this).EnableEvents((EventSource)(object)NavServerEventSource.Log, (EventLevel)0, (EventKeywords)1, (IDictionary<string, string>)dictionary);
			((EventListener)this).EnableEvents((EventSource)(object)NavDiagnosticsEventSource.Log, (EventLevel)0, (EventKeywords)1, (IDictionary<string, string>)dictionary);
		}
	}

	protected override void OnEventWritten(EventWrittenEventArgs eventData)
	{
		if (eventData == null)
		{
			throw new ArgumentNullException("eventData");
		}
		EventEntryAdaptor adaptor = EventEntryAdaptor.CreateInstance(eventData);
		WriteToEventLog(adaptor);
	}

	private void WriteToEventLog(EventEntryAdaptor adaptor)
	{
		if (adaptor != null && !string.IsNullOrEmpty(adaptor.Message))
		{
			EventLogWriter.Write(adaptor.Message, adaptor.EntryType, EventSourceName, adaptor.EventId, eventLogWriter, adaptor.TaskCategory);
		}
	}
}
