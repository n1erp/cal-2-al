using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlTableAndChange
{
	private readonly NCLMetaTable oldTable;

	private readonly TableChangeType change;

	private readonly NCLMetaTable newTable;

	private readonly NavText newHash;

	private readonly NavText baseHash;

	private readonly ReadOnlyCollection<NavSqlFieldAndChanges> fieldsAndChanges;

	private readonly ReadOnlyCollection<NavSqlIndexAndChanges> indexesAndChanges;

	private readonly bool changesAffectSqlSchema;

	private int dataTransferFromCompanyToken;

	private readonly NavBLOB newMetadata;

	private readonly NavBLOB oldMetadata;

	private readonly NavBLOB baseMetadata;

	private readonly bool changedSortingProperties;

	private readonly bool databaseConvertedFromNav2013;

	private readonly TablePartitionMetadata tablePartitionMetadata;

	private readonly int schemaHash;

	private bool? changeRequireRebuildTable;

	public NavTableSyncSetup NavTableSyncSetup { get; set; }

	public NavAppTableContext TableContext { get; }

	public TablePartitionMetadata TablePartitionMetadata => tablePartitionMetadata;

	internal bool ChangeCanCauseDataLoss
	{
		get
		{
			if (!changesAffectSqlSchema || change == TableChangeType.Missing || change == TableChangeType.None)
			{
				return false;
			}
			if (change == TableChangeType.Excessive || fieldsAndChanges.Any((NavSqlFieldAndChanges fieldAndChange) => fieldAndChange.IsUnsafeChange))
			{
				return true;
			}
			if (indexesAndChanges.Any((NavSqlIndexAndChanges indexAndChange) => indexAndChange.IsPotentialUniqueConstraintViolation))
			{
				return true;
			}
			return false;
		}
	}

	private bool ChangeRequireRebuildTable
	{
		get
		{
			if (!changeRequireRebuildTable.HasValue)
			{
				changeRequireRebuildTable = change == TableChangeType.Missing;
				if (!changeRequireRebuildTable.Value && changesAffectSqlSchema)
				{
					FieldChangeTypes alterNotPossibleChangeTypes = FieldChangeTypes.AutoIncrement | FieldChangeTypes.DataType | FieldChangeTypes.Sorting | FieldChangeTypes.SqlDataType;
					changeRequireRebuildTable = change == TableChangeType.Excessive || DataPerCompanyChanged || fieldsAndChanges.Any((NavSqlFieldAndChanges fieldAndChange) => (fieldAndChange.Changes & alterNotPossibleChangeTypes) != 0) || indexesAndChanges.Any((NavSqlIndexAndChanges indexAndChange) => indexAndChange.ClusterIndex || indexAndChange.PrimaryKey);
				}
			}
			return changeRequireRebuildTable.Value;
		}
	}

	public bool ChangesAffectSqlSchema => changesAffectSqlSchema;

	public NCLMetaTable OldTable => oldTable;

	public NCLMetaTable NewTable => newTable;

	public NavText NewHash => newHash;

	public NavText BaseHash => baseHash;

	public int SchemaHash => schemaHash;

	public bool ChangedSortingProperties => changedSortingProperties;

	public bool DatabaseConvertedFromNav2013 => databaseConvertedFromNav2013;

	internal TableChangeType ChangeType => change;

	public bool Missing => change == TableChangeType.Missing;

	public bool Excessive => change == TableChangeType.Excessive;

	public bool Changed => change == TableChangeType.Changed;

	public virtual DataUpgradeSetup DataUpgradeSetup
	{
		get
		{
			if (NavTableSyncSetup != null)
			{
				return NavTableSyncSetup.SyncSetup;
			}
			return DataUpgradeSetup.None;
		}
	}

	public bool AllowDeletionOfFieldsAndTablesWithContent
	{
		get
		{
			if (NavTableSyncSetup != null)
			{
				if (NavTableSyncSetup.SyncSetup != DataUpgradeSetup.ForceSync && NavTableSyncSetup.SyncSetup != DataUpgradeSetup.OldPopulatedNewEmpty)
				{
					return NavTableSyncSetup.SyncSetup == DataUpgradeSetup.OldAndNewPopulated;
				}
				return true;
			}
			return false;
		}
	}

	public bool AllowDeleteTableContentOnNonUniqueKey
	{
		get
		{
			if (NavTableSyncSetup != null)
			{
				if (NavTableSyncSetup.SyncSetup != DataUpgradeSetup.ForceSync)
				{
					return NavTableSyncSetup.SyncSetup == DataUpgradeSetup.OldPopulatedNewEmpty;
				}
				return true;
			}
			return false;
		}
	}

	public bool AllowDataLossOnChangeFromPerCompanyToGlobal
	{
		get
		{
			if (NavTableSyncSetup != null)
			{
				if (NavTableSyncSetup.SyncSetup != DataUpgradeSetup.ForceSync)
				{
					return NavTableSyncSetup.SyncSetup == DataUpgradeSetup.OldPopulatedNewEmpty;
				}
				return true;
			}
			return false;
		}
	}

	public virtual NCLMetaTable DataUpgradeTable => NavTableSyncSetup.UpgradeTableMetadata;

	public ReadOnlyCollection<NavSqlFieldAndChanges> FieldsAndChanges => fieldsAndChanges;

	public ReadOnlyCollection<NavSqlIndexAndChanges> IndexesAndChanges => indexesAndChanges;

	public int DataTransferFromCompanyToken
	{
		get
		{
			return dataTransferFromCompanyToken;
		}
		set
		{
			if (dataTransferFromCompanyToken != value && dataTransferFromCompanyToken != -1)
			{
				throw new InvalidOperationException("dataTransferFromCompanyToken already set");
			}
			dataTransferFromCompanyToken = value;
		}
	}

	public bool RebuildTable
	{
		get
		{
			if (!ChangeRequireRebuildTable)
			{
				if (NavTableSyncSetup != null)
				{
					if (NavTableSyncSetup.SyncSetup != DataUpgradeSetup.OldAndNewPopulated)
					{
						return NavTableSyncSetup.SyncSetup == DataUpgradeSetup.OldPopulatedNewEmpty;
					}
					return true;
				}
				return false;
			}
			return true;
		}
	}

	public bool DataPerCompanyChanged
	{
		get
		{
			if (!Missing && !Excessive)
			{
				return oldTable?.DataPerCompany != newTable?.DataPerCompany;
			}
			return false;
		}
	}

	public bool TableRenamed
	{
		get
		{
			if (!Missing && !Excessive && oldTable != null && newTable != null)
			{
				return !oldTable.TableName.Equals(newTable.TableName);
			}
			return false;
		}
	}

	public bool CompressionChanged
	{
		get
		{
			if (!Missing && !Excessive && oldTable?.CompressionType != newTable?.CompressionType)
			{
				NCLMetaTable nCLMetaTable = newTable;
				if (nCLMetaTable == null)
				{
					return true;
				}
				return nCLMetaTable.CompressionType != CompressionType.Unspecified;
			}
			return false;
		}
	}

	public NavBLOB NewMetadata => newMetadata;

	public NavBLOB OldMetadata => oldMetadata;

	public NavBLOB BaseMetadata => baseMetadata;

	public bool IsUpgradeChange { get; }

	public NavSqlTableAndChange(NCLMetaTable oldTable, TableChangeType change, NCLMetaTable newTable, NavText newHash, NavText baseHash, int schemaHash, bool changedSortingProperties, bool databaseConvertedFromNav2013, NavBLOB newMetadata = null, NavBLOB oldMetadata = null, NavBLOB baseMetadata = null, bool isUpgradeChange = false, TablePartitionMetadata tablePartitionMetadata = null, bool tableIsAlreadyPartitioned = false)
	{
		this.oldTable = oldTable;
		this.change = change;
		this.newTable = newTable;
		this.newHash = newHash;
		this.baseHash = baseHash;
		this.schemaHash = schemaHash;
		this.newMetadata = newMetadata;
		this.oldMetadata = oldMetadata;
		this.baseMetadata = baseMetadata;
		this.changedSortingProperties = changedSortingProperties;
		this.databaseConvertedFromNav2013 = databaseConvertedFromNav2013;
		this.tablePartitionMetadata = tablePartitionMetadata;
		TableContext = NavAppTableContext.Main;
		if (change == TableChangeType.Changed || change == TableChangeType.Excessive)
		{
			changesAffectSqlSchema = CheckChangeAffectSqlSchema(oldTable, newTable, changedSortingProperties, databaseConvertedFromNav2013, out fieldsAndChanges, out indexesAndChanges);
			if (this.tablePartitionMetadata != null && !tableIsAlreadyPartitioned)
			{
				changesAffectSqlSchema = true;
				changeRequireRebuildTable = true;
			}
		}
		dataTransferFromCompanyToken = -1;
		IsUpgradeChange = isUpgradeChange;
	}

	private NavSqlTableAndChange(NCLMetaTable oldTable, NCLMetaTable newTable, NavAppTableContext tableContext, TableChangeType change, IEnumerable<NavSqlFieldAndChanges> fieldAndChanges, IEnumerable<NavSqlIndexAndChanges> indexAndChanges)
	{
		this.oldTable = oldTable;
		this.newTable = newTable;
		TableContext = tableContext;
		this.change = change;
		tablePartitionMetadata = null;
		fieldsAndChanges = new ReadOnlyCollection<NavSqlFieldAndChanges>(fieldAndChanges.ToList());
		indexesAndChanges = new ReadOnlyCollection<NavSqlIndexAndChanges>(indexAndChanges.ToList());
		dataTransferFromCompanyToken = -1;
		changesAffectSqlSchema = newTable == null;
		changesAffectSqlSchema |= oldTable?.DataPerCompany != newTable?.DataPerCompany;
		changesAffectSqlSchema |= oldTable?.CompressionType != newTable?.CompressionType;
		changesAffectSqlSchema |= !string.Equals(oldTable?.TableName, newTable?.TableName);
		changesAffectSqlSchema |= fieldsAndChanges.Any((NavSqlFieldAndChanges f) => f.Changes != FieldChangeTypes.None);
		changesAffectSqlSchema |= indexesAndChanges.Any((NavSqlIndexAndChanges i) => i.Changes != IndexChangeTypes.None);
	}

	public static NavSqlTableAndChange CreateForAppSync(NCLMetaTable oldTable, NCLMetaTable newTable, NavAppTableContext tableContext, TableChangeType change, IEnumerable<NavSqlFieldAndChanges> fieldAndChanges, IEnumerable<NavSqlIndexAndChanges> indexAndChanges)
	{
		return new NavSqlTableAndChange(oldTable, newTable, tableContext, change, fieldAndChanges, indexAndChanges);
	}

	internal NavSqlTableAndChange CloneWithNewPartition(TablePartitionMetadata newTablePartitionMetadata, bool tableIsPartitioned)
	{
		return new NavSqlTableAndChange(oldTable, change, newTable, newHash, baseHash, schemaHash, changedSortingProperties, databaseConvertedFromNav2013, newMetadata, oldMetadata, baseMetadata, IsUpgradeChange, newTablePartitionMetadata, tableIsPartitioned)
		{
			DataTransferFromCompanyToken = DataTransferFromCompanyToken
		};
	}

	public static bool CheckChangeAffectSqlSchema(NCLMetaTable oldTable, NCLMetaTable newTable, bool changedSortingProperties, bool databaseConvertedFromNav2013, out ReadOnlyCollection<NavSqlFieldAndChanges> fieldsAndChanges, out ReadOnlyCollection<NavSqlIndexAndChanges> indexesAndChanges)
	{
		if (newTable == null)
		{
			fieldsAndChanges = null;
			indexesAndChanges = null;
			return true;
		}
		fieldsAndChanges = NavSqlMetadata.GetFieldsAndChanges(oldTable, newTable, changedSortingProperties);
		indexesAndChanges = NavSqlMetadata.GetIndexesAndChanges(oldTable, newTable, fieldsAndChanges, databaseConvertedFromNav2013);
		if (oldTable.DataPerCompany != newTable.DataPerCompany || !oldTable.TableName.Equals(newTable.TableName))
		{
			return true;
		}
		if (oldTable.CompressionType != newTable.CompressionType)
		{
			return true;
		}
		if (fieldsAndChanges.Any((NavSqlFieldAndChanges fieldAndChange) => fieldAndChange.Changes != FieldChangeTypes.None))
		{
			return true;
		}
		return indexesAndChanges.Any((NavSqlIndexAndChanges indexAndChange) => indexAndChange.Changes != IndexChangeTypes.None);
	}

	private void ValidateDataPerCompanyChangesWithSchemaSynchOption()
	{
		if (Changed && DataUpgradeTable != null && OldTable.DataPerCompany != DataUpgradeTable.DataPerCompany)
		{
			throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.DataPerCompanyMustBeTheSame, OldTable.TableName, DataUpgradeTable.TableName, NavBoolean.Create(OldTable.DataPerCompany).ToString(), NavBoolean.Create(DataUpgradeTable.DataPerCompany).ToString()));
		}
	}

	private void ValidatePrimaryKeyChangesWithSchemaSynchOption()
	{
		if (!Changed || DataUpgradeSetup != DataUpgradeSetup.OldAndNewPopulated)
		{
			return;
		}
		int fieldNo = -1;
		string fieldName = string.Empty;
		NavSqlFieldAndChanges navSqlFieldAndChanges = FieldsAndChanges.FirstOrDefault((NavSqlFieldAndChanges fieldAndChange) =>
		{
			if (OldTable.PrimaryKey.KeyFieldCount == 1 && fieldAndChange.IsUnsafeChange && fieldAndChange.OldField.FieldIsPartOfPrimaryKey)
			{
				fieldNo = fieldAndChange.OldField.FieldNo;
				fieldName = fieldAndChange.OldField.FieldName;
				return true;
			}
			if (NewTable.PrimaryKey.KeyFieldCount == 1 && fieldAndChange.Changes == FieldChangeTypes.Missing && fieldAndChange.NewField.FieldIsPartOfPrimaryKey)
			{
				fieldNo = fieldAndChange.NewField.FieldNo;
				fieldName = fieldAndChange.NewField.FieldName;
				return true;
			}
			return false;
		});
		if (navSqlFieldAndChanges != null)
		{
			throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.PrimaryKeyDestrChangesOptionCopy, OldTable.TableId, OldTable.TableName, fieldNo, fieldName));
		}
	}

	private void ValidateTableDeletedWithSchemaSynchOption()
	{
		if (DataUpgradeSetup == DataUpgradeSetup.OldAndNewPopulated && NewTable == null)
		{
			throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.TableSyncSetupCopyNotValidWhenNoNewExists, OldTable.TableId));
		}
	}

	private void ValidateTableIdsAreDifferent()
	{
		if (DataUpgradeTable.TableId == OldTable.TableId)
		{
			throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.IllegalUpgradeTableId, OldTable.TableName, OldTable.TableId));
		}
	}

	private void ValidateUpgradeTableDesign()
	{
		IReadOnlyCollection<string> readOnlyCollection = NavSqlMetadata.CheckUpgradeTableDesign(this);
		if (readOnlyCollection.Any())
		{
			string text = ((DataUpgradeSetup == DataUpgradeSetup.OldAndNewPopulated) ? Lang.SchemaSynchronizationOriginalAndUpgradeTableDesignIncompatibleInOldAndNewPopulatedMode : Lang.SchemaSynchronizationOriginalAndUpgradeTableDesignIncompatibleInOldPopulatedNewEmptyMode) + Environment.NewLine;
			throw new NavSqlDatabaseSyncException(text + string.Join(Environment.NewLine, readOnlyCollection).TrimEnd(Environment.NewLine.ToCharArray()));
		}
	}

	internal void ValidateTableSchemasSupportMoving()
	{
		ValidateTableIdsAreDifferent();
		ValidateTableDeletedWithSchemaSynchOption();
		ValidatePrimaryKeyChangesWithSchemaSynchOption();
		ValidateDataPerCompanyChangesWithSchemaSynchOption();
		ValidateUpgradeTableDesign();
	}
}
