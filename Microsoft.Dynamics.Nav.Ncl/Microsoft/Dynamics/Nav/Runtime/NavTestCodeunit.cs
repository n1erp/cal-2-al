using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavTestCodeunit : NavCodeunit
{
	internal class TestMethodComparer : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			MemberInfo memberInfo = (MemberInfo)x;
			MemberInfo memberInfo2 = (MemberInfo)y;
			if (memberInfo == null || memberInfo2 == null)
			{
				throw new ArgumentException("The object have to be of type MethodInfo");
			}
			object[] customAttributes = memberInfo.GetCustomAttributes(typeof(NavTestAttribute), inherit: false);
			NavTestAttribute navTestAttribute = null;
			if (customAttributes.Length != 0)
			{
				navTestAttribute = (NavTestAttribute)customAttributes[0];
			}
			if (navTestAttribute == null)
			{
				return 1;
			}
			object[] customAttributes2 = memberInfo2.GetCustomAttributes(typeof(NavTestAttribute), inherit: false);
			NavTestAttribute navTestAttribute2 = null;
			if (customAttributes2.Length != 0)
			{
				navTestAttribute2 = (NavTestAttribute)customAttributes2[0];
			}
			if (navTestAttribute2 == null)
			{
				return -1;
			}
			if (navTestAttribute.TestMethodNo < navTestAttribute2.TestMethodNo)
			{
				return -1;
			}
			if (navTestAttribute.TestMethodNo > navTestAttribute2.TestMethodNo)
			{
				return 1;
			}
			return 0;
		}
	}

	public override CodeunitSubType SubType => CodeunitSubType.Test;

	protected NavTestCodeunit(ITreeObject parent, int objectId)
		: this(parent, objectId, 0)
	{
	}

	protected NavTestCodeunit(ITreeObject parent, int objectId, int tableId)
		: base(parent, objectId, tableId)
	{
		if (tableId != 0)
		{
			NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(tableId);
			throw NavNCLTestCodeUnitMethodArgumentException.CreateNotSupportedRecord(NavCurrentThread.Session.RuntimeLanguage, ObjectId.ObjectNumber, NavCurrentThread.Session.GetObjectName(ObjectId), tableId, metaTableById.TableName);
		}
	}

	protected internal override bool DoRun(DataError errorLevel, NavRecord record)
	{
		bool flag = true;
		int num = 0;
		NavSession navSession = NavCurrentThread.Session;
		NavTestExecution testExecution = navSession.TestExecution;
		try
		{
			if (record != null)
			{
				throw NavNCLTestCodeUnitMethodArgumentException.CreateNotSupportedRecord(NavCurrentThread.Session.RuntimeLanguage, ObjectId.ObjectNumber, ObjectName, record.TableID, record.MetaTable.TableName);
			}
			bool flag2 = false;
			NavSqlConnectionScope navSqlConnectionScope = null;
			NavSqlConnectionScope navSqlConnectionScope2 = null;
			string text = null;
			testExecution.EnterTestCodeunit(this);
			try
			{
				using (new NavOpenDialogTracking())
				{
					if (!navSession.AccessLock.IsCurrentThreadExecuting)
					{
						navSession.AccessLock.BeginExclusiveExecution();
						flag2 = true;
					}
					while (navSession.IsTransactionActive())
					{
						num++;
						navSession.EndTransaction(commit: true);
					}
					text = navSession.ApplicationAreas;
					if (!testExecution.CommitTestCodeunits)
					{
						navSqlConnectionScope = NavSqlConnectionScope.CreateRootTestScope(Session.Database);
						navSqlConnectionScope2 = NavSqlConnectionScope.CreateRootTestScope(Session.AppDatabase);
					}
					navSession.BeginTransaction();
					if (!testExecution.BeforeTestRun(null, null))
					{
						return false;
					}
					navSession.Diagnostics.SendTraceTag("00001UQ", Category.Runtime, Verbosity.Normal, "Starting test run: name:'{0}', CurrentCulture:{1}", DiagnosticParameter.SystemMetadata(GetType()), DiagnosticParameter.SystemMetadata(Thread.CurrentThread.CurrentCulture));
					MethodInfo method = GetType().GetMethod("OnRun", BindingFlags.Instance | BindingFlags.NonPublic);
					if (method.DeclaringType == GetType())
					{
						if (!testExecution.BeforeTestRun(method, null))
						{
							testExecution.AfterTestRun(success: false);
							return false;
						}
						bool flag3 = false;
						try
						{
							OnRun(null);
							navSession.Commit();
							flag3 = true;
						}
						catch (Exception ex)
						{
							navSession.Rollback();
							MarkExceptions(ex);
						}
						testExecution.AfterTestRun(flag3);
						string content = method.DeclaringType.ToString() + "." + method.Name;
						navSession.Diagnostics.SendTraceTag("00001UR", Category.Runtime, Verbosity.Normal, "Test result: name:'{0}', status:{1}, CurrentCulture:{2}", DiagnosticParameter.SystemMetadata(content), DiagnosticParameter.SystemMetadata(flag3), DiagnosticParameter.SystemMetadata(Thread.CurrentThread.CurrentCulture));
						if (!flag3)
						{
							testExecution.AfterTestRun(success: false);
							return false;
						}
					}
					MethodInfo[] methods = GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
					Array.Sort(methods, new TestMethodComparer());
					MethodInfo[] array = methods;
					foreach (MethodInfo methodInfo in array)
					{
						object[] customAttributes = methodInfo.GetCustomAttributes(typeof(NavTestAttribute), inherit: false);
						if (customAttributes.Length == 0)
						{
							break;
						}
						NavTestAttribute navTestAttribute = (NavTestAttribute)customAttributes[0];
						if (testExecution.BeforeTestRun(methodInfo, navTestAttribute))
						{
							bool flag4 = ExecuteTestMethod(methodInfo, navTestAttribute.TransactionModel);
							if (!flag4)
							{
								flag = false;
							}
							testExecution.AfterTestRun(flag4);
						}
						navSession.ApplicationAreas = text;
					}
				}
				testExecution.AfterTestRun(flag);
				return flag;
			}
			finally
			{
				if (flag2)
				{
					navSession.AccessLock.EndExclusiveExecution();
				}
				navSession.EndTransaction(commit: false);
				if (text != null)
				{
					navSession.ApplicationAreas = text;
				}
				navSession.ClearPermissionSystemActivatedCache();
				testExecution.LeaveTestCodeunit();
				navSqlConnectionScope?.Dispose();
				navSqlConnectionScope2?.Dispose();
			}
		}
		finally
		{
			testExecution.ReleaseTestPageClient();
			while (num > 0)
			{
				navSession.BeginTransaction();
				num--;
			}
		}
	}

	private bool ExecuteTestMethod(MethodInfo method, TestTransactionModel transactionModel)
	{
		NavSession navSession = NavCurrentThread.Session;
		NavTestExecution testExecution = navSession.TestExecution;
		string content = method.DeclaringType?.ToString() + "." + method.Name;
		using (navSession.Diagnostics.CreateTraceScope("00000C4", DiagnosticParameter.SystemMetadata(content), MonitorType.None, Category.Test))
		{
			NavSqlConnectionScope navSqlConnectionScope = null;
			NavSqlConnectionScope navSqlConnectionScope2 = null;
			bool flag = false;
			try
			{
				if (!testExecution.CommitTestFunctions)
				{
					navSqlConnectionScope = NavSqlConnectionScope.CreateRootTestScope(Session.Database);
					navSqlConnectionScope2 = NavSqlConnectionScope.CreateRootTestScope(Session.AppDatabase);
				}
				int num = 0;
				try
				{
					if (transactionModel == TestTransactionModel.None)
					{
						while (navSession.IsTransactionActive())
						{
							num++;
							navSession.EndTransaction(commit: false);
						}
					}
					testExecution.CurrentTransactionModel = transactionModel;
					testExecution.InitHandlers();
					method.Invoke(this, new object[0]);
					testExecution.TestHandlers();
					switch (transactionModel)
					{
					case TestTransactionModel.AutoCommit:
						navSession.Commit();
						break;
					case TestTransactionModel.AutoRollback:
						navSession.Rollback();
						break;
					}
					flag = true;
				}
				finally
				{
					while (num > 0)
					{
						num--;
						navSession.BeginTransaction();
					}
					testExecution.CurrentTransactionModel = TestTransactionModel.AutoCommit;
				}
			}
			catch (Exception ex)
			{
				if (navSession.IsTransactionActive())
				{
					navSession.Rollback();
				}
				MarkExceptions(ex);
			}
			finally
			{
				navSqlConnectionScope2?.Dispose();
				navSqlConnectionScope?.Dispose();
			}
			navSession.Diagnostics.SendTraceTag("00001US", Category.Runtime, Verbosity.Normal, "Test result: name:'{0}', status:{1}, culture:{2}", DiagnosticParameter.SystemMetadata(content), DiagnosticParameter.SystemMetadata(flag), DiagnosticParameter.SystemMetadata(Thread.CurrentThread.CurrentCulture));
			return flag;
		}
	}

	private static void MarkExceptions(Exception ex)
	{
		if (ex is TargetInvocationException)
		{
			ex = ex.InnerException;
		}
		if (ex is NavBaseException)
		{
			if (string.CompareOrdinal(ex.Message, Lang.AssertError) == 0)
			{
				string message = ex.ToString().Replace('\\', '/');
				RegisterException(message, ex);
			}
		}
		else
		{
			string message = ex.ToString().Replace('\\', '/');
			RegisterException(Lang.UnexpectedClrExceptionThrown + ": " + message, ex);
		}
	}

	private static void RegisterException(string message, Exception innerException)
	{
		NavTestCodeUnitException lastException = new NavTestCodeUnitException(message, innerException);
		if (NavCurrentThread.Session != null)
		{
			NavCurrentThread.Session.LastException = lastException;
		}
	}
}
