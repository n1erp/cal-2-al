using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class LogWriterHelper
{
	public static void LogExceptionEvent(Exception exception, bool ignoreNavBaseExceptionUnlessAttributed = false)
	{
		if (EventLogWriter.GetEventLogEntryTypeForException(exception, ignoreNavBaseExceptionUnlessAttributed, out var eventLogEntryType))
		{
			LogExceptionEvent(exception, eventLogEntryType);
		}
	}

	public static void LogExceptionEvent(Exception exception, EventLogEntryType eventLogEntryType)
	{
		if (exception != null)
		{
			string message = string.Format(CultureInfo.InvariantCulture, "User: {0}", Thread.CurrentPrincipal.Identity.Name) + Environment.NewLine + ExceptionHandler.FormatException(exception);
			LogEvent(eventLogEntryType, message, isExceptionEvent: true);
		}
	}

	public static void LogEvent(EventLogEntryType entryType, string message, bool isExceptionEvent = false)
	{
		string currentTenantId = NavEnvironment.CurrentTenantId;
		string message2 = EventPayloadHelper.TruncateIfLarge(message);
		switch (entryType)
		{
		default:
			throw new InvalidOperationException("No event exists for " + entryType);
		case EventLogEntryType.Error:
			if (isExceptionEvent)
			{
				NavServerEventSource.Log.WriteExceptionErrorMessageEvent(NavEnvironment.ServiceInstanceName, currentTenantId, message2);
			}
			else
			{
				NavServerEventSource.Log.WriteSubsystemErrorMessageEvent(NavEnvironment.ServiceInstanceName, currentTenantId, message2);
			}
			break;
		case EventLogEntryType.Warning:
			if (isExceptionEvent)
			{
				NavServerEventSource.Log.WriteExceptionWarningMessageEvent(NavEnvironment.ServiceInstanceName, currentTenantId, message2);
			}
			else
			{
				NavServerEventSource.Log.WriteSubsystemWarningMessageEvent(NavEnvironment.ServiceInstanceName, currentTenantId, message2);
			}
			break;
		case EventLogEntryType.Information:
			if (isExceptionEvent)
			{
				NavServerEventSource.Log.WriteExceptionInfoMessageEvent(NavEnvironment.ServiceInstanceName, currentTenantId, message2);
			}
			else
			{
				NavServerEventSource.Log.WriteSubsystemInfoMessageEvent(NavEnvironment.ServiceInstanceName, currentTenantId, message2);
			}
			break;
		}
	}

	public static void LogWith(Action<string, string, string> logger, string message, string tenantIdParameter = null)
	{
		string arg = tenantIdParameter ?? NavEnvironment.CurrentTenantId;
		logger(NavEnvironment.ServiceInstanceName, arg, message);
	}

	public static void LogWith(Action<string, string> logger, string message)
	{
		logger(NavEnvironment.ServiceInstanceName, message);
	}

	public static void LogExceptionEventWithTelemetry(string tag, Category category, Exception exception, Verbosity verbosity, EventLogEntryType eventLogEntryType)
	{
		LogExceptionEvent(exception, eventLogEntryType);
		DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag(tag, category, exception, verbosity);
	}

	public static void LogExceptionEventWithTelemetry(string tag, Category category, Exception exception, Verbosity verbosity)
	{
		EventLogWriter.GetEventLogEntryTypeForException(exception, ignoreNavBaseExceptionsUnlessAttributed: false, out var eventLogEntryType);
		LogExceptionEventWithTelemetry(tag, category, exception, verbosity, eventLogEntryType);
	}
}
