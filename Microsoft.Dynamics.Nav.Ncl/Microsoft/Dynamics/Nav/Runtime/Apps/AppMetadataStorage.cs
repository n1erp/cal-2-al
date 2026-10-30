using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class AppMetadataStorage : TableStorage
{
	public const string TableName = "NAV App";

	private const string SelectMetadata = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] ";

	private const string SelectMetadataRecordQuery = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE [Package ID] = @0";

	private const string SelectAllMetadataRecordQuery = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0}";

	private const string SelectAllSymbolPackageMetadata = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE [Package Type] = 1";

	private const string SelectMetadataByName = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE UPPER([Name]) = UPPER(@0)";

	private const string SelectMetadataByNameAndPublisher = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE UPPER([Name]) = UPPER(@0) AND UPPER([Publisher]) = UPPER(@1)";

	private const string SelectMetadataByNameAndVersion = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE UPPER([Name]) = UPPER(@0) AND [Version Major] = @1 AND [Version Minor] = @2 AND [Version Build] = @3 AND [Version Revision] = @4";

	private const string SelectMetadataByAppIdAndVersion = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE [ID] = @0 AND [Version Major] = @1 AND [Version Minor] = @2 AND [Version Build] = @3 AND [Version Revision] = @4";

	private const string SelectMetadataByNamePublisherAndVersion = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE UPPER([Name]) = UPPER(@0) AND UPPER([Publisher]) = UPPER(@1) AND [Version Major] = @2 AND [Version Minor] = @3 AND [Version Build] = @4 AND [Version Revision] = @5";

	private const string SelectLatestMetadataByNamePublisherAndMinVersion = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE UPPER([Name]) = UPPER(@0) AND UPPER([Publisher]) = UPPER(@1) AND ([Version Major] > @2 OR ([Version Major] = @2 AND ([Version Minor] > @3 OR ([Version Minor] = @3 AND ([Version Build] > @4 OR ([Version Build] = @4 AND ([Version Revision] >= @5)))))))";

	private const string SelectMetadataCompatRecordQuery = "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE [ID] = @0 AND [Compatibility Major] = @1 AND [Compatibility Minor] = @2 AND [Compatibility Build] = @3 AND [Compatibility Revision] = @4";

	private const string SelectPackageIdOfLowestCompatibleAppQuery = "SELECT [Package ID], [Version Major], [Version Minor], [Version Build], [Version Revision] FROM {0}WHERE [ID] = @0 AND [Compatibility Major] = @1 AND [Compatibility Minor] = @2 AND [Compatibility Build] = @3 AND [Compatibility Revision] = @4 AND [Version Major] >= @5 AND [Version Minor] >= @6 AND [Version Build] >= @7 AND [Version Revision] >= @8";

	private const string SelectPackageFileFromBlobQuery = "SELECT [Blob] FROM {0} WHERE [Package ID] = @0";

	private const string SelectPackageSymbolsFromBlobQuery = "SELECT [Symbols] FROM {0} WHERE [Package ID] = @0";

	private const string SelectPrerequisiteRecordQuery = "SELECT [ID] FROM [Object] WHERE [Type] = @0 AND [ID] = @1";

	private const string InsertRecordQuery = "INSERT INTO {0} ([Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Blob], [Package Type], [Symbols], [Content Hash], [Tenant ID], [Show My Code], [Scope], [Package Hash]) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18, @19, @20, @21, @22, @23, @24, @25, @26, @27)";

	private const string DeleteRecordQuery = "DELETE FROM {0} WHERE [Package ID] = @0";

	public AppMetadataStorage(NavDatabase database, NavSqlConnectionScope connectionScope)
		: base("NAV App", database, connectionScope)
	{
	}

	public NavAppMetadata GetAppMetadataOrNull(PackageId packageId)
	{
		Result<IEnumerable<NavAppMetadata>> appMetadataCommon = GetAppMetadataCommon("GetAppMetadata", "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE [Package ID] = @0", (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		});
		if (((Result)appMetadataCommon).Failure)
		{
			throw ((Result)appMetadataCommon).Error.AsException();
		}
		return appMetadataCommon.Value.FirstOrDefault();
	}

	public Result<NavAppMetadata> GetAppMetadata(PackageId packageId)
	{
		Result<IEnumerable<NavAppMetadata>> appMetadataCommon = GetAppMetadataCommon("GetAppMetadata", "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE [Package ID] = @0", (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		});
		return ResultExtensions.OnSuccess<IEnumerable<NavAppMetadata>, NavAppMetadata>(appMetadataCommon, (Func<IEnumerable<NavAppMetadata>, Result<NavAppMetadata>>)((IEnumerable<NavAppMetadata> rows) => (rows.Count() != 1) ? Result.Fail<NavAppMetadata>(Error.FromMessage("The specified Extension is not published.", ErrorType.InvalidOperation)) : Result.Ok<NavAppMetadata>(rows.First())));
	}

	public Result<IEnumerable<NavAppMetadata>> GetAppMetadata(string name)
	{
		return GetAppMetadataCommon("GetAppMetadataByName", "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE UPPER([Name]) = UPPER(@0)", (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 250));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = name;
		});
	}

	public Result<IEnumerable<NavAppMetadata>> GetAppMetadata(string name, string publisher)
	{
		return GetAppMetadataCommon("SelectMetadataByNameAndPublisher", "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE UPPER([Name]) = UPPER(@0) AND UPPER([Publisher]) = UPPER(@1)", (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 250));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = name;
			cmd.Command.Parameters[1].Value = publisher;
		});
	}

	public Result<IEnumerable<NavAppMetadata>> GetAppMetadata(string name, Version version)
	{
		return GetAppMetadataCommon("SelectMetadataByNameAndVersion", "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE UPPER([Name]) = UPPER(@0) AND [Version Major] = @1 AND [Version Minor] = @2 AND [Version Build] = @3 AND [Version Revision] = @4", (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = name;
			cmd.Command.Parameters[1].Value = version.Major;
			cmd.Command.Parameters[2].Value = version.Minor;
			cmd.Command.Parameters[3].Value = version.Build;
			cmd.Command.Parameters[4].Value = version.Revision;
		});
	}

	public Result<IEnumerable<NavAppMetadata>> GetAppMetadata(AppId appId, Version version)
	{
		return GetAppMetadataCommon("SelectMetadataByAppIdAndVersion", "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE [ID] = @0 AND [Version Major] = @1 AND [Version Minor] = @2 AND [Version Build] = @3 AND [Version Revision] = @4", (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)appId).Value;
			cmd.Command.Parameters[1].Value = version.Major;
			cmd.Command.Parameters[2].Value = version.Minor;
			cmd.Command.Parameters[3].Value = version.Build;
			cmd.Command.Parameters[4].Value = version.Revision;
		});
	}

	public Result<NavAppMetadata> GetLatestAppMetadata(string name, string publisher, Version minVersion, string tenantId)
	{
		return ResultExtensions.OnSuccess<IEnumerable<NavAppMetadata>, NavAppMetadata>(GetAppMetadataCommon("SelectLatestMetadataByNamePublisherAndMinVersion", "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE UPPER([Name]) = UPPER(@0) AND UPPER([Publisher]) = UPPER(@1) AND ([Version Major] > @2 OR ([Version Major] = @2 AND ([Version Minor] > @3 OR ([Version Minor] = @3 AND ([Version Build] > @4 OR ([Version Build] = @4 AND ([Version Revision] >= @5)))))))", (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@5", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = name;
			cmd.Command.Parameters[1].Value = publisher;
			cmd.Command.Parameters[2].Value = minVersion.Major;
			cmd.Command.Parameters[3].Value = minVersion.Minor;
			cmd.Command.Parameters[4].Value = minVersion.Build;
			cmd.Command.Parameters[5].Value = minVersion.Revision;
		}), (Func<IEnumerable<NavAppMetadata>, Result<NavAppMetadata>>)((IEnumerable<NavAppMetadata> apps) =>
		{
			if (apps != null && apps.Any())
			{
				if (!string.IsNullOrEmpty(tenantId))
				{
					tenantId = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
					IEnumerable<NavAppMetadata> source = apps.Where((NavAppMetadata app) => app.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase));
					if (source.Count() == 1)
					{
						return Result.Ok<NavAppMetadata>(source.Single());
					}
				}
				return Result.Ok<NavAppMetadata>((from app in apps
					where string.IsNullOrEmpty(app.TenantId)
					orderby app.VersionMajor descending, app.VersionMinor descending, app.VersionBuild descending, app.VersionRevision descending
					select app).First());
			}
			return Result.Fail<NavAppMetadata>(Error.FromMessage("No published package matches the provided arguments.", ErrorType.Argument));
		}));
	}

	public Result<IEnumerable<NavAppMetadata>> GetAppMetadata(string name, string publisher, Version version)
	{
		return GetAppMetadataCommon("SelectMetadataByNamePublisherAndVersion", "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE UPPER([Name]) = UPPER(@0) AND UPPER([Publisher]) = UPPER(@1) AND [Version Major] = @2 AND [Version Minor] = @3 AND [Version Build] = @4 AND [Version Revision] = @5", (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@5", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = name;
			cmd.Command.Parameters[1].Value = publisher;
			cmd.Command.Parameters[2].Value = version.Major;
			cmd.Command.Parameters[3].Value = version.Minor;
			cmd.Command.Parameters[4].Value = version.Build;
			cmd.Command.Parameters[5].Value = version.Revision;
		});
	}

	public Result<IEnumerable<NavAppMetadata>> GetAllMetadataRecords()
	{
		return GetAppMetadataCommon("GetAllMetadataRecords", "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0}", (NavSqlCommand cmd) =>
		{
		}, (NavSqlCommand cmd) =>
		{
		});
	}

	public Result<IEnumerable<NavAppMetadata>> GetAllSymbolPackageMetadataRecords()
	{
		return GetAppMetadataCommon("GetAllSymbolPackageMetadataRecords", "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE [Package Type] = 1", (NavSqlCommand cmd) =>
		{
		}, (NavSqlCommand cmd) =>
		{
		});
	}

	private Result<IEnumerable<NavAppMetadata>> GetAppMetadataCommon(string cacheKey, string query, Action<NavSqlCommand> parameterCreateAction, Action<NavSqlCommand> setParametersAction)
	{
		List<NavAppMetadata> list = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey(cacheKey, Database), () => string.Format(CultureInfo.InvariantCulture, query, FullTableName), parameterCreateAction, setParametersAction, (NavSqlCommand cmd) =>
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Expected Obj, but got Unknown
			cmd.ExecuteReader();
			List<NavAppMetadata> list2 = new List<NavAppMetadata>();
			while (cmd.ReadNextRow())
			{
				NavAppMetadata val = new NavAppMetadata
				{
					PackageId = PackageId.op_Implicit(cmd.Reader.GetGuid(0)),
					AppId = AppId.op_Implicit(cmd.Reader.GetGuid(1)),
					Name = (cmd.Reader.GetString(2) ?? string.Empty),
					Publisher = (cmd.Reader.GetString(3) ?? string.Empty),
					VersionMajor = cmd.Reader.GetInt32(4),
					VersionMinor = cmd.Reader.GetInt32(5),
					VersionBuild = cmd.Reader.GetInt32(6),
					VersionRevision = cmd.Reader.GetInt32(7),
					CompatibilityMajor = cmd.Reader.GetInt32(8),
					CompatibilityMinor = cmd.Reader.GetInt32(9),
					CompatibilityBuild = cmd.Reader.GetInt32(10),
					CompatibilityRevision = cmd.Reader.GetInt32(11),
					Brief = (cmd.Reader.GetString(12) ?? string.Empty),
					Description = (ReadCompressedBlobAsString(cmd, 13) ?? string.Empty),
					PrivacyStatement = (cmd.Reader.GetString(14) ?? string.Empty),
					Eula = (cmd.Reader.GetString(15) ?? string.Empty),
					Url = (cmd.Reader.GetString(16) ?? string.Empty),
					Help = (cmd.Reader.GetString(17) ?? string.Empty),
					Logo = MediaId.op_Implicit(cmd.Reader.GetGuid(18)),
					Screenshots = MediaSetId.op_Implicit(cmd.Reader.GetGuid(19)),
					PackageType = (NavAppPackageType)cmd.Reader.GetInt32(20),
					ContentHash = cmd.Reader.GetString(21),
					TenantId = cmd.Reader.GetString(22),
					Scope = (NavAppScope)cmd.Reader.GetInt32(23)
				};
				Stream stream = ReadCompressedBlobAsStream(cmd, 24);
				using (NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(stream))
				{
					val.ExtensionType = ((navAppPackageReader.MetadataVersion == 1) ? NavExtensionType.CSide : NavExtensionType.ModernDev);
				}
				list2.Add(val);
			}
			return list2;
		});
		return Result.Ok<IEnumerable<NavAppMetadata>>((IEnumerable<NavAppMetadata>)list);
	}

	public Result<Stream> GetAppPackageStream(PackageId packageId)
	{
		return GetAppPackageStreamGeneric(packageId, "GetAppPackageStream", "SELECT [Blob] FROM {0} WHERE [Package ID] = @0");
	}

	public Result<Stream> GetAppSymbolsStream(PackageId packageId)
	{
		return GetAppPackageStreamGeneric(packageId, "GetAppPackageSymbolsStream", "SELECT [Symbols] FROM {0} WHERE [Package ID] = @0");
	}

	public Result AddAppMetadata(NavAppMetadata metadata, string packageFilePath, byte[] symbols)
	{
		using FileStream packageFileStream = new FileStream(packageFilePath, FileMode.Open, FileAccess.Read);
		return AddAppMetadata(metadata, packageFileStream, symbols);
	}

	public Result AddAppMetadata(NavAppMetadata metadata, Stream packageFileStream, byte[] symbols)
	{
		byte[] packageBytes = GetBytesFromStream(packageFileStream);
		string packageHash = Sha256Hash.ComputeHashForContent(packageBytes)?.HexString ?? string.Empty;
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("InsertAppMetadata", Database), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ([Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Blob], [Package Type], [Symbols], [Content Hash], [Tenant ID], [Show My Code], [Scope], [Package Hash]) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18, @19, @20, @21, @22, @23, @24, @25, @26, @27)", FullTableName), (NavSqlCommand cmd) =>
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
			cmd.Command.Parameters.Add(new SqlParameter("@12", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@13", SqlDbType.Binary, int.MaxValue));
			cmd.Command.Parameters.Add(new SqlParameter("@14", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@15", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@16", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@17", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@18", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@19", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@20", SqlDbType.Binary, int.MaxValue));
			cmd.Command.Parameters.Add(new SqlParameter("@21", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@22", SqlDbType.Binary, int.MaxValue));
			cmd.Command.Parameters.Add(new SqlParameter("@23", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@24", SqlDbType.NVarChar, 250));
			cmd.Command.Parameters.Add(new SqlParameter("@25", SqlDbType.TinyInt));
			cmd.Command.Parameters.Add(new SqlParameter("@26", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@27", SqlDbType.NVarChar, 64));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)metadata.PackageId).Value;
			cmd.Command.Parameters[1].Value = ((WrappedValue<Guid>)(object)metadata.AppId).Value;
			cmd.Command.Parameters[2].Value = metadata.Name;
			cmd.Command.Parameters[3].Value = metadata.Publisher;
			cmd.Command.Parameters[4].Value = metadata.VersionMajor;
			cmd.Command.Parameters[5].Value = metadata.VersionMinor;
			cmd.Command.Parameters[6].Value = metadata.VersionBuild;
			cmd.Command.Parameters[7].Value = metadata.VersionRevision;
			cmd.Command.Parameters[8].Value = metadata.CompatibilityMajor;
			cmd.Command.Parameters[9].Value = metadata.CompatibilityMinor;
			cmd.Command.Parameters[10].Value = metadata.CompatibilityBuild;
			cmd.Command.Parameters[11].Value = metadata.CompatibilityRevision;
			cmd.Command.Parameters[12].Value = metadata.Brief;
			cmd.Command.Parameters[13].Value = GetBlobFromString(metadata.Description);
			cmd.Command.Parameters[14].Value = metadata.PrivacyStatement;
			cmd.Command.Parameters[15].Value = metadata.Eula;
			cmd.Command.Parameters[16].Value = metadata.Url;
			cmd.Command.Parameters[17].Value = metadata.Help;
			cmd.Command.Parameters[18].Value = (((WrappedComparable<Guid>)(object)metadata.Logo == (WrappedComparable<Guid>)null) ? Guid.Empty : ((WrappedValue<Guid>)(object)metadata.Logo).Value);
			cmd.Command.Parameters[19].Value = (((WrappedComparable<Guid>)(object)metadata.Screenshots == (WrappedComparable<Guid>)null) ? Guid.Empty : ((WrappedValue<Guid>)(object)metadata.Screenshots).Value);
			cmd.Command.Parameters[20].Value = GetBlobFromBytes(packageBytes);
			cmd.Command.Parameters[21].Value = metadata.PackageType;
			cmd.Command.Parameters[22].Value = GetBlobFromBytes(symbols);
			cmd.Command.Parameters[23].Value = metadata.ContentHash;
			cmd.Command.Parameters[24].Value = metadata.TenantId ?? string.Empty;
			cmd.Command.Parameters[25].Value = metadata.ShowMyCode;
			cmd.Command.Parameters[26].Value = (int)metadata.Scope;
			cmd.Command.Parameters[27].Value = packageHash;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}

	public Result DeleteAppMetadata(PackageId packageId)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeleteAppMetadata", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [Package ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		return Result.Ok();
	}

	public Result IsCompatibleAppPresent(AppId id, Version minVersion, Version compatibilityId)
	{
		NavSqlCommand navSqlCommand = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetCompatibleApp", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Brief], [Description], [Privacy Statement], [EULA], [Url], [Help], [Logo], [Screenshots], [Package Type], [Content Hash], [Tenant ID], [Scope], [Blob] FROM {0} WHERE [ID] = @0 AND [Compatibility Major] = @1 AND [Compatibility Minor] = @2 AND [Compatibility Build] = @3 AND [Compatibility Revision] = @4", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)id).Value;
			cmd.Command.Parameters[1].Value = compatibilityId.Major;
			cmd.Command.Parameters[2].Value = compatibilityId.Minor;
			cmd.Command.Parameters[3].Value = compatibilityId.Build;
			cmd.Command.Parameters[4].Value = compatibilityId.Revision;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			return cmd;
		});
		int num = 0;
		while (navSqlCommand.ReadNextRow())
		{
			Version version = new Version(navSqlCommand.Reader.GetInt32(4), navSqlCommand.Reader.GetInt32(5), navSqlCommand.Reader.GetInt32(6), navSqlCommand.Reader.GetInt32(7));
			if (version >= minVersion)
			{
				num++;
			}
		}
		if (num < 1)
		{
			return Result.Fail(Error.FromMessage("Could not find", ErrorType.InvalidOperation));
		}
		return Result.Ok();
	}

	public Result IsPrerequisitePresent(ObjectType objectType, int id)
	{
		NavSqlCommand navSqlCommand = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetPrerequisite" + objectType.ToString() + id, Database), () => "SELECT [ID] FROM [Object] WHERE [Type] = @0 AND [ID] = @1", (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = (int)objectType;
			cmd.Command.Parameters[1].Value = id;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			return cmd;
		});
		int num = 0;
		while (navSqlCommand.ReadNextRow())
		{
			num++;
		}
		if (num < 1)
		{
			return Result.Fail(Error.FromMessage("Could not find", ErrorType.InvalidOperation));
		}
		return Result.Ok();
	}

	public Result<PackageId> GetLowestCompatibleAppPackageId(AppId id, Version minVersion, Version compatibilityId)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected Obj, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Expected Obj, but got Unknown
		NavSqlCommand navSqlCommand = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetLowestCompatibleAppPackageId", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Package ID], [Version Major], [Version Minor], [Version Build], [Version Revision] FROM {0}WHERE [ID] = @0 AND [Compatibility Major] = @1 AND [Compatibility Minor] = @2 AND [Compatibility Build] = @3 AND [Compatibility Revision] = @4 AND [Version Major] >= @5 AND [Version Minor] >= @6 AND [Version Build] >= @7 AND [Version Revision] >= @8", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@5", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@6", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@7", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@8", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)id).Value;
			cmd.Command.Parameters[1].Value = compatibilityId.Major;
			cmd.Command.Parameters[2].Value = compatibilityId.Minor;
			cmd.Command.Parameters[3].Value = compatibilityId.Build;
			cmd.Command.Parameters[4].Value = compatibilityId.Revision;
			cmd.Command.Parameters[5].Value = minVersion.Major;
			cmd.Command.Parameters[6].Value = minVersion.Minor;
			cmd.Command.Parameters[7].Value = minVersion.Build;
			cmd.Command.Parameters[8].Value = minVersion.Revision;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			return cmd;
		});
		List<Tuple<PackageId, Version>> list = new List<Tuple<PackageId, Version>>(navSqlCommand.TotalRecordsFetched);
		while (navSqlCommand.ReadNextRow())
		{
			list.Add(Tuple.Create<PackageId, Version>(new PackageId(navSqlCommand.Reader.GetGuid(0)), VersionHelper.Create(navSqlCommand.Reader.GetInt32(1), navSqlCommand.Reader.GetInt32(2), navSqlCommand.Reader.GetInt32(3), navSqlCommand.Reader.GetInt32(4))));
		}
		if (list.Count != 0)
		{
			return Result.Ok<PackageId>(list.Aggregate(Tuple.Create<PackageId, Version>(new PackageId(Guid.Empty), VersionHelper.Create(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue)), (Tuple<PackageId, Version> lhs, Tuple<PackageId, Version> rhs) => (!(lhs.Item2 < rhs.Item2)) ? rhs : lhs).Item1);
		}
		return Result.Fail<PackageId>(Error.FromMessage("No compatible apps found", ErrorType.InvalidOperation));
	}

	private static string ReadCompressedBlobAsString(NavSqlCommand command, int ordinal)
	{
		using NavBLOB navBLOB = new NavBLOB();
		command.GetBlobDataFromReader(isCompressed: true, ordinal, navBLOB);
		string result = null;
		if (!navBLOB.IsZeroOrEmpty)
		{
			result = new StreamReader(navBLOB.GetNewStream()).ReadToEnd();
		}
		return result;
	}

	private static Stream ReadCompressedBlobAsStream(NavSqlCommand command, int ordinal)
	{
		NavBLOB navBLOB = new NavBLOB();
		command.GetBlobDataFromReader(isCompressed: true, ordinal, navBLOB);
		return navBLOB.GetStream();
	}

	private static byte[] GetBytesFromStream(Stream stream)
	{
		if (stream != null && stream.CanRead)
		{
			byte[] array = new byte[stream.Length];
			stream.Position = 0L;
			stream.Read(array, 0, array.Length);
			return array;
		}
		return null;
	}

	private static object GetBlobFromBytes(byte[] bytes)
	{
		object result = DBNull.Value;
		if (bytes != null)
		{
			using NavBLOB navBLOB = NavBLOB.CreateFromObject(bytes);
			result = navBLOB.GetSqlWritableValue(SqlDataType.Default, int.MaxValue, compressed: true);
		}
		return result;
	}

	private static object GetBlobFromString(string str)
	{
		object result = DBNull.Value;
		if (str != null)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			using NavBLOB navBLOB = NavBLOB.CreateFromObject(bytes);
			result = navBLOB.GetSqlWritableValue(SqlDataType.Default, int.MaxValue, compressed: true);
		}
		return result;
	}

	private Result<Stream> GetAppPackageStreamGeneric(PackageId packageId, string commandCacheKey, string query)
	{
		NavSqlCommand navSqlCommand = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey(commandCacheKey, Database), () => string.Format(CultureInfo.InvariantCulture, query, FullTableName), (NavSqlCommand cmd) =>
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
		List<Stream> list = new List<Stream>();
		while (navSqlCommand.ReadNextRow())
		{
			list.Add(ReadCompressedBlobAsStream(navSqlCommand, 0));
		}
		if (list.Count != 1)
		{
			return Result.Fail<Stream>(Error.FromMessage("Could not find package data.", ErrorType.InvalidOperation));
		}
		return Result.Ok<Stream>(list.First());
	}
}
