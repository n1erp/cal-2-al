using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Model.Tools.Language;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools.Cmdlets;

public abstract class NavCmdlet : PSCmdlet, ILogger
{
	internal class Message
	{
		public MessageType Type { get; set; }

		public string Value { get; set; }
	}

	internal class ProgressLogger : IProgressLogger, IDisposable
	{
		private int progress;

		private readonly AutoResetEvent waitEvent = new AutoResetEvent(initialState: false);

		private readonly AutoResetEvent confirmationRequestsHandled = new AutoResetEvent(initialState: false);

		private readonly ConcurrentDictionary<string, bool?> shouldProcessRequests = new ConcurrentDictionary<string, bool?>();

		private readonly ConcurrentDictionary<string, bool?> shouldContinueRequests = new ConcurrentDictionary<string, bool?>();

		private bool yesToAll;

		private bool noToAll;

		public int Progress => progress;

		public bool IsCompleted { get; private set; }

		public void Increment()
		{
			Interlocked.Increment(ref progress);
			waitEvent.Set();
		}

		public void SetProgress(int newProgress)
		{
			Interlocked.Exchange(ref progress, newProgress);
			waitEvent.Set();
		}

		public void Completed()
		{
			IsCompleted = true;
		}

		public void Wait()
		{
			waitEvent.WaitOne(1);
		}

		public void Dispose()
		{
			confirmationRequestsHandled.Dispose();
			waitEvent.Dispose();
		}

		public void HandleConfirmationRequests(Cmdlet cmdlet)
		{
			foreach (KeyValuePair<string, bool?> shouldProcessRequest in shouldProcessRequests)
			{
				if (!shouldProcessRequest.Value.HasValue)
				{
					shouldProcessRequests[shouldProcessRequest.Key] = cmdlet.ShouldProcess(shouldProcessRequest.Key);
				}
			}
			foreach (KeyValuePair<string, bool?> shouldContinueRequest in shouldContinueRequests)
			{
				if (!shouldContinueRequest.Value.HasValue)
				{
					shouldContinueRequests[shouldContinueRequest.Key] = cmdlet.ShouldContinue(shouldContinueRequest.Key, NonLocalizable.OverwriteFile, ref yesToAll, ref noToAll);
				}
			}
			confirmationRequestsHandled.Set();
		}

		public bool ShouldProcess(string file)
		{
			shouldProcessRequests[file] = null;
			while (!shouldProcessRequests[file].HasValue)
			{
				confirmationRequestsHandled.WaitOne(1);
			}
			shouldProcessRequests.TryRemove(file, out var value);
			return value.Value;
		}

		public bool ShouldContinue(string file)
		{
			shouldContinueRequests[file] = null;
			while (!shouldContinueRequests[file].HasValue)
			{
				confirmationRequestsHandled.WaitOne(1);
			}
			shouldContinueRequests.TryRemove(file, out var value);
			return value.Value;
		}
	}

	private ProgressRecord progressRecord;

	private static int activityId;

	private readonly Queue<Message> messageQueue = new Queue<Message>();

	private bool isMultiThreaded;

	public void WriteLine(string message, MessageType type = MessageType.Normal)
	{
		if (isMultiThreaded)
		{
			lock (messageQueue)
			{
				messageQueue.Enqueue(new Message
				{
					Type = type,
					Value = message
				});
				return;
			}
		}
		InternalWriteLine(message, type);
	}

	private void WriteQueue()
	{
		if (messageQueue.Count <= 0)
		{
			return;
		}
		lock (messageQueue)
		{
			while (messageQueue.Count > 0)
			{
				Message message = messageQueue.Dequeue();
				InternalWriteLine(message.Value, message.Type);
			}
		}
	}

	private void InternalWriteLine(string message, MessageType type)
	{
		switch (type)
		{
		case MessageType.Normal:
			WriteVerbose(message);
			break;
		case MessageType.Error:
			WriteWarning(message);
			break;
		case MessageType.Warning:
			WriteWarning(message);
			break;
		case MessageType.Diagnostic:
			WriteDebug(message);
			break;
		}
	}

	protected virtual void InternalProcessRecord()
	{
	}

	protected virtual void InternalEndProcessing()
	{
	}

	protected sealed override void ProcessRecord()
	{
		try
		{
			InternalProcessRecord();
		}
		catch (Exception e)
		{
			if (!TryToHandleException(e))
			{
				throw;
			}
		}
	}

	protected sealed override void EndProcessing()
	{
		try
		{
			InternalEndProcessing();
		}
		catch (Exception e)
		{
			if (!TryToHandleException(e))
			{
				throw;
			}
		}
	}

	private bool TryToHandleException(Exception e)
	{
		if (e is NavCmdletException)
		{
			return false;
		}
		if (e is TypeSystemException)
		{
			WriteError(e, ErrorCategory.NotSpecified, string.Empty);
			return true;
		}
		if (e is PipelineStoppedException)
		{
			return false;
		}
		if (e is HostException)
		{
			return false;
		}
		WriteWarning("UnhandledErrorMessage");
		return false;
	}

	protected internal void WriteError(Exception exception, ErrorCategory category, object target)
	{
		WriteError(new ErrorRecord(exception, string.Empty, category, target));
	}

	public void InvokeParallelProcessing(string activity, int noOfItems, Action<IProgressLogger> action)
	{
		progressRecord = new ProgressRecord(++activityId, activity, activity);
		WriteProgress(progressRecord);
		ProgressLogger progressLogger = new ProgressLogger();
		isMultiThreaded = true;
		try
		{
			Task task = Task.Factory.StartNew(() =>
			{
				try
				{
					action(progressLogger);
				}
				finally
				{
					progressLogger.Completed();
				}
			});
			int num = -1;
			while (!progressLogger.IsCompleted)
			{
				if (progressLogger.Progress != num)
				{
					num = progressLogger.Progress;
					if (noOfItems > 0)
					{
						progressRecord.PercentComplete = Math.Min(num * 100 / noOfItems, 100);
						progressRecord.StatusDescription = string.Format(CultureInfo.InvariantCulture, "{0} of {1} objects", num, noOfItems);
					}
					else
					{
						progressRecord.PercentComplete = num;
						progressRecord.StatusDescription = string.Format(CultureInfo.InvariantCulture, "{0}% done", num);
					}
					WriteProgress(progressRecord);
					WriteQueue();
				}
				progressLogger.HandleConfirmationRequests(this);
				progressLogger.Wait();
			}
			if (task.Exception != null)
			{
				AggregateException exception = task.Exception;
				if (exception != null)
				{
					throw exception.Flatten().InnerExceptions.First();
				}
				throw task.Exception;
			}
		}
		finally
		{
			isMultiThreaded = false;
			WriteQueue();
		}
		progressRecord.RecordType = ProgressRecordType.Completed;
		WriteProgress(progressRecord);
	}

	protected string[] ResolveToFileList(string fileSpec)
	{
		return ResolveToFileList(fileSpec, allDirectories: false);
	}

	protected string[] ResolveToFileList(string fileSpec, bool allDirectories)
	{
		if (fileSpec == null)
		{
			throw new ArgumentNullException("fileSpec");
		}
		fileSpec = fileSpec.TrimEnd(Path.DirectorySeparatorChar);
		string[] array = GetResolvedProviderPathFromPSPath(fileSpec, out var _).ToArray();
		if (array.Length == 1 && Directory.Exists(array[0]))
		{
			return Directory.GetFiles(array[0], "*", allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		}
		return array;
	}

	protected string[] ResolveToFileList(string[] fileSpec)
	{
		return fileSpec.Where((string f) => !string.IsNullOrEmpty(f)).SelectMany(ResolveToFileList).ToArray();
	}

	protected string ResolveToFileOrDirectory(string fileSpec)
	{
		if (fileSpec == null)
		{
			throw new ArgumentNullException("fileSpec");
		}
		fileSpec = fileSpec.TrimEnd(Path.DirectorySeparatorChar);
		ProviderInfo provider;
		return Path.Combine(GetResolvedProviderPathFromPSPath(Path.GetDirectoryName(fileSpec), out provider)[0], Path.GetFileName(fileSpec));
	}

	internal static string GetPropertyName<T>(Expression<Func<T>> expression)
	{
		return ((MemberExpression)expression.Body).Member.Name;
	}

	public static string ConvertToUnsecureString(SecureString securePassword)
	{
		if (securePassword == null)
		{
			throw new ArgumentNullException("securePassword");
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
			return Marshal.PtrToStringUni(intPtr);
		}
		finally
		{
			Marshal.ZeroFreeGlobalAllocUnicode(intPtr);
		}
	}
}
