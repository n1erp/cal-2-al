using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class DatabaseNavAppMetadataBulkRetriever
{
	private const int MaxTargetPackageIdsFilterSize = 100;

	private static readonly int[] SystemTablesReadFrom = new int[5] { 2000000160, 2000000161, 2000000150, 2000000155, 2000000176 };

	private const string GetNavAppPackageBlobsQuery = "SELECT [Package ID], [Blob] FROM {0} WHERE [Package ID] IN ({1})";

	public static NavAppMetadataRecord GetMetadataForApp(PackageId targetPackageId)
	{
		return GetMetadata(new HashSet<PackageId> { targetPackageId }).FirstOrDefault();
	}

	public static IEnumerable<NavAppMetadataRecord> GetMetadataForAllApps()
	{
		return GetMetadata(null);
	}

	public static IEnumerable<NavAppMetadataRecord> GetMetadataForApps(ISet<PackageId> targetPackageIds)
	{
		return GetMetadata(targetPackageIds);
	}

	public static IEnumerable<Tuple<string, PackageId>> GetPublishReferences(string tenantId, Guid? packageId)
	{
		tenantId = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		List<Tuple<string, PackageId>> results = new List<Tuple<string, PackageId>>();
		RunActionInAMaximizedSession(() =>
		{
			using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001S4", DiagnosticParameter.Oii("Extension publish reference query -- Tenant: " + tenantId), MonitorType.None, Category.Extensions, createNewActivity: false))
			{
				using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000155);
				if (tenantId != null)
				{
					navRecord.ALSetRange(1, NavText.Create(tenantId));
				}
				if (packageId.HasValue)
				{
					navRecord.ALSetRange(2, new NavGuid(packageId.Value));
				}
				if (navRecord.ALFindSet(DataError.TrapError))
				{
					do
					{
						string text = navRecord.GetFieldValue(1).ToString();
						PackageId item = PackageId.op_Implicit(navRecord.GetFieldValue(2).ToGuid());
						results.Add(Tuple.Create<string, PackageId>(text, item));
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001S3", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Extension publish reference record -- Tenant:{0}; PackageId:{1}", text, packageId);
					}
					while (navRecord.ALNext(1) != 0);
				}
			}
		});
		return results;
	}

	public static IDictionary<PackageId, byte[]> GetPackageBlobs(IList<PackageId> packageIds)
	{
		return GetPackageBlobs(packageIds, ServerUserSettings.Instance.NavAppPackageBlobBatchReadSize.Value);
	}

	public static IDictionary<PackageId, byte[]> GetPackageBlobs(IList<PackageId> packageIds, int batchSize)
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected Obj, but got Unknown
		Dictionary<PackageId, byte[]> dictionary = new Dictionary<PackageId, byte[]>(packageIds.Count);
		if (!packageIds.Any())
		{
			return dictionary;
		}
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00009LD", DiagnosticParameter.SystemMetadata("Get Package Blobs -- PackageIds:" + string.Join(",", packageIds)), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase);
			string quotedTableName = NavSqlStatementHelper.GetQuotedTableName(NavGlobal.AppDatabase, "dbo", "NAV App");
			int num = ((packageIds.Count % batchSize == 0) ? (packageIds.Count / batchSize) : (packageIds.Count / batchSize + 1));
			for (int i = 0; i < num; i++)
			{
				string packagesIdsForCurrentPackageBlobBatch = GetPackagesIdsForCurrentPackageBlobBatch(i, batchSize, packageIds);
				using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
				navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "SELECT [Package ID], [Blob] FROM {0} WHERE [Package ID] IN ({1})", quotedTableName, packagesIdsForCurrentPackageBlobBatch));
				navSqlCommand.ExecuteReader();
				while (navSqlCommand.ReadNextRow())
				{
					Guid guid = navSqlCommand.Reader.GetGuid(0);
					using NavBLOB navBLOB = new NavBLOB();
					navSqlCommand.GetBlobDataFromReader(isCompressed: true, 1, navBLOB);
					byte[] value = (navBLOB.IsZeroOrEmpty ? Array.Empty<byte>() : navBLOB.GetBytes());
					dictionary.Add(new PackageId(guid), value);
				}
			}
			return dictionary;
		}
	}

	private static string GetPackagesIdsForCurrentPackageBlobBatch(int batchIndex, int batchSize, IList<PackageId> packageIds)
	{
		int num = batchIndex * batchSize;
		int num2 = (batchIndex + 1) * batchSize;
		int num3 = ((num2 - 1 > packageIds.Count - 1) ? (packageIds.Count - 1) : (num2 - 1));
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = num; i <= num3; i++)
		{
			stringBuilder.Append("'");
			stringBuilder.Append(((WrappedValue<Guid>)(object)packageIds[i]).Value);
			if (i != num3)
			{
				stringBuilder.Append("', ");
			}
			else
			{
				stringBuilder.Append("'");
			}
		}
		return stringBuilder.ToString();
	}

	public static Tuple<byte[], byte[]> GetObjectMetadataBlobBytes(NavAppObjectMetadataRecord record, NavAppObjectMetadataBlobFields blobsWeNeed)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		if (record.RecordBuffer == null)
		{
			throw new InvalidOperationException("Cannot read object metadata from a null record buffer");
		}
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000150);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(5);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(6);
		List<INavFieldMetadata> list = new List<INavFieldMetadata>();
		if (((Enum)blobsWeNeed).HasFlag((Enum)(object)(NavAppObjectMetadataBlobFields)1))
		{
			list.Add(fieldByNo);
		}
		if (((Enum)blobsWeNeed).HasFlag((Enum)(object)(NavAppObjectMetadataBlobFields)2))
		{
			list.Add(fieldByNo2);
		}
		byte[] array = null;
		byte[] array2 = null;
		if (list.Any())
		{
			int num = 2;
			bool flag;
			do
			{
				num--;
				flag = false;
				MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(record.RecordBuffer);
				NavSqlBlobCommand.GetBlob(metaTableById, NavGlobal.AppDatabase, 0, new FieldList(list.ToArray()), mutableRecordBuffer, locking: false);
				if (((Enum)blobsWeNeed).HasFlag((Enum)(object)(NavAppObjectMetadataBlobFields)1))
				{
					NavBLOB navBLOB = (NavBLOB)mutableRecordBuffer[fieldByNo.FieldIndex];
					array = navBLOB.GetBytes();
					if (array.Length == 0)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001LL", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Metadata blob query was empty for {0} {1} from package {2}", record.ObjectType, record.ObjectId, ((WrappedValue<Guid>)(object)record.AppPackageId).Value);
						flag = true;
					}
				}
				if (((Enum)blobsWeNeed).HasFlag((Enum)(object)(NavAppObjectMetadataBlobFields)2))
				{
					NavBLOB navBLOB2 = (NavBLOB)mutableRecordBuffer[fieldByNo2.FieldIndex];
					array2 = navBLOB2.GetBytes();
					if (array2.Length == 0)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001LM", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Code blob query was empty for {0} {1} from package {2}", record.ObjectType, record.ObjectId, ((WrappedValue<Guid>)(object)record.AppPackageId).Value);
						flag = true;
					}
				}
			}
			while (flag && num >= 0);
		}
		return Tuple.Create(array, array2);
	}

	public static byte[] GetResourceBlob(NavAppResourceRecord record)
	{
		if (record.RecordBuffer == null)
		{
			throw new InvalidOperationException("Cannot read resource metadata from a null record buffer");
		}
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000176);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(4);
		MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(record.RecordBuffer);
		NavSqlBlobCommand.GetBlob(metaTableById, NavGlobal.AppDatabase, 0, new FieldList(fieldByNo), mutableRecordBuffer, locking: false);
		NavBLOB navBLOB = (NavBLOB)mutableRecordBuffer[fieldByNo.FieldIndex];
		return navBLOB.GetBytes();
	}

	private static IEnumerable<NavAppMetadataRecord> GetMetadata(ISet<PackageId> targetPackageIds)
	{
		LinkedList<NavAppMetadataRecord> results = new LinkedList<NavAppMetadataRecord>();
		RunActionInAMaximizedSession(() =>
		{
			using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00000QI", DiagnosticParameter.SystemMetadata("Extension metadata query -- PackageIds:" + ((targetPackageIds == null) ? "null" : string.Join(",", targetPackageIds))), MonitorType.None, Category.Extensions, createNewActivity: false))
			{
				using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000160);
				int num = ((targetPackageIds != null) ? targetPackageIds.Count() : 0);
				switch (num)
				{
				case 1:
				{
					PackageId val = targetPackageIds.Single();
					navRecord.ALSetRange(1, NavGuid.CreateFromObject(((WrappedValue<Guid>)(object)val).Value));
					break;
				}
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
				case 10:
				case 11:
				case 12:
				case 13:
				case 14:
				case 15:
				case 16:
				case 17:
				case 18:
				case 19:
				case 20:
				case 21:
				case 22:
				case 23:
				case 24:
				case 25:
				case 26:
				case 27:
				case 28:
				case 29:
				case 30:
				case 31:
				case 32:
				case 33:
				case 34:
				case 35:
				case 36:
				case 37:
				case 38:
				case 39:
				case 40:
				case 41:
				case 42:
				case 43:
				case 44:
				case 45:
				case 46:
				case 47:
				case 48:
				case 49:
				case 50:
				case 51:
				case 52:
				case 53:
				case 54:
				case 55:
				case 56:
				case 57:
				case 58:
				case 59:
				case 60:
				case 61:
				case 62:
				case 63:
				case 64:
				case 65:
				case 66:
				case 67:
				case 68:
				case 69:
				case 70:
				case 71:
				case 72:
				case 73:
				case 74:
				case 75:
				case 76:
				case 77:
				case 78:
				case 79:
				case 80:
				case 81:
				case 82:
				case 83:
				case 84:
				case 85:
				case 86:
				case 87:
				case 88:
				case 89:
				case 90:
				case 91:
				case 92:
				case 93:
				case 94:
				case 95:
				case 96:
				case 97:
				case 98:
				case 99:
				case 100:
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 1; i <= num; i++)
					{
						stringBuilder.Append("%" + i);
						if (i != num)
						{
							stringBuilder.Append(" | ");
						}
					}
					NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(1);
					string expression = stringBuilder.ToString();
					NavGuid[] array = new NavGuid[num];
					int num2 = 0;
					foreach (PackageId targetPackageId in targetPackageIds)
					{
						array[num2] = new NavGuid(((WrappedValue<Guid>)(object)targetPackageId).Value);
						num2++;
					}
					NavValue[] values = array;
					navRecord.SetFilter(fieldByNo, expression, values);
					break;
				}
				}
				if (navRecord.ALFindSet(DataError.TrapError))
				{
					do
					{
						NavAppMetadataRecord navAppMetadataRecord = new NavAppMetadataRecord
						{
							SqlTimestamp = navRecord.GetFieldValue(0).ToInt64(),
							PackageId = PackageId.op_Implicit(navRecord.GetFieldValue(1).ToGuid()),
							AppId = AppId.op_Implicit(navRecord.GetFieldValue(2).ToGuid()),
							Name = navRecord.GetFieldValue(3).ToString(),
							Publisher = navRecord.GetFieldValue(4).ToString(),
							VersionMajor = navRecord.GetFieldValue(5).ToInt32(),
							VersionMinor = navRecord.GetFieldValue(6).ToInt32(),
							VersionBuild = navRecord.GetFieldValue(7).ToInt32(),
							VersionRevision = navRecord.GetFieldValue(8).ToInt32(),
							CompatibilityMajor = navRecord.GetFieldValue(9).ToInt32(),
							CompatibilityMinor = navRecord.GetFieldValue(10).ToInt32(),
							CompatibilityBuild = navRecord.GetFieldValue(11).ToInt32(),
							CompatibilityRevision = navRecord.GetFieldValue(12).ToInt32(),
							Dependencies = new NavAppDependencyRecord[0],
							ObjectMetadata = new NavAppObjectMetadataRecord[0],
							Resources = new NavAppResourceRecord[0],
							RecordBuffer = navRecord.RecordImplementation.CopyOfCurrentReadOnlyRecordBuffer(),
							PackageType = (NavAppPackageType)navRecord.GetFieldValue(25).ToInt32(),
							ContentHash = navRecord.GetFieldValue(27).ToString(),
							TenantId = navRecord.GetFieldValue(28).ToString(),
							ShowMyCode = navRecord.GetFieldValue(29).ToBoolean(),
							Scope = (NavAppScope)navRecord.GetFieldValue(30).ToInt32(),
							PackageHash = Sha256Hash.CreateFromHexString(navRecord.GetFieldValue(33).ToString())
						};
						List<NavAppDependencyRecord> dependencies = GetDependencies(navAppMetadataRecord.PackageId);
						List<NavAppObjectMetadataRecord> objectMetadata = GetObjectMetadata(navAppMetadataRecord.PackageId);
						List<NavAppResourceRecord> resources = GetResources(navAppMetadataRecord.PackageId);
						navAppMetadataRecord.Dependencies = dependencies.ToArray();
						navAppMetadataRecord.ObjectMetadata = objectMetadata.ToArray();
						navAppMetadataRecord.Resources = resources.ToArray();
						Version version = VersionHelper.Create(navAppMetadataRecord.VersionMajor, navAppMetadataRecord.VersionMinor, navAppMetadataRecord.VersionBuild, navAppMetadataRecord.VersionRevision);
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000QJ", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Extension metadata record -- PackageId:{0}; Name:{1}; Publisher:{2}; Version:{3}; ObjectMetadataCount:{4}", navAppMetadataRecord.PackageId, navAppMetadataRecord.Name, navAppMetadataRecord.Publisher, version, navAppMetadataRecord.ObjectMetadata.Length);
						results.AddLast(navAppMetadataRecord);
					}
					while (navRecord.ALNext(1) != 0);
				}
			}
		});
		return results;
	}

	private static List<NavAppDependencyRecord> GetDependencies(PackageId targetPackageId)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected Obj, but got Unknown
		List<NavAppDependencyRecord> list = new List<NavAppDependencyRecord>();
		using (NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000161))
		{
			navRecord.ALSetRange(1, NavGuid.CreateFromObject(((WrappedValue<Guid>)(object)targetPackageId).Value));
			if (navRecord.ALFindSet(DataError.TrapError))
			{
				do
				{
					NavAppDependencyRecord item = new NavAppDependencyRecord
					{
						PackageId = PackageId.op_Implicit(navRecord.GetFieldValue(1).ToGuid()),
						Id = AppId.op_Implicit(navRecord.GetFieldValue(2).ToGuid()),
						Name = navRecord.GetFieldValue(3).ToString(),
						Publisher = navRecord.GetFieldValue(4).ToString(),
						VersionMajor = navRecord.GetFieldValue(5).ToInt32(),
						VersionMinor = navRecord.GetFieldValue(6).ToInt32(),
						VersionBuild = navRecord.GetFieldValue(7).ToInt32(),
						VersionRevision = navRecord.GetFieldValue(8).ToInt32(),
						CompatibilityMajor = navRecord.GetFieldValue(9).ToInt32(),
						CompatibilityMinor = navRecord.GetFieldValue(10).ToInt32(),
						CompatibilityBuild = navRecord.GetFieldValue(11).ToInt32(),
						CompatibilityRevision = navRecord.GetFieldValue(12).ToInt32()
					};
					list.Add(item);
				}
				while (navRecord.ALNext(1) != 0);
			}
		}
		return list;
	}

	private static List<NavAppObjectMetadataRecord> GetObjectMetadata(PackageId targetPackageId)
	{
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		List<NavAppObjectMetadataRecord> list = new List<NavAppObjectMetadataRecord>();
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00000QK", DiagnosticParameter.SystemMetadata("Extension object metadata query -- PackageId:" + (object)targetPackageId), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000150);
			navRecord.ALSetRange(1, NavGuid.CreateFromObject(((WrappedValue<Guid>)(object)targetPackageId).Value));
			if (navRecord.ALFindSet(DataError.TrapError))
			{
				do
				{
					string text = navRecord.GetFieldValue(11).ToString();
					string text2 = navRecord.GetFieldValue(12).ToString();
					NavAppObjectMetadataRecord navAppObjectMetadataRecord = new NavAppObjectMetadataRecord
					{
						SqlTimestamp = navRecord.GetFieldValue(0).ToInt64(),
						AppPackageId = PackageId.op_Implicit(navRecord.GetFieldValue(1).ToGuid()),
						Format = (NavAppObjectFormat)navRecord.GetFieldValue(4).ToInt32(),
						MetadataVersion = navRecord.GetFieldValue(8).ToInt32(),
						ObjectType = (ObjectType)navRecord.GetFieldValue(2).ToInt32(),
						ObjectId = navRecord.GetFieldValue(3).ToInt32(),
						ObjectSubtype = navRecord.GetFieldValue(9).ToString(),
						ObjectName = navRecord.GetFieldValue(10).ToString(),
						MetadataHash = ((!string.IsNullOrEmpty(text)) ? Sha256Hash.CreateFromHexString(text) : null),
						UserCodeHash = ((!string.IsNullOrEmpty(text2)) ? Sha256Hash.CreateFromHexString(text2) : null),
						ObjectFlags = (NavAppObjectFlags)navRecord.GetFieldValue(13).ToInt32(),
						RecordBuffer = navRecord.RecordImplementation.CopyOfCurrentReadOnlyRecordBuffer()
					};
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000QL", Category.Extensions, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Extension object metadata record -- PackageId:{0}; Type:{1}; Id:{2}; Format:{3}; Subtype:{4}; Name:{5}", navAppObjectMetadataRecord.AppPackageId, navAppObjectMetadataRecord.ObjectType, navAppObjectMetadataRecord.ObjectId, navAppObjectMetadataRecord.Format, navAppObjectMetadataRecord.ObjectSubtype, navAppObjectMetadataRecord.ObjectName);
					list.Add(navAppObjectMetadataRecord);
				}
				while (navRecord.ALNext(1) != 0);
			}
		}
		return list;
	}

	private static List<NavAppResourceRecord> GetResources(PackageId targetPackageId)
	{
		List<NavAppResourceRecord> list = new List<NavAppResourceRecord>();
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001KQ", DiagnosticParameter.SystemMetadata("Extension resource query -- PackageId:" + (object)targetPackageId), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000176);
			navRecord.ALSetRange(1, NavGuid.CreateFromObject(((WrappedValue<Guid>)(object)targetPackageId).Value));
			if (navRecord.ALFindSet(DataError.TrapError))
			{
				do
				{
					NavAppResourceRecord navAppResourceRecord = new NavAppResourceRecord
					{
						SqlTimestamp = navRecord.GetFieldValue(0).ToInt64(),
						PackageId = PackageId.op_Implicit(navRecord.GetFieldValue(1).ToGuid()),
						Type = (NavAppResourceType)navRecord.GetFieldValue(2).ToInt32(),
						Name = navRecord.GetFieldValue(3).ToString(),
						Hash = Sha256Hash.CreateFromHexString(navRecord.GetFieldValue(5).ToString()),
						RecordBuffer = navRecord.RecordImplementation.CopyOfCurrentReadOnlyRecordBuffer()
					};
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001KP", Category.Extensions, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Extension object resource record -- PackageId:{0}; Type:{1}; Name:{2}", navAppResourceRecord.PackageId, navAppResourceRecord.Type, navAppResourceRecord.Name);
					list.Add(navAppResourceRecord);
				}
				while (navRecord.ALNext(1) != 0);
			}
		}
		return list;
	}

	private static void RunActionInAMaximizedSession(Action a)
	{
		if (NavCurrentThread.Session != null)
		{
			MaximizedAction();
			return;
		}
		ITenantSessionHandler tenant = NavGlobal.AppDatabase.Tenant;
		tenant.RunTenantActionInSystemSession((NavTenant navTenant, NavSession session) =>
		{
			MaximizedAction();
		});
		void MaximizedAction()
		{
			using (new MaximizedPermissionScope(NavCurrentThread.Session, SystemTablesReadFrom))
			{
				a();
			}
		}
	}
}
