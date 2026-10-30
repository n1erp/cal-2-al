using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

public static class EventLogWriter
{
	private class ServerExceptionService : ExceptionService
	{
		protected override bool IgnoreSecurityExceptionInternal()
		{
			return false;
		}

		protected override bool IgnoreWin32ExceptionInternal(Win32Exception exception)
		{
			if (exception.NativeErrorCode == 1502)
			{
				return true;
			}
			return false;
		}

		public static void CreateInstance()
		{
			if (ExceptionService.Instance == null)
			{
				ExceptionService.Instance = new ServerExceptionService();
			}
		}
	}

	private class ClientExceptionService : ExceptionService
	{
		protected override bool IgnoreSecurityExceptionInternal()
		{
			return true;
		}

		protected override bool IgnoreWin32ExceptionInternal(Win32Exception exception)
		{
			return true;
		}

		public static void CreateInstance()
		{
			if (ExceptionService.Instance == null)
			{
				ExceptionService.Instance = new ClientExceptionService();
			}
		}
	}

	public const int DefaultEventId = 0;

	private static string eventLogSourceName = CreateEventLogSourceName();

	private static IEventLogEntryWriter eventLogEntryWriter = new NavEventLogEntryWriter();

	private static readonly Queue<EventLogEntryWriterProxy> entryQueue = new Queue<EventLogEntryWriterProxy>();

	private static Thread processEventQueueThread;

	private static bool cancelRequested = false;

	public static Action<string, EventLogEntryType, string> CustomWriter { get; set; }

	public static string EventLogSourceName
	{
		get
		{
			return eventLogSourceName;
		}
		set
		{
			eventLogSourceName = value;
		}
	}

	public static IEventLogEntryWriter EventLogEntryWriter
	{
		get
		{
			return eventLogEntryWriter;
		}
		set
		{
			eventLogEntryWriter = value;
		}
	}

	public static void Write(Exception exception)
	{
		if (GetEventLogEntryTypeForException(exception, ignoreNavBaseExceptionsUnlessAttributed: false, out var eventLogEntryType))
		{
			Write(exception, eventLogEntryType);
		}
	}

	public static bool GetEventLogEntryTypeForException(Exception exception, bool ignoreNavBaseExceptionsUnlessAttributed, out EventLogEntryType eventLogEntryType)
	{
		eventLogEntryType = EventLogEntryType.Error;
		if (exception == null)
		{
			return false;
		}
		EventLogAttribute eventLogAttribute = GetEventLogAttribute(exception);
		if (eventLogAttribute != null)
		{
			eventLogEntryType = eventLogAttribute.LogType;
			return eventLogAttribute.WriteEntry;
		}
		if (exception is NavBaseException)
		{
			if (ignoreNavBaseExceptionsUnlessAttributed)
			{
				return false;
			}
			eventLogEntryType = EventLogEntryType.Warning;
		}
		return true;
	}

	public static void StopAndWait()
	{
		lock (entryQueue)
		{
			if (cancelRequested || entryQueue.Count == 0 || processEventQueueThread == null)
			{
				return;
			}
			cancelRequested = true;
			Monitor.Pulse(entryQueue);
		}
		processEventQueueThread?.Join(5000);
	}

	private static EventLogAttribute GetEventLogAttribute(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		Type type = exception.GetType();
		while (type != typeof(Exception))
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(EventLogAttribute), inherit: false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				EventLogAttribute eventLogAttribute = (EventLogAttribute)customAttributes[i];
				if (eventLogAttribute != null)
				{
					return eventLogAttribute;
				}
			}
			type = type.BaseType;
		}
		return null;
	}

	public static void Write(Exception exception, EventLogEntryType errorType)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		Write(ExceptionHandler.FormatException(exception), errorType, null, 0, null, 0);
	}

	public static void Write(string message, EventLogEntryType errorType, string eventSource = null, int eventId = 0, IEventLogEntryWriter writer = null, short taskCategory = 0, byte[] rawData = null)
	{
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		string text = eventSource ?? EventLogSourceName;
		if (CustomWriter != null)
		{
			CustomWriter(text, errorType, message);
		}
		else
		{
			EnqueueMessage(text, message, errorType, eventId, writer, taskCategory, rawData);
		}
	}

	private static void EnqueueMessage(string eventSource, string message, EventLogEntryType errorType, int eventId, IEventLogEntryWriter writer, short taskCategory, byte[] rawData)
	{
		EventLogEntryWriterProxy item = new EventLogEntryWriterProxy(eventSource, message, errorType, eventId, taskCategory, rawData, writer ?? EventLogEntryWriter);
		lock (entryQueue)
		{
			entryQueue.Enqueue(item);
			if (entryQueue.Count == 1)
			{
				if (processEventQueueThread == null)
				{
					processEventQueueThread = new Thread(ProcessEventQueue)
					{
						IsBackground = false,
						Name = "EventLog"
					};
					processEventQueueThread.Start();
				}
				else
				{
					processEventQueueThread.IsBackground = false;
					Monitor.Pulse(entryQueue);
				}
			}
		}
	}

	private static void ProcessEventQueue()
	{
		while (true)
		{
			EventLogEntryWriterProxy eventLogEntryWriterProxy;
			lock (entryQueue)
			{
				if (entryQueue.Count == 0)
				{
					processEventQueueThread.IsBackground = true;
					Monitor.Wait(entryQueue, TimeSpan.FromMinutes(1.0));
					if (cancelRequested)
					{
						break;
					}
					if (entryQueue.Count == 0)
					{
						processEventQueueThread = null;
						break;
					}
				}
				eventLogEntryWriterProxy = entryQueue.Dequeue();
			}
			eventLogEntryWriterProxy.WriteEntry();
		}
	}

	private static string CreateEventLogSourceName()
	{
		Assembly assembly = null;
		try
		{
			assembly = Assembly.GetEntryAssembly();
		}
		catch (InvalidOperationException)
		{
		}
		if (assembly == null)
		{
			assembly = Assembly.GetExecutingAssembly();
		}
		return DecorateEventLogSourceName(assembly.GetName().Name);
	}

	public static string DecorateEventLogSourceName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		string[] array = name.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
		StringBuilder stringBuilder = new StringBuilder(array.Length);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.Equals("NAV", StringComparison.OrdinalIgnoreCase))
			{
				stringBuilder.Append("NAV");
			}
			else
			{
				stringBuilder.Append(text);
			}
		}
		return stringBuilder.ToString();
	}

	public static void CreateClientExceptionService()
	{
		ClientExceptionService.CreateInstance();
	}

	public static void CreateServerExceptionService()
	{
		ServerExceptionService.CreateInstance();
	}
}
