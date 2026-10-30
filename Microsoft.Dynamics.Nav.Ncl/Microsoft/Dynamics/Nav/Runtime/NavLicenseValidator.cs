using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavLicenseValidator
{
	private readonly HashSet<Guid> activeWebServiceUsers = new HashSet<Guid>();

	private DateTime activeWebServiceUsersTimestamp = DateTime.UtcNow;

	private static readonly TimeSpan ActiveWebServiceUsersTTL = TimeSpan.FromSeconds(20.0);

	private static readonly TimeSpan WebServiceSessionTime = TimeSpan.FromHours(2.0);

	private static readonly TimeSpan LimitedUserWebServiceSessionTime = TimeSpan.FromMinutes(15.0);

	private static readonly string ClientTypeFilterWithoutWebService = 0 + "," + 7 + "," + 1 + "," + 9 + "," + 10 + "," + 11;

	private static readonly string ClientTypeFilter = ClientTypeFilterWithoutWebService + "," + 2;

	private static readonly string UserEnabledString = 0.ToString(CultureInfo.InvariantCulture);

	private const int CurrentUserInActiveSessionConnectionCount = -1;

	public void ValidateLicenseForNewSession(NavSession session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		switch (session.User.LicenseType)
		{
		case LicenseType.External:
			ValidateLicenseForExternalTypeSession(session);
			return;
		case LicenseType.DeviceOnly:
			ValidateLicenseForDeviceTypeSession(session);
			return;
		}
		switch (session.ClientConnectionType)
		{
		case ConnectionType.WebServiceClient:
			ValidateLicenseForNewWebServiceSession(session);
			break;
		case ConnectionType.RoleTailoredClient:
		case ConnectionType.SharePoint:
		case ConnectionType.WebClient:
			ValidateLicenseForClientSession(session);
			break;
		}
	}

	public bool CheckIfLicenseNamedUserLimitsAreOk(NavSession session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		INavLicense license = session.License;
		if (license.LicenseModel != LicenseModel.NamedUsers)
		{
			return true;
		}
		if (GetFullUsersCreatedInUserTableCount(session) <= license.NamedUsersNumberOfFullUsers)
		{
			return GetUsersCreatedInUserTableCount(session, LicenseType.Limited) <= license.NamedUsersNumberOfLimitedUsers;
		}
		return false;
	}

	private int GetFullUsersCreatedInUserTableCount(NavSession session)
	{
		return GetUsersCreatedInUserTableCount(session, LicenseType.Full) + GetUsersCreatedInUserTableCount(session, LicenseType.ExternalAccountant) + GetUsersCreatedInUserTableCount(session, LicenseType.ExternalAdmin);
	}

	public bool CheckIfLicenseDeviceLimitsAreOk(NavSession session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		INavLicense license = session.License;
		if (GetDevicesCreatedInDeviceTableCount(session, DeviceType.Full) <= license.NumberOfFullDevices && GetDevicesCreatedInDeviceTableCount(session, DeviceType.Limited) <= license.NumberOfLimitedDevices && GetDevicesCreatedInDeviceTableCount(session, DeviceType.Isv) <= license.NumberOfIsvDevices)
		{
			return GetDevicesCreatedInDeviceTableCount(session, DeviceType.IsvFunctional) <= license.NumberOfIsvFunctionalDevices;
		}
		return false;
	}

	public int GetDevicesCreatedInDeviceTableCount(NavSession session, DeviceType? deviceType = null)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		NCLMetaTable metaTableById = session.NCLMetadata.GetMetaTableById(2000000130);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(4);
		using (new MaximizedPermissionScope(session, new int[1] { 2000000130 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000130, SecurityFiltering.Ignored);
			navRecord.SetFilter(fieldByNo2, NavBoolean.True.Value.ToString(CultureInfo.CurrentCulture));
			if (deviceType.HasValue)
			{
				navRecord.SetFilter(fieldByNo, ((int)deviceType.Value).ToString(CultureInfo.CurrentCulture));
			}
			return navRecord.ALCount;
		}
	}

	public int GetUsersCreatedInUserTableCount(NavSession session, LicenseType? licenseType = null)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		NCLMetaTable metaTableById = session.NCLMetadata.GetMetaTableById(2000000120);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(10);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(4);
		NCLMetaField fieldByNo3 = metaTableById.GetFieldByNo(5);
		using (new MaximizedPermissionScope(session, new int[1] { 2000000120 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000120, SecurityFiltering.Ignored);
			navRecord.SetFilter(fieldByNo2, 0.ToString(CultureInfo.CurrentCulture));
			navRecord.SetFilter(fieldByNo3, "''|>=%1", NavDateTime.Now);
			if (licenseType.HasValue)
			{
				navRecord.SetFilter(fieldByNo, ((int)licenseType.Value).ToString(CultureInfo.CurrentCulture));
			}
			return navRecord.ALCount;
		}
	}

	public int GetLicenseTypeConnectionCount(NavSession session, LicenseType licenseType, bool excludeCurrentUserFromCount = false)
	{
		return InternalLicenseTypeConnectionCount(session, licenseType, checkCurrentUserInActiveSession: false, excludeCurrentUserFromCount);
	}

	private int InternalLicenseTypeConnectionCount(NavSession session, LicenseType licenseType, bool checkCurrentUserInActiveSession, bool excludeCurrentUserFromCount)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		int result = 0;
		string text = "IF (SELECT TOP 1 1 FROM {0}[User] U WHERE U.\"$companyId\" = @4 AND U.[State] = " + UserEnabledString + ") IS NULL   SELECT COUNT(*) FROM {0}[Active Session] A WHERE A.\"$companyId\" = @4 AND  A.[Client Type] IN (" + ClientTypeFilterWithoutWebService + ") ELSE ";
		if (checkCurrentUserInActiveSession)
		{
			text = text + "  IF (SELECT TOP 1 1 FROM {0}[Active Session] A WHERE  A.\"$companyId\" = @4 AND A.[User SID] = @3 AND A.[Client Type] IN (" + ClientTypeFilter + ") AND (A.[Session ID] > 0 OR A.[Login Datetime] > @2)) IS NOT NULL " + string.Format(CultureInfo.InvariantCulture, "    SELECT {0} ", -1) + "  ELSE ";
		}
		text = text + "    SELECT Count(*) FROM {0}[User] U WHERE U.\"$companyId\" = @4 AND U.[License Type] = @1 AND U.[User Security ID] IN       (SELECT A.[User SID] from {0}[Active Session] A        WHERE  A.\"$companyId\" = @4 AND  A.[Client Type] IN (" + ClientTypeFilter + ")          AND (A.[Session ID] > 0 OR A.[Login Datetime] > @2) ";
		if (excludeCurrentUserFromCount)
		{
			text += " AND  A.[User SID] != @3";
		}
		text += ")";
		CompanyId? companyId = session.Tenant.Database.GetCompanyIdForSystemTables(2000000110);
		if (!companyId.HasValue)
		{
			text = text.Replace("A.\"$companyId\" = @4 AND", string.Empty).Replace("U.\"$companyId\" = @4 AND", string.Empty);
		}
		ExecuteDirectSQL(session.Database, text, (NavSqlCommand command) => result = (int)command.ExecuteScalar(), (NavSqlCommand command) =>
		{
			command.Command.Parameters.AddRange(new SqlParameter[3]
			{
				new SqlParameter(NavSqlStatementHelper.ParameterName(1), SqlDbType.Int),
				new SqlParameter(NavSqlStatementHelper.ParameterName(2), SqlDbType.DateTime),
				new SqlParameter(NavSqlStatementHelper.ParameterName(3), SqlDbType.UniqueIdentifier)
			});
			if (companyId.HasValue)
			{
				command.Command.Parameters.Add(new SqlParameter(NavSqlStatementHelper.ParameterName(4), SqlDbType.Int));
			}
		}, (NavSqlCommand command) =>
		{
			command.Command.Parameters[0].Value = (int)licenseType;
			TimeSpan timeSpan = ((licenseType != LicenseType.Limited) ? WebServiceSessionTime : LimitedUserWebServiceSessionTime);
			command.Command.Parameters[1].Value = DateTime.UtcNow - timeSpan;
			command.Command.Parameters[2].Value = session.User.Id;
			if (companyId.HasValue)
			{
				command.Command.Parameters[3].Value = companyId.Value;
			}
		});
		return result;
	}

	public void ClearActiveWebUserCache()
	{
		activeWebServiceUsersTimestamp = DateTime.MinValue;
	}

	public bool CheckIfUserIsAnActiveWebServiceUser(NavUser user)
	{
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		lock (activeWebServiceUsers)
		{
			if (DateTime.UtcNow - activeWebServiceUsersTimestamp > ActiveWebServiceUsersTTL)
			{
				activeWebServiceUsers.Clear();
				activeWebServiceUsersTimestamp = DateTime.UtcNow;
				return false;
			}
			return activeWebServiceUsers.Contains(user.Id);
		}
	}

	public bool CheckIfSessionIsAnActiveWebServiceSession(NavSession session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (session.ClientConnectionType == ConnectionType.WebServiceClient)
		{
			return CheckIfUserIsAnActiveWebServiceUser(session.User);
		}
		return false;
	}

	public void AddUserToActiveWebServiceUsers(NavUser user)
	{
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		lock (activeWebServiceUsers)
		{
			activeWebServiceUsers.Add(user.Id);
		}
	}

	public static bool IsFullOrFullLikeUser(LicenseType licenseType)
	{
		if (licenseType != LicenseType.Full && licenseType != LicenseType.ExternalAccountant)
		{
			return licenseType == LicenseType.ExternalAdmin;
		}
		return true;
	}

	private void ValidateLicenseForExternalTypeSession(NavSession session)
	{
		switch (session.ClientConnectionType)
		{
		case ConnectionType.WebServiceClient:
			if (ServerUserSettings.Instance.CacheActiveExternalUsersForWebServiceRequests.Value)
			{
				AddUserToActiveWebServiceUsers(session.User);
			}
			break;
		case ConnectionType.UnknownClient:
		case ConnectionType.Background:
			break;
		default:
			throw session.Diagnostics.SendExceptionTag("00000I8", Category.License, new NavLicenseViolationException("A user with License Type 'External User' cannot log in using this client type."), Verbosity.Warning);
		}
	}

	private void ValidateLicenseForDeviceTypeSession(NavSession session)
	{
		INavLicense license = session.License;
		int devicesCreatedInDeviceTableCount = GetDevicesCreatedInDeviceTableCount(session);
		int val = license.NumberOfLimitedDevices + license.NumberOfFullDevices + license.NumberOfIsvDevices + license.NumberOfIsvFunctionalDevices;
		int maxDeviceConnectionCount = Math.Min(devicesCreatedInDeviceTableCount, val);
		ValidateDeviceHasAnActiveConnectionOrThereIsRoomForOneMore(session, maxDeviceConnectionCount);
		if (ServerUserSettings.Instance.CacheActiveDeviceOnlyUsersForWebServiceRequests.Value && session.ClientConnectionType == ConnectionType.WebServiceClient)
		{
			AddUserToActiveWebServiceUsers(session.User);
		}
	}

	private void ValidateDeviceHasAnActiveConnectionOrThereIsRoomForOneMore(NavSession session, int maxDeviceConnectionCount)
	{
		if (!HasDeviceAnActiveConnectionOrThereIsRoomForOneMore(session, maxDeviceConnectionCount))
		{
			SessionEventTableHandler.ForceCloseDeadSessions(session.Database);
			if (!HasDeviceAnActiveConnectionOrThereIsRoomForOneMore(session, maxDeviceConnectionCount))
			{
				throw session.Diagnostics.SendExceptionTag("00000I9", Category.License, new NavLicenseViolationException(Lang.DeviceOnlyConnectionsLicenseViolation), Verbosity.Warning);
			}
		}
	}

	private bool HasDeviceAnActiveConnectionOrThereIsRoomForOneMore(NavSession session, int maxDeviceConnectionCount)
	{
		int num = InternalLicenseTypeConnectionCount(session, session.User.LicenseType, checkCurrentUserInActiveSession: true, excludeCurrentUserFromCount: true);
		if (num == -1)
		{
			return true;
		}
		if (num >= maxDeviceConnectionCount)
		{
			return false;
		}
		return true;
	}

	private void ValidateLicenseForNewWebServiceSession(NavSession session)
	{
		if (!CheckIfUserIsAnActiveWebServiceUser(session.User))
		{
			ValidateUserHasAnActiveConnectionOrThereIsRoomForOneMore(session);
			AddUserToActiveWebServiceUsers(session.User);
		}
	}

	private void ValidateLicenseForClientSession(NavSession session)
	{
		ValidateUserHasAnActiveConnectionOrThereIsRoomForOneMore(session);
	}

	private void ValidateUserHasAnActiveConnectionOrThereIsRoomForOneMore(NavSession session)
	{
		if (session.License.LicenseModel != LicenseModel.NamedUsers && !HasUsersAnActiveConnectionOrThereIsRoomForOneMoreCountConnectionRule(session))
		{
			SessionEventTableHandler.ForceCloseDeadSessions(session.Database);
			if (!HasUsersAnActiveConnectionOrThereIsRoomForOneMoreCountConnectionRule(session))
			{
				throw session.Diagnostics.SendExceptionTag("00000IA", Category.License, new NavLicenseViolationException(Lang.ConcurrentConnectionsLicenseViolation), Verbosity.Warning);
			}
		}
	}

	private bool HasUsersAnActiveConnectionOrThereIsRoomForOneMoreCountConnectionRule(NavSession session)
	{
		int num = InternalLicenseTypeConnectionCount(session, session.User.LicenseType, checkCurrentUserInActiveSession: true, excludeCurrentUserFromCount: true);
		if (num == -1)
		{
			return true;
		}
		int num2 = ((session.User.LicenseType == LicenseType.Limited) ? session.License.ConcurrentConnectionsNumberOfLimitedUsers : session.License.ConcurrentConnectionsNumberOfFullUsers);
		return num < num2;
	}

	private void ExecuteDirectSQL<T>(NavDatabase database, string sqlCommand, Func<NavSqlCommand, T> doWithResultFunc, Action<NavSqlCommand> addParametersFunc = null, Action<NavSqlCommand> populateParametersFunc = null)
	{
		sqlCommand = ReplaceCurlyZeroWithDatabaseAndSchemaQualification(database, sqlCommand);
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Browse);
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey(sqlCommand, database), () => sqlCommand, addParametersFunc ?? new Action<NavSqlCommand>(NullAction), populateParametersFunc ?? new Action<NavSqlCommand>(NullAction), doWithResultFunc);
	}

	private string ReplaceCurlyZeroWithDatabaseAndSchemaQualification(NavDatabase database, string sqlCommand)
	{
		StringBuilder stringBuilder = new StringBuilder();
		NavSqlStatementHelper.AppendDatabaseAndSchemaQualification(stringBuilder, database, "dbo");
		sqlCommand = string.Format(CultureInfo.InvariantCulture, sqlCommand, stringBuilder);
		return sqlCommand;
	}

	private void NullAction<T>(T param)
	{
	}
}
