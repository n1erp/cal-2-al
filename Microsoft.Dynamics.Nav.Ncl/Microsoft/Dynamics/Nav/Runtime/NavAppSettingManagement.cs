using System;
using System.Data;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Utility;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavAppSettingManagement
{
	public static void RemoveApplicationSettings(NavSqlConnectionScope tenantDatabaseScope, AppId appId)
	{
		if ((WrappedComparable<Guid>)(object)appId == (WrappedComparable<Guid>)null)
		{
			throw new ArgumentNullException("appId");
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000078P", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removing application settings -- AppId:{0}", ((WrappedValue<Guid>)(object)appId).Value);
		Stopwatch stopwatch = Stopwatch.StartNew();
		DeleteApplicationSettings(tenantDatabaseScope, appId);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000078Q", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removed application settings -- AppId:{0}; Elapsed:{1}", ((WrappedValue<Guid>)(object)appId).Value, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	private static void DeleteApplicationSettings(NavSqlConnectionScope tenantDatabaseScope, AppId appId)
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(tenantDatabaseScope.Connection);
		navSqlCommand.SetCommandText("Delete FROM [NAV App Setting] WHERE [App ID] = @0 ");
		navSqlCommand.Command.Parameters.Add(0, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)appId).Value);
		navSqlCommand.PrepareStatement();
		navSqlCommand.ExecuteNonQuery();
	}
}
