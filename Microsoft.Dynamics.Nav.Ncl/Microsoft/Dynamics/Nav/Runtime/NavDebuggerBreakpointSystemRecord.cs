using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Runtime.Debugger;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavDebuggerBreakpointSystemRecord : NavRecord
{
	private static int[] invalidFields = new int[11]
	{
		7, 9, 11, 13, 15, 17, 19, 27, 29, 31,
		33
	};

	private static void UpdateFieldsFromBreakpoint(NavRecord record, BreakpointDefinition breakPointDef)
	{
		record.SetFieldValue(11, NavInteger.Create(breakPointDef.SourceSpan.From.Line));
		record.SetFieldValue(13, NavInteger.Create(breakPointDef.SourceSpan.From.Column));
		record.SetFieldValue(15, NavInteger.Create(breakPointDef.SourceSpan.To.Line));
		record.SetFieldValue(17, NavInteger.Create(breakPointDef.SourceSpan.To.Column));
		record.SetFieldValue(29, NavInteger.Create(breakPointDef.RelativeSourceSpan.From.Line));
		record.SetFieldValue(31, NavInteger.Create(breakPointDef.RelativeSourceSpan.To.Line));
		record.SetFieldValue(27, NavBigInteger.Create(breakPointDef.BreakpointId));
		record.SetFieldValue(19, new NavText(int.MaxValue, breakPointDef.MethodName));
	}

	private static BreakpointDefinition AddBreakpoint(SingleInstanceDebugRuntime debugger, ObjectType objectType, int objectId, ushort lineNo, ushort columnNo, string condition)
	{
		ApplicationObjectId applicationObjectId = new ApplicationObjectId(objectType, objectId);
		SourcePosition position = new SourcePosition(lineNo, columnNo);
		return debugger.Environment.AddBreakpoint(applicationObjectId, position, condition);
	}

	private static BreakpointDefinition AddBreakpoint(SingleInstanceDebugRuntime debugger, ObjectType objectType, int objectId, ushort lineNo, ushort columnNo, string condition, out bool breakpointAdded)
	{
		ApplicationObjectId applicationObjectId = new ApplicationObjectId(objectType, objectId);
		SourcePosition position = new SourcePosition(lineNo, columnNo);
		return debugger.Environment.AddBreakpoint(applicationObjectId, position, condition, out breakpointAdded);
	}

	private static BreakpointDefinition AddBreakpoint(SingleInstanceDebugRuntime debugger, ObjectType objectType, int objectId, string functionName, ushort relativeLineNo)
	{
		ApplicationObjectId applicationObjectId = new ApplicationObjectId(objectType, objectId);
		return debugger.Environment.AddBreakpoint(applicationObjectId, functionName, relativeLineNo);
	}

	private static BreakpointDefinition GetBreakpoint(SingleInstanceDebugRuntime debugger, ObjectType objectType, int objectId, ushort lineNo, ushort columnNo, string condition)
	{
		ApplicationObjectId applicationObjectId = new ApplicationObjectId(objectType, objectId);
		SourcePosition position = new SourcePosition(lineNo, columnNo);
		return debugger.Environment.GetBreakpointDefinition(applicationObjectId, position, condition);
	}

	private static BreakpointDefinition GetBreakpoint(SingleInstanceDebugRuntime debugger, ObjectType objectType, int objectId, string functionName, ushort relativeLineNo)
	{
		ApplicationObjectId applicationObjectId = new ApplicationObjectId(objectType, objectId);
		return debugger.Environment.GetBreakpointDefinition(applicationObjectId, functionName, relativeLineNo);
	}

	private void ValidateFieldModifications(NavRecord oldRecord)
	{
		int[] array = invalidFields;
		foreach (int fieldNo in array)
		{
			if (!GetFieldValue(fieldNo).Equals(oldRecord.GetFieldValue(fieldNo)))
			{
				throw NavNCLFieldCannotBeModifiedException.Create(CultureInfo.CurrentCulture, MetaTable.TableCaptionSafe, ALFieldCaption(fieldNo));
			}
		}
	}

	protected NavDebuggerBreakpointSystemRecord(ITreeObject parent, int tableId, NCLMetaTable metaTable, bool isTemporary, NavRecord sharedTable, string companyName, SecurityFiltering securityFiltering)
		: base(parent, tableId, metaTable, isTemporary, sharedTable, companyName, securityFiltering)
	{
	}

	public static void LoadBreakpoints()
	{
		SingleInstanceDebugRuntime debugRuntimeInstance = ALDebugger.DebugRuntimeInstance;
		if (debugRuntimeInstance == null)
		{
			return;
		}
		debugRuntimeInstance.Environment.RemoveAllBreakpoints();
		using NavRecord navRecord = NavCurrentThread.Session.GetGlobalRecordInstance(2000000100);
		using NavRecord navRecord2 = NavCurrentThread.Session.GetGlobalRecordInstance(2000000100);
		navRecord.ALLockTable();
		if (!navRecord.ALFindSet(DataError.TrapError, forUpdate: true, updateKey: false))
		{
			return;
		}
		do
		{
			bool flag = false;
			bool flag2 = false;
			BreakpointDefinition breakpointDefinition = null;
			try
			{
				navRecord.ALTestField(7);
				navRecord.ALTestField(9);
				if (navRecord.GetFieldValue(11).ToInt32() != 0)
				{
					if (navRecord.GetFieldValue(21).ToBoolean())
					{
						breakpointDefinition = AddBreakpoint(debugRuntimeInstance, (ObjectType)navRecord.GetFieldValue(7).ToInt32(), navRecord.GetFieldValue(9).ToInt32(), (ushort)navRecord.GetFieldValue(11).ToInt32(), (ushort)navRecord.GetFieldValue(13).ToInt32(), navRecord.GetFieldValue(23).ToString());
						flag = true;
					}
					else
					{
						breakpointDefinition = GetBreakpoint(debugRuntimeInstance, (ObjectType)navRecord.GetFieldValue(7).ToInt32(), navRecord.GetFieldValue(9).ToInt32(), (ushort)navRecord.GetFieldValue(11).ToInt32(), (ushort)navRecord.GetFieldValue(13).ToInt32(), navRecord.GetFieldValue(23).ToString());
					}
				}
				else
				{
					navRecord.ALTestField(19);
					navRecord.ALTestField(29);
					if (navRecord.GetFieldValue(21).ToBoolean())
					{
						breakpointDefinition = AddBreakpoint(debugRuntimeInstance, (ObjectType)navRecord.GetFieldValue(7).ToInt32(), navRecord.GetFieldValue(9).ToInt32(), navRecord.GetFieldValue(19).ToString(), (ushort)navRecord.GetFieldValue(29).ToInt32());
						flag = true;
					}
					else
					{
						breakpointDefinition = GetBreakpoint(debugRuntimeInstance, (ObjectType)navRecord.GetFieldValue(7).ToInt32(), navRecord.GetFieldValue(9).ToInt32(), navRecord.GetFieldValue(19).ToString(), (ushort)navRecord.GetFieldValue(29).ToInt32());
					}
				}
				flag2 = true;
				navRecord2.ALSetFilter(1, "<>%1", navRecord.GetFieldValue(1));
				navRecord2.ALSetRange(7, navRecord.GetFieldValue(7));
				navRecord2.ALSetRange(9, navRecord.GetFieldValue(9));
				navRecord2.ALSetRange(11, NavInteger.Create(breakpointDefinition.SourceSpan.From.Line));
				navRecord2.ALSetRange(13, NavInteger.Create(breakpointDefinition.SourceSpan.From.Column));
				if (!navRecord2.ALIsEmpty)
				{
					throw new NavDebuggerDuplicateBreakpointException();
				}
				UpdateFieldsFromBreakpoint(navRecord, breakpointDefinition);
				navRecord.ALModify(runApplicationTrigger: false);
			}
			catch (NavBaseException)
			{
				if (flag)
				{
					debugRuntimeInstance.Environment.RemoveBreakpoint(breakpointDefinition.BreakpointId);
				}
			}
			catch (Exception)
			{
				ALDebugger.ALDeactivate();
				throw;
			}
			finally
			{
				if (!flag2)
				{
					navRecord.ALDelete(runApplicationTrigger: false);
				}
			}
		}
		while (navRecord.NextRecord());
	}

	public override bool Insert(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger)
	{
		bool flag = false;
		SingleInstanceDebugRuntime debugRuntimeInstance = ALDebugger.DebugRuntimeInstance;
		BreakpointDefinition breakpointDefinition = null;
		bool result = false;
		NavSession debuggingSession = ALDebugger.DebuggingSession;
		using (NavRecord navRecord = new NavRecord(ParentSession, TableID))
		{
			try
			{
				if (runApplicationTrigger && !IsTemporary && debuggingSession != null && GetFieldValue(21).ToBoolean() && !navRecord.Get(DataError.TrapError, ALRecordId))
				{
					breakpointDefinition = AddBreakpoint(debugRuntimeInstance, (ObjectType)GetFieldValue(7).ToInt32(), GetFieldValue(9).ToInt32(), (ushort)GetFieldValue(11).ToInt32(), (ushort)GetFieldValue(13).ToInt32(), GetFieldValue(23).ToString(), out var breakpointAdded);
					UpdateFieldsFromBreakpoint(this, breakpointDefinition);
					if (!breakpointAdded)
					{
						throw new NavDebuggerDuplicateBreakpointException();
					}
					flag = true;
				}
				if (!IsTemporary && !navRecord.Get(DataError.TrapError, ALRecordId))
				{
					navRecord.ALSetRange(7, GetFieldValue(7));
					navRecord.ALSetRange(9, GetFieldValue(9));
					if (GetFieldValue(11).ToInt32() != 0)
					{
						navRecord.ALSetRange(11, GetFieldValue(11));
						navRecord.ALSetRange(13, GetFieldValue(13));
					}
					else
					{
						navRecord.ALSetRange(29, GetFieldValue(29));
						navRecord.ALSetRange(19, GetFieldValue(19));
					}
					if (!navRecord.ALIsEmpty)
					{
						throw new NavDebuggerDuplicateBreakpointException();
					}
				}
				result = base.Insert(errorLevel, runApplicationTrigger, runGlobalTrigger);
			}
			catch (NavBaseException ex)
			{
				if (flag)
				{
					debugRuntimeInstance.Environment.RemoveBreakpoint(breakpointDefinition.BreakpointId);
				}
				if (errorLevel == DataError.ThrowError || !(ex is NavDebuggerDuplicateBreakpointException))
				{
					throw;
				}
			}
		}
		return result;
	}

	public override bool Delete(DataError errorLevel, bool runApplicationTrigger, bool isCalledFromUI)
	{
		return Delete(errorLevel, runApplicationTrigger, isCalledFromUI, isBulkDelete: false);
	}

	public override bool Delete(DataError errorLevel, bool runApplicationTrigger, bool isCalledFromUI, bool isBulkDelete)
	{
		SingleInstanceDebugRuntime debugRuntimeInstance = ALDebugger.DebugRuntimeInstance;
		NavSession debuggingSession = ALDebugger.DebuggingSession;
		if (runApplicationTrigger && !IsTemporary && debuggingSession != null && GetFieldValue(21).ToBoolean())
		{
			using NavRecord navRecord = new NavRecord(ParentSession, TableID);
			if (navRecord.Get(DataError.TrapError, ALRecordId))
			{
				long breakpointId = navRecord.GetFieldValue(27).ToInt64();
				debugRuntimeInstance.Environment.RemoveBreakpoint(breakpointId);
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
		BreakpointDefinition breakpointDefinition = null;
		bool result = false;
		NavSession debuggingSession = ALDebugger.DebuggingSession;
		using (NavRecord navRecord = new NavRecord(ParentSession, TableID))
		{
			try
			{
				if (runApplicationTrigger && !IsTemporary && debuggingSession != null && navRecord.Get(DataError.TrapError, ALRecordId))
				{
					ValidateFieldModifications(navRecord);
					if (navRecord.GetFieldValue(21).ToBoolean())
					{
						long breakpointId = navRecord.GetFieldValue(27).ToInt64();
						debugRuntimeInstance.Environment.RemoveBreakpoint(breakpointId);
						SetFieldValue(27, NavBigInteger.Default);
					}
					if (GetFieldValue(21).ToBoolean())
					{
						breakpointDefinition = AddBreakpoint(debugRuntimeInstance, (ObjectType)GetFieldValue(7).ToInt32(), GetFieldValue(9).ToInt32(), (ushort)GetFieldValue(11).ToInt32(), (ushort)GetFieldValue(13).ToInt32(), GetFieldValue(23).ToString(), out var breakpointAdded);
						UpdateFieldsFromBreakpoint(this, breakpointDefinition);
						if (!breakpointAdded)
						{
							throw new NavDebuggerDuplicateBreakpointException();
						}
						flag = true;
					}
				}
				if (!IsTemporary && navRecord.Get(DataError.TrapError, ALRecordId))
				{
					navRecord.ALSetFilter(1, "<>%1", GetFieldValue(1));
					navRecord.ALSetRange(7, GetFieldValue(7));
					navRecord.ALSetRange(9, GetFieldValue(9));
					if (GetFieldValue(11).ToInt32() != 0)
					{
						navRecord.ALSetRange(11, GetFieldValue(11));
						navRecord.ALSetRange(13, GetFieldValue(13));
					}
					else
					{
						navRecord.ALSetRange(29, GetFieldValue(29));
						navRecord.ALSetRange(19, GetFieldValue(19));
					}
					if (!navRecord.ALIsEmpty)
					{
						throw new NavDebuggerDuplicateBreakpointException();
					}
				}
				result = base.Modify(errorLevel, runApplicationTrigger, runGlobalTrigger, isBulkModify);
			}
			catch (NavBaseException ex)
			{
				if (flag)
				{
					debugRuntimeInstance.Environment.RemoveBreakpoint(breakpointDefinition.BreakpointId);
				}
				if (errorLevel == DataError.ThrowError || !(ex is NavDebuggerDuplicateBreakpointException))
				{
					throw;
				}
			}
		}
		return result;
	}

	protected override void ModifyAll(NCLMetaField metaField, NavValue newValue, bool runApplicationTrigger)
	{
		base.ModifyAll(metaField, newValue, runApplicationTrigger: false);
		LoadBreakpoints();
	}

	protected override void DeleteAll(bool runApplicationTrigger)
	{
		base.DeleteAll(runApplicationTrigger && !IsTemporary && NavCurrentThread.Session.IsDebugging());
	}

	protected override void OnModify()
	{
	}

	protected override void OnDelete()
	{
	}
}
