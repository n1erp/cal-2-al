using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.TaskScheduling;

internal class NavTaskScheduleActionList
{
	private readonly List<TaskScheduleAppAction> uncommittedTaskScheduleActions = new List<TaskScheduleAppAction>();

	private NavSqlConnectionScope connectionScope;

	internal void AddTaskScheduleAction(TaskScheduleAppAction action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		uncommittedTaskScheduleActions.Add(action);
	}

	internal void OnTransactionEnding(bool commit)
	{
		if (connectionScope != null)
		{
			NavCurrentThread.Session.Diagnostics.SendTraceTag("00001O7", Category.TaskScheduling, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Connection scope was still initialized when starting new commit (commit = {0})", commit);
		}
		try
		{
			if (!commit || uncommittedTaskScheduleActions.Count <= 0)
			{
				return;
			}
			connectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
			foreach (TaskScheduleAppAction uncommittedTaskScheduleAction in uncommittedTaskScheduleActions)
			{
				uncommittedTaskScheduleAction.Execute(connectionScope);
			}
		}
		finally
		{
			uncommittedTaskScheduleActions.Clear();
		}
	}

	internal void OnTransactionEnded(bool commit)
	{
		if (uncommittedTaskScheduleActions.Count != 0)
		{
			NavCurrentThread.Session.Diagnostics.SendTraceTag("00001O8", Category.TaskScheduling, Verbosity.Warning, PrivacyClassification.SystemMetadata, "There are tasks pending insert when we are about to commit already inserted tasks, this should not happen. (commit = {0})", commit);
		}
		try
		{
			if (commit)
			{
				connectionScope?.Dispose();
			}
			else
			{
				connectionScope?.TransactionEnd(NavSqlEndTransactionType.Rollback);
			}
		}
		finally
		{
			connectionScope = null;
		}
	}
}
