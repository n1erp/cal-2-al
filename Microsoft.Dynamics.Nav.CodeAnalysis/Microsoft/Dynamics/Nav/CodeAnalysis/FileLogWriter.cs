using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public class FileLogWriter : IDisposable, ILogger
{
	private Lazy<TextWriter> lazyTextWriter;

	public FileLogWriter(string logFilePath, bool deleteExisting)
	{
		FileLogWriter fileLogWriter = this;
		if (!Path.IsPathRooted(logFilePath))
		{
			logFilePath = Path.Combine(AppContextUtility.GetBaseDirectory(), logFilePath);
		}
		lazyTextWriter = new Lazy<TextWriter>(() => fileLogWriter.LazyGetTextWriter(logFilePath, deleteExisting));
	}

	private TextWriter LazyGetTextWriter(string filepath, bool deleteExisting)
	{
		if (TryOpenLogFile(filepath, deleteExisting, out var textWriter))
		{
			return textWriter;
		}
		TryOpenLogFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Path.GetFileName(filepath)), deleteExisting, out textWriter);
		return textWriter;
	}

	public void Dispose()
	{
		if (lazyTextWriter.IsValueCreated && lazyTextWriter.Value != null)
		{
			lazyTextWriter.Value.Flush();
			lazyTextWriter.Value.Dispose();
			lazyTextWriter = null;
		}
	}

	private bool TryOpenLogFile(string logFilePath, bool deleteExisting, out TextWriter textWriter)
	{
		textWriter = null;
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
			textWriter = TextWriter.Synchronized(new StreamWriter(new FileStream(logFilePath, deleteExisting ? FileMode.Create : FileMode.Append), Encoding.UTF8));
			return true;
		}
		catch (Exception ex)
		{
			if (ex is UnauthorizedAccessException || ex is IOException)
			{
				return false;
			}
			throw;
		}
	}

	public void LogMessage(LogLevel level, string message)
	{
		string value = FormattableString.Invariant($"{DateTime.Now} [{Task.CurrentId}/{Thread.CurrentThread.ManagedThreadId}] {message}");
		lazyTextWriter.Value.WriteLine(value);
		lazyTextWriter.Value.Flush();
	}

	public void LogException(Exception e)
	{
		LogMessage(LogLevel.Error, e.ToString());
	}
}
