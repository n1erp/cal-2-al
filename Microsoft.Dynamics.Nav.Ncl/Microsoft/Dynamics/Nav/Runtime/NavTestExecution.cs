using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Dynamics.Nav.Client;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavTestExecution : TreeObject
{
	private class ServerRequestsProcessor : IDisposable
	{
		public void Dispose()
		{
			if (NavCurrentThread.Session.CallbackAllowed)
			{
				NavDialog.ProcessServerRequests();
			}
			GC.SuppressFinalize(this);
		}
	}

	private static readonly object testPageClientSessionCreateLock = new object();

	private NavTestRunnerCodeUnit executingTestRunner;

	private NavTestCodeunit executingTestCodeUnit;

	private MethodInfo executingTestMethod;

	private NavTestAttribute executingTestAttr;

	private string executingHandlers;

	private ITestClientSession testClientSession;

	private IService testServiceConnection;

	private Stack<FormResult> formResultStack = new Stack<FormResult>();

	private Stack<InvokeDialogHandler> dialogHandlerStack = new Stack<InvokeDialogHandler>();

	private List<NavTestPage> traps = new List<NavTestPage>();

	private const string OnAfterTestRunMethodName = "OnAfterTestRun";

	private HashSet<string> effectivePermissions;

	private static Guid testRunMessageGuid = new Guid("{5521b828-0076-4330-952a-991c81cbb518}");

	private StringBuilder testRunMessage;

	internal FormResult ReportOutputFormat { get; set; }

	internal string ReportOutputFileName { get; set; }

	internal string ReportParameterOutputFileName { get; set; }

	internal TestTransactionModel CurrentTransactionModel { get; set; }

	internal bool CommitTestCodeunits
	{
		get
		{
			if (executingTestRunner != null)
			{
				return executingTestRunner.CommitTestCodeunits;
			}
			return true;
		}
	}

	internal bool CommitTestFunctions
	{
		get
		{
			if (executingTestRunner != null)
			{
				return executingTestRunner.CommitTestFunctions;
			}
			return true;
		}
	}

	internal ITestClientSession ClientSession
	{
		get
		{
			if (testClientSession == null)
			{
				testClientSession = CreateTestClientSession();
			}
			return testClientSession;
		}
	}

	internal IService ServiceConnection
	{
		get
		{
			if (ClientSession != null)
			{
				return testServiceConnection;
			}
			return null;
		}
	}

	public bool InTest => executingTestCodeUnit != null;

	public HashSet<string> EffectivePermissionSets
	{
		get
		{
			return effectivePermissions;
		}
		internal set
		{
			if (effectivePermissions == null && value != null && !NavCurrentThread.Session.Permissions.IsSuperForAllCompanies)
			{
				throw NavCurrentThread.Session.Diagnostics.SendExceptionTag("000004F", Category.Runtime, new NavMustBeSuperUserException(Lang.MustBeSuperUserToSetEffectivePermissions));
			}
			NavCurrentThread.Session.Diagnostics.SendTraceTag("000004G", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Setting effective permisisons");
			effectivePermissions = value;
			NavCurrentThread.Session.Permissions.ClearPermissions();
		}
	}

	public bool EffectivePermissionsInUse
	{
		get
		{
			if (InTest && effectivePermissions != null)
			{
				return effectivePermissions.Any();
			}
			return false;
		}
	}

	public string LastTestRunMessage
	{
		get
		{
			if (testRunMessage == null)
			{
				throw new InvalidOperationException("No test code units have been run, so no message is available.");
			}
			return testRunMessage.ToString();
		}
	}

	public NavTestExecution(ITreeObject parent)
		: base(parent)
	{
	}

	public static bool IsInTestMode()
	{
		NavSession session = NavCurrentThread.Session;
		if (session?.TestExecution != null)
		{
			return session.TestExecution.InTest;
		}
		return false;
	}

	public static bool UseEffectivePermissions()
	{
		return NavCurrentThread.Session.TestExecution.EffectivePermissionsInUse;
	}

	public static HashSet<string> GetEffectiveTestPermissions()
	{
		NavSession session = NavCurrentThread.Session;
		if (session?.TestExecution != null)
		{
			return session.TestExecution.EffectivePermissionSets;
		}
		return null;
	}

	public static void SetEffectiveTestPermissions(HashSet<string> effectivePermissions)
	{
		NavSession session = NavCurrentThread.Session;
		if (session?.TestExecution != null)
		{
			session.TestExecution.EffectivePermissionSets = effectivePermissions;
		}
	}

	internal void EnterTestRunner(NavTestRunnerCodeUnit testRunner)
	{
		testRunner.PreviouslyExecutingRunner = executingTestRunner;
		executingTestRunner = testRunner;
	}

	internal void LeaveTestRunner()
	{
		executingTestRunner = executingTestRunner.PreviouslyExecutingRunner;
	}

	internal void EnterTestCodeunit(NavTestCodeunit testCodeUnit)
	{
		if (executingTestCodeUnit != null)
		{
			throw NavNCLTestCodeUnitNestedInvocationException.Create(NavCurrentThread.Session.RuntimeLanguage, testCodeUnit.ObjectId.ObjectNumber, testCodeUnit.ObjectName);
		}
		if (NavCurrentThread.Session.CallbackAllowed)
		{
			NavDialog.ProcessServerRequests();
		}
		executingTestCodeUnit = testCodeUnit;
	}

	internal void LeaveTestCodeunit()
	{
		executingTestCodeUnit = null;
	}

	private void EnterTestMethod(MethodInfo testMethod, NavTestAttribute testAttr)
	{
		executingTestMethod = testMethod;
		executingTestAttr = testAttr;
	}

	private NavTestPermissions LeaveTestMethod()
	{
		executingTestMethod = null;
		NavTestPermissions result = ((executingTestAttr == null) ? NavTestPermissions.Restrictive : executingTestAttr.TestPermissions);
		executingTestAttr = null;
		return result;
	}

	internal bool BeforeTestRun(MethodInfo method, NavTestAttribute testAttr)
	{
		using (new ServerRequestsProcessor())
		{
			string text;
			if (testAttr != null)
			{
				text = testAttr.CalName;
			}
			else
			{
				text = ((method != null) ? method.Name : string.Empty);
			}
			if (method == null)
			{
				testRunMessage = new StringBuilder(512);
				testRunMessage.AppendFormat(CultureInfo.InvariantCulture, "Test Codeunit {0} {1}\\", executingTestCodeUnit.ObjectId.ObjectNumber, executingTestCodeUnit.ObjectName);
			}
			else
			{
				testRunMessage.AppendFormat(CultureInfo.InvariantCulture, "  {0}: ", text);
			}
			if (executingTestRunner != null)
			{
				NavSession session = NavCurrentThread.Session;
				NavSqlConnectionScope navSqlConnectionScope = null;
				NavSqlConnectionScope navSqlConnectionScope2 = null;
				if (!CommitTestCodeunits || !CommitTestFunctions)
				{
					navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(session.Database, TransactionType.UpdateNoLocks);
					navSqlConnectionScope2 = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks);
				}
				try
				{
					bool flag = (executingTestRunner.OnTestRunMethodsHaveTestPermissionsParameter ? executingTestRunner.OnBeforeTestRun(executingTestCodeUnit.ObjectId.ObjectNumber, new NavText(executingTestCodeUnit.ObjectName), new NavText(text), testAttr?.TestPermissions ?? NavTestPermissions.Restrictive) : executingTestRunner.OnBeforeTestRun(executingTestCodeUnit.ObjectId.ObjectNumber, new NavText(executingTestCodeUnit.ObjectName), new NavText(text)));
					session.Commit();
					if (flag)
					{
						EnterTestMethod(method, testAttr);
					}
					return flag;
				}
				catch (Exception)
				{
					session.Rollback();
					throw;
				}
				finally
				{
					if (navSqlConnectionScope != null)
					{
						navSqlConnectionScope.Dispose();
						navSqlConnectionScope2.Dispose();
					}
				}
			}
			EnterTestMethod(method, testAttr);
			return true;
		}
	}

	private static void DisplayMessage(StringBuilder message)
	{
		NavDialog.ALMessage(testRunMessageGuid, message.ToString());
	}

	internal void AfterTestRun(bool success)
	{
		EffectivePermissionSets = null;
		using (new ServerRequestsProcessor())
		{
			string text = executingTestAttr?.CalName;
			MethodInfo methodInfo = executingTestMethod;
			NavTestPermissions navTestPermissions = LeaveTestMethod();
			NavSession session = NavCurrentThread.Session;
			testRunMessage.Append(success ? "SUCCESS\\" : "FAILURE\\");
			if (methodInfo != null && !success)
			{
				testRunMessage.AppendFormat(CultureInfo.InvariantCulture, "    {0}\\", session.GetLastErrorText);
			}
			bool flag = false;
			if (executingTestRunner != null)
			{
				flag = null != executingTestRunner.GetType().GetMethod("OnAfterTestRun", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			}
			if (flag)
			{
				NavSqlConnectionScope navSqlConnectionScope = null;
				NavSqlConnectionScope navSqlConnectionScope2 = null;
				if (!CommitTestCodeunits || !CommitTestFunctions)
				{
					navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks);
					navSqlConnectionScope2 = NavSqlConnectionScope.CreateForceNewConnection(session.Database, TransactionType.UpdateNoLocks);
				}
				try
				{
					string value;
					if (text != null)
					{
						value = text;
					}
					else
					{
						value = ((methodInfo != null) ? methodInfo.Name : string.Empty);
					}
					if (!executingTestRunner.OnTestRunMethodsHaveTestPermissionsParameter)
					{
						executingTestRunner.OnAfterTestRun(executingTestCodeUnit.ObjectId.ObjectNumber, new NavText(executingTestCodeUnit.ObjectName), new NavText(value), success);
					}
					else
					{
						executingTestRunner.OnAfterTestRun(executingTestCodeUnit.ObjectId.ObjectNumber, new NavText(executingTestCodeUnit.ObjectName), new NavText(value), (executingTestAttr != null) ? executingTestAttr.TestPermissions : navTestPermissions, success);
					}
					session.Commit();
					return;
				}
				catch (Exception)
				{
					session.Rollback();
					throw;
				}
				finally
				{
					if (navSqlConnectionScope2 != null)
					{
						navSqlConnectionScope2.Dispose();
						navSqlConnectionScope.Dispose();
					}
				}
			}
			if (methodInfo == null)
			{
				DisplayMessage(testRunMessage);
			}
		}
	}

	private ITestClientSession CreateTestClientSession()
	{
		try
		{
			string fullName = Assembly.GetExecutingAssembly().FullName;
			Assembly assembly = Assembly.Load("Microsoft.Dynamics.Nav.Client.TestPageClient" + fullName.Substring(fullName.IndexOf(',')));
			if (assembly != null)
			{
				Type type = assembly.GetType("Microsoft.Dynamics.Nav.Client.TestPageClient.TestPageClientSession", throwOnError: true);
				testServiceConnection = new TestServiceConnection(NavCurrentThread.Session);
				lock (testPageClientSessionCreateLock)
				{
					return (ITestClientSession)type.InvokeMember("Create", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, null, new object[6]
					{
						testServiceConnection,
						NavCurrentThread.Session.WindowsCulture,
						new OnShowFormCallback(ShowForm),
						new OnShowFormCallback(ShowDialog),
						new OnShowFormCallback(ShowLookupForm),
						new OnShowMessageCallback(ShowMessage)
					}, CultureInfo.InvariantCulture);
				}
			}
		}
		catch (FileNotFoundException)
		{
		}
		throw new NavTestTestClientNotInstalledException();
	}

	internal bool ActivatePage(Guid handle, bool refresh)
	{
		if (testClientSession == null)
		{
			return false;
		}
		return testClientSession.ActivatePage(handle, refresh);
	}

	internal bool ClosePage(NavForm form)
	{
		if (testClientSession == null)
		{
			return false;
		}
		ITestPage page = testClientSession.GetPage(form.Handle, forClose: true);
		if (page != null)
		{
			if (testServiceConnection?.CallbackHandler != null)
			{
				testServiceConnection.CallbackHandler.FormClose(NavCurrentThread.Session.MakeFormCloseRequest(form));
			}
			page.Close();
			return true;
		}
		return false;
	}

	private object ShowForm(Guid handle)
	{
		NavForm registeredForm = NavCurrentThread.Session.Company.GetRegisteredForm(handle);
		NavTestPage navTestPage = FindTrap(registeredForm.ObjectId.ObjectNumber);
		if (navTestPage != null)
		{
			RemoveTrap(navTestPage);
			navTestPage.Attach(ClientSession.GetPage(handle));
			return null;
		}
		MethodInfo methodInfo = FindHandler(NavHandlerType.Page, registeredForm);
		if (methodInfo == null)
		{
			throw NavTestPageInvokedWithoutHandlerException.Create(CultureInfo.CurrentCulture, registeredForm.ObjectId.ObjectNumber);
		}
		using (NavTestPageHandle navTestPageHandle = new NavTestPageHandle(NavCurrentThread.Session.CurrentMethodScope, new NavTestPage(NavCurrentThread.Session.Company.SharedObjects, registeredForm.ObjectId, ClientSession.GetPage(handle))))
		{
			InvokeHandler(methodInfo, navTestPageHandle);
		}
		return null;
	}

	private object ShowDialog(Guid handle)
	{
		if (!Guid.Empty.Equals(handle))
		{
			return dialogHandlerStack.Pop()();
		}
		return FormResult.None;
	}

	private object ShowLookupForm(Guid handle)
	{
		NavForm registeredForm = NavCurrentThread.Session.Company.GetRegisteredForm(handle);
		MethodInfo methodInfo = FindHandler(NavHandlerType.ModalPage, registeredForm);
		if (methodInfo == null)
		{
			return false;
		}
		using (NavTestPageHandle navTestPageHandle = new NavTestPageHandle(NavCurrentThread.Session.CurrentMethodScope, new NavTestPage(NavCurrentThread.Session.Company.SharedObjects, registeredForm.ObjectId, ClientSession.GetPage(handle))))
		{
			InvokeHandler(methodInfo, navTestPageHandle);
		}
		return null;
	}

	private bool ShowMessage(string message, Guid handle)
	{
		if (!TestHandleMessage(message))
		{
			NavDialog.ALMessage(handle, message);
			return false;
		}
		return true;
	}

	public void SetLastFormResult(FormResult formResult)
	{
		formResultStack.Pop();
		formResultStack.Push(formResult);
	}

	internal void InitHandlers()
	{
		if (executingTestAttr != null)
		{
			executingHandlers = executingTestAttr.Handlers;
		}
	}

	internal void TestHandlers()
	{
		RemoveOptionalHandlers();
		if (executingHandlers != null && executingHandlers.Length > 0)
		{
			throw new NavNCLUnexecutedUIHandlerException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.UnexecutedUIHandler, executingHandlers));
		}
	}

	internal void RemoveOptionalHandlers()
	{
		if (executingHandlers == null)
		{
			return;
		}
		string[] array = executingHandlers.Split(',');
		foreach (string handlerName in array)
		{
			if (null != FindHandler((NavHandlerAttribute handlerAttribute) => handlerAttribute.Optional, null))
			{
				RemoveHandlerName(handlerName);
			}
		}
	}

	private MethodInfo FindHandler(NavHandlerType handlerType, NavApplicationObjectBase appObject, bool throwIfNotFound = true, string handlerDescription = null)
	{
		MethodInfo methodInfo = FindHandler((NavHandlerAttribute handlerAttribute) => handlerAttribute.HandlerType == handlerType, appObject);
		if (((methodInfo == null) & throwIfNotFound) && executingTestRunner != null && executingTestMethod != null)
		{
			throw new NavNCLMissingUIHandlerException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.MissingUIHandler, handlerType, appObject?.ToString() ?? handlerDescription));
		}
		return methodInfo;
	}

	private MethodInfo FindHandler(Func<NavHandlerAttribute, bool> handlerTypeAccepted, NavApplicationObjectBase appObject)
	{
		if (executingTestMethod == null)
		{
			return null;
		}
		if (executingTestAttr != null && executingTestAttr.Handlers != null)
		{
			string[] array = executingTestAttr.Handlers.Split(',');
			foreach (string text in array)
			{
				MethodInfo method = executingTestMethod.DeclaringType.GetMethod(text);
				if (!(method != null))
				{
					continue;
				}
				object[] customAttributes = method.GetCustomAttributes(typeof(NavHandlerAttribute), inherit: false);
				NavHandlerAttribute navHandlerAttribute = ((customAttributes.Length == 1) ? ((NavHandlerAttribute)customAttributes[0]) : null);
				if (navHandlerAttribute == null || !handlerTypeAccepted(navHandlerAttribute))
				{
					continue;
				}
				if (appObject != null)
				{
					ParameterInfo parameterInfo = method.GetParameters()[0];
					if (parameterInfo.ParameterType == appObject.HandleType || parameterInfo.ParameterType == typeof(NavTestPageHandle))
					{
						object[] customAttributes2 = parameterInfo.GetCustomAttributes(typeof(NavObjectIdAttribute), inherit: false);
						if (customAttributes2.Length != 1 || ((NavObjectIdAttribute)customAttributes2[0]).ObjectId != appObject.ObjectId.ObjectNumber)
						{
							continue;
						}
					}
				}
				RemoveHandlerName(text);
				return method;
			}
		}
		return null;
	}

	private void RemoveHandlerName(string handlerName)
	{
		int num = executingHandlers.IndexOf(handlerName, 0, StringComparison.Ordinal);
		if (num >= 0)
		{
			int num2 = handlerName.Length;
			if (executingHandlers.Length >= num + num2 + 1)
			{
				num2++;
			}
			executingHandlers = executingHandlers.Remove(num, num2);
		}
	}

	private object InvokeHandler(MethodInfo handler, params object[] parameters)
	{
		try
		{
			return handler.Invoke(executingTestCodeUnit, parameters);
		}
		catch (TargetInvocationException ex)
		{
			throw ex.GetBaseException();
		}
	}

	internal bool TestHandleForm(NavForm form, NavFormRuntimeParameters parameters)
	{
		MethodInfo methodInfo = null;
		bool flag = HasTrap(form.ObjectId.ObjectNumber);
		if (!flag)
		{
			methodInfo = FindHandler(NavHandlerType.Page, form);
			if (methodInfo == null)
			{
				return false;
			}
		}
		using NavScope parent = new NavScope(NavCurrentThread.Session);
		object obj = form.CreateHandle(parent);
		if (flag || methodInfo.GetParameters()[0].ParameterType == typeof(NavTestPageHandle))
		{
			NavCurrentThread.Session.Company.RegisterForm(form);
			FormRunRequest formRunRequest = new FormRunRequest();
			NavCurrentThread.Session.SetServerFormRequestData(form, parameters, formRunRequest.Data);
			ServiceConnection.CallbackHandler.FormRun(formRunRequest);
		}
		else
		{
			InvokeHandler(methodInfo, obj);
		}
		return true;
	}

	internal bool TestHandleModalForm(NavForm form, ref FormResult action, NavFormRuntimeParameters parameters)
	{
		NavHandlerType navHandlerType = FindPageType(form);
		MethodInfo handler = FindHandler(navHandlerType, form);
		if (handler == null)
		{
			return false;
		}
		using NavScope parent = new NavScope(NavCurrentThread.Session);
		if (navHandlerType == NavHandlerType.FilterPage && form is FilterPage { IndexedNavRecords: var indexedNavRecords })
		{
			object[] array = indexedNavRecords.Select((NavRecordRef record) => new ByRef<NavRecordRef>(() => record, (NavRecordRef value) =>
			{
				record = value;
			})).ToArray();
			object[] source = array;
			action = (((bool)InvokeHandler(handler, source.ToArray())) ? FormResult.OK : FormResult.Cancel);
			return true;
		}
		FormResult localAction = action;
		ByRef<FormResult> byRef = new ByRef<FormResult>(() => localAction, (FormResult value) =>
		{
			localAction = value;
		});
		if (handler.GetParameters()[0].ParameterType == typeof(NavTestPageHandle))
		{
			NavCurrentThread.Session.Company.RegisterForm(form);
			FormRunModalRequest runRequest = new FormRunModalRequest();
			NavCurrentThread.Session.SetServerFormRequestData(form, parameters, runRequest.Data);
			dialogHandlerStack.Push(() =>
			{
				NavTestPage target = (form.IsRequestPage ? new NavTestRequestPage(NavCurrentThread.Session.Company.SharedObjects, form.ObjectId, testClientSession.GetPage(runRequest.Data.FormHandle)) : new NavTestPage(NavCurrentThread.Session.Company.SharedObjects, form.ObjectId, testClientSession.GetPage(runRequest.Data.FormHandle)));
				using NavTestPageHandle navTestPageHandle = new NavTestPageHandle(NavCurrentThread.Session.CurrentMethodScope, target);
				InvokeHandler(handler, navTestPageHandle);
				if (navTestPageHandle.Target.IsTestPageDisposed)
				{
					throw new NavTestCodeUnitException(string.Format(CultureInfo.InvariantCulture, "The RunModal procedure could not close the page {0} as it has already been closed.", navTestPageHandle.Target.ObjectId));
				}
				FormResult formResult = navTestPageHandle.Target.FormResult;
				navTestPageHandle.Target.Close();
				return formResult;
			});
			try
			{
				formResultStack.Push(FormResult.None);
				ServiceConnection.CallbackHandler.FormRunModal(runRequest);
			}
			finally
			{
				action = formResultStack.Pop();
			}
		}
		else
		{
			NavFormHandle navFormHandle = (NavFormHandle)form.CreateHandle(parent);
			InvokeHandler(handler, navFormHandle, byRef);
			action = byRef.Value;
		}
		return true;
	}

	private static NavHandlerType FindPageType(NavForm form)
	{
		switch (form.MasterPage.PageProperties.PageType)
		{
		case PageType.ReportPreview:
		case PageType.ReportProcessingOnly:
		case PageType.XmlPort:
			return NavHandlerType.RequestPage;
		case PageType.FilterPage:
			return NavHandlerType.FilterPage;
		default:
			return NavHandlerType.ModalPage;
		}
	}

	internal bool TestHandleStrMenu(string option, ref int choice, string instruction)
	{
		MethodInfo methodInfo = FindHandler(NavHandlerType.StrMenu, null, throwIfNotFound: true, option);
		if (methodInfo == null)
		{
			return false;
		}
		int localChoice = choice;
		ByRef<int> byRef = new ByRef<int>(() => localChoice, (int value) =>
		{
			localChoice = value;
		});
		InvokeHandler(methodInfo, new NavText(option), byRef, new NavText(instruction));
		choice = byRef.Value;
		return true;
	}

	internal bool TestHandleMessage(string message)
	{
		MethodInfo methodInfo = FindHandler(NavHandlerType.Message, null, throwIfNotFound: true, message);
		if (methodInfo == null)
		{
			return false;
		}
		try
		{
			InvokeHandler(methodInfo, new NavText(NavDialog.ReplaceCRLFWithBackSlash(message)));
		}
		catch (NavNCLDialogException ex)
		{
			throw new NavTestWrappedException(ex.Message, ex);
		}
		return true;
	}

	internal bool TestHandleConfirm(string message, ref bool confirmation)
	{
		MethodInfo methodInfo = FindHandler(NavHandlerType.Confirm, null, throwIfNotFound: true, message);
		if (methodInfo == null)
		{
			return false;
		}
		try
		{
			bool localConfirmation = confirmation;
			ByRef<bool> byRef = new ByRef<bool>(() => localConfirmation, (bool value) =>
			{
				localConfirmation = value;
			});
			InvokeHandler(methodInfo, new NavText(NavDialog.ReplaceCRLFWithBackSlash(message)), byRef);
			confirmation = byRef.Value;
		}
		catch (NavNCLDialogException ex)
		{
			throw new NavTestWrappedException(ex.Message, ex);
		}
		return true;
	}

	internal bool TestHandleReport(NavReport report)
	{
		MethodInfo methodInfo = FindHandler(NavHandlerType.Report, report, throwIfNotFound: false);
		if (methodInfo == null)
		{
			return false;
		}
		using NavScope parent = new NavScope(NavCurrentThread.Session);
		InvokeHandler(methodInfo, report.CreateHandle(parent));
		return true;
	}

	internal bool TestHandleReportData(NavReport report, bool throwIfNoHandler)
	{
		MethodInfo methodInfo = FindHandler(NavHandlerType.ReportData, report, throwIfNoHandler);
		if (methodInfo == null)
		{
			return false;
		}
		return true;
	}

	internal bool TestHandleHyperlink(string hyperlink)
	{
		MethodInfo methodInfo = FindHandler(NavHandlerType.Hyperlink, null, throwIfNotFound: true, hyperlink);
		if (methodInfo == null)
		{
			return false;
		}
		InvokeHandler(methodInfo, new NavText(hyperlink));
		return true;
	}

	internal bool TestHandleSessionSettings(NavSessionSettings sessionSettings)
	{
		MethodInfo methodInfo = FindHandler(NavHandlerType.SessionSettings, null);
		if (methodInfo == null)
		{
			return false;
		}
		object obj = new ByRef<NavSessionSettings>(() => sessionSettings, (NavSessionSettings value) =>
		{
			sessionSettings = value;
		});
		InvokeHandler(methodInfo, obj);
		return true;
	}

	internal bool RedirectNotificationOperationToTestHandler(NavNotification notification, NavHandlerType handlerType)
	{
		MethodInfo methodInfo = FindHandler(handlerType, null, throwIfNotFound: false);
		if (methodInfo == null)
		{
			return executingTestRunner != null;
		}
		ByRef<NavNotification> byRef = new ByRef<NavNotification>(() => notification, (NavNotification value) =>
		{
			notification = value;
		});
		InvokeHandler(methodInfo, byRef);
		return true;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				ReleaseTestPageClient();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	internal void ReleaseTestPageClient()
	{
		if (testServiceConnection != null)
		{
			testServiceConnection.Dispose();
			testServiceConnection = null;
		}
		if (testClientSession != null)
		{
			testClientSession.Dispose();
			testClientSession = null;
		}
	}

	internal void AddTrap(NavTestPage navTestPage)
	{
		if (!traps.Contains(navTestPage))
		{
			traps.Add(navTestPage);
		}
	}

	internal void RemoveTrap(NavTestPage navTestPage)
	{
		traps.Remove(navTestPage);
	}

	internal NavTestPage FindTrap(int pageNo)
	{
		for (int num = traps.Count - 1; num >= 0; num--)
		{
			if (traps[num].ObjectId.ObjectNumber == pageNo)
			{
				return traps[num];
			}
		}
		return null;
	}

	internal bool HasTrap(int pageNo)
	{
		return FindTrap(pageNo) != null;
	}
}
