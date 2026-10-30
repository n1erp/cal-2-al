using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALDatabase
{
	public static string ALUserID => NavCurrentThread.Session.User.Name;

	public static string ALCompanyName
	{
		get
		{
			NavSession session = NavCurrentThread.Session;
			if (session.IsOpen)
			{
				return session.CompanyName;
			}
			return string.Empty;
		}
	}

	public static TransactionType ALCurrentTransactionType
	{
		get
		{
			NavSession session = NavCurrentThread.Session;
			if (session.IsOpen)
			{
				return session.DataAccessSource.SessionTransactionManager.CurrentTransactionType;
			}
			return TransactionType.Browse;
		}
		set
		{
			NavSession session = NavCurrentThread.Session;
			session.CheckConnectionIsOpen();
			session.DataAccessSource.SessionTransactionManager.CurrentTransactionType = value;
		}
	}

	public static string ALSerialNumber
	{
		get
		{
			NavSession session = NavCurrentThread.Session;
			session.CheckConnectionIsOpen();
			return session.License.SerialNumber;
		}
	}

	public static bool ALLockTimeout
	{
		get
		{
			NavSession session = NavCurrentThread.Session;
			session.CheckConnectionIsOpen();
			return ((SqlTableDataProvider)session.DataAccessSource.TenantDataAccess.DataProvider).LockTimeoutEnabled;
		}
		set
		{
			NavSession session = NavCurrentThread.Session;
			session.CheckConnectionIsOpen();
			((SqlTableDataProvider)session.DataAccessSource.TenantDataAccess.DataProvider).LockTimeoutEnabled = value;
			((SqlTableDataProvider)session.DataAccessSource.AppDataAccess.DataProvider).LockTimeoutEnabled = value;
		}
	}

	public static void ALSelectLatestVersion()
	{
		NavSession session = NavCurrentThread.Session;
		session.CheckConnectionIsOpen();
		session.FlushDataCache();
	}

	public static void ALCommit()
	{
		NavSession session = NavCurrentThread.Session;
		if (session.TestExecution != null && session.TestExecution.CurrentTransactionModel == TestTransactionModel.AutoRollback)
		{
			throw new NavTestExplicitCommitNotAllowedException();
		}
		if (session.AppInstallationContext != null)
		{
			string original = "COMMIT called and skipped in extension installation context";
			original = TraceStringHelper.AppendStackTrace(original);
			session.Diagnostics.SendTraceTag("00000IR", Category.Extensions, Verbosity.Warning, PrivacyClassification.SystemMetadata, original);
		}
		else if (session.AppUpgradeContext != null)
		{
			string original2 = "COMMIT called and skipped in extension upgrade context";
			original2 = TraceStringHelper.AppendStackTrace(original2);
			session.Diagnostics.SendTraceTag("000015R", Category.Extensions, Verbosity.Warning, PrivacyClassification.SystemMetadata, original2);
		}
		else
		{
			session.CheckTryFunction("COMMIT");
			DiagnosticsResolver.GetMostSpecificInstance(session).SendTraceTag("00000UG", Category.AL, Verbosity.Verbose, PrivacyClassification.SystemMetadata, TraceStringHelper.CreateALFunctionStatementTrace(session, "COMMIT"));
			session.Commit();
		}
	}

	public static bool ALKeyGroupEnable(string groupName)
	{
		throw new NavNotSupportedException(Lang.KeyGroupNotSupported);
	}

	public static bool ALKeyGroupDisable(string groupName)
	{
		throw new NavNotSupportedException(Lang.KeyGroupNotSupported);
	}

	public static void ALCheckLicenseFile(int keyNumber)
	{
		NavSession session = NavCurrentThread.Session;
		session.CheckConnectionIsOpen();
		session.VerifyExecutePermission(new ApplicationObjectId(ObjectType.System, keyNumber));
	}

	public static bool ALKeyGroupEnabled(string groupName)
	{
		throw new NavNotSupportedException(Lang.KeyGroupNotSupported);
	}

	public static Guid ALUserSecurityId()
	{
		return NavCurrentThread.Session.User.Id;
	}

	public static bool ALSetUserPassword(DataError errorLevel, Guid userSecurityId, string password)
	{
		bool result = false;
		try
		{
			ALSetUserPassword(userSecurityId, password);
			result = true;
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return result;
	}

	public static void ALSetUserPassword(Guid userSecurityId, string password)
	{
		bool commit = false;
		UserManagement.SessionHasSuperOrSecurityPermissionsForUser(NavCurrentThread.Session, userSecurityId);
		NavCurrentThread.Session.BeginTransaction();
		try
		{
			using (NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000121))
			{
				navRecord.ALSetCurrentKey(1);
				navRecord.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
				navRecord.ALFind("-");
				navRecord.SetFieldValueSafe(2, NavType.Text, new NavText(CryptographyHelper.GenerateSaltedPasswordHashFromPassword(password, userSecurityId)));
				navRecord.ALModify();
			}
			using (NavRecord navRecord2 = new NavRecord(NavCurrentThread.Session, 2000000120))
			{
				navRecord2.ALSetCurrentKey(1);
				navRecord2.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
				navRecord2.ALFind("-");
				navRecord2.SetFieldValueSafe(8, NavType.Boolean, NavBoolean.True);
				navRecord2.ALModify();
			}
			commit = true;
		}
		catch (Exception)
		{
			commit = false;
			throw;
		}
		finally
		{
			NavCurrentThread.Session.EndTransaction(commit);
		}
	}

	public static bool ALChangeUserPassword(DataError errorLevel, string oldPassword, string newPassword)
	{
		bool result = false;
		try
		{
			ALChangeUserPassword(oldPassword, newPassword);
			result = true;
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return result;
	}

	public static void ALChangeUserPassword(string oldPassword, string newPassword)
	{
		bool commit = false;
		Guid id = NavCurrentThread.Session.User.Id;
		NavCurrentThread.Session.BeginTransaction();
		try
		{
			using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000121 }))
			{
				using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000121, SecurityFiltering.Ignored);
				navRecord.ALSetCurrentKey(1);
				navRecord.ALSetFilter(1, "%1", new NavGuid(id));
				navRecord.ALFind("-");
				string text = navRecord.GetFieldValueSafe(2, NavType.Text).ToString();
				if (text != CryptographyHelper.GenerateSaltedPasswordHashFromPassword(oldPassword, id))
				{
					throw NavNCLWrongPasswordException.Create();
				}
				navRecord.SetFieldValueSafe(2, NavType.Text, new NavText(CryptographyHelper.GenerateSaltedPasswordHashFromPassword(newPassword, id)));
				navRecord.ALModify();
			}
			using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000120 }))
			{
				using NavRecord navRecord2 = new NavRecord(NavCurrentThread.Session, 2000000120, SecurityFiltering.Ignored);
				navRecord2.ALSetCurrentKey(1);
				navRecord2.ALSetFilter(1, "%1", new NavGuid(id));
				navRecord2.ALFind("-");
				navRecord2.SetFieldValueSafe(8, NavType.Boolean, NavBoolean.False);
				navRecord2.ALModify();
			}
			commit = true;
		}
		catch (Exception)
		{
			commit = false;
			throw;
		}
		finally
		{
			NavCurrentThread.Session.EndTransaction(commit);
		}
	}

	public static string ALSid()
	{
		return ALSid(string.Empty);
	}

	public static string ALSid(string userName)
	{
		if (string.IsNullOrEmpty(userName))
		{
			return NavCurrentThread.Session.User.Sid;
		}
		try
		{
			NTAccount nTAccount = new NTAccount(userName);
			SecurityIdentifier securityIdentifier = (SecurityIdentifier)nTAccount.Translate(typeof(SecurityIdentifier));
			return securityIdentifier.ToString();
		}
		catch (IdentityNotMappedException)
		{
			return string.Empty;
		}
		catch (SystemException ex2)
		{
			if (ex2.HResult == -2146233087)
			{
				return string.Empty;
			}
			throw new NavUserNotFoundException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToFindUserSID, ex2.Message), ex2);
		}
	}

	public static string UserName(string sid)
	{
		if (string.IsNullOrEmpty(sid))
		{
			return NavCurrentThread.Session.User.Name;
		}
		try
		{
			return new SecurityIdentifier(sid).Translate(typeof(NTAccount)).Value;
		}
		catch (IdentityNotMappedException)
		{
			return sid;
		}
		catch (Exception ex2)
		{
			throw new NavUserNotFoundException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToFindUserName, ex2.Message), ex2);
		}
	}

	public static string ALTenantID()
	{
		NavSession session = NavCurrentThread.Session;
		session.CheckConnectionIsOpen();
		return session.Tenant.Id;
	}

	public static int ALSessionID()
	{
		NavSession session = NavCurrentThread.Session;
		session.CheckConnectionIsOpen();
		return session.Id;
	}

	public static int ALServiceInstanceID()
	{
		return NavEnvironment.Instance.GetServiceInstanceId();
	}

	public static bool ALCopyCompany(DataError errorLevel, string sourceName, string destinationName)
	{
		bool result = false;
		try
		{
			ALCopyCompany(sourceName, destinationName);
			result = true;
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return result;
	}

	public static void ALCopyCompany(string sourceName, string destinationName)
	{
		new NavManagementTasks(NavCurrentThread.Session).CopyCompany(sourceName, destinationName);
	}

	private static IEnumerable<string> GetIncludedCompanies(NavRecord companyRecord)
	{
		if (companyRecord.ALFindSet(DataError.TrapError))
		{
			do
			{
				yield return ((NavText)companyRecord.GetFieldValue(1)).Value;
			}
			while (companyRecord.ALNext() != 0);
		}
	}

	public static bool ALDataFileInformation(DataError errorLevel, bool showDialog, ByRef<NavText> fileName, ByRef<NavText> description, ByRef<bool> hasApplication, ByRef<bool> hasApplicationData, ByRef<bool> hasGlobalData, ByRef<NavText> tenantId, ByRef<NavDateTime> exportDate, NavRecord companyRecord)
	{
		if (companyRecord.TableID != 2000000006)
		{
			throw new NavCSideException(Lang.MustSpecifyACompanyTable);
		}
		NavSession session = NavCurrentThread.Session;
		session.GetPermissionSet(null, new ApplicationObjectId[1]
		{
			new ApplicationObjectId(ObjectType.System, 5420)
		}).VerifyPermissions(null, PermissionMask.Execute, session.LimitedUserPermissionChecker);
		Action<StreamTransfer> uploadAction = session.UploadAction;
		try
		{
			session.UploadAction = (StreamTransfer streamTransfer) =>
			{
				using BinaryReader reader = new BinaryReader(streamTransfer.Stream);
				NavSqlDataExportHeader navSqlDataExportHeader = new NavSqlDataExportHeader(reader);
				description.Value = new NavText(navSqlDataExportHeader.Description);
				hasApplication.Value = navSqlDataExportHeader.HasApplication;
				hasApplicationData.Value = navSqlDataExportHeader.HasAppDatabaseTables;
				hasGlobalData.Value = navSqlDataExportHeader.HasGlobalTables;
				exportDate.Value = NavDateTime.Create(navSqlDataExportHeader.ExportDateTime);
				tenantId.Value = new NavText(navSqlDataExportHeader.TenantId);
				NCLMetaField fieldByNo = companyRecord.MetaTable.GetFieldByNo(1);
				if (navSqlDataExportHeader.CompanyNames != null)
				{
					foreach (string companyName in navSqlDataExportHeader.CompanyNames)
					{
						companyRecord.ALInit();
						companyRecord.SetFieldValue(1, new NavText(fieldByNo.FieldDefinedLength, companyName));
						companyRecord.ALInsert();
					}
					return;
				}
			};
			string fileName2 = ((NavText)fileName).Value;
			IClientCallback clientCallback = session.ClientCallback;
			if (!clientCallback.ImportDataAction(ref fileName2, showDialog ? Lang.DatafileInformationDialogTitle : Lang.ConfirmDatafileInformationDialogTitle, showDialog))
			{
				return false;
			}
			fileName.Value = new NavText(fileName2);
			return true;
		}
		finally
		{
			session.UploadAction = uploadAction;
		}
	}

	public static bool ALImportData(DataError errorLevel, bool showDialog, ByRef<NavText> fileName, bool includeApplicationData = false, bool includeGlobalData = false, NavRecord companyRecord = null)
	{
		if (!ServerUserSettings.Instance.EnableDataExportImport.Value)
		{
			throw new NavFunctionalityNotEnabledException(string.Format(CultureInfo.CurrentCulture, Lang.ImportDataDisabled, ProductNames.ProductNameServer));
		}
		NavSession session = NavCurrentThread.Session;
		session.GetPermissionSet(null, new ApplicationObjectId[1]
		{
			new ApplicationObjectId(ObjectType.System, 5420)
		}).VerifyPermissions(null, PermissionMask.Execute, session.LimitedUserPermissionChecker);
		if (includeApplicationData && !session.Tenant.TenantSettings.AllowAppDatabaseWrite)
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.CannotImportApplicationDataTableRecords));
		}
		IEnumerable<string> companyNames = null;
		if (companyRecord != null)
		{
			if (companyRecord.TableID != 2000000006)
			{
				throw new NavCSideException(Lang.MustSpecifyACompanyTable);
			}
			companyNames = GetIncludedCompanies(companyRecord);
		}
		Action<StreamTransfer> uploadAction = NavCurrentThread.Session.UploadAction;
		try
		{
			NavCurrentThread.Session.UploadAction = (StreamTransfer streamTransfer) =>
			{
				NavSqlImportExportData.Import(streamTransfer.Stream, includeApplication: false, includeApplicationData, session.Tenant.Database, includeGlobalData, new ReadOnlyDictionary<string, bool>(companyNames.ToDictionary((string c) => c, (string c) => false)), null, 0L);
			};
			string fileName2 = ((NavText)fileName).Value;
			IClientCallback clientCallback = NavCurrentThread.Session.ClientCallback;
			if (!clientCallback.ImportDataAction(ref fileName2, showDialog ? Lang.ImportDataDialogTitle : Lang.ConfirmImportDataDialogTitle, showDialog))
			{
				return false;
			}
			fileName.Value = new NavText(fileName2);
		}
		finally
		{
			NavCurrentThread.Session.UploadAction = uploadAction;
		}
		return true;
	}

	public static bool ALExportData(DataError errorLevel, bool showDialog, ByRef<NavText> fileName, string description, bool includeApplication = false, bool includeApplicationData = false, bool includeGlobalData = false, NavRecord companyRecord = null)
	{
		if (!ServerUserSettings.Instance.EnableDataExportImport.Value)
		{
			throw new NavFunctionalityNotEnabledException(string.Format(CultureInfo.CurrentCulture, Lang.ExportDataDisabled, ProductNames.ProductNameServer));
		}
		NavSession session = NavCurrentThread.Session;
		session.GetPermissionSet(null, new ApplicationObjectId[1]
		{
			new ApplicationObjectId(ObjectType.System, 5410)
		}).VerifyPermissions(null, PermissionMask.Execute, session.LimitedUserPermissionChecker);
		ReadOnlyCollection<string> companyNames = null;
		if (companyRecord != null)
		{
			if (companyRecord.TableID != 2000000006)
			{
				throw new NavCSideException(Lang.MustSpecifyACompanyTable);
			}
			companyNames = new ReadOnlyCollection<string>(GetIncludedCompanies(companyRecord).ToArray());
		}
		try
		{
			string fileName2 = ((NavText)fileName).Value;
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			ReaderBlocksWriterStream readerBlocksWriterStream = new ReaderBlocksWriterStream(cancellationTokenSource.Token);
			try
			{
				CultureInfo sessionCulture = session.WindowsCulture;
				CultureInfo sessionUiCulture = session.Culture;
				Task task = null;
				try
				{
					task = NavTaskFactory.RunTask(() =>
					{
						Thread.CurrentThread.CurrentCulture = sessionCulture;
						Thread.CurrentThread.CurrentUICulture = sessionUiCulture;
						using (session.CreateExclusiveTenantActionScope())
						{
							NavSqlImportExportData.Export(readerBlocksWriterStream, description, includeApplication, includeApplicationData, includeGlobalData, includeAllCompanies: false, companyNames, session);
						}
					}, cancellationTokenSource.Token);
					IClientCallback clientCallback = NavCurrentThread.Session.ClientCallback;
					if (!clientCallback.ExportDataAction(readerBlocksWriterStream.Reader, ref fileName2, showDialog ? Lang.ExportDataDialogTitle : Lang.ConfirmExportDataDialogTitle, showDialog))
					{
						cancellationTokenSource.Cancel(throwOnFirstException: true);
						return false;
					}
					fileName.Value = new NavText(fileName2);
				}
				catch
				{
					cancellationTokenSource.Cancel(throwOnFirstException: true);
					throw;
				}
				finally
				{
					task?.WaitThrowingOriginalException();
				}
			}
			finally
			{
				if (readerBlocksWriterStream != null)
				{
					((IDisposable)readerBlocksWriterStream).Dispose();
				}
			}
		}
		catch (OperationCanceledException)
		{
			return false;
		}
		return true;
	}

	public static void ALRegisterTableConnection(TableConnectionType connectionType, string name, string connectionString)
	{
		NavCurrentThread.Session.TableConnectionManager.RegisterTableConnection(connectionType, name, connectionString);
	}

	public static void ALRegisterTableConnection(CompilationTarget compilationTarget, TableConnectionType connectionType, string name, string connectionString)
	{
		if (!IsRegisterTableConnectionAllowed(compilationTarget, connectionType))
		{
			string message = string.Format(CultureInfo.CurrentCulture, Lang.NotAllowedCompilationTarget_RegisterTableConnection, connectionType);
			throw new NavNCLNotAllowedForCompilationTargetException(message);
		}
		ALRegisterTableConnection(connectionType, name, connectionString);
	}

	public static void ALUnregisterTableConnection(TableConnectionType connectionType, string name)
	{
		NavCurrentThread.Session.TableConnectionManager.UnregisterTableConnection(connectionType, name);
	}

	public static void ALSetDefaultTableConnection(TableConnectionType connectionType, string name, bool scoped = false)
	{
		NavCurrentThread.Session.TableConnectionManager.SetDefaultTableConnection(connectionType, name, scoped);
	}

	public static string ALGetDefaultTableConnection(TableConnectionType connectionType)
	{
		return NavCurrentThread.Session.TableConnectionManager.GetDefaultTableConnection(connectionType);
	}

	public static bool ALHasTableConnection(TableConnectionType connectionType, string name)
	{
		return NavCurrentThread.Session.TableConnectionManager.FindTableConnection<TableConnection>(connectionType, name) != null;
	}

	private static bool IsRegisterTableConnectionAllowed(CompilationTarget compilationTarget, TableConnectionType tableConnectionType)
	{
		return compilationTarget switch
		{
			CompilationTarget.Extension => tableConnectionType == TableConnectionType.CRM, 
			CompilationTarget.Internal => true, 
			_ => false, 
		};
	}
}
