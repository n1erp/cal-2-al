using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class AppDependenciesStorage : TableStorage
{
	private const string TableName = "NAV App Dependencies";

	private const string SelectRecordsForAppQuery = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision] FROM {0} WHERE [Package ID] = @0";

	private const string SelectAllRecords = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision] FROM {0}";

	private const string SelectRecordsWhichDependOnPackage = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision] FROM {0} WHERE [ID] = @0 AND [Compatibility Major] = @1 AND [Compatibility Minor] = @2 AND [Compatibility Build]= @3 AND [Compatibility Revision] = @4";

	private const string InsertRecordQuery = "INSERT INTO {0} ([Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision]) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11 )";

	private const string DeleteRecordQuery = "DELETE FROM {0} WHERE [Package ID] = @0";

	public AppDependenciesStorage(NavDatabase database, NavSqlConnectionScope connectionScope)
		: base("NAV App Dependencies", database, connectionScope)
	{
	}

	public Result<List<NavAppDependency>> GetAppDependencies(PackageId packageId)
	{
		NavSqlCommand navSqlCommand = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetAppDependency", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision] FROM {0} WHERE [Package ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			return cmd;
		});
		List<NavAppDependency> list = new List<NavAppDependency>();
		while (navSqlCommand.ReadNextRow())
		{
			list.Add(new NavAppDependency
			{
				AppId = navSqlCommand.Reader.GetGuid(1),
				Name = navSqlCommand.Reader.GetString(2),
				Publisher = navSqlCommand.Reader.GetString(3),
				MinVersion = VersionHelper.Create(navSqlCommand.Reader.GetInt32(4), navSqlCommand.Reader.GetInt32(5), navSqlCommand.Reader.GetInt32(6), navSqlCommand.Reader.GetInt32(7)),
				CompatibilityId = VersionHelper.Create(navSqlCommand.Reader.GetInt32(8), navSqlCommand.Reader.GetInt32(9), navSqlCommand.Reader.GetInt32(10), navSqlCommand.Reader.GetInt32(11))
			});
		}
		return Result.Ok<List<NavAppDependency>>(list);
	}

	public Result AddAppDependency(PackageId packageId, NavAppDependency dependency)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("InsertAppDependency", Database), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ([Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision]) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11 )", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@5", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@6", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@7", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@8", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@9", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@10", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@11", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
			cmd.Command.Parameters[1].Value = dependency.AppId;
			cmd.Command.Parameters[2].Value = dependency.Name;
			cmd.Command.Parameters[3].Value = dependency.Publisher;
			cmd.Command.Parameters[4].Value = dependency.MinVersion.Major;
			cmd.Command.Parameters[5].Value = dependency.MinVersion.Minor;
			cmd.Command.Parameters[6].Value = dependency.MinVersion.Build;
			cmd.Command.Parameters[7].Value = dependency.MinVersion.Revision;
			cmd.Command.Parameters[8].Value = dependency.CompatibilityId.Major;
			cmd.Command.Parameters[9].Value = dependency.CompatibilityId.Minor;
			cmd.Command.Parameters[10].Value = dependency.CompatibilityId.Build;
			cmd.Command.Parameters[11].Value = dependency.CompatibilityId.Revision;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}

	public Result DeleteAppDependencies(PackageId packageId)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeleteAppDependency", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [Package ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}

	public Result<List<AppDependencyRecord>> GetAllDependencyRecords()
	{
		List<AppDependencyRecord> list = ConnectionScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("GetAllDependencyRecords", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision] FROM {0}", FullTableName), (NavSqlCommand cmd) =>
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Expected Obj, but got Unknown
			cmd.ExecuteReader();
			List<AppDependencyRecord> list2 = new List<AppDependencyRecord>();
			while (cmd.ReadNextRow())
			{
				list2.Add(new AppDependencyRecord
				{
					PackageId = PackageId.op_Implicit(cmd.Reader.GetGuid(0)),
					Id = AppId.op_Implicit(cmd.Reader.GetGuid(1)),
					Name = cmd.Reader.GetString(2),
					Publisher = cmd.Reader.GetString(3),
					MinVersion = VersionHelper.Create(cmd.Reader.GetInt32(4), cmd.Reader.GetInt32(5), cmd.Reader.GetInt32(6), cmd.Reader.GetInt32(7)),
					CompatibilityId = VersionHelper.Create(cmd.Reader.GetInt32(8), cmd.Reader.GetInt32(9), cmd.Reader.GetInt32(10), cmd.Reader.GetInt32(11))
				});
			}
			return list2;
		});
		return Result.Ok<List<AppDependencyRecord>>(list);
	}

	public Result<List<NavAppDependency>> GetAppsDependentUponApp(AppId appId, int compatibilityMajor, int compatibilityMinor, int compatibilityBuild, int compatibilityRevision, int minVersionMajor, int minVersionMinor, int minVersionBuild, int minVersionRevision)
	{
		List<NavAppDependency> source = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetAppsDependentUponApp", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision] FROM {0} WHERE [ID] = @0 AND [Compatibility Major] = @1 AND [Compatibility Minor] = @2 AND [Compatibility Build]= @3 AND [Compatibility Revision] = @4", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)appId).Value;
			cmd.Command.Parameters[1].Value = compatibilityMajor;
			cmd.Command.Parameters[2].Value = compatibilityMinor;
			cmd.Command.Parameters[3].Value = compatibilityBuild;
			cmd.Command.Parameters[4].Value = compatibilityRevision;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			List<NavAppDependency> list = new List<NavAppDependency>();
			while (cmd.ReadNextRow())
			{
				list.Add(new NavAppDependency
				{
					AppId = cmd.Reader.GetGuid(1),
					Name = cmd.Reader.GetString(2),
					Publisher = cmd.Reader.GetString(3),
					MinVersion = VersionHelper.Create(cmd.Reader.GetInt32(4), cmd.Reader.GetInt32(5), cmd.Reader.GetInt32(6), cmd.Reader.GetInt32(7)),
					CompatibilityId = VersionHelper.Create(cmd.Reader.GetInt32(8), cmd.Reader.GetInt32(9), cmd.Reader.GetInt32(10), cmd.Reader.GetInt32(11))
				});
			}
			return list;
		});
		return Result.Ok<List<NavAppDependency>>(source.Where((NavAppDependency x) => x.MinVersion <= VersionHelper.Create(minVersionMajor, minVersionMinor, minVersionBuild, minVersionRevision)).ToList());
	}

	public Result<List<PackageId>> GetAppsDependentUponApp(AppId appId, int compatibilityMajor, int compatibilityMinor, int compatibilityBuild, int compatibilityRevision)
	{
		List<PackageId> rowResults = new List<PackageId>();
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetAppsDependentUponAppPackageId", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision] FROM {0} WHERE [ID] = @0 AND [Compatibility Major] = @1 AND [Compatibility Minor] = @2 AND [Compatibility Build]= @3 AND [Compatibility Revision] = @4", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)appId).Value;
			cmd.Command.Parameters[1].Value = compatibilityMajor;
			cmd.Command.Parameters[2].Value = compatibilityMinor;
			cmd.Command.Parameters[3].Value = compatibilityBuild;
			cmd.Command.Parameters[4].Value = compatibilityRevision;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			while (cmd.ReadNextRow())
			{
				rowResults.Add(PackageId.op_Implicit(cmd.Reader.GetGuid(0)));
			}
			return rowResults;
		});
		return Result.Ok<List<PackageId>>(rowResults);
	}
}
