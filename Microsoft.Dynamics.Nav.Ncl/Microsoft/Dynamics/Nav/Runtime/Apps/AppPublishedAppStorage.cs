using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class AppPublishedAppStorage : TableStorage
{
	private const string TableName = "NAV App Published App";

	public AppPublishedAppStorage(NavDatabase database, NavSqlConnectionScope connectionScope)
		: base("NAV App Published App", database, connectionScope)
	{
	}

	public void InsertOrUpdate(NavAppRuntimeMetadata runtimeMetadata)
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(ConnectionScope.Connection);
		navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "IF (OBJECT_ID('{0}') IS NOT NULL) UPDATE [{0}] SET [Package ID] = @5, [Name] = @6, [Publisher] = @7, [Content Hash] = @8, [Hash Algorithm] = @9 WHERE [App ID] = @0 AND [Version Major] = @1 AND [Version Minor] = @2 AND [Version Build] = @3 AND [Version Revision] = @4 {1} IF @@ROWCOUNT = 0 INSERT INTO [{0}] ([App ID], [Version Major], [Version Minor], [Version Build], [Version Revision], [Package ID], [Name], [Publisher], [Content Hash], [Hash Algorithm]{2}) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9{3})", "NAV App Published App", Database.IsSharedTenantSchema ? "AND \"$companyId\" = @10" : string.Empty, Database.IsSharedTenantSchema ? ", \"$companyId\"" : string.Empty, Database.IsSharedTenantSchema ? ", @10" : string.Empty));
		navSqlCommand.Command.Parameters.Add(0, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)runtimeMetadata.AppId).Value);
		navSqlCommand.Command.Parameters.Add(1, SqlDbType.Int, runtimeMetadata.Version.Major);
		navSqlCommand.Command.Parameters.Add(2, SqlDbType.Int, runtimeMetadata.Version.Minor);
		navSqlCommand.Command.Parameters.Add(3, SqlDbType.Int, runtimeMetadata.Version.Build);
		navSqlCommand.Command.Parameters.Add(4, SqlDbType.Int, runtimeMetadata.Version.Revision);
		navSqlCommand.Command.Parameters.Add(5, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)runtimeMetadata.PackageId).Value);
		navSqlCommand.Command.Parameters.Add(6, SqlDbType.NVarChar, 500, runtimeMetadata.Name);
		navSqlCommand.Command.Parameters.Add(7, SqlDbType.NVarChar, 500, runtimeMetadata.Publisher);
		navSqlCommand.Command.Parameters.Add(8, SqlDbType.NVarChar, 250, runtimeMetadata.ContentHash);
		navSqlCommand.Command.Parameters.Add(9, SqlDbType.Int, 1);
		if (Database.IsSharedTenantSchema)
		{
			navSqlCommand.Command.Parameters.Add(10, SqlDbType.Int, Database.GetCompanyIdForSystemTables(2000000153).Value);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001S7", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Inserting or updating published app record -- AppId:{0}; Version:{1}; PackageId:{2}; Name:{3}; Publisher:{4}", runtimeMetadata.AppId, runtimeMetadata.Version, runtimeMetadata.PackageId, runtimeMetadata.Name, runtimeMetadata.Publisher);
		navSqlCommand.PrepareStatement();
		navSqlCommand.ExecuteNonQuery();
	}

	public void Delete(PackageId packageId)
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(ConnectionScope.Connection);
		navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "IF (OBJECT_ID('{0}') IS NOT NULL) DELETE FROM [{0}] WHERE {1} [Package ID] = @0", "NAV App Published App", Database.IsSharedTenantSchema ? "\"$companyId\" = @1 AND " : string.Empty));
		navSqlCommand.Command.Parameters.Add(0, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)packageId).Value);
		if (Database.IsSharedTenantSchema)
		{
			navSqlCommand.Command.Parameters.Add(1, SqlDbType.Int, Database.GetCompanyIdForSystemTables(2000000156).Value);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001S8", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Deleting published app record -- PackageId:{0}", packageId);
		navSqlCommand.PrepareStatement();
		navSqlCommand.ExecuteNonQuery();
	}

	public void DeleteAll()
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(ConnectionScope.Connection);
		navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "IF (OBJECT_ID('{0}') IS NOT NULL) DELETE FROM [{0}]", "NAV App Published App", Database.IsSharedTenantSchema ? " WHERE \"$companyId\" = @0 " : string.Empty));
		if (Database.IsSharedTenantSchema)
		{
			navSqlCommand.Command.Parameters.Add(0, SqlDbType.Int, Database.GetCompanyIdForSystemTables(2000000156).Value);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001S9", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Deleting published app records");
		navSqlCommand.PrepareStatement();
		navSqlCommand.ExecuteNonQuery();
	}

	public List<INavAppMetadataReference> GetAll()
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Expected Obj, but got Unknown
		List<INavAppMetadataReference> list = new List<INavAppMetadataReference>();
		StringBuilder stringBuilder = new StringBuilder();
		using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(ConnectionScope.Connection))
		{
			string text = string.Format(CultureInfo.InvariantCulture, "IF (OBJECT_ID('[{0}].[{1}]', 'U') IS NOT NULL) SELECT [App ID], [Version Major], [Version Minor], [Version Build], [Version Revision], [Package ID], [Name], [Publisher], [Content Hash], [Hash Algorithm] FROM [{0}].[{1}]", "dbo", "NAV App Published App");
			if (ConnectionScope.Connection.Database.IsSharedTenantSchema && !ConnectionScope.Connection.Database.Tenant.IsSystemTenant)
			{
				CompanyId? companyIdForSystemTables = ConnectionScope.Connection.Database.GetCompanyIdForSystemTables(2000000156);
				if (!companyIdForSystemTables.HasValue)
				{
					return new List<INavAppMetadataReference>(0);
				}
				text = text + " WHERE " + NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(0);
				NavSqlStatementHelper.SetSqlParameter(NavSqlStatementHelper.GetOrCreateSqlParameterNo(navSqlCommand.Command.Parameters, 0), companyIdForSystemTables.Value);
			}
			navSqlCommand.SetCommandText(text);
			navSqlCommand.PrepareStatement();
			using (navSqlCommand.ExecuteReader())
			{
				while (navSqlCommand.ReadNextRow())
				{
					NavAppMetadata val = new NavAppMetadata
					{
						AppId = AppId.op_Implicit(navSqlCommand.Reader.GetGuid(0)),
						VersionMajor = navSqlCommand.Reader.GetInt32(1),
						VersionMinor = navSqlCommand.Reader.GetInt32(2),
						VersionBuild = navSqlCommand.Reader.GetInt32(3),
						VersionRevision = navSqlCommand.Reader.GetInt32(4),
						PackageId = PackageId.op_Implicit(navSqlCommand.Reader.GetGuid(5)),
						Name = navSqlCommand.Reader.GetString(6),
						Publisher = navSqlCommand.Reader.GetString(7),
						ContentHash = navSqlCommand.Reader.GetString(8),
						HashAlgorithm = (NavAppHashAlgorithm)navSqlCommand.Reader.GetInt32(9)
					};
					list.Add((INavAppMetadataReference)(object)val);
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}  AppId:{1}; Version:{2}; PackageId:{3}; Name:{4}; Publisher:{5}", Environment.NewLine, val.AppId, val.Version, val.PackageId, val.Name, val.Publisher);
				}
			}
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001SA", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Retrieving published app records -- Count:{0}{1}", list.Count, stringBuilder);
		return list;
	}
}
