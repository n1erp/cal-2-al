using System;
using Microsoft.Dynamics.Nav.Runtime.Debugger;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavDebuggerWatchSystemRecord : NavRecord
{
	private static bool IsValidWatchPath(string watchPath)
	{
		if (watchPath.StartsWith("\"", StringComparison.Ordinal) && watchPath.EndsWith("\"", StringComparison.Ordinal))
		{
			watchPath = watchPath.Substring(1, watchPath.Length - 2);
		}
		if (watchPath == "<Globals>" || watchPath.EndsWith("<Global Text Constants>", StringComparison.Ordinal) || watchPath.EndsWith("<Local Text Constants>", StringComparison.Ordinal))
		{
			return false;
		}
		return true;
	}

	protected NavDebuggerWatchSystemRecord(ITreeObject parent, int tableId, NCLMetaTable metaTable, bool isTemporary, NavRecord sharedTable, string companyName, SecurityFiltering securityFiltering)
		: base(parent, tableId, metaTable, isTemporary, sharedTable, companyName, securityFiltering)
	{
	}

	public static void LoadWatches()
	{
		SingleInstanceDebugRuntime debugRuntimeInstance = ALDebugger.DebugRuntimeInstance;
		if (debugRuntimeInstance == null)
		{
			return;
		}
		NavSession navSession = NavCurrentThread.Session;
		debugRuntimeInstance.Environment.RemoveAllWatches();
		using NavRecord navRecord = navSession.GetGlobalRecordInstance(2000000104);
		navRecord.ALLockTable();
		if (!navRecord.ALFindSet(DataError.TrapError, forUpdate: true, updateKey: false))
		{
			return;
		}
		do
		{
			bool flag = false;
			string value = null;
			try
			{
				string text = navRecord.GetFieldValue(7).ToString();
				if (!string.IsNullOrEmpty(text) && IsValidWatchPath(text))
				{
					value = debugRuntimeInstance.Environment.AddWatch(text, null);
					flag = true;
				}
			}
			catch (NavBaseException)
			{
			}
			catch (Exception)
			{
				ALDebugger.ALDeactivate();
				throw;
			}
			finally
			{
				if (flag)
				{
					navRecord.SetFieldValue(7, new NavText(int.MaxValue, value));
					navRecord.ALModify(runApplicationTrigger: false);
				}
			}
		}
		while (navRecord.NextRecord());
	}

	public override bool Insert(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger)
	{
		bool flag = false;
		SingleInstanceDebugRuntime debugRuntimeInstance = ALDebugger.DebugRuntimeInstance;
		NavSession debuggingSession = ALDebugger.DebuggingSession;
		string text = null;
		if (runApplicationTrigger && !IsTemporary && debuggingSession != null)
		{
			ALTestField(7);
			text = GetFieldValue(7).ToString();
			if (IsValidWatchPath(text))
			{
				using NavRecord navRecord = new NavRecord(ParentSession, TableID);
				if (!navRecord.Get(DataError.TrapError, ALRecordId))
				{
					string value = debugRuntimeInstance.Environment.AddWatch(text, null);
					SetFieldValue(7, new NavText(int.MaxValue, value));
					flag = true;
				}
			}
		}
		try
		{
			return base.Insert(errorLevel, runApplicationTrigger, runGlobalTrigger);
		}
		catch (NavBaseException)
		{
			if (flag)
			{
				debugRuntimeInstance.Environment.DeleteWatch(text);
			}
			throw;
		}
	}

	public override bool Delete(DataError errorLevel, bool runApplicationTrigger, bool isCalledFromUI)
	{
		return Delete(errorLevel, runApplicationTrigger, isCalledFromUI, isBulkDelete: false);
	}

	public override bool Delete(DataError errorLevel, bool runApplicationTrigger, bool isCalledFromUI, bool isBulkDelete)
	{
		SingleInstanceDebugRuntime debugRuntimeInstance = ALDebugger.DebugRuntimeInstance;
		NavSession debuggingSession = ALDebugger.DebuggingSession;
		if (runApplicationTrigger && !IsTemporary && debuggingSession != null)
		{
			using NavRecord navRecord = new NavRecord(ParentSession, TableID);
			if (navRecord.Get(DataError.TrapError, ALRecordId))
			{
				string text = navRecord.GetFieldValue(7).ToString();
				if (!string.IsNullOrEmpty(text))
				{
					debugRuntimeInstance.Environment.DeleteWatch(text);
				}
			}
		}
		return base.Delete(errorLevel, runApplicationTrigger, isCalledFromUI, isBulkDelete);
	}

	public override bool Modify(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger)
	{
		return Modify(errorLevel, runApplicationTrigger, runGlobalTrigger, isBulkModify: false);
	}

	public override bool Modify(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger, bool isBulkModify)
	{
		bool flag = false;
		SingleInstanceDebugRuntime debugRuntimeInstance = ALDebugger.DebugRuntimeInstance;
		NavSession debuggingSession = ALDebugger.DebuggingSession;
		string text = null;
		if (runApplicationTrigger && !IsTemporary && debuggingSession != null)
		{
			using NavRecord navRecord = new NavRecord(ParentSession, TableID);
			if (navRecord.Get(DataError.TrapError, ALRecordId))
			{
				text = navRecord.GetFieldValue(7).ToString();
				if (!string.IsNullOrEmpty(text) && IsValidWatchPath(text))
				{
					debugRuntimeInstance.Environment.DeleteWatch(text);
				}
				ALTestField(7);
				text = GetFieldValue(7).ToString();
				if (IsValidWatchPath(text))
				{
					string value = debugRuntimeInstance.Environment.AddWatch(text, null);
					SetFieldValue(7, new NavText(int.MaxValue, value));
					flag = true;
				}
			}
		}
		try
		{
			return base.Modify(errorLevel, runApplicationTrigger, runGlobalTrigger, isBulkModify);
		}
		catch (NavBaseException)
		{
			if (flag)
			{
				debugRuntimeInstance.Environment.DeleteWatch(text);
			}
			throw;
		}
	}

	protected override void ModifyAll(NCLMetaField metaField, NavValue newValue, bool runApplicationTrigger)
	{
		base.ModifyAll(metaField, newValue, runApplicationTrigger: false);
		LoadWatches();
	}

	protected override void DeleteAll(bool runApplicationTrigger)
	{
		base.DeleteAll(runApplicationTrigger: false);
		LoadWatches();
	}

	protected override void OnModify()
	{
	}

	protected override void OnDelete()
	{
	}
}
