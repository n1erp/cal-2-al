using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SessionTransactionManager
{
	private class TransactionManagerKey
	{
		private readonly TableType tableType;

		private readonly string identifier;

		public TransactionManagerKey(TableType tableType, string identifier)
		{
			this.tableType = tableType;
			this.identifier = identifier;
		}

		public override bool Equals(object obj)
		{
			TransactionManagerKey transactionManagerKey = obj as TransactionManagerKey;
			if (tableType == transactionManagerKey.tableType)
			{
				return identifier == transactionManagerKey.identifier;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCodeHelper.CombineHashCodes(tableType.GetHashCode(), identifier.GetHashCode());
		}
	}

	internal const string AppDbConnectionIdentifier = "ApplicationDbConnection";

	internal const string TenantDbConnectionIdentifier = "TenantDbConnection";

	private readonly TransactionManager defaultTransactionManager;

	private readonly ConcurrentDictionary<TransactionManagerKey, TransactionManager> transactionManagers = new ConcurrentDictionary<TransactionManagerKey, TransactionManager>();

	private readonly InconsistentRecords inconsistentRecords = new InconsistentRecords();

	internal int CommitCount { get; private set; }

	public bool IsTransactionActive => defaultTransactionManager.IsTransactionActive;

	public TransactionType CurrentTransactionType
	{
		get
		{
			return defaultTransactionManager.CurrentTransactionType;
		}
		set
		{
			TransactionType previousValue = defaultTransactionManager.CurrentTransactionType;
			try
			{
				ForEachTransactionManager((TransactionManager transactionManager) =>
				{
					transactionManager.CurrentTransactionType = value;
				});
			}
			finally
			{
				if (TransactionManagers.Any((TransactionManager transactionManager) => transactionManager.CurrentTransactionType != value))
				{
					ForEachTransactionManager((TransactionManager transactionManager) =>
					{
						transactionManager.CurrentTransactionType = previousValue;
					});
				}
			}
		}
	}

	private IEnumerable<TransactionManager> TransactionManagers => transactionManagers.Values;

	public SessionTransactionManager(TransactionManager appTransactionManager, TransactionManager tenantTransactionManager)
	{
		transactionManagers.TryAdd(new TransactionManagerKey(TableType.Normal, "ApplicationDbConnection"), appTransactionManager);
		transactionManagers.TryAdd(new TransactionManagerKey(TableType.Normal, "TenantDbConnection"), tenantTransactionManager);
		defaultTransactionManager = tenantTransactionManager;
		appTransactionManager.WriteTransactionEnding += OnWriteTransactionEnding;
		tenantTransactionManager.WriteTransactionEnding += OnWriteTransactionEnding;
	}

	public void TransactionScope(Action code)
	{
		BeginTransaction();
		bool commit = false;
		try
		{
			if (code != null)
			{
				code();
				commit = true;
			}
		}
		finally
		{
			EndTransaction(commit);
		}
	}

	public void BeginTransaction()
	{
		ForEachTransactionManagerWithRollbackOnFailure((TransactionManager transactionManager) =>
		{
			transactionManager.BeginTransaction(inconsistentRecords);
		}, (TransactionManager transactionManager) =>
		{
			transactionManager.EndTransaction(inconsistentRecords, commit: false);
		});
	}

	public void EnsureWriteTransactionStarted()
	{
		ForEachTransactionManager((TransactionManager transactionManager) =>
		{
			transactionManager.EnsureWriteTransactionStarted();
		});
	}

	public void EndTransaction(bool commit)
	{
		ForEachTransactionManager((TransactionManager transactionManager) =>
		{
			transactionManager.EndTransaction(inconsistentRecords, commit);
		}, (TransactionManager transactionManager) =>
		{
			transactionManager.EndTransaction(inconsistentRecords, commit: false);
		});
	}

	public void Commit()
	{
		CommitCount++;
		ForEachTransactionManager((TransactionManager transactionManager) =>
		{
			transactionManager.Commit(inconsistentRecords);
		});
	}

	public void Rollback()
	{
		ForEachTransactionManager((TransactionManager transactionManager) =>
		{
			transactionManager.Rollback(inconsistentRecords);
		}, (TransactionManager transactionManager) =>
		{
			transactionManager.Rollback(inconsistentRecords);
		});
	}

	public void BeginTransactionWorldAndTransaction()
	{
		ForEachTransactionManagerWithRollbackOnFailure((TransactionManager transactionManager) =>
		{
			transactionManager.BeginTransactionWorldAndTransaction(inconsistentRecords);
		}, (TransactionManager transactionManager) =>
		{
			transactionManager.EndTransactionWorldAndTransaction(inconsistentRecords, commit: false);
		});
	}

	public void EndTransactionWorldAndTransaction(bool commit)
	{
		ForEachTransactionManager((TransactionManager transactionManager) =>
		{
			transactionManager.EndTransactionWorldAndTransaction(inconsistentRecords, commit);
		}, (TransactionManager transactionManager) =>
		{
			transactionManager.EndTransactionWorldAndTransaction(inconsistentRecords, commit: false);
		});
	}

	public void BeginTransactionWorld()
	{
		ForEachTransactionManagerWithRollbackOnFailure((TransactionManager transactionManager) =>
		{
			transactionManager.BeginTransactionWorld(inconsistentRecords);
		}, (TransactionManager transactionManager) =>
		{
			transactionManager.EndTransactionWorld();
		});
	}

	public void EndTransactionWorld()
	{
		ForEachTransactionManager((TransactionManager transactionManager) =>
		{
			transactionManager.EndTransactionWorld();
		}, (TransactionManager transactionManager) =>
		{
			transactionManager.EndTransactionWorld();
		});
	}

	public void ThrowIfWriteTransactionStarted()
	{
		ForEachTransactionManager((TransactionManager transactionManager) =>
		{
			transactionManager.ThrowIfWriteTransactionStarted();
		});
	}

	public bool AnyHasWriteTransactionStarted()
	{
		bool result = false;
		ForEachTransactionManager((TransactionManager transactionManager) =>
		{
			result = result || transactionManager.IsTransactionOpenForWrites;
		});
		return result;
	}

	public void SetRecordConsistent(NavRecord record)
	{
		inconsistentRecords.SetRecordConsistent(record);
	}

	public void SetRecordInconsistent(NavRecord record)
	{
		inconsistentRecords.SetRecordInconsistent(record);
	}

	private void OnWriteTransactionEnding(object sender, EndTransactionEventArgs e)
	{
		if (!e.Commit)
		{
			return;
		}
		ForEachTransactionManager((TransactionManager transactionManager) =>
		{
			if (transactionManager != sender)
			{
				transactionManager.TransactionalDataProvider.FlushBufferedWrites();
			}
		});
	}

	internal void FlushBufferedWrites()
	{
		ForEachTransactionManager((TransactionManager transactionManager) =>
		{
			transactionManager.TransactionalDataProvider.FlushBufferedWrites();
		});
	}

	private void ForEachTransactionManager(Action<TransactionManager> actionToPerform, Action<TransactionManager> actionToPerformOnFailure = null)
	{
		Exception ex = null;
		foreach (TransactionManager transactionManager in TransactionManagers)
		{
			try
			{
				if (actionToPerformOnFailure != null && ex != null)
				{
					actionToPerformOnFailure(transactionManager);
				}
				else
				{
					actionToPerform(transactionManager);
				}
			}
			catch (Exception ex2)
			{
				if (actionToPerformOnFailure == null)
				{
					throw;
				}
				if (ex == null)
				{
					ex = ex2;
				}
			}
		}
		if (ex != null)
		{
			throw ex;
		}
	}

	private void ForEachTransactionManagerWithRollbackOnFailure(Action<TransactionManager> actionToPerform, Action<TransactionManager> rollbackAction)
	{
		List<TransactionManager> list = new List<TransactionManager>();
		try
		{
			foreach (TransactionManager transactionManager in TransactionManagers)
			{
				actionToPerform(transactionManager);
				list.Add(transactionManager);
			}
		}
		catch
		{
			foreach (TransactionManager item in list)
			{
				rollbackAction(item);
			}
			throw;
		}
	}

	internal void AddTransactionManager(TableType tableType, string connectionIdentifier, TransactionManager transactionManagerToAdd)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000005X", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Adding session transaction manager for table type {0}, connectionIdentifier {1}", tableType, connectionIdentifier);
		TransactionManagerKey key = new TransactionManagerKey(tableType, connectionIdentifier);
		if (transactionManagers.ContainsKey(key))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000005Y", Category.Runtime, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Transaction manager already contains an entry for table type {0}, connectionIdentifier {1}, removing the old object.", tableType, connectionIdentifier);
			RemoveTransactionManager(tableType, connectionIdentifier);
		}
		transactionManagers.TryAdd(key, transactionManagerToAdd);
		defaultTransactionManager.SyncTransactionMananger(transactionManagerToAdd, null);
		transactionManagerToAdd.WriteTransactionEnding += OnWriteTransactionEnding;
	}

	internal void RemoveTransactionManager(TableType tableType, string connectionIdentifier, TransactionManager instanceToRemove = null)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000005Z", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Removing session transaction manager for table type {0}, connectionIdentifier {1}", tableType, connectionIdentifier);
		TransactionManagerKey key = new TransactionManagerKey(tableType, connectionIdentifier);
		if (!transactionManagers.TryGetValue(key, out var value))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000060", Category.Runtime, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Could not find session transaction manager for table type {0}, connectionIdentifier {1}. Remove object ignorred.", tableType, connectionIdentifier);
		}
		else if (instanceToRemove == null || value == instanceToRemove)
		{
			transactionManagers.TryRemove(key, out var _);
		}
	}
}
