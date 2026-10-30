using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.EventSubscription;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Extensions;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Exceptions.Apps;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

[DebuggerDisplay("{this.TableId}/{this.TableName} SqlName: {this.SqlTableName}")]
public class NCLMetaTable : NCLMetaApplicationObject, IAppOrigin
{
	private enum FieldNoToIndexType
	{
		NoMapExists,
		NoMapFieldNoIsIndex,
		MapFieldNoIsLookup,
		MapFieldNoIsHash
	}

	[Flags]
	private enum TableTriggers
	{
		Insert = 1,
		Modify = 2,
		Delete = 4,
		Rename = 8,
		Unknown = 0x10
	}

	[Flags]
	internal enum TableTypes
	{
		Tenant = 1,
		App = 2,
		System = 4,
		Virtual = 8,
		CrossTenant = 0x20
	}

	private delegate NavRecord CreateNavRecord(ITreeObject parent, NCLMetaTable metaTable, bool isTemporary, NavRecord sharedTable, string companyName, SecurityFiltering securityFiltering);

	internal enum FSpec
	{
		TableId,
		TableName,
		LookupFormId,
		ReadConsistency,
		RecordLevelLocking,
		Field,
		TableCaptions,
		Key
	}

	private class ReferencingRelationsEntry
	{
		public int BumperToken { get; }

		public NCLMetaFieldRelation[] Relations { get; }

		public ReferencingRelationsEntry(int bumperToken, NCLMetaFieldRelation[] relations)
		{
			BumperToken = bumperToken;
			Relations = relations;
		}
	}

	private class TableLoadResult
	{
		public MetaTable MetaTable { get; set; }

		public List<NCLTableExtension> Extensions { get; set; }

		public long Timestamp { get; set; }
	}

	public const int MaxFieldLen = 2048;

	public const int MaxFieldSize = 4096;

	private short[] fieldNoToIndexMap;

	private FieldNoToIndexType fieldNoToIndexType;

	private Dictionary<string, int> fieldIndexByName;

	private NCLMetaField[] blobFields;

	private int[] calculatedFields;

	private NCLMetaField timestampField;

	private NCLMetaField userdefinedTimestampField;

	private NCLMetaField autoIncrementField;

	private ReadOnlyCollection<NCLMetaField> sqlReadableFields;

	private int hasFlowFieldsSourcedFromVirtualTables;

	private string sqlTableName;

	private NCLMetaKey clusterIndex;

	private NavTableTriggerEventHandler tableTriggerEventHandler;

	private NavAppNCLCaptionStrings captionStrings = new NavAppNCLCaptionStrings();

	private bool? anySiftIndexes;

	private Guid? sourceAppId;

	private NavExtensionType sourceExtensionType;

	private List<NCLTableExtension> orderedExtensionObjects = new List<NCLTableExtension>();

	private List<NCLMetaApplicationObject> orderedExtensionObjectsBaseType = new List<NCLMetaApplicationObject>();

	private Dictionary<int, NCLTableExtension> extensionObjectsByObjectId = new Dictionary<int, NCLTableExtension>();

	private TableTriggers definedTriggers = TableTriggers.Unknown;

	private readonly AppGroupAwareLookup<ReferencingRelationsEntry> referencingRelationsLookup;

	private NCLMetaField fieldForUpdateWhenNoChanges;

	private int approximateRowByteSize = -1;

	private readonly ConcurrentDictionary<int, SortedList<string, int>> fieldCaptionLookupCache = new ConcurrentDictionary<int, SortedList<string, int>>();

	private readonly TableTypes tableTypes;

	private string obsoleteReason;

	private static readonly MethodInfo genericCreateFieldTriggerHandlerDelegateMethod = new Func<MethodInfo, Action<NavApplicationObjectBase>>(CreateFieldTriggerHandlerDelegate<NavApplicationObjectBase>).Method.GetGenericMethodDefinition();

	protected override Type ApplicationObjectConstructorDelegateType => typeof(CreateNavRecord);

	public override IList<NCLMetaApplicationObject> ExtensionObjects => orderedExtensionObjectsBaseType;

	public int TableId => ObjectId;

	public static int MaxSqlIndexColumns
	{
		get
		{
			if (!FeatureSwitches.Instance.SharedDB)
			{
				return 16;
			}
			return 15;
		}
	}

	public string TableName { get; protected set; }

	protected internal NCLCaptionStrings TableCaptions
	{
		get
		{
			if (ObjectId == 2000000071 || ObjectId == 2000000053 || ObjectId == 2000000045)
			{
				return captionStrings.GetCaptionStrings(0, CreateCaptionStringsForSpecialTables);
			}
			NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup();
			int groupId = navAppGroup.GroupId;
			return captionStrings.GetCaptionStrings(groupId, CreateCaptionStrings);
		}
	}

	internal string TableCaptionSafe
	{
		get
		{
			string text = string.Empty;
			if (TableCaptions != null)
			{
				text = TableCaptions.GetValueOrDefault();
			}
			if (string.IsNullOrEmpty(text))
			{
				text = TableName;
			}
			return text;
		}
	}

	public int LookupFormId { get; protected set; }

	public int DrillDownPageId { get; protected set; }

	public string DataCaptionFields { get; protected set; }

	public bool PasteIsValid { get; protected set; }

	public bool RecordLevelLocking { get; protected set; }

	public bool IsLinked { get; protected set; }

	public bool DataPerCompany { get; protected set; }

	public bool ReadConsistency { get; protected set; }

	public TableType TableType { get; protected set; }

	public string ExternalName { get; protected set; }

	public string ExternalSchema { get; protected set; }

	public bool IsAppArchiveTable { get; protected set; }

	public Guid? SourceAppId => sourceAppId;

	public NavExtensionType SourceExtensionType => sourceExtensionType;

	public NavAppOrigin AppOrigin
	{
		get
		{
			if (!sourceAppId.HasValue)
			{
				return NavAppOrigin.Base;
			}
			return NavAppOrigin.CreateForApp(sourceAppId);
		}
	}

	public int FieldCount => AllFields.Length;

	public bool IsExternalDataSource
	{
		get
		{
			if (!IsLinked)
			{
				return TableType != TableType.Normal;
			}
			return true;
		}
	}

	public ObsoleteState ObsoleteState { get; protected set; }

	public string ObsoleteReason
	{
		get
		{
			return obsoleteReason;
		}
		protected set
		{
			obsoleteReason = value ?? string.Empty;
		}
	}

	public ALDataClassification DataClassification { get; protected set; }

	public bool ReplicateData { get; protected set; }

	public CompressionType CompressionType { get; protected set; }

	public IEnumerable<NCLMetaField> Fields => AllFields;

	public IEnumerable<NCLMetaField> NormalFieldsOwnedByCompanionTables => NormalFields.Where((NCLMetaField f) => f.IsCompanionTableField);

	public IEnumerable<NavAppOrigin> DistinctCompanionTableOrigins => NormalFieldsOwnedByCompanionTables.Select((NCLMetaField f) => f.AppOrigin).Distinct();

	public bool IsSystemOrVirtualTable => (tableTypes & (TableTypes.System | TableTypes.Virtual)) != 0;

	public bool IsVirtualTable => (tableTypes & TableTypes.Virtual) == TableTypes.Virtual;

	public bool IsNonVirtualSystemTable => (tableTypes & (TableTypes.System | TableTypes.Virtual)) == TableTypes.System;

	public bool IsAppTable => (tableTypes & TableTypes.App) == TableTypes.App;

	public bool IsTenantTable => (tableTypes & TableTypes.Tenant) == TableTypes.Tenant;

	public bool IsMediaTable => TableId.In(2000000183, 2000000184, 2000000185, 2000000180, 2000000181);

	public bool IsCrossTenantTable
	{
		get
		{
			if (IsTenantTable)
			{
				return (tableTypes & TableTypes.CrossTenant) == TableTypes.CrossTenant;
			}
			return false;
		}
	}

	internal int ApproximateRowByteSize
	{
		get
		{
			if (approximateRowByteSize < 0)
			{
				approximateRowByteSize = SqlReadableFields.Sum((NCLMetaField f) => f.ApproximateByteSize) + AllFields.Length * 32;
			}
			return approximateRowByteSize;
		}
	}

	internal bool FieldNumbersAscending { get; private set; }

	public IEnumerable<NCLMetaField> NormalFields => FieldsFilteredByClass(FieldClass.Normal);

	public ReadOnlyCollection<NCLMetaField> SqlReadableFields
	{
		get
		{
			if (sqlReadableFields == null)
			{
				sqlReadableFields = new ReadOnlyCollection<NCLMetaField>(InternalSqlReadableFields().ToArray());
			}
			return sqlReadableFields;
		}
	}

	public int BlobFieldCount => blobFields?.Length ?? 0;

	internal NCLMetaField[] BlobFields => blobFields;

	public int MediaFieldCount => MediaFields?.Length ?? 0;

	internal NCLMetaField[] MediaFields { get; private set; }

	public int EnumFieldCount => EnumFields?.Length ?? 0;

	internal NCLMetaField[] EnumFields { get; private set; }

	public int[] CalculatedFields
	{
		get
		{
			if (calculatedFields == null)
			{
				List<int> list = new List<int>();
				foreach (NCLMetaField field in Fields)
				{
					if (field.FieldClass == FieldClass.FlowField || field.FieldNavType == NavType.BLOB)
					{
						list.Add(field.FieldNo);
					}
				}
				calculatedFields = list.ToArray();
			}
			return calculatedFields;
		}
	}

	public int KeyCount => Keys.Length;

	public NCLMetaKey PrimaryKey => Keys[0];

	public NCLMetaKey ClusterIndex
	{
		get
		{
			if (clusterIndex == null)
			{
				clusterIndex = Keys.First((NCLMetaKey key) => key.Clustered);
			}
			return clusterIndex;
		}
	}

	internal NCLMetaField TimestampField => timestampField;

	internal NCLMetaField UserDefinedTimestampField => userdefinedTimestampField;

	internal bool SqlHasTimestamp
	{
		get
		{
			if (IsExternalDataSource)
			{
				return userdefinedTimestampField != null;
			}
			return true;
		}
	}

	internal NCLMetaField AutoIncrementField => autoIncrementField;

	internal bool HasFlowFieldsSourcedFromVirtualTables
	{
		get
		{
			if (hasFlowFieldsSourcedFromVirtualTables == 0)
			{
				bool flag = AllFields.Any((NCLMetaField f) => f.FieldClass == FieldClass.FlowField && NavRecord.IsTableVirtual(f.CalculationFormula.TableId));
				hasFlowFieldsSourcedFromVirtualTables = ((!flag) ? 1 : 2);
				return flag;
			}
			return hasFlowFieldsSourcedFromVirtualTables == 2;
		}
	}

	internal bool AnySiftIndexes
	{
		get
		{
			if (!anySiftIndexes.HasValue)
			{
				anySiftIndexes = Keys.Any((NCLMetaKey key) => key.MaintainSumIndex);
			}
			return anySiftIndexes.Value;
		}
	}

	protected internal NCLMetaField[] AllFields { get; set; }

	protected internal NCLMetaField[] DisabledFields { get; set; }

	protected internal NCLMetaKey[] Keys { get; set; }

	public string SqlTableName
	{
		get
		{
			if (sqlTableName == null)
			{
				sqlTableName = ((!string.IsNullOrEmpty(ExternalName)) ? ExternalName : NavSqlStatementHelper.ConvertToSqlIdentifier(TableName));
			}
			return sqlTableName;
		}
	}

	public NCLMetaField FieldForUpdateWhenNoChanges
	{
		get
		{
			if (fieldForUpdateWhenNoChanges == null)
			{
				IEnumerable<NCLMetaField> inner = from _003C_003Eh__TransparentIdentifier0 in Keys.Where((NCLMetaKey key) => key.KeyIsActive && key.MaintainSqlIndex).Select((NCLMetaKey key) =>
					{
						ReadOnlyCollection<NCLMetaField> keyFieldsList = key.KeyFieldsList;
						IEnumerable<NCLMetaField> sqlIndexFields = key.SqlIndexFields;
						return new
						{
							key = key,
							keyFields = keyFieldsList.Concat(sqlIndexFields ?? Enumerable.Empty<NCLMetaField>()).Distinct()
						};
					})
					let siftFields = (key.MaintainSumIndex ? (key.SumFieldsList ?? FieldList.Empty) : FieldList.Empty).AsNclMetaFields()
					from indexedField in keyFields.Concat(siftFields)
					select indexedField;
				IEnumerable<NCLMetaField> source = from _003C_003Eh__TransparentIdentifier0 in SqlReadableFields.Where((NCLMetaField nCLMetaField) => nCLMetaField.FieldNclType != NavNclType.NavBlob && !nCLMetaField.FieldIsTimestamp && !nCLMetaField.FieldIsAutoIncrement).GroupJoin(inner, (NCLMetaField nCLMetaField) => nCLMetaField.FieldNo, (NCLMetaField indexedField) => indexedField.FieldNo, (NCLMetaField field2, IEnumerable<NCLMetaField> g) => new
					{
						field = field2,
						g = g
					})
					orderby _003C_003Eh__TransparentIdentifier0.g.Count()
					select _003C_003Eh__TransparentIdentifier0.field;
				fieldForUpdateWhenNoChanges = source.FirstOrDefault((NCLMetaField f) => !f.IsCompanionTableField);
			}
			return fieldForUpdateWhenNoChanges;
		}
	}

	public bool IsInsertTriggerDefined => (DefinedTriggers & TableTriggers.Insert) != 0;

	public bool IsModifyTriggerDefined => (DefinedTriggers & TableTriggers.Modify) != 0;

	public bool IsDeleteTriggerDefined => (DefinedTriggers & TableTriggers.Delete) != 0;

	public bool IsRenameTriggerDefined => (DefinedTriggers & TableTriggers.Rename) != 0;

	private TableTriggers DefinedTriggers
	{
		get
		{
			if (definedTriggers == TableTriggers.Unknown)
			{
				TableTriggers tableTriggers = (TableTriggers)0;
				tableTriggers = (TableTriggers)((int)tableTriggers | (IsTriggerImplemented<NavRecord>("OnInsert", isPublic: false) ? 1 : 0));
				tableTriggers = (TableTriggers)((int)tableTriggers | (IsTriggerImplemented<NavRecord>("OnModify", isPublic: false) ? 2 : 0));
				tableTriggers = (TableTriggers)((int)tableTriggers | (IsTriggerImplemented<NavRecord>("OnDelete", isPublic: false) ? 4 : 0));
				tableTriggers = (TableTriggers)((int)tableTriggers | (IsTriggerImplemented<NavRecord>("OnRename", isPublic: false) ? 8 : 0));
				foreach (NCLTableExtension orderedExtensionObject in orderedExtensionObjects)
				{
					if ((tableTriggers & TableTriggers.Insert) == 0 && (orderedExtensionObject.IsTriggerImplemented<NavRecordExtension>("OnBeforeInsert", isPublic: true) || orderedExtensionObject.IsTriggerImplemented<NavRecordExtension>("OnInsert", isPublic: true) || orderedExtensionObject.IsTriggerImplemented<NavRecordExtension>("OnAfterInsert", isPublic: true)))
					{
						tableTriggers |= TableTriggers.Insert;
					}
					if ((tableTriggers & TableTriggers.Modify) == 0 && (orderedExtensionObject.IsTriggerImplemented<NavRecordExtension>("OnBeforeModify", isPublic: true) || orderedExtensionObject.IsTriggerImplemented<NavRecordExtension>("OnModify", isPublic: true) || orderedExtensionObject.IsTriggerImplemented<NavRecordExtension>("OnAfterModify", isPublic: true)))
					{
						tableTriggers |= TableTriggers.Modify;
					}
					if ((tableTriggers & TableTriggers.Delete) == 0 && (orderedExtensionObject.IsTriggerImplemented<NavRecordExtension>("OnBeforeDelete", isPublic: true) || orderedExtensionObject.IsTriggerImplemented<NavRecordExtension>("OnDelete", isPublic: true) || orderedExtensionObject.IsTriggerImplemented<NavRecordExtension>("OnAfterDelete", isPublic: true)))
					{
						tableTriggers |= TableTriggers.Delete;
					}
					if ((tableTriggers & TableTriggers.Rename) == 0 && (orderedExtensionObject.IsTriggerImplemented<NavRecordExtension>("OnBeforeRename", isPublic: true) || orderedExtensionObject.IsTriggerImplemented<NavRecordExtension>("OnRename", isPublic: true) || orderedExtensionObject.IsTriggerImplemented<NavRecordExtension>("OnAfterRename", isPublic: true)))
					{
						tableTriggers |= TableTriggers.Rename;
					}
				}
				definedTriggers = tableTriggers;
			}
			return definedTriggers;
		}
	}

	internal NavTableTriggerEventHandler TableTriggerEventHandler => tableTriggerEventHandler;

	internal override NavTriggerEventHandler TriggerEventHandler => tableTriggerEventHandler;

	public MetadataExtension<MetaTable> MetaTableExtension { get; private set; }

	protected NCLMetaTable(INCLMetaApplicationObjectLoader loader, int tableId, NavAppGroup metadataAppGroup, int appDependencyOrder = -1)
		: base(loader, ObjectType.Table, tableId, metadataAppGroup, appDependencyOrder)
	{
		if (NavRecord.IsSystemTable(tableId))
		{
			tableTypes |= TableTypes.System;
			if (NavRecord.IsTableVirtual(tableId))
			{
				tableTypes |= TableTypes.Virtual;
			}
		}
		tableTypes |= (TableTypes)((!SystemTables.ApplicationDatabaseTables.Contains(tableId)) ? 1 : 2);
		tableTypes |= (TableTypes)(SystemTables.TenantDatabaseCrossTenantTables.Contains(tableId) ? 32 : 0);
		referencingRelationsLookup = AppGroupAwareLookup<ReferencingRelationsEntry>.CreateWithCustomRefreshLogic(ComputeReferencingRelations, DoReferencingRelationsNeedToBeRefreshed);
	}

	internal static NCLMetaTable CreateEmptyNCLMetaTable(INCLMetaApplicationObjectLoader loader, int tableId, NavAppGroup appGroup, int appDependencyOrder = -1)
	{
		return new NCLMetaTable(loader, tableId, appGroup, appDependencyOrder);
	}

	internal static NCLMetaTable CreateFromQuery(NCLMetaQuery nclMetaQuery, MetaQuery metaQuery)
	{
		if (nclMetaQuery == null)
		{
			throw new ArgumentNullException("nclMetaQuery");
		}
		if (metaQuery == null)
		{
			throw new ArgumentNullException("metaQuery");
		}
		NCLMetaTable nCLMetaTable = new NCLMetaTable(null, nclMetaQuery.ObjectId, NavAppGroup.BaseGroup)
		{
			TableType = TableType.Query,
			TableName = nclMetaQuery.Name,
			ExternalName = nclMetaQuery.Name,
			ReadConsistency = false,
			RecordLevelLocking = false,
			DataPerCompany = true
		};
		List<NCLMetaField> nclDefinedFields = new List<NCLMetaField>(nclMetaQuery.QueryDefinition.Columns.Count);
		List<MetaField> list = new List<MetaField>(nclMetaQuery.QueryDefinition.Columns.Count);
		List<MetaQueryColumn> source = metaQuery.DataItems.SelectMany((MetaQueryDataItem di) => di.QueryColumns).ToList();
		foreach (NCLMetaQueryColumn column in nclMetaQuery.QueryDefinition.Columns)
		{
			nclDefinedFields.Add(new NCLMetaField(column.ColumnIndex + 1, column.NclType, column.SqlDataType, column.NavDefinedLengthMetadata, column.FieldClass, column.Name, fieldActive: true, fieldAutoIncrement: false, fieldCompressed: false, fieldIsPartOfPrimaryKey: true, column.NavOptionMetadata, null, validateRelations: false, testRelations: false, null, null, nCLMetaTable, column.ColumnIndex + 1, column.ColumnIndex, column.Id.ToString(CultureInfo.InvariantCulture)));
			MetaQueryColumn metaQueryColumn = source.SingleOrDefault((MetaQueryColumn c) => c.Id == column.Id);
			list.Add(new MetaField
			{
				Id = column.ColumnIndex + 1,
				Type = column.NavType,
				ClrType = CommonTypeInformation.ResolveClrType(column.NavType).ToString(),
				Length = column.NavDefinedLengthMetadata,
				FieldClass = column.FieldClass,
				Name = column.Name,
				Enabled = true,
				AutoIncrement = false,
				OptionCaption = column.NavOptionMetadata?.OptionCaption,
				OptionCaptionML = column.NavOptionMetadata?.OptionCaptionML,
				OptionOrdinalValues = column.NavOptionMetadata?.OrdinalValues,
				OptionString = column.NavOptionMetadata?.OptionString,
				OptionValues = column.NavOptionMetadata?.OrdinalValues?.ToList(),
				CalcFormula = null,
				Validation = false,
				InitValue = column.InitValue.ToString(),
				ExternalName = column.Id.ToString(CultureInfo.InvariantCulture),
				Caption = (metaQueryColumn?.Caption ?? column.Caption),
				CaptionML = (metaQueryColumn?.CaptionML ?? column.Caption)
			});
		}
		NCLMetaField[] array = ((!nclMetaQuery.QueryDefinition.PositioningFields.Any()) ? nclDefinedFields.OrderBy((NCLMetaField f) => f.ColumnIndex).ToArray() : (from pf in nclMetaQuery.QueryDefinition.PositioningFields
			select nclDefinedFields.Single((NCLMetaField f) => f.ColumnIndex - 1 == pf.Field.ColumnIndex) into f
			orderby f.ColumnIndex
			select f).ToArray());
		nCLMetaTable.Keys = new NCLMetaKey[1]
		{
			new NCLMetaKey(nCLMetaTable, array, keyIsActive: true, 0, maintainSqlIndex: false, maintainSumIndex: false, clustered: false, array.Select((NCLMetaField f) => f.GetHashCode()).Aggregate(HashCodeHelper.CombineHashCodes), null, null, 0)
		};
		NCLMetaField item = new NCLMetaField(0, NavNclType.NavBigInteger, SqlDataType.Timestamp, 8, FieldClass.Normal, "timestamp", fieldActive: true, fieldAutoIncrement: false, fieldCompressed: false, fieldIsPartOfPrimaryKey: false, null, null, validateRelations: false, testRelations: false, null, string.Empty, nCLMetaTable, 0, 0);
		List<NCLMetaField> list2 = new List<NCLMetaField>(nclDefinedFields.Count) { item };
		list2.AddRange(nclDefinedFields);
		nCLMetaTable.AllFields = list2.ToArray();
		nCLMetaTable.DisabledFields = new NCLMetaField[0];
		nCLMetaTable.SetSystemFields();
		nCLMetaTable.timestampField = item;
		nCLMetaTable.BuildLookupMaps();
		MetaTable metaTable = new MetaTable
		{
			Id = nCLMetaTable.ObjectId,
			Name = nCLMetaTable.TableName,
			ExternalName = nCLMetaTable.ExternalName,
			TableType = nCLMetaTable.TableType,
			IsDataPerCompany = true,
			Caption = metaQuery.Caption,
			CaptionML = metaQuery.CaptionML
		};
		metaTable.Fields.AddRange(list);
		MetaKey metaKey = new MetaKey();
		metaKey.FieldRelations.AddRange(array.Select((NCLMetaField k) => new FieldMetadataRelation
		{
			Id = k.FieldNo,
			Name = k.FieldName
		}));
		metaTable.Keys.Add(metaKey);
		nCLMetaTable.MetaTableExtension = new MetadataExtension<MetaTable>(metaTable);
		return nCLMetaTable;
	}

	protected void SetSystemFields()
	{
		List<NCLMetaField> list = null;
		List<NCLMetaField> list2 = null;
		List<NCLMetaField> list3 = null;
		for (int i = 1; i < AllFields.Length; i++)
		{
			NCLMetaField nCLMetaField = AllFields[i];
			switch (nCLMetaField.FieldNclType)
			{
			case NavNclType.NavBlob:
				if (list == null)
				{
					list = new List<NCLMetaField>(AllFields.Length);
				}
				list.Add(nCLMetaField);
				break;
			case NavNclType.NavOption:
				if (nCLMetaField.FieldOptionMetadata.IsEnum)
				{
					if (list3 == null)
					{
						list3 = new List<NCLMetaField>(AllFields.Length);
					}
					list3.Add(nCLMetaField);
				}
				break;
			case NavNclType.NavMedia:
			case NavNclType.NavMediaSet:
				if (list2 == null)
				{
					list2 = new List<NCLMetaField>(AllFields.Length);
				}
				list2.Add(nCLMetaField);
				break;
			}
			if (nCLMetaField.FieldIsUserDefinedTimestamp)
			{
				userdefinedTimestampField = nCLMetaField;
			}
			_ = nCLMetaField.FieldNo;
			if (nCLMetaField.FieldIsAutoIncrement)
			{
				autoIncrementField = nCLMetaField;
			}
		}
		if (list != null && list.Count > 0)
		{
			blobFields = list.ToArray();
		}
		if (list2 != null && list2.Count > 0)
		{
			MediaFields = list2.ToArray();
		}
		if (list3 != null && list3.Count > 0)
		{
			EnumFields = list3.ToArray();
		}
	}

	protected void BuildLookupMaps()
	{
		int num = -1;
		bool flag = true;
		bool flag2 = true;
		for (int i = 0; i < AllFields.Length; i++)
		{
			NCLMetaField nCLMetaField = AllFields[i];
			int fieldNo = nCLMetaField.FieldNo;
			flag = flag && fieldNo == i;
			flag2 = flag2 && (flag || i == 0 || fieldNo > AllFields[i - 1].FieldNo);
			if (fieldNo > num)
			{
				num = fieldNo;
			}
		}
		FieldNumbersAscending = flag2;
		if (flag)
		{
			fieldNoToIndexType = FieldNoToIndexType.NoMapFieldNoIsIndex;
			return;
		}
		int num2 = (AllFields.Length * 3 / 2 + 16) / 17 * 17;
		if (num < 25 || num <= num2)
		{
			fieldNoToIndexType = FieldNoToIndexType.MapFieldNoIsLookup;
			num2 = num + 1;
		}
		else
		{
			fieldNoToIndexType = FieldNoToIndexType.MapFieldNoIsHash;
		}
		fieldNoToIndexMap = new short[num2];
		for (int j = 0; j < fieldNoToIndexMap.Length; j++)
		{
			fieldNoToIndexMap[j] = -1;
		}
		if (fieldNoToIndexType == FieldNoToIndexType.MapFieldNoIsLookup)
		{
			for (int k = 0; k < AllFields.Length; k++)
			{
				fieldNoToIndexMap[AllFields[k].FieldNo] = (short)k;
			}
			return;
		}
		for (int l = 0; l < AllFields.Length; l++)
		{
			int num3 = GetMapIndexFromFieldNo(AllFields[l].FieldNo);
			while (fieldNoToIndexMap[num3] >= 0)
			{
				num3++;
				if (num3 >= fieldNoToIndexMap.Length)
				{
					num3 = 0;
				}
			}
			fieldNoToIndexMap[num3] = (short)l;
		}
	}

	internal void SetTableCaptionStringsForTestPurposes(int appGroupId, NCLCaptionStrings newCaptionStrings)
	{
		captionStrings = new NavAppNCLCaptionStrings();
		captionStrings.GetCaptionStrings(appGroupId, (int groupId) => newCaptionStrings);
	}

	private NCLCaptionStrings CreateCaptionStringsForSpecialTables(int languageAppGroupId)
	{
		return NCLCaptionStrings.CreateNCLCaptionStrings(string.Empty, TableName);
	}

	private NCLCaptionStrings CreateCaptionStrings(int languageAppGroupId)
	{
		MetaTable metaTableClone = GetMetaTableClone();
		metaTableClone.MergeMultiLanguageTexts(languageAppGroupId, includeFields: false);
		return NCLCaptionStrings.CreateNCLCaptionStrings(metaTableClone.MergedCaptionMultiLanguage ?? string.Empty, metaTableClone.Name);
	}

	public IEnumerable<NCLMetaField> NormalFieldsForContext(NavAppTableContext tableContext)
	{
		if (tableContext.Equals(NavAppTableContext.Main) || NormalFields.Any((NCLMetaField f) => f.IsCompanionTableField && f.OriginatesFrom(tableContext)))
		{
			return NormalFields.Where((NCLMetaField f) => f.FieldBelongsInPhysicalTable(tableContext)).ToList().AsReadOnly();
		}
		return new ReadOnlyCollection<NCLMetaField>(new List<NCLMetaField>());
	}

	public ReadOnlyCollection<NCLMetaField> SqlReadableFieldsForContext(NavAppTableContext tableContext)
	{
		if (tableContext.Equals(NavAppTableContext.Main) || SqlReadableFields.Any((NCLMetaField f) => f.IsCompanionTableField && f.OriginatesFrom(tableContext)))
		{
			return SqlReadableFields.Where((NCLMetaField f) => f.FieldBelongsInPhysicalTable(tableContext)).ToList().AsReadOnly();
		}
		return new ReadOnlyCollection<NCLMetaField>(new List<NCLMetaField>());
	}

	protected override Type LoadClrType()
	{
		int tableId = TableId;
		if (tableId == 2000000071 || tableId == 2000001071)
		{
			return null;
		}
		return base.LoadClrType();
	}

	protected override void OnClrTypeLoaded()
	{
		if (ApplicationObjectClrType != null)
		{
			ResolveFieldTriggerHandlers();
		}
		base.OnClrTypeLoaded();
	}

	private void ResolveFieldTriggerHandlers()
	{
		ResolveFieldTriggerHandlersForType(ApplicationObjectClrType, 0);
		foreach (NCLTableExtension orderedExtensionObject in orderedExtensionObjects)
		{
			ResolveFieldTriggerHandlersForType(orderedExtensionObject.ApplicationObjectClrType, orderedExtensionObject.ObjectId);
		}
	}

	private void ResolveFieldTriggerHandlersForType(Type objectClrType, int extensionId)
	{
		Func<MethodInfo, Action<NavApplicationObjectBase>> func = null;
		MethodInfo[] methods = objectClrType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo.IsDefined(typeof(FieldTriggerHandlerAttribute), inherit: false))
			{
				if (func == null)
				{
					MethodInfo method = genericCreateFieldTriggerHandlerDelegateMethod.MakeGenericMethod(objectClrType);
					func = (Func<MethodInfo, Action<NavApplicationObjectBase>>)Delegate.CreateDelegate(typeof(Func<MethodInfo, Action<NavApplicationObjectBase>>), method);
				}
				FieldTriggerHandler<NavApplicationObjectBase> fieldTriggerHandler = new FieldTriggerHandler<NavApplicationObjectBase>(objectClrType, func(methodInfo));
				FieldTriggerHandlerAttribute customAttribute = methodInfo.GetCustomAttribute<FieldTriggerHandlerAttribute>(inherit: false);
				NCLMetaField nCLMetaField = ((extensionId > 0) ? GetFieldByNo(extensionId, customAttribute.FieldNo) : GetFieldByNo(customAttribute.FieldNo));
				switch (customAttribute.TriggerType)
				{
				case FieldTriggerType.OnLookup:
					nCLMetaField.LookupHandler = fieldTriggerHandler;
					break;
				case FieldTriggerType.OnBeforeValidate:
					nCLMetaField.OnBeforeValidateHandlers.Add(fieldTriggerHandler);
					break;
				case FieldTriggerType.OnValidate:
					nCLMetaField.ValidateHandler = fieldTriggerHandler;
					break;
				case FieldTriggerType.OnAfterValidate:
					nCLMetaField.OnAfterValidateHandlers.Add(fieldTriggerHandler);
					break;
				}
			}
		}
	}

	private static Action<NavApplicationObjectBase> CreateFieldTriggerHandlerDelegate<TRecord>(MethodInfo handlerMethod) where TRecord : NavApplicationObjectBase
	{
		Action<TRecord> handlerDelegate = (Action<TRecord>)Delegate.CreateDelegate(typeof(Action<TRecord>), null, handlerMethod);
		return (NavApplicationObjectBase record) =>
		{
			handlerDelegate((TRecord)record);
		};
	}

	public IEnumerable<NCLMetaField> FieldsFilteredByClass(FieldClass fieldClass)
	{
		foreach (NCLMetaField field in Fields)
		{
			if (field.FieldClass == fieldClass)
			{
				yield return field;
			}
		}
	}

	private IEnumerable<NCLMetaField> InternalSqlReadableFields()
	{
		foreach (NCLMetaField normalField in NormalFields)
		{
			if (normalField.FieldActive && (!normalField.FieldIsTimestamp || SqlHasTimestamp) && !normalField.FieldIsUserDefinedTimestamp)
			{
				yield return normalField;
			}
		}
	}

	internal int GetFieldIndexByNoNoException(int fieldNo)
	{
		switch (fieldNoToIndexType)
		{
		case FieldNoToIndexType.NoMapExists:
		{
			for (int i = 0; i < AllFields.Length; i++)
			{
				if (AllFields[i].FieldNo == fieldNo)
				{
					return i;
				}
			}
			break;
		}
		case FieldNoToIndexType.NoMapFieldNoIsIndex:
			if (fieldNo >= 0 && fieldNo < AllFields.Length)
			{
				return fieldNo;
			}
			break;
		case FieldNoToIndexType.MapFieldNoIsLookup:
			if (fieldNo >= 0 && fieldNo < fieldNoToIndexMap.Length)
			{
				return fieldNoToIndexMap[fieldNo];
			}
			break;
		case FieldNoToIndexType.MapFieldNoIsHash:
		{
			int num = GetMapIndexFromFieldNo(fieldNo);
			int num2;
			while (true)
			{
				num2 = fieldNoToIndexMap[num];
				if (num2 < 0)
				{
					return -1;
				}
				if (AllFields[num2].FieldNo == fieldNo)
				{
					break;
				}
				num++;
				if (num >= fieldNoToIndexMap.Length)
				{
					num = 0;
				}
			}
			return num2;
		}
		}
		return -1;
	}

	internal bool TryGetFieldIndexByNo(int fieldNo, out int fieldIndex)
	{
		fieldIndex = GetFieldIndexByNoNoException(fieldNo);
		return fieldIndex > 0;
	}

	internal int GetFieldIndexByNo(int fieldNo)
	{
		int fieldIndexByNoNoException = GetFieldIndexByNoNoException(fieldNo);
		if (fieldIndexByNoNoException < 0)
		{
			throw NavNCLFieldNotFoundException.Create(CultureInfo.CurrentCulture, fieldNo, TableCaptionSafe);
		}
		return fieldIndexByNoNoException;
	}

	public bool TryGetFieldByNo(int fieldNo, out NCLMetaField result)
	{
		int fieldIndexByNoNoException = GetFieldIndexByNoNoException(fieldNo);
		if (fieldIndexByNoNoException < 0)
		{
			result = null;
			return false;
		}
		result = AllFields[fieldIndexByNoNoException];
		return true;
	}

	public NCLMetaField GetFieldByNo(int fieldNo)
	{
		return GetFieldByNo(fieldNo, trapError: false);
	}

	private NCLMetaField GetFieldByNo(int fieldNo, bool trapError)
	{
		int fieldIndexByNoNoException = GetFieldIndexByNoNoException(fieldNo);
		if (fieldIndexByNoNoException < 0 && DisabledFields != null)
		{
			for (int i = 0; i < DisabledFields.Length; i++)
			{
				if (DisabledFields[i].FieldNo == fieldNo)
				{
					return DisabledFields[i];
				}
			}
		}
		if (fieldIndexByNoNoException < 0)
		{
			if (trapError)
			{
				return null;
			}
			throw NavNCLFieldNotFoundException.Create(CultureInfo.CurrentCulture, fieldNo, TableCaptionSafe);
		}
		NCLMetaField nCLMetaField = AllFields[fieldIndexByNoNoException];
		if (nCLMetaField == null)
		{
			throw new InvalidOperationException("field is null");
		}
		return nCLMetaField;
	}

	public NCLMetaField GetFieldByNo(int extensionObjectId, int fieldNo)
	{
		if (!extensionObjectsByObjectId.ContainsKey(extensionObjectId))
		{
			throw NavNCLExtensionFieldNotFoundException.Create(CultureInfo.CurrentCulture, extensionObjectId, fieldNo, TableCaptionSafe);
		}
		return GetFieldByNo(fieldNo);
	}

	public NCLMetaField GetFieldByIndex(int fieldIndex)
	{
		if (fieldIndex < 0 || fieldIndex >= AllFields.Length)
		{
			throw new NavNCLIndexOutOfBoundsException();
		}
		return AllFields[fieldIndex];
	}

	internal bool TryGetFieldIndexByName(string fieldName, out int fieldIndex)
	{
		if (fieldIndexByName == null)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>(FieldCount, StringComparer.OrdinalIgnoreCase);
			int num = 0;
			try
			{
				for (int i = 0; i < FieldCount; i++)
				{
					num = i;
					dictionary.Add(AllFields[i].FieldName, i);
				}
			}
			catch (ArgumentException)
			{
				throw new NavNCLArgumentException(string.Format(CultureInfo.InvariantCulture, "Table {1} {0} contains multiple fields that have the name {2}.", TableCaptionSafe, ObjectId, AllFields[num].FieldName));
			}
			fieldIndexByName = dictionary;
		}
		if (fieldIndexByName.TryGetValue(fieldName, out fieldIndex))
		{
			return true;
		}
		fieldIndex = -1;
		return false;
	}

	public bool TryGetFieldByName(string name, out NCLMetaField result)
	{
		if (TryGetFieldIndexByName(name, out var fieldIndex))
		{
			result = AllFields[fieldIndex];
			return true;
		}
		result = null;
		return false;
	}

	public bool TryGetFieldByCaption(string caption, out NCLMetaField result)
	{
		SortedList<string, int> orAdd = fieldCaptionLookupCache.GetOrAdd(NavCurrentThread.Session.LocalLanguage, (int lcid) =>
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>(FieldCount);
			NCLMetaField[] allFields = AllFields;
			foreach (NCLMetaField nCLMetaField in allFields)
			{
				string fieldCaption = nCLMetaField.FieldCaption;
				if (!string.Equals(fieldCaption, nCLMetaField.FieldName))
				{
					if (!dictionary.ContainsKey(fieldCaption))
					{
						dictionary.Add(fieldCaption, nCLMetaField.FieldIndex);
					}
					else
					{
						dictionary[fieldCaption] = -1;
					}
				}
			}
			return new SortedList<string, int>(dictionary, StringComparer.Create(CultureInfo.GetCultureInfo(lcid), ignoreCase: true));
		});
		if (orAdd.TryGetValue(caption, out var value))
		{
			if (value == -1)
			{
				throw new NavNCLFieldNotFoundException(string.Format(CultureInfo.CurrentCulture, Lang.CaptionAmbiguousBetweenFieldsOnTable, caption, TableCaptions.GetValueOrDefault()));
			}
			result = AllFields[value];
			return true;
		}
		result = null;
		return false;
	}

	public NCLMetaField GetFieldByName(string name)
	{
		if (TryGetFieldByName(name, out var result))
		{
			return result;
		}
		throw NavNCLFieldNotFoundException.Create(CultureInfo.CurrentCulture, name, TableCaptionSafe);
	}

	public int GetFieldNoByName(string name)
	{
		return GetFieldByName(name).FieldNo;
	}

	public int GetFieldNoByIndex(int fieldIndex)
	{
		return GetFieldByIndex(fieldIndex).FieldNo;
	}

	public NCLMetaKey GetKeyByIndex(int keyIndex)
	{
		if (keyIndex < 0 || keyIndex >= Keys.Length)
		{
			throw new NavNCLIndexOutOfBoundsException();
		}
		return Keys[keyIndex];
	}

	internal string GetTableCaption(int lcid)
	{
		return TableCaptions.GetValueOrDefault(lcid);
	}

	public NavRecord CreateObjectInstance(ITreeObject parent)
	{
		return CreateObjectInstance(parent, isTemporary: false, null, string.Empty, SecurityFiltering.Validated);
	}

	public NavRecord CreateObjectInstance(ITreeObject parent, bool isTemporary, NavRecord sharedTable, string companyName, SecurityFiltering securityFiltering)
	{
		CreateNavRecord createNavRecord = (CreateNavRecord)ApplicationObjectConstructor;
		NavRecord navRecord = ((createNavRecord == null) ? new NavRecord(parent, TableId, this, isTemporary, sharedTable, companyName, securityFiltering) : createNavRecord(parent, this, isTemporary, sharedTable, companyName, securityFiltering));
		if (navRecord != null && orderedExtensionObjects.Any())
		{
			foreach (NCLTableExtension orderedExtensionObject in orderedExtensionObjects)
			{
				orderedExtensionObject.CreateExtensionInstanceAndBindToParent(navRecord);
			}
		}
		return navRecord;
	}

	internal override bool CheckIsStale()
	{
		if (AllFields != null && AllFields.Any((NCLMetaField field) => (field.FieldClass == FieldClass.FlowField && field.CalculationFormula != null && field.CalculationFormula.CheckIsStale()) || (field.FieldClass == FieldClass.Normal && field.FieldRelations != null && field.FieldRelations.Any((NCLMetaFieldRelation fieldRelation) => fieldRelation.CheckIsStale()))))
		{
			return true;
		}
		IEnumerable<ReferencingRelationsEntry> enumerable = from r in referencingRelationsLookup.GetAllKnownReferences()
			select r.Item2;
		foreach (ReferencingRelationsEntry item in enumerable)
		{
			if (item.Relations.Any((NCLMetaFieldRelation r) => r.CheckIsStale()))
			{
				return true;
			}
		}
		return false;
	}

	public NCLMetaFieldRelation[] GetReferencingRelations()
	{
		return referencingRelationsLookup.GetReferenceUsingCurrentAppGroup().Relations;
	}

	private bool DoReferencingRelationsNeedToBeRefreshed(ReferencingRelationsEntry entry)
	{
		return entry.BumperToken != ObjectLoader.MetadataCache.BaseTableMetadataChangeBumperToken;
	}

	private ReferencingRelationsEntry ComputeReferencingRelations(NavAppGroup appGroup)
	{
		int baseTableMetadataChangeBumperToken = ObjectLoader.MetadataCache.BaseTableMetadataChangeBumperToken;
		ReadOnlyCollection<NCLMetaTable> snapshotOfAllNonVirtualMetaTables = ObjectLoader.MetadataCache.GetSnapshotOfAllNonVirtualMetaTables(appGroup.GroupId);
		NCLMetaFieldRelation[] relations = (from childTable in snapshotOfAllNonVirtualMetaTables.AsParallel()
			from childField in childTable.AllFields
			let relations = childField.FieldRelations
			where relations != null
			from relation in relations
			where relation.SourceTableId == TableId
			select relation).ToArray();
		return new ReferencingRelationsEntry(baseTableMetadataChangeBumperToken, relations);
	}

	public bool IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers globalTriggerWanted)
	{
		NavSystemCodeunitGlobalTriggers.Triggers triggersOnTable = NavCurrentThread.Session.SystemCodeunitFactory.GlobalTriggers.GetTriggersOnTable(TableId);
		return (triggersOnTable & globalTriggerWanted) != 0;
	}

	internal override void CreateAndSetTriggerEventHandler()
	{
		tableTriggerEventHandler = new NavTableTriggerEventHandler(this);
	}

	private int GetMapIndexFromFieldNo(int fieldNo)
	{
		return (fieldNo & 0xFFFFFFF) * 7 % fieldNoToIndexMap.Length;
	}

	public NCLMetaField FindFieldMatch(string fieldIdentifier, bool prefixFallback = false, bool trapError = false)
	{
		string s;
		if (fieldIdentifier.StartsWith("Field", StringComparison.OrdinalIgnoreCase))
		{
			s = fieldIdentifier.Substring(5);
		}
		else
		{
			s = ((!fieldIdentifier.StartsWith("#", StringComparison.Ordinal)) ? fieldIdentifier : fieldIdentifier.Substring(1));
		}
		if (int.TryParse(s, out var result))
		{
			return GetFieldByNo(result, trapError);
		}
		if (TryGetFieldByName(fieldIdentifier, out var result2) || TryGetFieldByCaption(fieldIdentifier, out result2))
		{
			return result2;
		}
		if (prefixFallback)
		{
			result2 = AllFields.FirstOrDefault((NCLMetaField f) => f.FieldName.StartsWith(fieldIdentifier, StringComparison.OrdinalIgnoreCase)) ?? AllFields.FirstOrDefault((NCLMetaField f) => f.FieldCaption.StartsWith(fieldIdentifier, StringComparison.CurrentCultureIgnoreCase));
			if (result2 != null)
			{
				return result2;
			}
		}
		if (trapError)
		{
			return null;
		}
		throw NavNCLFieldNotFoundException.Create(CultureInfo.CurrentCulture, fieldIdentifier, TableCaptions.GetValueOrDefault());
	}

	internal static MetaTable GetStaticTableMetadata(int id)
	{
		string s;
		switch (id)
		{
		case 2000000001:
			s = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><MetaTable xmlns=\"urn:schemas-microsoft-com:dynamics:NAV:MetaObjects\" ID=\"2000000001\" IsVirtual=\"1\" Name=\"Object\" DataPerCompany=\"1\">  <Fields>    <Field ID=\"1\" Datatype=\"Option\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Type\" OptionString=\"TableData,Table,,Report,,Codeunit,XMLport,MenuSuite,Page,Query,System,FieldNumber\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"2\" Datatype=\"OemText\" DataLength=\"30\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Company Name\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\">      <TableRelations TableID=\"2000000006\" TableName=\"Company\" FieldID=\"1\" />    </Field>    <Field ID=\"3\" Datatype=\"Integer\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"ID\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"4\" Datatype=\"OemText\" DataLength=\"30\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Name\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"5\" Datatype=\"Boolean\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Modified\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"6\" Datatype=\"Boolean\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Compiled\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"7\" Datatype=\"BLOB\" SubType=\"UserDefined\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"BLOB Reference\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"8\" Datatype=\"Integer\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"BLOB Size\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"9\" Datatype=\"Integer\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"DBM Table No.\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"10\" Datatype=\"Date\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Date\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"11\" Datatype=\"Time\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Time\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"12\" Datatype=\"OemText\" DataLength=\"248\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Version List\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"40\" Datatype=\"Boolean\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Locked\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"50\" Datatype=\"OemText\" DataLength=\"132\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Locked By\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"20\" Datatype=\"Text\" DataLength=\"250\" Enabled=\"1\" FieldClass=\"FlowField\" Name=\"Caption\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\">      <CalculationFormula FlowType=\"Lookup\" ReverseSign=\"0\" Table=\"2000000058\" Field=\"20\">        <TableFilters>          <Filter FilterGroup=\"0\" FieldID=\"1\" FilterType=\"FIELD\" ValueIsFilter=\"0\" OnlyMaxLimit=\"0\" FilterValue=\"1\" />          <Filter FilterGroup=\"0\" FieldID=\"3\" FilterType=\"FIELD\" ValueIsFilter=\"0\" OnlyMaxLimit=\"0\" FilterValue=\"3\" />        </TableFilters>      </CalculationFormula>    </Field>  </Fields>  <Keys>    <Key Enabled=\"1\" Key=\"Field1,Field2,Field3\" MaintainSQLIndex=\"1\" MaintainSIFTIndex=\"1\" Clustered=\"0\" />    <Key Enabled=\"1\" Key=\"Field1,Field4\" MaintainSQLIndex=\"1\" MaintainSIFTIndex=\"1\" Clustered=\"0\" />    <Key Enabled=\"1\" Key=\"Field40,Field50\" MaintainSQLIndex=\"1\" MaintainSIFTIndex=\"1\" Clustered=\"0\" />  </Keys>  <FieldGroups /></MetaTable>";
			break;
		case 2000000071:
			s = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><MetaTable xmlns=\"urn:schemas-microsoft-com:dynamics:NAV:MetaObjects\" ID=\"2000000071\" CaptionML=\"ENU=Object Metadata\" DataPerCompany=\"0\" Name=\"Object Metadata\">  <Fields>    <Field ID=\"3\" Datatype=\"Option\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Object Type\" CaptionML=\"ENU=Object Type\" OptionString=\"TableData,Table,,Report,,Codeunit,XMLport,MenuSuite,Page,Query,System,FieldNumber\" OptionCaptionML=\"ENU=TableData,Table,,Report,,Codeunit,XMLport,MenuSuite,Page,Query,System,FieldNumber\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"6\" Datatype=\"Integer\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Object ID\" CaptionML=\"ENU=Object ID\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"0\" ExtendedDatatype=\"None\">      <TableRelations TableID=\"2000000001\" TableName=\"Object\" FieldID=\"3\">         <Filters FilterGroup=\"0\" FieldID=\"1\" FilterType=\"FIELD\" FilterValue=\"3\" />      </TableRelations>    </Field>    <Field ID=\"9\" Datatype=\"BLOB\" SubType=\"UserDefined\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Metadata\" CaptionML=\"ENU=Metadata\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"15\" Datatype=\"BLOB\" SubType=\"UserDefined\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"User Code\" CaptionML=\"ENU=User Code\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"18\" Datatype=\"BLOB\" SubType=\"UserDefined\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"User AL Code\" CaptionML=\"ENU=User AL Code\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"27\" Datatype=\"Integer\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Metadata Version\" CaptionML=\"ENU=Metadata Version\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"30\" Datatype=\"Text\" DataLength=\"32\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Hash\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"33\" Datatype=\"Text\" DataLength=\"30\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Object Subtype\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"34\" Datatype=\"Boolean\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Has Subscribers\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"35\" Datatype=\"BLOB\" SubType=\"UserDefined\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Symbol Reference\" CaptionML=\"ENU=Symbol Reference\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"36\" Datatype=\"Integer\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Schema Hash\" CaptionML=\"ENU=Schema Hash\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />  </Fields>  <Keys>    <Key Enabled=\"1\" Key=\"Field3,Field6\" />  </Keys>  <FieldGroups /></MetaTable>";
			break;
		case 2000001071:
			s = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><MetaTable xmlns=\"urn:schemas-microsoft-com:dynamics:NAV:MetaObjects\" ID=\"2000001071\" CaptionML=\"ENU=Object Metadata\" DataPerCompany=\"0\" Name=\"Object Metadata Snapshot\">  <Fields>    <Field ID=\"3\" Datatype=\"Option\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Object Type\" CaptionML=\"ENU=Object Type\" OptionString=\"TableData,Table,,Report,,Codeunit,XMLport,MenuSuite,Page,Query,System,FieldNumber\" OptionCaptionML=\"ENU=TableData,Table,,Report,,Codeunit,XMLport,MenuSuite,Page,Query,System,FieldNumber\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"6\" Datatype=\"Integer\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Object ID\" CaptionML=\"ENU=Object ID\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"0\" ExtendedDatatype=\"None\" />    <Field ID=\"9\" Datatype=\"BLOB\" SubType=\"UserDefined\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Metadata\" CaptionML=\"ENU=Metadata\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"10\" Datatype=\"Text\" DataLength=\"30\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Name\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"11\" Datatype=\"Boolean\" DataLength=\"4\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Data Per Company\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"30\" Datatype=\"Text\" DataLength=\"100\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Hash\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"33\" Datatype=\"BLOB\" SubType=\"UserDefined\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Base Metadata\" CaptionML=\"ENU=Base Metadata\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"36\" Datatype=\"Text\" DataLength=\"32\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Base Hash\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" AutoIncrement=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"39\" Datatype=\"Integer\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Table Blob Version\" CaptionML=\"ENU=Table Blob Version\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />    <Field ID=\"40\" Datatype=\"Integer\" Enabled=\"1\" FieldClass=\"Normal\" Name=\"Schema Hash\" CaptionML=\"ENU=Schema Hash\" BlankNumbers=\"DontBlank\" BlankZero=\"0\" SignDisplacement=\"0\" Editable=\"1\" NotBlank=\"0\" Numeric=\"0\" DateFormula=\"0\" ClosingDates=\"0\" Title=\"0\" ValidateTableRelation=\"1\" TestTableRelation=\"1\" ExtendedDatatype=\"None\" />  </Fields>  <Keys>    <Key Enabled=\"1\" Key=\"Field3,Field6\" />  </Keys>  <FieldGroups /></MetaTable>";
			break;
		case 2000000024:
		case 2000000042:
		case 2000000046:
		case 2000000047:
		case 2000000070:
		case 2000000077:
		case 2000000090:
			throw new NavCSideException(string.Format(CultureInfo.InvariantCulture, "Table {0} is not available and cannot be opened.", id));
		default:
			return null;
		}
		XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
		xmlDocument.Load(XmlHelper.SafeXmlReader(new StringReader(s)));
		return MetaTable.CreateMetaTableFromXml(xmlDocument.DocumentElement, 0, 0L);
	}

	protected override void LoadMetadata()
	{
		base.LoadMetadata();
		TableLoadResult tableLoadResult = LoadTableMetadata();
		orderedExtensionObjects = tableLoadResult.Extensions;
		orderedExtensionObjectsBaseType = tableLoadResult.Extensions.Cast<NCLMetaApplicationObject>().ToList();
		extensionObjectsByObjectId = tableLoadResult.Extensions.ToDictionary((NCLTableExtension o) => o.ObjectId);
		AssignFromMetaTable(tableLoadResult.MetaTable, databaseConvertedFromNav2013: false);
		Timestamp = tableLoadResult.Timestamp;
	}

	public override void ValidateMetadata()
	{
		base.ValidateMetadata();
		LoadTableMetadata();
	}

	private static void EnsureAppChangesHaveNotMadeFatalChangesToMetaTable(MetaTable mt)
	{
		EnsureNoFieldIdsOccurMoreThanOnce(mt);
		EnsureNoFieldNamesOccurMoreThanOnce(mt);
	}

	private static void EnsureNoFieldNamesOccurMoreThanOnce(MetaTable mt)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (MetaField field in mt.Fields)
		{
			if (hashSet.Contains(field.Name))
			{
				string message = string.Format(CultureInfo.InvariantCulture, "The field name '{0}' can only appear once in the table '{1}'.", field.Name, mt.Name);
				throw NavAppObjectMetadataException.Create(message);
			}
			hashSet.Add(field.Name);
		}
	}

	private static void EnsureNoFieldIdsOccurMoreThanOnce(MetaTable mt)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (MetaField field in mt.Fields)
		{
			if (hashSet.Contains(field.Id))
			{
				string message = string.Format(CultureInfo.InvariantCulture, "The field ID '{0}' can only appear once in the table '{1}'.", field.Id, mt.Name);
				throw NavAppObjectMetadataException.Create(message);
			}
			hashSet.Add(field.Id);
		}
	}

	internal void AssignFromMetaTable(MetaTable metaTable, bool databaseConvertedFromNav2013, bool isAppArchiveTable = false)
	{
		MetaTableExtension = new MetadataExtension<MetaTable>(metaTable);
		TableType = metaTable.TableType;
		TableName = metaTable.Name;
		LookupFormId = metaTable.LookupFormId;
		ReadConsistency = false;
		RecordLevelLocking = true;
		DataPerCompany = metaTable.IsDataPerCompany && ObjectId < 2000000001 && TableType == TableType.Normal;
		IsLinked = metaTable.IsLinked;
		DrillDownPageId = metaTable.DrillDownFormId;
		DataCaptionFields = string.Empty;
		ExternalName = metaTable.ExternalName;
		ExternalSchema = metaTable.ExternalSchema;
		IsAppArchiveTable = isAppArchiveTable;
		PasteIsValid = metaTable.PasteIsValid;
		ObsoleteState = metaTable.ObsoleteState;
		ObsoleteReason = metaTable.ObsoleteReason ?? string.Empty;
		DataClassification = metaTable.DataClassification;
		ReplicateData = metaTable.ReplicateData;
		CompressionType = metaTable.CompressionType;
		if (Guid.TryParse(metaTable.SourceAppId, out var result))
		{
			sourceAppId = result;
		}
		sourceExtensionType = (NavExtensionType)metaTable.SourceExtensionType;
		bool flag = true;
		foreach (int dataCaptionField in metaTable.DataCaptionFields)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				DataCaptionFields += ",";
			}
			DataCaptionFields += dataCaptionField.ToString(CultureInfo.InvariantCulture);
		}
		int num = metaTable.Fields.Count + 1;
		AllFields = new NCLMetaField[num];
		int num2 = 1;
		int num3 = 0;
		short num4 = 1;
		int num5 = 1;
		MetaField[] array = metaTable.Keys[0].FieldRelations.Select((FieldMetadataRelation fieldRelation) => metaTable.Fields.First((MetaField field) => field.Id == fieldRelation.Id)).ToArray();
		MetaField[] source = metaTable.Fields.Except(array).ToArray();
		foreach (MetaField item in array.Concat(source.Where((MetaField field) => field.FieldClass == FieldClass.Normal)).Concat(source.Where((MetaField field) => field.FieldClass == FieldClass.FlowFilter)).Concat(source.Where((MetaField field) => field.FieldClass == FieldClass.FlowField)))
		{
			NCLMetaField nCLMetaField = NCLMetaField.CreateFromMetaField(this, item, num5 <= array.Length, num2, num4);
			if (!nCLMetaField.FieldActive)
			{
				if (DisabledFields == null)
				{
					DisabledFields = new NCLMetaField[num];
				}
				DisabledFields[num3] = nCLMetaField;
				num3++;
				num5++;
			}
			else
			{
				AllFields[num2] = nCLMetaField;
				num2++;
				num4++;
				num5++;
			}
		}
		if (num3 > 0)
		{
			NCLMetaField[] array2 = AllFields;
			NCLMetaField[] array3 = DisabledFields;
			Array.Resize(ref array2, num2);
			Array.Resize(ref array3, num3);
			AllFields = array2;
			DisabledFields = array3;
		}
		SetSystemFields();
		string externalName = ((IsExternalDataSource && userdefinedTimestampField != null) ? userdefinedTimestampField.ExternalName : null);
		timestampField = (AllFields[0] = new NCLMetaField(0, NavNclType.NavBigInteger, SqlDataType.Timestamp, 8, FieldClass.Normal, "timestamp", fieldActive: true, fieldAutoIncrement: false, fieldCompressed: false, fieldIsPartOfPrimaryKey: false, null, null, validateRelations: false, testRelations: false, null, string.Empty, this, 0, 0, externalName));
		BuildLookupMaps();
		Keys = new NCLMetaKey[metaTable.Keys.Count];
		bool flag2 = false;
		int num6 = 0;
		while (!flag2 && num6 < Keys.Length)
		{
			flag2 = metaTable.Keys[num6].Clustered;
			num6++;
		}
		int num7 = 0;
		for (int num8 = 0; num8 < Keys.Length; num8++)
		{
			MetaKey metaKey = metaTable.Keys[num8];
			Keys[num8] = NCLMetaKey.CreateFromMetaKey(this, metaKey, num8, !flag2 && num8 == 0, databaseConvertedFromNav2013 ? num7 : num8);
			if (metaKey.Enabled)
			{
				num7++;
			}
		}
	}

	internal MetaTable GetMetaTableOriginal()
	{
		return MetaTableExtension?.Item;
	}

	public MetaTable GetMetaTableClone()
	{
		MetaTable metaTable = (MetaTable)(GetMetaTableOriginal()?.Clone());
		ApplyAppGroupAwareEnumMetadata(metaTable);
		return metaTable;
	}

	private void ApplyAppGroupAwareEnumMetadata(MetaTable metaTable)
	{
		if (metaTable == null || EnumFieldCount <= 0)
		{
			return;
		}
		NCLMetaField[] enumFields = EnumFields;
		foreach (NCLMetaField nCLMetaField in enumFields)
		{
			MetaField fieldsById = metaTable.GetFieldsById(nCLMetaField.FieldNo);
			if (fieldsById != null)
			{
				fieldsById.OptionString = nCLMetaField.FieldOptionMetadata.OptionString;
				fieldsById.OptionValues = nCLMetaField.FieldOptionMetadata.OrdinalValues.ToList();
				fieldsById.OptionCaption = nCLMetaField.FieldOptionMetadata.OptionCaption;
				fieldsById.OptionCaptionML = nCLMetaField.FieldOptionMetadata.OptionCaptionML;
				fieldsById.MergedOptionCaptionML = fieldsById.OptionCaptionML;
			}
		}
	}

	private TableLoadResult LoadTableMetadata()
	{
		MetaTable metaTable = GetStaticTableMetadata(ObjectId);
		List<NCLTableExtension> extensions = new List<NCLTableExtension>();
		if (metaTable == null)
		{
			metaTable = ObjectLoader.XmlMetadataLoader.GetMetaTable(ObjectId, MetadataAppGroup).Value;
			if (metaTable != null)
			{
				extensions = GetExtensionObjects<NCLTableExtension>(MetadataAppGroup).ToList();
				ApplyExtensionObjects(extensions, metaTable);
				metaTable.MergeMultiLanguageTexts(MetadataAppGroup.GroupId, includeFields: true, force: true);
			}
		}
		EnsureAppChangesHaveNotMadeFatalChangesToMetaTable(metaTable);
		return new TableLoadResult
		{
			MetaTable = metaTable,
			Extensions = extensions,
			Timestamp = (metaTable?.Timestamp ?? 0)
		};
	}
}
