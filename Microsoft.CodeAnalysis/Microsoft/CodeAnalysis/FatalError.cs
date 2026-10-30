using System;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis;

internal static class FatalError
{
	private static Action<Exception> s_fatalHandler;

	private static Action<Exception> s_nonFatalHandler;

	private static Exception s_reportedException;

	private static string s_reportedExceptionMessage;

	private static object s_reportedMarker = new object();

	public static Action<Exception> Handler
	{
		get
		{
			return s_fatalHandler;
		}
		set
		{
			if (s_fatalHandler != value)
			{
				s_fatalHandler = value;
			}
		}
	}

	public static Action<Exception> NonFatalHandler
	{
		get
		{
			return s_nonFatalHandler;
		}
		set
		{
			if (s_nonFatalHandler != value)
			{
				s_nonFatalHandler = value;
			}
		}
	}

	public static void OverwriteHandler(Action<Exception> value)
	{
		s_fatalHandler = value;
	}

	[DebuggerHidden]
	public static bool ReportUnlessCanceled(Exception exception)
	{
		if (exception is OperationCanceledException)
		{
			return false;
		}
		return Report(exception);
	}

	[DebuggerHidden]
	public static bool ReportWithoutCrashUnlessCanceled(Exception exception)
	{
		if (exception is OperationCanceledException)
		{
			return false;
		}
		return ReportWithoutCrash(exception);
	}

	[DebuggerHidden]
	public static bool ReportUnlessNotImplemented(Exception exception)
	{
		if (exception is NotImplementedException)
		{
			return false;
		}
		return Report(exception);
	}

	[DebuggerHidden]
	public static bool Report(Exception exception)
	{
		Report(exception, s_fatalHandler);
		return false;
	}

	[DebuggerHidden]
	public static bool ReportWithoutCrash(Exception exception)
	{
		Report(exception, s_nonFatalHandler);
		return true;
	}

	[DebuggerHidden]
	public static bool ReportWithoutCrashAndPropagate(Exception exception)
	{
		Report(exception, s_nonFatalHandler);
		return false;
	}

	[DebuggerHidden]
	public static bool ReportWithoutCrashUnlessCanceledAndPropagate(Exception exception)
	{
		if (!(exception is OperationCanceledException))
		{
			Report(exception, s_nonFatalHandler);
		}
		return false;
	}

	private static void Report(Exception exception, Action<Exception> handler)
	{
		s_reportedException = exception;
		s_reportedExceptionMessage = exception.ToString();
		if (handler != null && exception.Data[s_reportedMarker] == null && (!(exception is AggregateException ex) || ex.InnerExceptions.Count != 1 || ex.InnerExceptions[0].Data[s_reportedMarker] == null))
		{
			if (!exception.Data.IsReadOnly)
			{
				exception.Data[s_reportedMarker] = s_reportedMarker;
			}
			handler(exception);
		}
	}
}
