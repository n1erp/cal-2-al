using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class SessionTransactionExtensions
{
	public static bool IsTransactionActive(this NavSession session)
	{
		if (session.IsOpen)
		{
			return session.DataAccessSource.SessionTransactionManager.IsTransactionActive;
		}
		return false;
	}

	public static void TransactionScope(this NavSession session, Action operation)
	{
		session.DataAccessSource.SessionTransactionManager.TransactionScope(operation);
	}

	public static void BeginTransaction(this NavSession session)
	{
		session.DataAccessSource.SessionTransactionManager.BeginTransaction();
	}

	public static void EnsureWriteTransactionStarted(this NavSession session)
	{
		session.DataAccessSource.SessionTransactionManager.EnsureWriteTransactionStarted();
	}

	public static void EndTransaction(this NavSession session, bool commit)
	{
		session.DataAccessSource.SessionTransactionManager.EndTransaction(commit);
	}

	public static void EndTransactionIfTransactionActive(this NavSession session, bool commit)
	{
		if (session != null && session.IsTransactionActive())
		{
			session.DataAccessSource.SessionTransactionManager.EndTransaction(commit);
		}
	}

	public static void Commit(this NavSession session)
	{
		session.DataAccessSource.SessionTransactionManager.Commit();
	}

	public static int CommitCount(this NavSession session)
	{
		return session.DataAccessSource.SessionTransactionManager.CommitCount;
	}

	public static void Rollback(this NavSession session)
	{
		session.DataAccessSource.SessionTransactionManager.Rollback();
	}

	public static void BeginTransactionWorldAndTransaction(this NavSession session)
	{
		session.DataAccessSource.SessionTransactionManager.BeginTransactionWorldAndTransaction();
	}

	public static void EndTransactionWorldAndTransaction(this NavSession session, bool commit)
	{
		session.DataAccessSource.SessionTransactionManager.EndTransactionWorldAndTransaction(commit);
	}

	public static void BeginTransactionWorld(this NavSession session)
	{
		session.DataAccessSource.SessionTransactionManager.BeginTransactionWorld();
	}

	public static void EndTransactionWorld(this NavSession session)
	{
		session.DataAccessSource.SessionTransactionManager.EndTransactionWorld();
	}

	public static void SetRecordConsistent(this NavSession session, NavRecord record)
	{
		session.DataAccessSource.SessionTransactionManager.SetRecordConsistent(record);
	}

	public static void SetRecordInconsistent(this NavSession session, NavRecord record)
	{
		session.DataAccessSource.SessionTransactionManager.SetRecordInconsistent(record);
	}
}
