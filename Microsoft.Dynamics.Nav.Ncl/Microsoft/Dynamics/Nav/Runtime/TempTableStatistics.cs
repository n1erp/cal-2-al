using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TempTableStatistics
{
	private class ConcurrentQueueWithOneWorker
	{
		private const int Frequency = 20000;

		private readonly ConcurrentQueue<QueuedTaskInfo> queuedTasks = new ConcurrentQueue<QueuedTaskInfo>();

		private readonly ManualResetEventSlim taskEnqueued = new ManualResetEventSlim(initialState: false);

		private readonly object syncObject = new object();

		private readonly Action<QueuedTaskInfo> calculateSizeAction;

		private RegisteredWaitHandle processQueueCallbackHandle;

		public ConcurrentQueueWithOneWorker(Action<QueuedTaskInfo> calculateSizeActionParameter)
		{
			calculateSizeAction = calculateSizeActionParameter;
			RegisterQueueCallback();
		}

		public void Enqueue(QueuedTaskInfo item)
		{
			queuedTasks.Enqueue(item);
			taskEnqueued.Set();
		}

		private void RegisterQueueCallback()
		{
			lock (syncObject)
			{
				if (processQueueCallbackHandle != null)
				{
					processQueueCallbackHandle.Unregister(null);
				}
				processQueueCallbackHandle = ThreadPool.RegisterWaitForSingleObject(taskEnqueued.WaitHandle, ProcessQueue, null, -1, executeOnlyOnce: true);
			}
		}

		private void ProcessQueue(object state, bool timedOut)
		{
			while (!queuedTasks.IsEmpty)
			{
				try
				{
					if (queuedTasks.TryDequeue(out var result))
					{
						calculateSizeAction(result);
					}
				}
				catch (Exception exception)
				{
					LogWriterHelper.LogExceptionEvent(exception);
				}
				lock (syncObject)
				{
					taskEnqueued.Reset();
				}
			}
			RegisterQueueCallback();
		}
	}

	private class QueuedTaskInfo
	{
		public TempTableSizeInfo SizeInfo { get; private set; }

		public long LastKnownTotalRowSize { get; private set; }

		public long PrivateBytes { get; private set; }

		public QueuedTaskInfo(TempTableSizeInfo sizeInfo, long lastKnownTotalRowSize, long privateBytes)
		{
			SizeInfo = sizeInfo;
			LastKnownTotalRowSize = lastKnownTotalRowSize;
			PrivateBytes = privateBytes;
		}

		public override bool Equals(object x)
		{
			if (x == null)
			{
				return false;
			}
			if (x == this)
			{
				return true;
			}
			if (GetType() != x.GetType())
			{
				return false;
			}
			QueuedTaskInfo queuedTaskInfo = (QueuedTaskInfo)x;
			if (LastKnownTotalRowSize == queuedTaskInfo.LastKnownTotalRowSize && PrivateBytes == queuedTaskInfo.PrivateBytes)
			{
				return SizeInfo.Equals(queuedTaskInfo.SizeInfo);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCodeHelper.CombineHashCodes(PrivateBytes.GetHashCode(), LastKnownTotalRowSize.GetHashCode(), SizeInfo.GetHashCode());
		}
	}

	private class TempTableSizeInfo : IComparable<TempTableSizeInfo>
	{
		public const int InvalidTableNumber = -1;

		public static readonly TempTableSizeInfo DefaultSizeInfo = new TempTableSizeInfo(-1, 0, 1);

		private long count;

		public int TableId { get; private set; }

		public int Increment { get; private set; }

		public int ApproximateRowCountByteSize { get; private set; }

		public long Count
		{
			get
			{
				return count;
			}
			set
			{
				count = ((value < 0) ? 0 : value);
			}
		}

		public long Size => count * ApproximateRowCountByteSize;

		public TempTableSizeInfo(int tableId, int approximateRowCountByteSize, int increment)
		{
			TableId = tableId;
			ApproximateRowCountByteSize = approximateRowCountByteSize;
			Increment = increment;
			Count = increment;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} -> {1}", TableId, Size);
		}

		public override bool Equals(object x)
		{
			if (x == null)
			{
				return false;
			}
			if (x == this)
			{
				return true;
			}
			if (GetType() != x.GetType())
			{
				return false;
			}
			TempTableSizeInfo tempTableSizeInfo = (TempTableSizeInfo)x;
			if (TableId == tempTableSizeInfo.TableId)
			{
				return Count == tempTableSizeInfo.Count;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCodeHelper.CombineHashCodes(TableId.GetHashCode(), Count.GetHashCode());
		}

		public int CompareTo(TempTableSizeInfo other)
		{
			if (other == this)
			{
				return 0;
			}
			if (other == null)
			{
				return 1;
			}
			long num = Size - other.Size;
			if (num != 0L)
			{
				if (num >= 0)
				{
					return 1;
				}
				return -1;
			}
			return 0;
		}
	}

	public const int MinimimumPrivateBytesNeededForReporting = 209715200;

	private static volatile TempTableStatistics instance = new TempTableStatistics();

	private static readonly Process currentProcess = Process.GetCurrentProcess();

	private const int TopNumberOfTempTables = 10;

	private readonly int reportingMargin = 10485760;

	private readonly TempTableSizeInfo[] topTempTables = new TempTableSizeInfo[10];

	private readonly object mutex = new object();

	private readonly ConcurrentQueueWithOneWorker concurrentQueue;

	private Action<EventLogEntryType, string, bool> logger = LogWriterHelper.LogEvent;

	private long totalNumberOfRows;

	private bool reachedFifty;

	private bool reachedSeventyFive;

	private int currentMaxTableRowSize;

	private int currentMinTableRowSize = int.MaxValue;

	private long lastApproximateSize;

	internal static TempTableStatistics Instance => instance;

	internal Action<EventLogEntryType, string, bool> Logger
	{
		get
		{
			return logger;
		}
		set
		{
			logger = value;
		}
	}

	private long LowestSize
	{
		get
		{
			if (topTempTables[0].TableId == -1)
			{
				return 0L;
			}
			return topTempTables[0].Size;
		}
	}

	private int ApproximateTableRowByteSize => (currentMinTableRowSize + currentMaxTableRowSize) / 2;

	private TempTableStatistics()
	{
		lock (mutex)
		{
			for (int i = 0; i < 10; i++)
			{
				topTempTables[i] = TempTableSizeInfo.DefaultSizeInfo;
			}
			if (ServerUserSettings.Instance.EnableTempTableSizeReporting.Value)
			{
				concurrentQueue = new ConcurrentQueueWithOneWorker((QueuedTaskInfo queuedTaskInfo) =>
				{
					ReportIncrementChange(queuedTaskInfo);
				});
			}
		}
	}

	internal void ReportIncrementChange(int tableId, int tableRowByteSize, int increment)
	{
		if (increment == 0)
		{
			return;
		}
		long tempTotalNumberOfRows = Interlocked.Add(ref totalNumberOfRows, increment);
		if (tempTotalNumberOfRows >= 0)
		{
			NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithTotalNumberOfRowsInTempTables(tempTotalNumberOfRows);
		}
		else
		{
			Interlocked.Exchange(ref totalNumberOfRows, 0L);
		}
		if (concurrentQueue != null)
		{
			NavTaskFactory.StartNewTask(() =>
			{
				TempTableSizeInfo sizeInfo = new TempTableSizeInfo(tableId, tableRowByteSize, increment);
				long privateMemorySize = currentProcess.PrivateMemorySize64;
				QueuedTaskInfo item = new QueuedTaskInfo(sizeInfo, tempTotalNumberOfRows, privateMemorySize);
				concurrentQueue.Enqueue(item);
			});
		}
	}

	private void ReportIncrementChange(QueuedTaskInfo queuedTaskInfo)
	{
		_ = string.Empty;
		if (queuedTaskInfo.SizeInfo.ApproximateRowCountByteSize > currentMaxTableRowSize)
		{
			currentMaxTableRowSize = queuedTaskInfo.SizeInfo.ApproximateRowCountByteSize;
		}
		if (queuedTaskInfo.SizeInfo.ApproximateRowCountByteSize < currentMinTableRowSize)
		{
			currentMinTableRowSize = queuedTaskInfo.SizeInfo.ApproximateRowCountByteSize;
		}
		if (UpdateTopTempTables(queuedTaskInfo.SizeInfo) && queuedTaskInfo.PrivateBytes >= 209715200)
		{
			string reportString = GetReportString(queuedTaskInfo.PrivateBytes, queuedTaskInfo.LastKnownTotalRowSize);
			if (reportString.Length > 0)
			{
				logger(EventLogEntryType.Warning, reportString, arg3: false);
			}
		}
	}

	private bool UpdateTopTempTables(TempTableSizeInfo sizeInfo)
	{
		for (int i = 0; i < topTempTables.Length; i++)
		{
			if (topTempTables[i].TableId == sizeInfo.TableId)
			{
				UpdateCountAtIndex(i, sizeInfo.Increment);
				return true;
			}
		}
		if (sizeInfo.Size <= LowestSize)
		{
			return false;
		}
		Insert(sizeInfo);
		return true;
	}

	[Conditional("MYBUILD")]
	private void EnsureTopTempTableIsSorted()
	{
		for (int i = 0; i < topTempTables.Length - 1; i++)
		{
		}
	}

	private void UpdateCountAtIndex(int index, int increment)
	{
		topTempTables[index].Count += increment;
		if (topTempTables[index].Size == 0L || topTempTables[index].Size < LowestSize)
		{
			RemoveAtIndex(index);
		}
		else
		{
			Array.Sort(topTempTables);
		}
	}

	private void RemoveAtIndex(int index)
	{
		if (index > 0)
		{
			for (int num = index; num > 0; num--)
			{
				topTempTables[num] = topTempTables[num - 1];
			}
		}
		topTempTables[0] = TempTableSizeInfo.DefaultSizeInfo;
	}

	private void Insert(TempTableSizeInfo sizeInfo)
	{
		int num = Array.BinarySearch(topTempTables, sizeInfo);
		int num2;
		if (num < 0)
		{
			num2 = ~num;
			if (num2 < topTempTables.Length && topTempTables[num2].TableId == -1)
			{
				topTempTables[num2] = sizeInfo;
				return;
			}
			if (num2 > 0)
			{
				num2--;
			}
		}
		else
		{
			if (num <= 0)
			{
				topTempTables[0] = sizeInfo;
				return;
			}
			num2 = num - 1;
		}
		for (int i = 0; i < num2; i++)
		{
			topTempTables[i] = topTempTables[i + 1];
		}
		topTempTables[num2] = sizeInfo;
	}

	private string GetReportString(long totalMemory, long lastKnownTotalRows)
	{
		long num = ApproximateTableRowByteSize * lastKnownTotalRows;
		bool flag = 3 * totalMemory <= 4 * num;
		bool flag2 = !flag && totalMemory <= 2 * num;
		bool flag3 = reachedFifty;
		bool flag4 = reachedSeventyFive;
		reachedFifty = flag2;
		reachedSeventyFive = flag;
		if ((!reachedFifty && !reachedSeventyFive) || Math.Abs(num - lastApproximateSize) < reportingMargin)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = null;
		if ((flag3 & !reachedFifty & flag) || (!flag3 & reachedFifty & !flag4) || (!flag3 & !reachedFifty & !flag4 & flag))
		{
			lastApproximateSize = num;
			stringBuilder = new StringBuilder(string.Format(CultureInfo.CurrentCulture, Lang.TempTableExcessiveUsage, flag2 ? 50 : 75));
			stringBuilder.AppendLine();
			for (int i = 0; i < topTempTables.Length; i++)
			{
				if (topTempTables[i].TableId != -1)
				{
					stringBuilder.AppendLine(string.Format(CultureInfo.CurrentCulture, "{0}\t{1} KB", topTempTables[i].TableId, topTempTables[i].Size >> 10));
				}
			}
		}
		if (stringBuilder == null)
		{
			return string.Empty;
		}
		return stringBuilder.ToString();
	}
}
