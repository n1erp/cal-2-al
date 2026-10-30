using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTestPage : NavTestPageBase
{
	internal NavTestPage(ITreeObject parent, ApplicationObjectId pageUnderTestId)
		: base(parent, pageUnderTestId)
	{
		if (!NavCurrentThread.Session.TestExecution.InTest)
		{
			throw new NavTestPageOnlyAllowedInTestsException();
		}
	}

	internal NavTestPage(ITreeObject parent, ApplicationObjectId pageUnderTestId, ITestPage testPage)
		: base(parent, pageUnderTestId, testPage)
	{
	}

	public void ALOpenNew()
	{
		Open(ViewMode.Create);
	}

	public void ALOpenEdit()
	{
		Open(ViewMode.Edit);
	}

	public void ALOpenView()
	{
		Open(ViewMode.View);
	}

	public void ALClose()
	{
		CheckPageOpened();
		Close();
	}

	public void ALTrap()
	{
		if (IsOpened)
		{
			Close();
		}
		NavCurrentThread.Session.TestExecution.AddTrap(this);
	}

	public void ALSaveAsPdf(string fileName)
	{
		NavCurrentThread.Session.TestExecution.ReportOutputFileName = fileName;
		GetBuiltInAction(FormResult.Pdf).ALInvoke();
	}

	public void ALSaveAsWord(string fileName)
	{
		NavCurrentThread.Session.TestExecution.ReportOutputFileName = fileName;
		GetBuiltInAction(FormResult.Word).ALInvoke();
	}

	public void ALSaveAsExcel(string fileName)
	{
		NavCurrentThread.Session.TestExecution.ReportOutputFileName = fileName;
		GetBuiltInAction(FormResult.Excel).ALInvoke();
	}

	public void ALSaveAsXml(string parametersFileName, string dataSetFileName)
	{
		NavCurrentThread.Session.TestExecution.ReportOutputFormat = FormResult.Xml;
		NavCurrentThread.Session.TestExecution.ReportOutputFileName = dataSetFileName;
		NavCurrentThread.Session.TestExecution.ReportParameterOutputFileName = parametersFileName;
		GetBuiltInAction(FormResult.Preview).ALInvoke();
	}

	public NavText ALRunPageBackgroundTask(int objectId, ByRef<NavDictionary<NavText, NavText>> parameters)
	{
		return ALRunPageBackgroundTask(objectId, parameters, runCompletionTriggers: false);
	}

	public NavText ALRunPageBackgroundTask(int objectId, ByRef<NavDictionary<NavText, NavText>> parameters, bool runCompletionTriggers)
	{
		PageBackgroundTask pageBackgroundTask = new PageBackgroundTask(NavCurrentThread.Session, ServerForm, ServerForm.SourceTable.ALRecordId, objectId, (NavSession s) => new NavCodeunitHandle(s, objectId), parameters.Value, cancelOnRecordChange: false, -1);
		IPageBackgroundTaskCompletionTrigger taskCompletionTrigger;
		if (!runCompletionTriggers)
		{
			IPageBackgroundTaskCompletionTrigger pageBackgroundTaskCompletionTrigger = new PageBackgroundTaskCompletionIgnoreTrigger(pageBackgroundTask);
			taskCompletionTrigger = pageBackgroundTaskCompletionTrigger;
		}
		else
		{
			IPageBackgroundTaskCompletionTrigger pageBackgroundTaskCompletionTrigger = new PageBackgroundTaskCompletionImmediateTrigger(pageBackgroundTask);
			taskCompletionTrigger = pageBackgroundTaskCompletionTrigger;
		}
		pageBackgroundTask.TaskCompletionTrigger = taskCompletionTrigger;
		return ALRunPageBackgroundTask(pageBackgroundTask);
	}

	internal static NavText ALRunPageBackgroundTask(PageBackgroundTask task)
	{
		NavChildSessionTaskTestRuntime navChildSessionTaskTestRuntime = new NavChildSessionTaskTestRuntime(task.ChildSessionTask, NavCurrentThread.Session);
		navChildSessionTaskTestRuntime.Run();
		if (task.Error != null)
		{
			throw task.Error.Exception;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<NavText, NavText> item in task.Results.Value)
		{
			stringBuilder.Append(item.Key.Value).Append("=").Append(item.Value.Value)
				.Append(";");
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Length--;
		}
		return new NavText(stringBuilder.ToString());
	}

	protected override void Open(ViewMode mode)
	{
		base.Open(mode);
		ITestPage value;
		try
		{
			value = NavCurrentThread.Session.TestExecution.ClientSession.CreatePage(ObjectId.ObjectNumber, mode);
		}
		catch (Exception ex) when (ex.GetType().Name.Equals("RowEntryCollectionException"))
		{
			throw new NavPermissionException(18023808, string.Format(CultureInfo.CurrentCulture, Lang.MissingPermissions, ObjectId.ObjectType, Session.Permissions.GetPermissionManager(Session.Company.CompanyNameToken).GetObjectName(ObjectId), PermissionMask.Insert, NavCurrentThread.SessionId));
		}
		Attach(value);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			NavCurrentThread.Session.TestExecution.RemoveTrap(this);
		}
		base.Dispose(disposing);
	}
}
