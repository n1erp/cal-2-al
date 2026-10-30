using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavCodeunit : NavApplicationObjectBase, ITreeSharedObject, ITreeObject, IDisposable
{
	[Flags]
	private enum EventBindingStatusOptions
	{
		None = 0,
		Bound = 1,
		WasBoundThrowOnEvent = 2
	}

	public const string RunMethodName = "Run";

	public const string OnRunMethodName = "OnRun";

	private NCLMetaCodeunit metaCodeunit;

	private int tableId;

	private bool inClear;

	private EventBindingStatusOptions eventBindingStatus;

	private static readonly NavMetric2Dimensions CodeunitRunMetric = new NavMetric2Dimensions("CodeunitRun", "CodeunitId", "ExecutionSuccess");

	public virtual CodeunitSubType SubType => CodeunitSubType.Normal;

	public string Value => string.Empty;

	public override string ObjectName => Session.GetObjectName(ObjectId);

	public NCLMetaCodeunit MetaCodeunit
	{
		get
		{
			if (metaCodeunit == null)
			{
				metaCodeunit = Session.NCLMetadata.GetMetaCodeunitById(ObjectId.ObjectNumber, OriginalAppGroupId);
			}
			return metaCodeunit;
		}
	}

	public int TableId => tableId;

	public virtual bool IsSingleInstance => false;

	public override NavNclType NclType => NavNclType.NavCodeunit;

	public override Type HandleType => typeof(NavCodeunitHandle);

	internal override NCLMetaApplicationObject MetaApplicationObject => MetaCodeunit;

	public bool IsSubscriptionBound
	{
		get
		{
			return (eventBindingStatus & EventBindingStatusOptions.Bound) != 0;
		}
		internal set
		{
			eventBindingStatus = (EventBindingStatusOptions)((int)(eventBindingStatus & ~EventBindingStatusOptions.Bound) | (value ? 1 : 0));
		}
	}

	protected internal virtual bool DoRun(DataError errorLevel, NavRecord record)
	{
		string text = ObjectId.ToString();
		using ITraceScope traceScope = DiagnosticsResolver.GetMostSpecificInstance(NavCurrentThread.Session).CreateTimingScope("0000050", Category.Runtime, DiagnosticParameter.SystemMetadata("Running codeunit ID " + ObjectId.ToString()));
		using IfxTimingScope ifxTimingScope = new IfxTimingScope(CodeunitRunMetric, text, false.ToString());
		bool flag = false;
		if (tableId == 0)
		{
			record = null;
		}
		else if (record == null)
		{
			record = NavGlobal.NCLMetadata.GetMetaTableById(tableId).CreateObjectInstance(this);
			flag = true;
		}
		else if (record.TableID != tableId)
		{
			throw new NavNCLInvalidTypeException(string.Format(CultureInfo.CurrentCulture, Lang.DifferentTableIdsMessage, record.TableID, tableId));
		}
		bool flag2 = false;
		try
		{
			using (new NavOpenDialogTracking())
			{
				NavSession navSession = NavCurrentThread.Session;
				if (errorLevel == DataError.ThrowError)
				{
					navSession.BeginTransaction();
					try
					{
						OnRun(record);
						flag2 = true;
					}
					finally
					{
						navSession.EndTransaction(flag2);
					}
				}
				else
				{
					navSession.BeginTransactionWorldAndTransaction();
					RecordState recordState = null;
					if (record != null)
					{
						recordState = record.CreateRecordState();
						recordState.BackupTableAndRecordHandle();
					}
					try
					{
						OnRun(record);
						Session.DataAccessSource.SessionTransactionManager.FlushBufferedWrites();
						flag2 = true;
					}
					catch (NavBaseException)
					{
						recordState?.RestoreRecord();
					}
					finally
					{
						recordState?.Dispose();
						navSession.EndTransactionWorldAndTransaction(flag2);
					}
				}
				return flag2;
			}
		}
		finally
		{
			traceScope.SetResult(flag2 ? NavDiagnosticsScopeResult.Success : NavDiagnosticsScopeResult.Failure);
			if (flag)
			{
				record?.Dispose();
			}
			if (flag2)
			{
				ifxTimingScope.SetDimensionValues(text, true.ToString());
			}
		}
	}

	protected virtual void OnRun(INavRecordHandle record)
	{
	}

	protected NavCodeunit(ITreeObject parent, int objectId)
		: this(parent, objectId, 0)
	{
	}

	protected NavCodeunit(ITreeObject parent, int objectId, int tableId)
		: base(parent, new ApplicationObjectId(ObjectType.CodeUnit, objectId))
	{
		this.tableId = tableId;
	}

	public override void Clear()
	{
		if (!IsSingleInstance && !inClear)
		{
			inClear = true;
			ClearAll();
			inClear = false;
		}
	}

	public bool Run()
	{
		return Run(DataError.ThrowError, null);
	}

	public bool Run(DataError errorLevel)
	{
		return Run(errorLevel, null);
	}

	public bool Run(NavRecord record)
	{
		return Run(DataError.ThrowError, record);
	}

	public bool Run(DataError errorLevel, NavRecord record)
	{
		return DoRun(errorLevel, record);
	}

	public static bool RunCodeunit(int objectId)
	{
		return RunCodeunit(DataError.ThrowError, objectId);
	}

	public static bool RunCodeunit(DataError errorLevel, int objectId)
	{
		using NavCodeunitHandle navCodeunitHandle = new NavCodeunitHandle(NavCurrentThread.Session, objectId);
		return navCodeunitHandle.Target.Run(errorLevel);
	}

	public static bool RunCodeunit(int objectId, NavRecord record)
	{
		return RunCodeunit(DataError.ThrowError, objectId, record);
	}

	public static bool RunCodeunit(DataError errorLevel, int objectId, NavRecord record)
	{
		using NavCodeunitHandle navCodeunitHandle = new NavCodeunitHandle(NavCurrentThread.Session, objectId);
		return navCodeunitHandle.Target.Run(errorLevel, record);
	}

	public static object Invoke(int objectId, string methodName, object[] args)
	{
		using NavCodeunitHandle navCodeunitHandle = new NavCodeunitHandle(NavCurrentThread.Session, objectId);
		return NavApplicationMethod.InvokeMethod(navCodeunitHandle.Target, methodName, args);
	}

	public static object Invoke(ITreeObject parent, int objectId, string methodName, object[] args)
	{
		using NavCodeunitHandle navCodeunitHandle = new NavCodeunitHandle(parent, objectId);
		return NavApplicationMethod.InvokeMethod(navCodeunitHandle.Target, methodName, args);
	}

	public static object InvokeRun(int objectId, object[] args)
	{
		using NavCodeunitHandle navCodeunitHandle = new NavCodeunitHandle(NavCurrentThread.Session, objectId);
		if (args == null || args.Length == 0 || navCodeunitHandle.Target.TableId == 0)
		{
			return navCodeunitHandle.Target.Run();
		}
		if (args.Length == 1 && args[0] is NavRecord record)
		{
			return navCodeunitHandle.Target.Run(record);
		}
		return NavApplicationMethod.InvokeMethod(navCodeunitHandle.Target, "Run", args);
	}

	public override int GetHashCode()
	{
		return ObjectId.GetHashCode();
	}

	public override NavComplexValue CreateHandle(ITreeObject parent)
	{
		return new NavCodeunitHandle(parent, this);
	}

	public void BindSubscription()
	{
		if (!MetaCodeunit.IsEventManualBinding)
		{
			throw new NavNCLEventBindingException(string.Format(CultureInfo.CurrentCulture, Lang.BindingWrongBindingType, ObjectId.ObjectNumber));
		}
		if (IsSubscriptionBound)
		{
			throw new NavNCLEventBindingException(string.Format(CultureInfo.CurrentCulture, Lang.BindingAlreadyBound, ObjectId.ObjectNumber));
		}
		IList<NavCodeunit> eventBindings = Tree.Session.EventBindings;
		eventBindings.Add(this);
		IsSubscriptionBound = true;
	}

	public void UnBindSubscription()
	{
		if (!MetaCodeunit.IsEventManualBinding)
		{
			throw new NavNCLEventBindingException(string.Format(CultureInfo.CurrentCulture, Lang.BindingWrongBindingType, ObjectId.ObjectNumber));
		}
		if (IsSubscriptionBound)
		{
			IsSubscriptionBound = false;
			IList<NavCodeunit> eventBindings = Tree.Session.EventBindings;
			eventBindings.Remove(this);
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && IsSubscriptionBound)
			{
				IsSubscriptionBound = false;
				Tree.Session.EventBindings.Remove(this);
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
}
