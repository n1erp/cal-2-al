using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Diagnostic.EventSource;

[EventSource(Name = "Microsoft-DynamicsNAV-Common")]
public sealed class NavDiagnosticsEventSource : EventSource
{
	public static class Keywords
	{
		private const ulong ReservedBitMask = 17592186044415uL;

		public const EventKeywords EventViewer = (EventKeywords)1L;

		public const EventKeywords TelemetryTracing = (EventKeywords)32L;

		[NonEvent]
		internal static bool Compare(EventKeywords keyword1, EventKeywords keyword2)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected I8, but got Unknown
			//IL_0018: Invalid comparison between Unknown and I8
			_003F val = keyword1 & 0xFFFFFFFFFFFL;
			ulong num = (ulong)(keyword2 & 0xFFFFFFFFFFFL);
			return (long)val == (long)num;
		}
	}

	public static class Tasks
	{
		public const EventTask TelemetryData = (EventTask)33;
	}

	private class NavDiagnosticsEventViewerEventsImplementation : INavDiagnosticsEventViewerEvents
	{
		private readonly NavDiagnosticsEventSource eventSource;

		public NavDiagnosticsEventViewerEventsImplementation(NavDiagnosticsEventSource eventSource)
		{
			this.eventSource = eventSource;
		}

		[NonEvent]
		void INavDiagnosticsEventViewerEvents.WriteTelemetryEvent(int eventId, string serverInstanceName, string navTenantId, string category, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, DateTime eventTime, string message, int processId, string tag, int threadId, string counterInformation, string alternateTenantIds, string platformVersion, int privacyClassification, Guid transientEventId, string aadTenantId, string aadUserId)
		{
			WriteEventFast(eventId, serverInstanceName, navTenantId, category, clientSessionId, clientActivityId, serverSessionId, serverActivityId, eventTime, message, processId, tag, threadId, counterInformation, alternateTenantIds, platformVersion, privacyClassification, transientEventId, aadTenantId, aadUserId);
		}

		[NonEvent]
		void INavDiagnosticsEventViewerEvents.WriteTelemetryActivityTransferEvent(string serverInstanceName, string navTenantId, string category, Guid sessionId, Guid oldActivityId, Guid newActivityId, DateTime eventTime, int processId, string tag, int threadId, string aadTenantId)
		{
			WriteTelemetryActivityTransferEvent(serverInstanceName, navTenantId, category, sessionId, oldActivityId, newActivityId, eventTime, processId, tag, threadId, aadTenantId);
		}

		[NonEvent]
		private unsafe void LargeEventHandler(int eventId, string tag, string[] messages, int originalMessageLength, EventData* pEventData, int eventDataSize)
		{
			for (int i = 1; i < messages.Length; i++)
			{
				fixed (char* ptr = messages[i])
				{
					((EventData)(pEventData + 8)).DataPointer = (IntPtr)ptr;
					((EventData)(pEventData + 8)).Size = (messages[i].Length + 1) * 2;
					RunWriteEventMethodWithExceptionHandling(() =>
					{
						((EventSource)eventSource).WriteEventCore(eventId, eventDataSize, pEventData);
					}, tag);
				}
			}
			string text = "The tag " + tag + " exceeded the ETW payload threshold. Length of Data: " + originalMessageLength + ", Payload threshold = " + 25000;
			fixed (char* ptr2 = text)
			{
				fixed (char* ptr3 = "00001WT")
				{
					((EventData)(pEventData + 8)).DataPointer = (IntPtr)ptr2;
					((EventData)(pEventData + 8)).Size = (text.Length + 1) * 2;
					((EventData)(pEventData + 10)).DataPointer = (IntPtr)ptr3;
					((EventData)(pEventData + 10)).Size = ("00001WT".Length + 1) * 2;
					RunWriteEventMethodWithExceptionHandling(() =>
					{
						((EventSource)eventSource).WriteEventCore(702, eventDataSize, pEventData);
					}, "00001WT");
				}
			}
		}

		[NonEvent]
		private unsafe void WriteEventFast(int eventId, string serverInstanceName, string navTenantId, string category, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, DateTime eventTime, string message, int processId, string tag, int threadId, string counterInformation, string alternateTenantIds, string platformVersion, int privacyClassification, Guid transientEventId, string aadTenantId, string aadUserId)
		{
			bool flag = aadUserId != null;
			int eventDataSize = 18 + (flag ? 1 : 0);
			string text = counterInformation ?? string.Empty;
			string text2 = serverInstanceName ?? string.Empty;
			string text3 = navTenantId ?? string.Empty;
			string text4 = alternateTenantIds ?? string.Empty;
			string text5 = platformVersion ?? string.Empty;
			string text6 = aadTenantId ?? string.Empty;
			string text7 = aadUserId ?? string.Empty;
			int length = message.Length;
			string[] array = null;
			if (EventPayloadHelper.IsLargeMessage(message))
			{
				array = EventPayloadHelper.SplitLargeMessage(message).ToArray();
				message = array[0];
			}
			fixed (char* ptr = text2)
			{
				fixed (char* ptr2 = text3)
				{
					fixed (char* ptr3 = category)
					{
						fixed (char* ptr4 = message)
						{
							fixed (char* ptr5 = tag)
							{
								fixed (char* ptr6 = text)
								{
									fixed (char* ptr7 = text4)
									{
										fixed (char* ptr8 = text6)
										{
											fixed (char* ptr9 = text7)
											{
												fixed (char* ptr10 = text5)
												{
													EventData* ptr11 = stackalloc EventData[eventDataSize];
													EventData* pEventData = ptr11;
													((EventData)pEventData).DataPointer = (IntPtr)ptr;
													((EventData)pEventData).Size = (text2.Length + 1) * 2;
													((EventData)((byte*)pEventData + System.Runtime.CompilerServices.Unsafe.SizeOf<EventData>())).DataPointer = (IntPtr)ptr2;
													((EventData)((byte*)pEventData + System.Runtime.CompilerServices.Unsafe.SizeOf<EventData>())).Size = (text3.Length + 1) * 2;
													((EventData)(pEventData + 2)).DataPointer = (IntPtr)ptr3;
													((EventData)(pEventData + 2)).Size = (category.Length + 1) * 2;
													((EventData)(pEventData + 3)).DataPointer = (IntPtr)(&clientSessionId);
													((EventData)(pEventData + 3)).Size = sizeof(Guid);
													((EventData)(pEventData + 4)).DataPointer = (IntPtr)(&clientActivityId);
													((EventData)(pEventData + 4)).Size = sizeof(Guid);
													((EventData)(pEventData + 5)).DataPointer = (IntPtr)(&serverSessionId);
													((EventData)(pEventData + 5)).Size = sizeof(Guid);
													((EventData)(pEventData + 6)).DataPointer = (IntPtr)(&serverActivityId);
													((EventData)(pEventData + 6)).Size = sizeof(Guid);
													long num = eventTime.ToFileTimeUtc();
													((EventData)(pEventData + 7)).DataPointer = (IntPtr)(&num);
													((EventData)(pEventData + 7)).Size = 8;
													((EventData)(pEventData + 8)).DataPointer = (IntPtr)ptr4;
													((EventData)(pEventData + 8)).Size = (message.Length + 1) * 2;
													((EventData)(pEventData + 9)).DataPointer = (IntPtr)(&processId);
													((EventData)(pEventData + 9)).Size = 4;
													((EventData)(pEventData + 10)).DataPointer = (IntPtr)ptr5;
													((EventData)(pEventData + 10)).Size = (tag.Length + 1) * 2;
													((EventData)(pEventData + 11)).DataPointer = (IntPtr)(&threadId);
													((EventData)(pEventData + 11)).Size = 4;
													((EventData)(pEventData + 12)).DataPointer = (IntPtr)ptr6;
													((EventData)(pEventData + 12)).Size = (text.Length + 1) * 2;
													((EventData)(pEventData + 13)).DataPointer = (IntPtr)ptr7;
													((EventData)(pEventData + 13)).Size = (text4.Length + 1) * 2;
													((EventData)(pEventData + 14)).DataPointer = (IntPtr)ptr10;
													((EventData)(pEventData + 14)).Size = (text5.Length + 1) * 2;
													((EventData)(pEventData + 15)).DataPointer = (IntPtr)(&privacyClassification);
													((EventData)(pEventData + 15)).Size = 4;
													((EventData)(pEventData + 16)).DataPointer = (IntPtr)(&transientEventId);
													((EventData)(pEventData + 16)).Size = sizeof(Guid);
													((EventData)(pEventData + 17)).DataPointer = (IntPtr)ptr8;
													((EventData)(pEventData + 17)).Size = (text6.Length + 1) * 2;
													if (flag)
													{
														((EventData)(pEventData + 18)).DataPointer = (IntPtr)ptr9;
														((EventData)(pEventData + 18)).Size = (aadUserId.Length + 1) * 2;
													}
													RunWriteEventMethodWithExceptionHandling(() =>
													{
														((EventSource)eventSource).WriteEventCore(eventId, eventDataSize, pEventData);
													}, tag);
													if (array != null)
													{
														LargeEventHandler(eventId, tag, array, length, pEventData, eventDataSize);
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[NonEvent]
		private unsafe void WriteTelemetryActivityTransferEvent(string serverInstanceName, string navTenantId, string category, Guid serverSessionId, Guid oldActivityId, Guid newActivityId, DateTime eventTime, int processId, string tag, int threadId, string aadTenantId)
		{
			string text = (string.IsNullOrEmpty(serverInstanceName) ? string.Empty : serverInstanceName);
			string text2 = navTenantId ?? string.Empty;
			string text3 = aadTenantId ?? string.Empty;
			int eventDataSize = 11;
			fixed (char* ptr = text)
			{
				fixed (char* ptr2 = text2)
				{
					fixed (char* ptr3 = category)
					{
						fixed (char* ptr4 = tag)
						{
							fixed (char* ptr5 = text3)
							{
								EventData* ptr6 = stackalloc EventData[eventDataSize];
								EventData* pEventData = ptr6;
								((EventData)pEventData).DataPointer = (IntPtr)ptr;
								((EventData)pEventData).Size = (text.Length + 1) * 2;
								((EventData)((byte*)pEventData + System.Runtime.CompilerServices.Unsafe.SizeOf<EventData>())).DataPointer = (IntPtr)ptr2;
								((EventData)((byte*)pEventData + System.Runtime.CompilerServices.Unsafe.SizeOf<EventData>())).Size = (text2.Length + 1) * 2;
								((EventData)(pEventData + 2)).DataPointer = (IntPtr)ptr3;
								((EventData)(pEventData + 2)).Size = (category.Length + 1) * 2;
								((EventData)(pEventData + 3)).DataPointer = (IntPtr)(&serverSessionId);
								((EventData)(pEventData + 3)).Size = sizeof(Guid);
								((EventData)(pEventData + 4)).DataPointer = (IntPtr)(&oldActivityId);
								((EventData)(pEventData + 4)).Size = sizeof(Guid);
								((EventData)(pEventData + 5)).DataPointer = (IntPtr)(&newActivityId);
								((EventData)(pEventData + 5)).Size = sizeof(Guid);
								long num = eventTime.ToFileTimeUtc();
								((EventData)(pEventData + 6)).DataPointer = (IntPtr)(&num);
								((EventData)(pEventData + 6)).Size = 8;
								((EventData)(pEventData + 7)).DataPointer = (IntPtr)(&processId);
								((EventData)(pEventData + 7)).DataPointer = (IntPtr)(&processId);
								((EventData)(pEventData + 7)).Size = 4;
								((EventData)(pEventData + 8)).DataPointer = (IntPtr)ptr4;
								((EventData)(pEventData + 8)).Size = (tag.Length + 1) * 2;
								((EventData)(pEventData + 9)).DataPointer = (IntPtr)(&threadId);
								((EventData)(pEventData + 9)).Size = 4;
								((EventData)(pEventData + 10)).DataPointer = (IntPtr)ptr5;
								((EventData)(pEventData + 10)).Size = (text3.Length + 1) * 2;
								RunWriteEventMethodWithExceptionHandling(() =>
								{
									((EventSource)eventSource).WriteEventCore(706, eventDataSize, pEventData);
								}, tag);
							}
						}
					}
				}
			}
		}
	}

	public const string TelemetryTrackingStatementChannelMessage = "Server instance: {0}\nCategory: {2}\nClientSessionId: {3}\nClientActivityId: {4}\nServerSessionUniqueId: {5}\nServerActivityId: {6}\nEventTime: {7}\nMessage {8}\nProcessId: {9}\nTag: {10}\nThreadId: {11}\nCounterInformation: {12}";

	public const string TelemetryActivityTransferChannelMessage = "Server instance: {0}\nCategory: {2}\nSessionId: {3}\nOld activity: {4}\nNew activity: {5}\nEventTime: {6}\nProcessId: {7}\nTag: {8}\nThreadId: {9}\nTransferring activity from {4} to {5}";

	private static readonly Lazy<NavDiagnosticsEventSource> log = new Lazy<NavDiagnosticsEventSource>(() => new NavDiagnosticsEventSource());

	[ThreadStatic]
	private static bool writeEventWithExceptionHandlingAlreadyCalledOnThisThead;

	private readonly object mutex = new object();

	private volatile INavDiagnosticsEventViewerEvents navDiagnosticsEventViewerEvents;

	public static NavDiagnosticsEventSource Log
	{
		[NonEvent]
		get
		{
			return log.Value;
		}
	}

	public INavDiagnosticsEventViewerEvents NavDiagnosticsEventViewerEvents
	{
		[NonEvent]
		get
		{
			if (navDiagnosticsEventViewerEvents == null)
			{
				lock (mutex)
				{
					if (navDiagnosticsEventViewerEvents == null)
					{
						return navDiagnosticsEventViewerEvents = new NavDiagnosticsEventViewerEventsImplementation(this);
					}
				}
			}
			return navDiagnosticsEventViewerEvents;
		}
		[NonEvent]
		set
		{
			lock (mutex)
			{
				navDiagnosticsEventViewerEvents = value;
			}
		}
	}

	private NavDiagnosticsEventSource()
		: base(true)
	{
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTelemetryCriticalEvent(string instanceName, string navTenantId, string category, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, DateTime eventTime, string message, int processId, string tag, int threadId, string counterInformation, string alternateTenantIds, string platformVersion, int privacyClassification, Guid transientEventId, string aadTenantId)
	{
		NavDiagnosticsEventViewerEvents.WriteTelemetryEvent(700, instanceName, navTenantId, category, clientSessionId, clientActivityId, serverSessionId, serverActivityId, eventTime, message, processId, tag, threadId, counterInformation, alternateTenantIds, platformVersion, privacyClassification, transientEventId, aadTenantId);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTelemetryErrorEvent(string instanceName, string navTenantId, string category, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, DateTime eventTime, string message, int processId, string tag, int threadId, string counterInformation, string alternateTenantIds, string platformVersion, int privacyClassification, Guid transientEventId, string aadTenantId)
	{
		NavDiagnosticsEventViewerEvents.WriteTelemetryEvent(701, instanceName, navTenantId, category, clientSessionId, clientActivityId, serverSessionId, serverActivityId, eventTime, message, processId, tag, threadId, counterInformation, alternateTenantIds, platformVersion, privacyClassification, transientEventId, aadTenantId);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTelemetryWarningEvent(string instanceName, string navTenantId, string category, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, DateTime eventTime, string message, int processId, string tag, int threadId, string counterInformation, string alternateTenantIds, string platformVersion, int privacyClassification, Guid transientEventId, string aadTenantId)
	{
		NavDiagnosticsEventViewerEvents.WriteTelemetryEvent(705, instanceName, navTenantId, category, clientSessionId, clientActivityId, serverSessionId, serverActivityId, eventTime, message, processId, tag, threadId, counterInformation, alternateTenantIds, platformVersion, privacyClassification, transientEventId, aadTenantId);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTelemetryInformationalEvent(string instanceName, string navTenantId, string category, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, DateTime eventTime, string message, int processId, string tag, int threadId, string counterInformation, string alternateTenantIds, string platformVersion, int privacyClassification, Guid transientEventId, string aadTenantId)
	{
		NavDiagnosticsEventViewerEvents.WriteTelemetryEvent(702, instanceName, navTenantId, category, clientSessionId, clientActivityId, serverSessionId, serverActivityId, eventTime, message, processId, tag, threadId, counterInformation, alternateTenantIds, platformVersion, privacyClassification, transientEventId, aadTenantId);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTelemetryVerboseEvent(string instanceName, string navTenantId, string category, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, DateTime eventTime, string message, int processId, string tag, int threadId, string counterInformation, string alternateTenantIds, string platformVersion, int privacyClassification, Guid transientEventId, string aadTenantId)
	{
		NavDiagnosticsEventViewerEvents.WriteTelemetryEvent(704, instanceName, navTenantId, category, clientSessionId, clientActivityId, serverSessionId, serverActivityId, eventTime, message, processId, tag, threadId, counterInformation, alternateTenantIds, platformVersion, privacyClassification, transientEventId, aadTenantId);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTelemetryActivityTransferEvent(string instanceName, string navTenantId, string category, Guid sessionId, Guid oldActivityId, Guid newActivityId, DateTime eventTime, int processId, string tag, int threadId, string aadTenantId)
	{
		NavDiagnosticsEventViewerEvents.WriteTelemetryActivityTransferEvent(instanceName, navTenantId, category, sessionId, oldActivityId, newActivityId, eventTime, processId, tag, threadId, aadTenantId);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTransientTelemetryCriticalEvent(string instanceName, string navTenantId, string category, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, DateTime eventTime, string message, int processId, string tag, int threadId, string counterInformation, string alternateTenantIds, string platformVersion, int privacyClassification, Guid transientEventId, string aadTenantId, string aadUserId)
	{
		NavDiagnosticsEventViewerEvents.WriteTelemetryEvent(707, instanceName, navTenantId, category, clientSessionId, clientActivityId, serverSessionId, serverActivityId, eventTime, message, processId, tag, threadId, counterInformation, alternateTenantIds, platformVersion, privacyClassification, transientEventId, aadTenantId, aadUserId ?? "");
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTransientTelemetryErrorEvent(string instanceName, string navTenantId, string category, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, DateTime eventTime, string message, int processId, string tag, int threadId, string counterInformation, string alternateTenantIds, string platformVersion, int privacyClassification, Guid transientEventId, string aadTenantId, string aadUserId)
	{
		NavDiagnosticsEventViewerEvents.WriteTelemetryEvent(708, instanceName, navTenantId, category, clientSessionId, clientActivityId, serverSessionId, serverActivityId, eventTime, message, processId, tag, threadId, counterInformation, alternateTenantIds, platformVersion, privacyClassification, transientEventId, aadTenantId, aadUserId ?? "");
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTransientTelemetryWarningEvent(string instanceName, string navTenantId, string category, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, DateTime eventTime, string message, int processId, string tag, int threadId, string counterInformation, string alternateTenantIds, string platformVersion, int privacyClassification, Guid transientEventId, string aadTenantId, string aadUserId)
	{
		NavDiagnosticsEventViewerEvents.WriteTelemetryEvent(712, instanceName, navTenantId, category, clientSessionId, clientActivityId, serverSessionId, serverActivityId, eventTime, message, processId, tag, threadId, counterInformation, alternateTenantIds, platformVersion, privacyClassification, transientEventId, aadTenantId, aadUserId ?? "");
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTransientTelemetryInformationalEvent(string instanceName, string navTenantId, string category, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, DateTime eventTime, string message, int processId, string tag, int threadId, string counterInformation, string alternateTenantIds, string platformVersion, int privacyClassification, Guid transientEventId, string aadTenantId, string aadUserId)
	{
		NavDiagnosticsEventViewerEvents.WriteTelemetryEvent(709, instanceName, navTenantId, category, clientSessionId, clientActivityId, serverSessionId, serverActivityId, eventTime, message, processId, tag, threadId, counterInformation, alternateTenantIds, platformVersion, privacyClassification, transientEventId, aadTenantId, aadUserId ?? "");
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTransientTelemetryVerboseEvent(string instanceName, string navTenantId, string category, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, DateTime eventTime, string message, int processId, string tag, int threadId, string counterInformation, string alternateTenantIds, string platformVersion, int privacyClassification, Guid transientEventId, string aadTenantId, string aadUserId)
	{
		NavDiagnosticsEventViewerEvents.WriteTelemetryEvent(711, instanceName, navTenantId, category, clientSessionId, clientActivityId, serverSessionId, serverActivityId, eventTime, message, processId, tag, threadId, counterInformation, alternateTenantIds, platformVersion, privacyClassification, transientEventId, aadTenantId, aadUserId ?? "");
	}

	private static void RunWriteEventMethodWithExceptionHandling(Action action, string tag)
	{
		//IL_0009: Expected Obj, but got Unknown
		try
		{
			action();
		}
		catch (EventSourceException ex)
		{
			EventSourceException exception = ex;
			if (!writeEventWithExceptionHandlingAlreadyCalledOnThisThead)
			{
				writeEventWithExceptionHandlingAlreadyCalledOnThisThead = true;
				try
				{
					NavDiagnostics.GlobalInstance?.SendExceptionTag("00001JJ", Category.Operations, (Exception)(object)exception, Verbosity.Normal, DiagnosticParameter.SystemMetadata("Failed to emit tag " + tag + "."));
					return;
				}
				finally
				{
					writeEventWithExceptionHandlingAlreadyCalledOnThisThead = false;
				}
			}
		}
	}
}
