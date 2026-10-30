using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaKey : IAppOrigin
{
	private readonly NCLMetaTable parent;

	private readonly NCLMetaField[] keyFields;

	private readonly bool keyIsActive;

	private readonly int keyIndex;

	private readonly bool maintainSqlIndex;

	private readonly bool maintainSumIndex;

	private readonly bool clustered;

	private readonly bool unique;

	private readonly int compilerHashCode;

	private ReadOnlyCollection<NCLMetaField> keyFieldsList;

	private readonly NCLMetaField[] sumFields;

	private FieldList sumFieldsList;

	private readonly ReadOnlyCollection<NCLMetaField> sqlIndexFieldsList;

	private FieldList definedKeyFields;

	private SortingFieldList definedSortingFields;

	private FieldList keyFieldsWithPrimaryKeyFields;

	private SortingFieldList sortingFieldsWithPrimarykeyFields;

	private readonly int sqlIndexNo;

	private readonly List<NavAppTableContext> distinctTableContextsAffectedByKey;

	private string obsoleteReason;

	private readonly string keyName;

	public const int MaxKeyFieldCount = 20;

	public NCLMetaTable Parent => parent;

	public int KeyIndex => keyIndex;

	public int SqlIndexNo => sqlIndexNo;

	public string KeyName => keyName;

	public bool KeyIsActive => keyIsActive;

	public bool KeyIsObsoleted => ObsolescenceGuard.ObsolescenceIsEffective(ObsoleteState);

	public bool MaintainSqlIndex
	{
		get
		{
			if (keyIsActive)
			{
				return maintainSqlIndex;
			}
			return false;
		}
	}

	public bool MaintainSumIndex
	{
		get
		{
			if (keyIsActive && maintainSumIndex && sumFields != null)
			{
				return sumFields.Length != 0;
			}
			return false;
		}
	}

	public bool Clustered => clustered;

	public bool Unique => unique;

	public bool IsPrimaryKey => keyIndex == 0;

	public int KeyFieldCount => keyFields.Length;

	public Guid? SourceAppId { get; }

	public NavExtensionType SourceExtensionType { get; }

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

	public NavAppOrigin AppOrigin
	{
		get
		{
			if (!SourceAppId.HasValue)
			{
				return NavAppOrigin.Base;
			}
			return NavAppOrigin.CreateForApp(SourceAppId);
		}
	}

	public bool IsCompanionTableKey
	{
		get
		{
			if (SourceExtensionType == NavExtensionType.ModernDev)
			{
				Guid? sourceAppId = SourceAppId;
				Guid? sourceAppId2 = Parent.SourceAppId;
				if (sourceAppId.HasValue != sourceAppId2.HasValue)
				{
					return true;
				}
				if (!sourceAppId.HasValue)
				{
					return false;
				}
				return sourceAppId.GetValueOrDefault() != sourceAppId2.GetValueOrDefault();
			}
			return false;
		}
	}

	public NavAppTableContext OriginatingTableContext
	{
		get
		{
			if (!IsCompanionTableKey)
			{
				return NavAppTableContext.Main;
			}
			return NavAppTableContext.Companion(AppOrigin);
		}
	}

	public IEnumerable<NavAppTableContext> DistinctTableContextsAffectedByKey => distinctTableContextsAffectedByKey;

	public int CompilerHashCode => compilerHashCode;

	public ReadOnlyCollection<NCLMetaField> KeyFieldsList
	{
		get
		{
			if (keyFieldsList == null)
			{
				keyFieldsList = new ReadOnlyCollection<NCLMetaField>(keyFields);
			}
			return keyFieldsList;
		}
	}

	public ReadOnlyCollection<NCLMetaField> SqlIndexFields => sqlIndexFieldsList;

	public FieldList SumFieldsList
	{
		get
		{
			if (sumFields == null)
			{
				return null;
			}
			if (sumFieldsList == null)
			{
				INavFieldMetadata[] fields = sumFields;
				return sumFieldsList = new FieldList(fields);
			}
			return sumFieldsList;
		}
	}

	internal FieldList DefinedKeyFields
	{
		get
		{
			FieldList fieldList = definedKeyFields;
			if (fieldList == null)
			{
				INavFieldMetadata[] fields = keyFields;
				fieldList = (definedKeyFields = new FieldList(fields));
			}
			return fieldList;
		}
	}

	internal SortingFieldList DefinedSortingFields
	{
		get
		{
			if (definedSortingFields == null)
			{
				definedSortingFields = SortingFieldList.Create(DefinedKeyFields);
			}
			return definedSortingFields;
		}
	}

	internal FieldList KeyFieldsWithPrimaryKeyFields
	{
		get
		{
			if (IsPrimaryKey)
			{
				return DefinedKeyFields;
			}
			if (keyFieldsWithPrimaryKeyFields == null)
			{
				keyFieldsWithPrimaryKeyFields = DefinedKeyFields.UnionFieldsAssumingSameParent(parent.PrimaryKey.DefinedKeyFields);
				if (keyFieldsWithPrimaryKeyFields.Count > NCLMetaTable.MaxSqlIndexColumns)
				{
					FieldList source = new FieldList(keyFieldsWithPrimaryKeyFields.ToArray());
					int num = (from navFieldMetadata in source.Skip(NCLMetaTable.MaxSqlIndexColumns)
						where ((NCLMetaField)navFieldMetadata).FieldIsPartOfPrimaryKey
						select navFieldMetadata).Count();
					FieldList fieldList = new FieldList(source.Take(NCLMetaTable.MaxSqlIndexColumns - num).ToArray());
					keyFieldsWithPrimaryKeyFields = fieldList.UnionFieldsAssumingSameParent(parent.PrimaryKey.DefinedKeyFields);
				}
			}
			return keyFieldsWithPrimaryKeyFields;
		}
	}

	internal SortingFieldList SortingFieldsWithPrimaryKeyFields
	{
		get
		{
			if (sortingFieldsWithPrimarykeyFields == null)
			{
				sortingFieldsWithPrimarykeyFields = SortingFieldList.Create(KeyFieldsWithPrimaryKeyFields);
			}
			return sortingFieldsWithPrimarykeyFields;
		}
	}

	public NCLMetaKey(NCLMetaTable parent, NCLMetaField[] keyFields, bool keyIsActive, int keyIndex, bool maintainSqlIndex, bool maintainSumIndex, bool clustered, int compilerHashCode, NCLMetaField[] sumFields, ReadOnlyCollection<NCLMetaField> sqlIndexFieldsList, int sqlIndexNo, string sourceAppId = null, int sourceExtensionType = 0, string keyName = null, ObsoleteState obsoleteState = ObsoleteState.No, string obsoleteReason = "", bool unique = false)
	{
		this.parent = parent;
		this.keyFields = keyFields;
		this.keyIsActive = keyIsActive;
		this.keyIndex = keyIndex;
		this.maintainSqlIndex = maintainSqlIndex;
		this.maintainSumIndex = maintainSumIndex;
		this.clustered = clustered;
		this.unique = unique;
		this.compilerHashCode = compilerHashCode;
		this.sumFields = sumFields;
		this.sqlIndexFieldsList = sqlIndexFieldsList;
		this.sqlIndexNo = sqlIndexNo;
		this.keyName = keyName;
		distinctTableContextsAffectedByKey = keyFields.Select((NCLMetaField f) => f.OriginatingTableContext).Distinct().ToList();
		if (Guid.TryParse(sourceAppId, out var result))
		{
			SourceAppId = result;
		}
		SourceExtensionType = (NavExtensionType)sourceExtensionType;
		ObsoleteState = obsoleteState;
		ObsoleteReason = obsoleteReason;
	}

	public static NCLMetaKey CreateFromMetaKey(NCLMetaTable parentTable, MetaKey metaKey, int keyIndex, bool clusteredOverride, int sqlIndexNo)
	{
		NCLMetaField[] array = new NCLMetaField[(metaKey.FieldRelations != null) ? metaKey.FieldRelations.Count : 0];
		int num = 199;
		for (int i = 0; i < array.Length; i++)
		{
			num ^= 13 * (int)(array[i] = parentTable.GetFieldByNo(metaKey.FieldRelations[i].Id)).FieldNavType * (i + 1);
		}
		NCLMetaField[] array2 = null;
		if (metaKey.SumIndexFields.Count > 0)
		{
			array2 = new NCLMetaField[metaKey.SumIndexFields.Count];
			for (int j = 0; j < array2.Length; j++)
			{
				NCLMetaField fieldByNo = parentTable.GetFieldByNo(metaKey.SumIndexFields[j].Id);
				array2[j] = fieldByNo;
			}
		}
		NCLMetaField[] array3 = null;
		if (metaKey.MaintainSqlIndex && metaKey.SqlIndexFields.Count > 0)
		{
			array3 = new NCLMetaField[metaKey.SqlIndexFields.Count];
			for (int k = 0; k < array3.Length; k++)
			{
				NCLMetaField fieldByNo2 = parentTable.GetFieldByNo(metaKey.SqlIndexFields[k].Id);
				array3[k] = fieldByNo2;
			}
		}
		return new NCLMetaKey(parentTable, array, metaKey.Enabled, keyIndex, metaKey.MaintainSqlIndex, metaKey.MaintainSiftIndex, clusteredOverride || metaKey.Clustered, num, array2, (array3 == null) ? null : new ReadOnlyCollection<NCLMetaField>(array3), sqlIndexNo, metaKey.SourceAppId, metaKey.SourceExtensionType, metaKey.KeyName, metaKey.ObsoleteState, metaKey.ObsoleteReason, metaKey.Unique);
	}

	public NCLMetaField GetKeyFieldByIndex(int index)
	{
		if (index < 0 || index >= keyFields.Length)
		{
			throw new NavNCLIndexOutOfBoundsException();
		}
		return keyFields[index];
	}

	public override int GetHashCode()
	{
		return (parent.TableId << 8) + keyIndex;
	}

	public string GetKeyName()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < KeyFieldCount; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", GetKeyFieldByIndex(i).FieldName);
		}
		return stringBuilder.ToString();
	}
}
