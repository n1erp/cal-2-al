using System;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlTransactionHistory
{
	public delegate bool Predicate(ref HistoryEntry entry);

	public struct HistoryEntry
	{
		public int SessionId { get; set; }

		public ObjectType ObjectType { get; set; }

		public int ObjectNumber { get; set; }

		public string ScopeName { get; set; }

		public long TransactionId { get; set; }

		public string UserName { get; set; }

		public bool Running { get; set; }

		public DateTime StartTime { get; set; }
	}

	private static readonly TimeSpan MaxHistoryEntryLifetime = new TimeSpan(0, 0, 5, 0);

	public const int RingBufferSize = 1000;

	private readonly HistoryEntry[] historyRingBuffer = new HistoryEntry[1000];

	private int currentIndex;

	public HistoryEntry? Find(Predicate predicate)
	{
		for (int i = 0; i < 1000; i++)
		{
			if (predicate(ref historyRingBuffer[i]))
			{
				return historyRingBuffer[i];
			}
		}
		return null;
	}

	public void Foreach(Action<HistoryEntry> action)
	{
		for (int i = 0; i < 1000; i++)
		{
			action(historyRingBuffer[i]);
		}
	}

	public int TrackBeginTransaction(long transactionId)
	{
		if (NavCurrentThread.Session == null)
		{
			return -1;
		}
		int num = 0;
		while (num++ < 1000)
		{
			int num2 = MoveToNextIndex();
			if (TrySetStartInformation(ref historyRingBuffer[num2], transactionId))
			{
				return num2;
			}
		}
		return -1;
	}

	public void TrackEndTransaction(int historyToken, long transactionId)
	{
		if (historyToken >= 0 && historyToken < 1000 && historyRingBuffer[historyToken].TransactionId == transactionId)
		{
			historyRingBuffer[historyToken].Running = false;
		}
	}

	private int MoveToNextIndex()
	{
		int num = Interlocked.Increment(ref currentIndex);
		return Math.Abs(num % 1000);
	}

	private static bool TrySetStartInformation(ref HistoryEntry entry, long transactionId)
	{
		if (entry.Running && DateTime.UtcNow - entry.StartTime < MaxHistoryEntryLifetime)
		{
			return false;
		}
		NavSession session = NavCurrentThread.Session;
		entry.SessionId = session.Id;
		entry.UserName = session.UserDisplayName;
		entry.TransactionId = transactionId;
		entry.Running = true;
		entry.StartTime = DateTime.UtcNow;
		entry.ObjectNumber = 0;
		entry.ObjectType = ObjectType.TableData;
		entry.ScopeName = null;
		NavMethodScope currentMethodScope = session.CurrentMethodScope;
		if (currentMethodScope != null && !currentMethodScope.IsDisposed)
		{
			NavMethodScope navMethodScope = (session.CurrentMethodScope?.StackTrace)?.LastOrDefault();
			if (navMethodScope != null)
			{
				NavApplicationObjectBase navApplicationObjectBase = navMethodScope?.ApplicationObject;
				if (navApplicationObjectBase != null)
				{
					entry.ObjectNumber = navApplicationObjectBase.ObjectId.ObjectNumber;
					entry.ObjectType = navApplicationObjectBase.ObjectId.ObjectType;
				}
				entry.ScopeName = navMethodScope.ScopeName;
			}
		}
		else if (session.CurrentMethodScope != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000IP", Category.Runtime, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Current method scope is already disposed. And can't be used by transaction history. StackTrace:{0}", Environment.StackTrace);
		}
		return true;
	}
}
