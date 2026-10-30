using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public static class GlobalLogger
{
	private static readonly List<ILogger> Loggers = new List<ILogger>();

	public static bool Enabled { get; private set; }

	public static LogLevel LogLevel { get; private set; }

	public static void SetLogLevel(LogLevel level)
	{
		LogLevel = level;
		Enabled = true;
	}

	public static void AddLogger(ILogger logger)
	{
		Loggers.Add(logger);
	}

	[Conditional("DEBUG")]
	public static void LogDebug(string message)
	{
		LogMessage(LogLevel.Debug, message);
	}

	[Conditional("DEBUG")]
	public static void LogDebug(string message, object param1)
	{
		LogMessage(LogLevel.Debug, message, param1);
	}

	public static void LogVerbose(string message)
	{
		LogMessage(LogLevel.Verbose, message);
	}

	public static void LogVerbose(string message, object param1)
	{
		LogMessage(LogLevel.Verbose, message, param1);
	}

	public static void LogVerbose(string message, object param1, object param2)
	{
		LogMessage(LogLevel.Verbose, message, param1, param2);
	}

	public static void LogNormal(string message)
	{
		LogMessage(LogLevel.Normal, message);
	}

	public static void LogWarning(string message)
	{
		LogMessage(LogLevel.Warning, message);
	}

	public static void LogError(string message)
	{
		LogMessage(LogLevel.Error, message);
	}

	public static void LogError(string message, object param1, object param2)
	{
		LogMessage(LogLevel.Error, message, param1, param2);
	}

	private static void LogMessage(LogLevel level, string message)
	{
		if (!Enabled || level < LogLevel)
		{
			return;
		}
		foreach (ILogger logger in Loggers)
		{
			try
			{
				logger.LogMessage(level, message);
			}
			catch
			{
			}
		}
	}

	private static void LogMessage(LogLevel level, string message, object param1)
	{
		if (Enabled && level >= LogLevel)
		{
			string message2 = string.Format(CultureInfo.InvariantCulture, message, param1);
			LogMessage(level, message2);
		}
	}

	private static void LogMessage(LogLevel level, string message, object param1, object param2)
	{
		if (Enabled && level >= LogLevel)
		{
			string message2 = string.Format(CultureInfo.InvariantCulture, message, param1, param2);
			LogMessage(level, message2);
		}
	}

	public static void LogException(Exception ex)
	{
		if (!Enabled)
		{
			return;
		}
		foreach (ILogger logger in Loggers)
		{
			try
			{
				logger.LogException(ex);
			}
			catch
			{
			}
		}
	}
}
