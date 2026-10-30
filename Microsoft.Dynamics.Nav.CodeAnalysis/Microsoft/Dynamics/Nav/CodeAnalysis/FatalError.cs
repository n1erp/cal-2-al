using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class FatalError
{
	private static Action<Exception> fatalHandler;

	private static Action<Exception> nonFatalHandler;

	public static Action<Exception> Handler
	{
		get
		{
			return fatalHandler;
		}
		set
		{
			if (fatalHandler != value)
			{
				DebugAssertHelper.Assert(fatalHandler == null, "Handler already set");
				fatalHandler = value;
			}
		}
	}

	public static Action<Exception> NonFatalHandler
	{
		get
		{
			return nonFatalHandler;
		}
		set
		{
			if (nonFatalHandler != value)
			{
				DebugAssertHelper.Assert(nonFatalHandler == null, "Handler already set");
				nonFatalHandler = value;
			}
		}
	}

	public static void OverwriteHandler(Action<Exception> value)
	{
		fatalHandler = value;
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
		Report(exception, fatalHandler);
		return false;
	}

	[DebuggerHidden]
	public static bool ReportWithoutCrash(Exception exception)
	{
		Report(exception, nonFatalHandler);
		return true;
	}

	private static void Report(Exception exception, Action<Exception> handler)
	{
		handler?.Invoke(exception);
	}
}
