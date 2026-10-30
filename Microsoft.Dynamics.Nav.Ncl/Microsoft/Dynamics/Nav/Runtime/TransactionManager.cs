using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TransactionManager
{
	private class LogicalTransaction
	{
		public bool BeginTransactionIssued { get; set; }

		public int TransactionCount { get; set; }

		public bool TransactionOpenForReads { get; set; }

		public bool TransactionOpenForWrites { get; set; }

		public TransactionType TransactionType { get; set; }

		public int TransactionWorldCount { get; set; }
	}

	private readonly Stack<LogicalTransaction> logicalTransactions = new Stack<LogicalTransaction>();

	private readonly LogicalTransaction rootTransaction;

	private readonly DataProvider transactionalDataProvider;

	public TransactionType CurrentTransactionType
	{
		get
		{
			return logicalTransactions.Peek().TransactionType;
		}
		set
		{
			LogicalTransaction logicalTransaction = logicalTransactions.Peek();
			if (!logicalTransaction.BeginTransactionIssued)
			{
				logicalTransaction.TransactionType = value;
				return;
			}
			switch (logicalTransaction.TransactionType)
			{
			case TransactionType.UpdateNoLocks:
				if ((uint)(value - 1) > 1u)
				{
					return;
				}
				break;
			case TransactionType.Update:
				return;
			case TransactionType.Snapshot:
				if ((uint)value > 1u)
				{
					return;
				}
				break;
			case TransactionType.Browse:
			case TransactionType.Report:
				if ((uint)value > 2u)
				{
					return;
				}
				break;
			}
			throw new NavCSideException(18023779, string.Format(CultureInfo.CurrentCulture, Lang.CannotChangeTransactionType, logicalTransaction.TransactionType, value));
		}
	}

	public bool IsTransactionActive
	{
		get
		{
			if (logicalTransactions.Count > 1)
			{
				return logicalTransactions.Peek().TransactionWorldCount == 0;
			}
			return false;
		}
	}

	public bool IsTransactionOpenForWrites
	{
		get
		{
			if (IsTransactionActive)
			{
				return logicalTransactions.Peek().TransactionOpenForWrites;
			}
			return false;
		}
	}

	public DataProvider TransactionalDataProvider => transactionalDataProvider;

	public event EventHandler<EndTransactionEventArgs> WriteTransactionEnding;

	public event EventHandler<EndTransactionEventArgs> WriteTransactionEnded;

	public TransactionManager(DataProvider transactionalDataProvider)
	{
		if (transactionalDataProvider == null)
		{
			throw new ArgumentNullException("transactionalDataProvider");
		}
		this.transactionalDataProvider = transactionalDataProvider;
		rootTransaction = new LogicalTransaction
		{
			TransactionType = TransactionType.Browse
		};
		logicalTransactions.Push(rootTransaction);
	}

	public void BeginTransaction(InconsistentRecords inconsistentRecords)
	{
		LogicalTransaction logicalTransaction = logicalTransactions.Peek();
		if (IsTransactionActive)
		{
			logicalTransaction.TransactionCount++;
			return;
		}
		if (logicalTransaction.TransactionOpenForWrites)
		{
			throw new NavCSideException(Lang.TransactionWorldWithActiveWriteTransaction);
		}
		CommitImpl(inconsistentRecords, commit: true);
		logicalTransactions.Push(new LogicalTransaction
		{
			TransactionType = TransactionType.UpdateNoLocks,
			TransactionCount = 1
		});
	}

	public void BeginTransactionWorld(InconsistentRecords inconsistentRecords)
	{
		LogicalTransaction logicalTransaction = logicalTransactions.Peek();
		if (logicalTransaction.TransactionWorldCount == 0)
		{
			ThrowIfWriteTransactionStarted();
			if (IsTransactionActive)
			{
				CommitImpl(inconsistentRecords, commit: true);
			}
		}
		logicalTransaction.TransactionWorldCount++;
	}

	public void ThrowIfWriteTransactionStarted()
	{
		if (IsTransactionOpenForWrites)
		{
			throw new NavCSideException(Lang.TransactionWorldWithActiveWriteTransaction);
		}
	}

	public void BeginTransactionWorldAndTransaction(InconsistentRecords inconsistentRecords)
	{
		BeginTransactionWorld(inconsistentRecords);
		BeginTransaction(inconsistentRecords);
	}

	public void Commit(InconsistentRecords inconsistentRecords)
	{
		CommitImpl(inconsistentRecords, commit: true);
	}

	public void EndTransaction(InconsistentRecords inconsistentRecords, bool commit)
	{
		try
		{
			EndTransactionImpl(inconsistentRecords, commit);
		}
		catch (NavBaseException)
		{
			if (IsTransactionActive)
			{
				EndTransactionImpl(inconsistentRecords, commit: false);
			}
			throw;
		}
	}

	public void EndTransactionWorld()
	{
		LogicalTransaction logicalTransaction = logicalTransactions.Peek();
		if (logicalTransaction.TransactionWorldCount == 0)
		{
			throw new NavCSideException("begin transaction world not called");
		}
		logicalTransaction.TransactionWorldCount--;
	}

	public void EndTransactionWorldAndTransaction(InconsistentRecords inconsistentRecords, bool commit)
	{
		try
		{
			EndTransaction(inconsistentRecords, commit);
		}
		finally
		{
			EndTransactionWorld();
		}
	}

	public void Rollback(InconsistentRecords inconsistentRecords)
	{
		CommitImpl(inconsistentRecords, commit: false);
	}

	private void OnWriteTransactionEnding(bool commit)
	{
		if (WriteTransactionEnding != null)
		{
			WriteTransactionEnding(this, new EndTransactionEventArgs(commit));
		}
	}

	private void OnWriteTransactionEnded(bool commit)
	{
		if (WriteTransactionEnded != null)
		{
			WriteTransactionEnded(this, new EndTransactionEventArgs(commit));
		}
	}

	public void EnsureReadTransactionStarted()
	{
		LogicalTransaction logicalTransaction = logicalTransactions.Peek();
		if (!logicalTransaction.TransactionOpenForReads)
		{
			if (logicalTransaction != rootTransaction && !logicalTransaction.BeginTransactionIssued)
			{
				TransactionType transactionType = logicalTransaction.TransactionType;
				transactionalDataProvider.BeginTransaction(transactionType);
				logicalTransaction.BeginTransactionIssued = true;
			}
			logicalTransaction.TransactionOpenForReads = true;
		}
	}

	public void EnsureWriteTransactionStarted()
	{
		ThrowIfNoTransaction();
		LogicalTransaction logicalTransaction = logicalTransactions.Peek();
		if (logicalTransaction.TransactionType != TransactionType.Update && logicalTransaction.TransactionType != TransactionType.UpdateNoLocks)
		{
			throw new NavCSideException(18023772, Lang.CannotWriteWithTransactionType);
		}
		if (!logicalTransaction.BeginTransactionIssued)
		{
			transactionalDataProvider.BeginTransaction(logicalTransaction.TransactionType);
			logicalTransaction.BeginTransactionIssued = true;
		}
		logicalTransaction.TransactionOpenForReads = true;
		logicalTransaction.TransactionOpenForWrites = true;
	}

	public void SyncTransactionMananger(TransactionManager transactionManagerToSync, InconsistentRecords inconsistentRecords)
	{
		foreach (LogicalTransaction item in logicalTransactions.Reverse())
		{
			for (int i = 0; i < item.TransactionCount; i++)
			{
				transactionManagerToSync.BeginTransaction(inconsistentRecords);
			}
			for (int j = 0; j < item.TransactionWorldCount; j++)
			{
				transactionManagerToSync.BeginTransactionWorld(inconsistentRecords);
			}
			transactionManagerToSync.CurrentTransactionType = item.TransactionType;
			if (item.TransactionOpenForWrites)
			{
				transactionManagerToSync.EnsureWriteTransactionStarted();
			}
			else if (item.TransactionOpenForReads)
			{
				transactionManagerToSync.EnsureReadTransactionStarted();
			}
		}
	}

	private void CommitImpl(InconsistentRecords inconsistentRecords, bool commit)
	{
		LogicalTransaction logicalTransaction = logicalTransactions.Peek();
		inconsistentRecords?.VerifyAllRecordsConsistent(commit);
		if (logicalTransaction.BeginTransactionIssued)
		{
			try
			{
				IssueEndTransaction(commit);
			}
			finally
			{
				logicalTransaction.BeginTransactionIssued = false;
				logicalTransaction.TransactionOpenForReads = false;
				logicalTransaction.TransactionOpenForWrites = false;
			}
		}
	}

	private void EndTransactionImpl(InconsistentRecords inconsistentRecords, bool commit)
	{
		ThrowIfNoTransaction();
		LogicalTransaction logicalTransaction = logicalTransactions.Peek();
		logicalTransaction.TransactionCount--;
		if (logicalTransaction.TransactionCount > 0)
		{
			return;
		}
		inconsistentRecords?.VerifyAllRecordsConsistent(commit);
		try
		{
			if (logicalTransaction.BeginTransactionIssued)
			{
				IssueEndTransaction(commit);
			}
		}
		finally
		{
			logicalTransactions.Pop();
		}
	}

	private void IssueEndTransaction(bool commit)
	{
		bool flag = false;
		try
		{
			OnWriteTransactionEnding(commit);
			flag = true;
		}
		finally
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				commit = false;
			}
			try
			{
				transactionalDataProvider.EndTransaction(commit);
				flag = true;
			}
			finally
			{
				OnWriteTransactionEnded(commit & flag);
			}
		}
	}

	private void ThrowIfNoTransaction()
	{
		if (!IsTransactionActive)
		{
			throw new NavCSideException(18022407, Lang.NoTransaction);
		}
	}
}
