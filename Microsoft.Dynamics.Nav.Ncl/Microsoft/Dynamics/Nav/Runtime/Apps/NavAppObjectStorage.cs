using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppObjectStorage(NavDatabase database, NavSqlConnectionScope connectionScope) : TableStorage("NAV App Object Metadata", database, connectionScope)
{
	private const string TableName = "NAV App Object Metadata";

	private readonly NavAppObjectMetadataFullComparer fullComparer = new NavAppObjectMetadataFullComparer();

	private readonly NavAppObjectMetadataByKeyComparer keyComparer = new NavAppObjectMetadataByKeyComparer();

	private const string SelectRecordsForAppQuery = "SELECT CAST([timestamp] AS BIGINT), [App Package ID], [Object Type], [Object ID], [Metadata Format], [Metadata], [User Code], [User AL Code], [Metadata Version], [Object Subtype], [Object Name], [Metadata Hash], [User Code Hash], [Object Flags] FROM {0} WHERE [App Package ID] = @0";

	private const string InsertRecordQuery = "INSERT INTO {0} ([App Package ID], [Object Type], [Object ID], [Metadata Format], [Metadata], [User Code], [User AL Code], [Metadata Version], [Object Subtype], [Object Name], [Metadata Hash], [User Code Hash], [Object Flags]) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12)";

	private const string UpdateRecordQuery = "UPDATE {0} SET [Metadata Format] = @3, [Metadata] = @4, [User Code] = @5, [User AL Code] = @6, [Metadata Version] = @7, [Object Subtype] = @8, [Object Name] = @9, [Metadata Hash] = @10, [User Code Hash] = @11, [Object Flags] = @12 WHERE [App Package ID] = @0 AND [Object Type] = @1 AND [Object ID] = @2";

	private const string DeleteRecordQuery = "DELETE FROM {0} WHERE [App Package ID] = @0 AND [Object Type] = @1 AND [Object ID] = @2";

	private const string DeleteRecordsForAppQuery = "DELETE FROM {0} WHERE [App Package ID] = @0";

	private const string UpdatePackageIdQuery = "UPDATE {0} SET [App Package ID] = @0 WHERE [App Package ID] = @1";

	public IEnumerable<NavAppObjectMetadata> GetAppRecords(PackageId packageId)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected Obj, but got Unknown
		List<NavAppObjectMetadata> list = new List<NavAppObjectMetadata>();
		NavSqlCommand navSqlCommand = ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GetAppRecords", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT CAST([timestamp] AS BIGINT), [App Package ID], [Object Type], [Object ID], [Metadata Format], [Metadata], [User Code], [User AL Code], [Metadata Version], [Object Subtype], [Object Name], [Metadata Hash], [User Code Hash], [Object Flags] FROM {0} WHERE [App Package ID] = @0", FullTableName), (NavSqlCommand cmd) =>
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
		while (navSqlCommand.ReadNextRow())
		{
			NavAppObjectMetadata val = new NavAppObjectMetadata
			{
				SqlTimestamp = navSqlCommand.Reader.GetInt64(0),
				AppPackageId = packageId,
				ObjectType = (ObjectType)navSqlCommand.Reader.GetInt32(2),
				ObjectId = navSqlCommand.Reader.GetInt32(3),
				Format = (NavAppObjectFormat)navSqlCommand.Reader.GetInt32(4),
				Metadata = ReadCompressedBlobAsString(navSqlCommand, 5),
				UserCode = ReadCompressedBlobAsString(navSqlCommand, 6),
				UserALCode = ReadCompressedBlobAsString(navSqlCommand, 7),
				MetadataVersion = navSqlCommand.Reader.GetInt32(8),
				ObjectSubtype = navSqlCommand.Reader.GetString(9),
				ObjectName = navSqlCommand.Reader.GetString(10)
			};
			string text = navSqlCommand.Reader.GetString(11);
			string text2 = navSqlCommand.Reader.GetString(12);
			val.MetadataHash = ((!string.IsNullOrEmpty(text)) ? Sha256Hash.CreateFromHexString(text) : null);
			val.UserCodeHash = ((!string.IsNullOrEmpty(text2)) ? Sha256Hash.CreateFromHexString(text2) : null);
			val.ObjectFlags = (NavAppObjectFlags)navSqlCommand.Reader.GetInt32(13);
			list.Add(val);
		}
		return list;
	}

	public void DeleteAppRecords(PackageId packageId)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeleteAppRecords", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [App Package ID] = @0", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)packageId).Value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}

	public void AddOrUpdateOrDeleteAppRecords(PackageId packageId, ObjectChangeModelDefinition changes, IList<NavAppObjectMetadata> currentRecordsList)
	{
		if (changes.Added != null)
		{
			IEnumerable<NavAppObjectMetadata> source = currentRecordsList.Where((NavAppObjectMetadata r) => changes.Added.Any((ObjectChangeElement c) => c.Id.HasValue && c.Id.Value == r.ObjectId && MapToObjectType(c.Kind) == r.ObjectType)).ToList();
			source.ForEach(InsertRecordInternal);
		}
		if (changes.Removed != null)
		{
			ObjectChangeElement[] removed = changes.Removed;
			foreach (ObjectChangeElement objectChangeElement in removed)
			{
				if (objectChangeElement.Id.HasValue)
				{
					DeleteRecordInternal(((WrappedValue<Guid>)(object)packageId).Value, MapToObjectType(objectChangeElement.Kind), objectChangeElement.Id.Value);
				}
			}
		}
		if (changes.Modified == null)
		{
			return;
		}
		IEnumerable<NavAppObjectMetadata> enumerable = currentRecordsList.Where((NavAppObjectMetadata r) => changes.Modified.Any((ObjectChangeElement c) => c.Id.HasValue && c.Id.Value == r.ObjectId && MapToObjectType(c.Kind) == r.ObjectType)).ToList();
		foreach (NavAppObjectMetadata item in enumerable)
		{
			AddOrUpdateRecordInternal(item, "UpdateRecordInternal", "UPDATE {0} SET [Metadata Format] = @3, [Metadata] = @4, [User Code] = @5, [User AL Code] = @6, [Metadata Version] = @7, [Object Subtype] = @8, [Object Name] = @9, [Metadata Hash] = @10, [User Code Hash] = @11, [Object Flags] = @12 WHERE [App Package ID] = @0 AND [Object Type] = @1 AND [Object ID] = @2");
		}
		static ObjectType MapToObjectType(SymbolKind kind)
		{
			return kind switch
			{
				SymbolKind.Codeunit => ObjectType.CodeUnit, 
				SymbolKind.Page => ObjectType.Page, 
				SymbolKind.Table => ObjectType.Table, 
				SymbolKind.Report => ObjectType.Report, 
				SymbolKind.Query => ObjectType.Query, 
				SymbolKind.XmlPort => ObjectType.XmlPort, 
				SymbolKind.PageExtension => ObjectType.PageExtension, 
				SymbolKind.Enum => ObjectType.Enum, 
				SymbolKind.EnumExtension => ObjectType.EnumExtension, 
				SymbolKind.TableExtension => ObjectType.TableExtension, 
				_ => throw new InvalidOperationException(kind.ToString()), 
			};
		}
	}

	public void AddOrUpdateAppRecords(IEnumerable<NavAppObjectMetadata> currentRecords, bool deleteUnknown = true)
	{
		List<NavAppObjectMetadata> list = currentRecords.ToList();
		Dictionary<string, NavAppObjectMetadata> dictionary = list.ToDictionary((NavAppObjectMetadata r) => r.ToKeyString());
		PackageId appPackageId = list.FirstOrDefault().AppPackageId;
		List<NavAppObjectMetadata> list2 = GetAppRecords(appPackageId).ToList();
		List<NavAppObjectMetadata> list3 = list2.Except(list, keyComparer).ToList();
		List<NavAppObjectMetadata> list4 = list.Except(list2, keyComparer).ToList();
		List<NavAppObjectMetadata> list5 = list2.Intersect(list, keyComparer).ToList();
		if (deleteUnknown)
		{
			list3.ForEach(DeleteRecordInternal);
		}
		list4.ForEach(InsertRecordInternal);
		foreach (NavAppObjectMetadata item in list5)
		{
			NavAppObjectMetadata modRecord = dictionary[item.ToKeyString()];
			UpdateRecordInternalIfChanged(item, modRecord);
		}
	}

	public bool UpdatePackageId(PackageId newPackageId, PackageId existingPackageId)
	{
		return ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("UpdatePacakageId", Database), () => string.Format(CultureInfo.InvariantCulture, "UPDATE {0} SET [App Package ID] = @0 WHERE [App Package ID] = @1", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.UniqueIdentifier));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)newPackageId).Value;
			cmd.Command.Parameters[1].Value = ((WrappedValue<Guid>)(object)existingPackageId).Value;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery()) > 0;
	}

	private void DeleteRecordInternal(NavAppObjectMetadata record)
	{
		DeleteRecordInternal(((WrappedValue<Guid>)(object)record.AppPackageId).Value, record.ObjectType, record.ObjectId);
	}

	private void DeleteRecordInternal(Guid packageId, ObjectType objectType, int objectId)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeleteRecordInternal", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [App Package ID] = @0 AND [Object Type] = @1 AND [Object ID] = @2", FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = packageId;
			cmd.Command.Parameters[1].Value = objectType;
			cmd.Command.Parameters[2].Value = objectId;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}

	private void InsertRecordInternal(NavAppObjectMetadata record)
	{
		AddOrUpdateRecordInternal(record, "InsertRecordInternal", "INSERT INTO {0} ([App Package ID], [Object Type], [Object ID], [Metadata Format], [Metadata], [User Code], [User AL Code], [Metadata Version], [Object Subtype], [Object Name], [Metadata Hash], [User Code Hash], [Object Flags]) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12)");
	}

	private void UpdateRecordInternalIfChanged(NavAppObjectMetadata origRecord, NavAppObjectMetadata modRecord)
	{
		if (!fullComparer.Equals(origRecord, modRecord))
		{
			AddOrUpdateRecordInternal(modRecord, "UpdateRecordInternal", "UPDATE {0} SET [Metadata Format] = @3, [Metadata] = @4, [User Code] = @5, [User AL Code] = @6, [Metadata Version] = @7, [Object Subtype] = @8, [Object Name] = @9, [Metadata Hash] = @10, [User Code Hash] = @11, [Object Flags] = @12 WHERE [App Package ID] = @0 AND [Object Type] = @1 AND [Object ID] = @2");
		}
	}

	private void AddOrUpdateRecordInternal(NavAppObjectMetadata record, string cacheName, string query)
	{
		ConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey(cacheName, Database), () => string.Format(CultureInfo.InvariantCulture, query, FullTableName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.Binary, int.MaxValue));
			cmd.Command.Parameters.Add(new SqlParameter("@5", SqlDbType.Binary, int.MaxValue));
			cmd.Command.Parameters.Add(new SqlParameter("@6", SqlDbType.Binary, int.MaxValue));
			cmd.Command.Parameters.Add(new SqlParameter("@7", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@8", SqlDbType.NVarChar, 30));
			cmd.Command.Parameters.Add(new SqlParameter("@9", SqlDbType.NVarChar, 30));
			cmd.Command.Parameters.Add(new SqlParameter("@10", SqlDbType.NVarChar, 64));
			cmd.Command.Parameters.Add(new SqlParameter("@11", SqlDbType.NVarChar, 64));
			cmd.Command.Parameters.Add(new SqlParameter("@12", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			object sqlWritableValueForBlobField = GetSqlWritableValueForBlobField(record.Metadata);
			object sqlWritableValueForBlobField2 = GetSqlWritableValueForBlobField(record.UserCode);
			object sqlWritableValueForBlobField3 = GetSqlWritableValueForBlobField(record.UserALCode);
			cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)record.AppPackageId).Value;
			cmd.Command.Parameters[1].Value = record.ObjectType;
			cmd.Command.Parameters[2].Value = record.ObjectId;
			cmd.Command.Parameters[3].Value = record.Format;
			cmd.Command.Parameters[4].Value = sqlWritableValueForBlobField;
			cmd.Command.Parameters[5].Value = sqlWritableValueForBlobField2;
			cmd.Command.Parameters[6].Value = sqlWritableValueForBlobField3;
			cmd.Command.Parameters[7].Value = record.MetadataVersion;
			cmd.Command.Parameters[8].Value = record.ObjectSubtype ?? string.Empty;
			cmd.Command.Parameters[9].Value = record.ObjectName ?? string.Empty;
			cmd.Command.Parameters[10].Value = record.MetadataHash?.HexString ?? string.Empty;
			cmd.Command.Parameters[11].Value = record.UserCodeHash?.HexString ?? string.Empty;
			cmd.Command.Parameters[12].Value = record.ObjectFlags;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}

	private static object GetSqlWritableValueForBlobField(string content)
	{
		object value = DBNull.Value;
		if (content != null)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			return GetSqlWritableValueForBlobField(bytes);
		}
		return value;
	}

	private static object GetSqlWritableValueForBlobField(byte[] bytes)
	{
		object result = DBNull.Value;
		if (bytes != null)
		{
			using NavBLOB navBLOB = NavBLOB.CreateFromObject(bytes);
			result = navBLOB.GetSqlWritableValue(SqlDataType.Default, int.MaxValue, compressed: true);
		}
		return result;
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
}
