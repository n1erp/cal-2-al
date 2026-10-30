using System;
using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class IsolatedStorageRepository
{
	public const int KeyLength = 200;

	public static bool Set(DataError dataError, NavGuid appId, DataScope dataScope, string companyName, NavGuid userId, string key, string value, EncryptionStatus encryptionStatus)
	{
		ValidateKey(key);
		if (value == null)
		{
			throw new NavNCLArgumentException("value");
		}
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000107 }))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000107, SecurityFiltering.Ignored);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000024Q", Category.Runtime, Verbosity.Verbose, "Inserting or setting the value of a isolated storage record -- DataScope:{0}; AppId:{1}; CompanyIs:{2}; UserId:{3}; Name:{4}; EncryptionStatus:{5};", DiagnosticParameter.SystemMetadata(dataScope.ToString()), DiagnosticParameter.SystemMetadata(appId), DiagnosticParameter.Oii(companyName), DiagnosticParameter.Euii(userId), DiagnosticParameter.Oii(key), DiagnosticParameter.SystemMetadata(encryptionStatus));
			if (!navRecord.ALGet(DataError.TrapError, appId, new NavEnumDataScope(dataScope), new NavText(30, companyName), userId, NavText.Create(key)))
			{
				navRecord.SetFieldValueSafe(1, NavType.GUID, appId);
				navRecord.SetFieldValueSafe(2, NavType.Option, NavOption.Create(navRecord.MetaTable.GetFieldByNo(2).FieldOptionMetadata, (int)dataScope));
				navRecord.SetFieldValueSafe(3, NavType.Text, new NavText(30, companyName));
				navRecord.SetFieldValueSafe(4, NavType.GUID, userId);
				navRecord.SetFieldValueSafe(5, NavType.Text, NavText.Create(key));
				navRecord.SetFieldValueSafe(7, NavType.Option, NavOption.Create(navRecord.MetaTable.GetFieldByNo(7).FieldOptionMetadata, (int)encryptionStatus));
				if (!navRecord.ALInsert(dataError))
				{
					return false;
				}
			}
			navRecord.SetFieldValueSafe(7, NavType.Option, NavOption.Create(navRecord.MetaTable.GetFieldByNo(7).FieldOptionMetadata, (int)encryptionStatus));
			navRecord.SetFieldValueSafe(6, NavType.BLOB, new NavBLOB(Encoding.UTF8, value));
			return navRecord.ALModify(dataError);
		}
	}

	public static (bool OperationResult, EncryptionStatus EncryptionStatus) Get(DataError dataError, NavGuid appId, DataScope dataScope, string companyName, NavGuid userId, string key, ByRef<NavText> value)
	{
		ValidateKey(key);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00007MT", Category.Runtime, Verbosity.Verbose, "Retrieving the value of a Isolated Storage record -- DataScope:{0}; AppId:{1}; CompanyIs:{2}; UserId:{3}; Name:{4};", DiagnosticParameter.SystemMetadata(dataScope.ToString()), DiagnosticParameter.SystemMetadata(appId), DiagnosticParameter.Oii(companyName), DiagnosticParameter.Euii(userId), DiagnosticParameter.Oii(key));
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000107 }))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000107, SecurityFiltering.Ignored);
			if (!navRecord.ALGet(dataError, appId, new NavEnumDataScope(dataScope), new NavText(30, companyName), userId, NavText.Create(key)))
			{
				return (OperationResult: false, EncryptionStatus: EncryptionStatus.PlainText);
			}
			navRecord.ALCalcFields(6);
			if (!(navRecord.GetFieldValueSafe(6, NavType.BLOB) is NavBLOB navBLOB))
			{
				throw new NavALException(Lang.IsolatedStorageNotFound);
			}
			byte[] bytes = navBLOB.GetBytes();
			EncryptionStatus value2 = (EncryptionStatus)((NavOption)navRecord.GetFieldValue(7)).Value;
			value.Value = new NavText(value.Value.MaxLength, Encoding.UTF8.GetString(bytes));
			return (OperationResult: true, EncryptionStatus: value2);
		}
	}

	public static bool Contains(NavGuid appId, DataScope dataScope, string companyName, NavGuid userId, string key)
	{
		ValidateKey(key);
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000107 }))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000107, SecurityFiltering.Ignored);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000024V", Category.Runtime, Verbosity.Verbose, "Checking if the value exist in Isolated Storage  -- DataScope:{0}; AppId:{1}; CompanyIs:{2}; UserId:{3}; Name:{4};", DiagnosticParameter.SystemMetadata(dataScope.ToString()), DiagnosticParameter.SystemMetadata(appId), DiagnosticParameter.Oii(companyName), DiagnosticParameter.Euii(userId), DiagnosticParameter.Oii(key));
			return navRecord.ALGet(DataError.TrapError, appId, new NavEnumDataScope(dataScope), new NavText(30, companyName), userId, NavText.Create(key));
		}
	}

	public static bool Delete(DataError dataError, NavGuid appId, DataScope dataScope, string companyName, NavGuid userId, string key)
	{
		ValidateKey(key);
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000107 }))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000107, SecurityFiltering.Ignored);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000024S", Category.Runtime, Verbosity.Verbose, "Deleting the value of a Isolated Storage -- DataScope:{0}; AppId:{1}; CompanyIs:{2}; UserId:{3}; Name:{4};", DiagnosticParameter.SystemMetadata(dataScope.ToString()), DiagnosticParameter.SystemMetadata(appId), DiagnosticParameter.Oii(companyName), DiagnosticParameter.Euii(userId), DiagnosticParameter.Oii(key));
			navRecord.SetFieldValueSafe(1, NavType.GUID, appId);
			navRecord.SetFieldValueSafe(2, NavType.Option, NavOption.Create(navRecord.MetaTable.GetFieldByNo(2).FieldOptionMetadata, (int)dataScope));
			navRecord.SetFieldValueSafe(3, NavType.Text, new NavText(30, companyName));
			navRecord.SetFieldValueSafe(4, NavType.GUID, userId);
			navRecord.SetFieldValueSafe(5, NavType.Text, NavText.Create(key));
			return navRecord.ALDelete(dataError);
		}
	}

	public static void DeleteApplicationData(NavSqlConnectionScope conScope, Guid appId)
	{
		if (appId == Guid.Empty)
		{
			throw new ArgumentNullException("appId");
		}
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText("Delete FROM [Isolated Storage] WHERE [App Id] = @0 ");
		navSqlCommand.Command.Parameters.Add(0, SqlDbType.UniqueIdentifier, appId);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000024T", Category.Runtime, Verbosity.Verbose, "DeleteApplicationData- deleting all the values for an applicatoin from Isolated Storage  -- AppId:{1};", DiagnosticParameter.SystemMetadata(appId));
		navSqlCommand.PrepareStatement();
		navSqlCommand.ExecuteNonQuery();
	}

	private static void ValidateKey(string key)
	{
		if (key == null)
		{
			throw new NavNCLArgumentException("key");
		}
		if (key.Length > 200)
		{
			throw new NavALException(string.Format(CultureInfo.CurrentCulture, Lang.IsolatedStorageMaxKeyLength, 200));
		}
	}

	internal static void DecryptData()
	{
		ChangeEncrptionStatus(EncryptionStatus.Encrypted, EncryptionStatus.PendingForEncryption);
	}

	internal static void EncryptPendingData()
	{
		ChangeEncrptionStatus(EncryptionStatus.PendingForEncryption, EncryptionStatus.Encrypted);
	}

	private static void ChangeEncrptionStatus(EncryptionStatus fromEncryptionStatus, EncryptionStatus toEncryptionStatus)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006ZH", Category.Runtime, Verbosity.Verbose, "Changing the encryptionStatus and Encrypt/Decrypt from {0} to {1}-- ", DiagnosticParameter.ObjectMetadata(fromEncryptionStatus), DiagnosticParameter.ObjectMetadata(toEncryptionStatus));
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000107 }))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000107, SecurityFiltering.Ignored);
			navRecord.ALSetRangeSafe(7, NavType.Option, NavOption.Create(navRecord.MetaTable.GetFieldByNo(7).FieldOptionMetadata, (int)fromEncryptionStatus));
			if (!navRecord.ALFindSet(DataError.TrapError, forUpdate: true))
			{
				return;
			}
			navRecord.ALCalcFields(6);
			do
			{
				if (!(navRecord.GetFieldValueSafe(6, NavType.BLOB) is NavBLOB navBLOB))
				{
					throw new NavALException(Lang.IsolatedStorageNotFound);
				}
				if (fromEncryptionStatus == EncryptionStatus.Encrypted && toEncryptionStatus == EncryptionStatus.PendingForEncryption)
				{
					navRecord.SetFieldValueSafe(6, NavType.BLOB, new NavBLOB(Encoding.UTF8, ALSystemEncryption.ALDecrypt(Encoding.UTF8.GetString(navBLOB.GetBytes()))));
				}
				else if (fromEncryptionStatus == EncryptionStatus.PendingForEncryption && toEncryptionStatus == EncryptionStatus.Encrypted)
				{
					navRecord.SetFieldValueSafe(6, NavType.BLOB, new NavBLOB(Encoding.UTF8, ALSystemEncryption.ALEncrypt(Encoding.UTF8.GetString(navBLOB.GetBytes()))));
				}
				navRecord.SetFieldValueSafe(7, NavType.Option, NavOption.Create(navRecord.MetaTable.GetFieldByNo(7).FieldOptionMetadata, (int)toEncryptionStatus));
				navRecord.ALModify(DataError.TrapError, runApplicationTrigger: false);
			}
			while (navRecord.ALNext() != 0);
		}
	}
}
