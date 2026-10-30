using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.EventSubscription;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NCLMetaField : INavFieldMetadata, INavValueMetadata, INavSortingField, ISizableObject, IAppOrigin
{
	[Flags]
	private enum FieldFlags
	{
		None = 0,
		ValidateRelations = 1,
		Active = 2,
		AutoIncrement = 4,
		Compressed = 8,
		IsPartOfPrimaryKey = 0x10,
		UserDefinedTimestamp = 0x20,
		TestRelations = 0x40
	}

	private class ExternalMetadata
	{
		internal static readonly ExternalMetadata Default = new ExternalMetadata();

		internal string Name { get; private set; }

		internal string ExternalType { get; private set; }

		internal ExternalAccess Access { get; private set; }

		internal int[] OptionOrdinalValues { get; private set; }

		internal BlobSubType BlobSubType { get; private set; }

		private ExternalMetadata()
		{
		}

		internal static ExternalMetadata Create(string name, string externalType, ExternalAccess access, int[] optionOrdinalValues, BlobSubType blobSubType)
		{
			if (name != null || externalType != null || access != ExternalAccess.Full || optionOrdinalValues != null || blobSubType != BlobSubType.UserDefined)
			{
				return new ExternalMetadata
				{
					Name = name,
					ExternalType = externalType,
					Access = access,
					OptionOrdinalValues = optionOrdinalValues,
					BlobSubType = blobSubType
				};
			}
			return Default;
		}
	}

	public const int TimestampFieldIndexAndNo = 0;

	public const SortOrder DefaultSortOrder = SortOrder.Ascending;

	public const string TimestampColumnName = "timestamp";

	private readonly int fieldNo;

	private readonly NavNclType fieldNclType;

	private readonly SqlDataType sqlType;

	private readonly FieldFlags fieldFlags;

	private readonly int fieldDefinedLength;

	private readonly FieldClass fieldClass;

	private readonly string fieldName;

	private readonly NavAppNCLCaptionStrings captionStrings = new NavAppNCLCaptionStrings();

	private readonly NCLOptionMetadata fieldOptionMetadata;

	private readonly NCLMetaCalculationFormula calculationFormula;

	private readonly ReadOnlyCollection<NCLMetaFieldRelation> fieldRelations;

	private NavValue emptyValue;

	private FilterExpressionContext expressionContext;

	private ExternalMetadata externalMetadata;

	private Guid? sourceAppId;

	private Lazy<SqlDataType> flowFilterSqlDataType;

	private readonly NCLMetaTable parent;

	private readonly int fieldIndex;

	private readonly int sqlColumnIndex;

	private readonly string initialValueText;

	private NavValue initValue;

	private string sqlColumnName;

	private NavEventScope[] eventScopes;

	private string obsoleteReason;

	private ALDataClassification dataClassification;

	public NCLMetaTable Parent => parent;

	public int FieldIndex => fieldIndex;

	public int FieldNo => fieldNo;

	public bool FieldActive => (fieldFlags & FieldFlags.Active) != 0;

	public bool FieldObsoleted => ObsolescenceGuard.ObsolescenceIsEffective(ObsoleteState);

	public NavType FieldNavType => NavNclTypeHelper.GetNavTypeFromNclType(fieldNclType);

	public NavNclType FieldNclType => fieldNclType;

	public bool CanBeAggregated
	{
		get
		{
			if (fieldNclType != NavNclType.NavDecimal && fieldNclType != NavNclType.NavInteger && (fieldNclType != NavNclType.NavBigInteger || SqlDataType != SqlDataType.BigInt))
			{
				return fieldNclType == NavNclType.NavDuration;
			}
			return true;
		}
	}

	public Type FieldClrType => CommonTypeInformation.ResolveClrType(FieldNavType);

	public bool ValidateRelations => (fieldFlags & FieldFlags.ValidateRelations) != 0;

	public bool TestRelations => (fieldFlags & FieldFlags.TestRelations) != 0;

	public int FieldDefinedLength => fieldDefinedLength;

	public FieldClass FieldClass => fieldClass;

	public NavValue InitValue
	{
		get
		{
			if (FieldNclType == NavNclType.NavBlob)
			{
				return NavBLOB.Default();
			}
			if (initValue == null)
			{
				initValue = (string.IsNullOrEmpty(initialValueText) ? NavValue.GetDefaultNavValue(this, IsNullable) : ALSystemVariable.EvaluateIntoNavValue(DataError.ThrowError, this, initialValueText, 9));
			}
			return initValue;
		}
	}

	internal bool IsNullable => Parent.TableType == TableType.CRM;

	public NavValue EmptyValue
	{
		get
		{
			if (FieldNclType == NavNclType.NavBlob)
			{
				return NavBLOB.Default();
			}
			if (emptyValue == null)
			{
				emptyValue = NavValue.GetDefaultNavValue(this);
			}
			return emptyValue;
		}
	}

	public string FieldName => fieldName;

	public NCLOptionMetadata FieldOptionMetadata => fieldOptionMetadata;

	public string FieldCaption
	{
		get
		{
			switch (Parent.ObjectId)
			{
			case 2000000045:
			case 2000000053:
			case 2000000071:
			{
				NCLCaptionStrings nCLCaptionStrings = captionStrings.GetCaptionStrings(0, CreateCaptionStringsForSpecialTables);
				return nCLCaptionStrings.GetValueOrDefault();
			}
			case 2000000007:
				if (FieldNo == 0)
				{
					return FieldName;
				}
				return MetaField.GetDateSystemTableCaption(FieldNo);
			default:
			{
				NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup();
				int groupId = navAppGroup.GroupId;
				NCLCaptionStrings nCLCaptionStrings = captionStrings.GetCaptionStrings(groupId, CreateCaptionStrings);
				return nCLCaptionStrings.GetValueOrDefault();
			}
			}
		}
	}

	public int FieldDataSize
	{
		get
		{
			switch (fieldNclType)
			{
			case NavNclType.NavMedia:
			case NavNclType.NavMediaSet:
			case NavNclType.NavGuid:
				return 16;
			case NavNclType.NavBigInteger:
			case NavNclType.NavDuration:
				return 8;
			case NavNclType.NavBlob:
				return 8;
			case NavNclType.NavBoolean:
			case NavNclType.NavInteger:
			case NavNclType.NavOption:
				return 4;
			case NavNclType.NavDecimal:
				return 12;
			case NavNclType.NavRecordId:
				return 448;
			case NavNclType.NavDate:
			case NavNclType.NavTime:
				return 4;
			case NavNclType.NavDateTime:
				return 8;
			case NavNclType.NavTableFilter:
				return 504;
			case NavNclType.NavDateFormula:
				return 32;
			case NavNclType.NavOemText:
				return fieldDefinedLength + 1;
			case NavNclType.NavText:
				return (fieldDefinedLength + 1) * 2;
			case NavNclType.NavOemCode:
				return fieldDefinedLength + 2;
			case NavNclType.NavCode:
				return (fieldDefinedLength + 1) * 2 + 1;
			default:
				throw new NotSupportedException();
			}
		}
	}

	public bool FieldIsTimestamp => FieldNo == 0;

	public bool FieldIsUserDefinedTimestamp => (fieldFlags & FieldFlags.UserDefinedTimestamp) != 0;

	public bool FieldIsAutoIncrement => (fieldFlags & FieldFlags.AutoIncrement) != 0;

	public bool FieldIsCompressed => (fieldFlags & FieldFlags.Compressed) != 0;

	public bool FieldIsPartOfPrimaryKey => (fieldFlags & FieldFlags.IsPartOfPrimaryKey) != 0;

	public NCLMetaCalculationFormula CalculationFormula => calculationFormula;

	public string ExternalName => externalMetadata.Name;

	public string ExternalType => externalMetadata.ExternalType;

	public ExternalAccess ExternalAccess => externalMetadata.Access;

	public BlobSubType BlobSubType
	{
		get
		{
			if (externalMetadata == null)
			{
				return BlobSubType.UserDefined;
			}
			return externalMetadata.BlobSubType;
		}
	}

	public bool IsCompanionTableField
	{
		get
		{
			if (SourceExtensionType == NavExtensionType.ModernDev && FieldClass == FieldClass.Normal)
			{
				Guid? guid = SourceAppId;
				Guid? guid2 = Parent.SourceAppId;
				if (guid.HasValue != guid2.HasValue)
				{
					return true;
				}
				if (!guid.HasValue)
				{
					return false;
				}
				return guid.GetValueOrDefault() != guid2.GetValueOrDefault();
			}
			return false;
		}
	}

	public NavAppTableContext OriginatingTableContext
	{
		get
		{
			if (!IsCompanionTableField)
			{
				return NavAppTableContext.Main;
			}
			return NavAppTableContext.Companion(AppOrigin);
		}
	}

	public Guid? SourceAppId => sourceAppId;

	public NavExtensionType SourceExtensionType { get; }

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

	public NCLMetaField SourceTableField
	{
		get
		{
			if (fieldClass != FieldClass.FlowField)
			{
				return this;
			}
			return calculationFormula.SourceField;
		}
	}

	public ReadOnlyCollection<NCLMetaFieldRelation> FieldRelations => fieldRelations;

	internal FieldTriggerHandler<NavApplicationObjectBase> LookupHandler { get; set; }

	internal List<FieldTriggerHandler<NavApplicationObjectBase>> OnBeforeValidateHandlers { get; } = new List<FieldTriggerHandler<NavApplicationObjectBase>>();

	internal FieldTriggerHandler<NavApplicationObjectBase> ValidateHandler { get; set; }

	internal List<FieldTriggerHandler<NavApplicationObjectBase>> OnAfterValidateHandlers { get; } = new List<FieldTriggerHandler<NavApplicationObjectBase>>();

	public bool HasSubscribers
	{
		get
		{
			NavEventScope[] array = eventScopes;
			if (array != null)
			{
				if (array[0] == null || !array[0].HasSubscribers)
				{
					if (array[1] != null)
					{
						return array[1].HasSubscribers;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public string SqlColumnName
	{
		get
		{
			if (sqlColumnName == null)
			{
				if (FieldIsUserDefinedTimestamp)
				{
					sqlColumnName = parent.TimestampField.SqlColumnName;
				}
				else
				{
					sqlColumnName = (string.IsNullOrEmpty(ExternalName) ? (sqlColumnName = NavSqlStatementHelper.ConvertToSqlIdentifier(fieldName)) : (sqlColumnName = ExternalName));
				}
			}
			return sqlColumnName;
		}
	}

	public string SqlCollation => NavGlobal.AppDatabase.SqlSortingProperties.Collation;

	public ObsoleteState ObsoleteState { get; set; }

	public ALDataClassification DataClassification
	{
		get
		{
			if (FieldClass != FieldClass.Normal)
			{
				return ALDataClassification.SystemMetadata;
			}
			if (parent.IsSystemOrVirtualTable)
			{
				return SystemTablesDataClassification.SystemTableClassification(parent.TableId, FieldNo);
			}
			return dataClassification;
		}
		set
		{
			dataClassification = value;
		}
	}

	public ExtendedDatatype ExtendedDataType { get; private set; }

	public string ObsoleteReason
	{
		get
		{
			return obsoleteReason;
		}
		set
		{
			obsoleteReason = value ?? string.Empty;
		}
	}

	NavType INavValueMetadata.NavType => FieldNavType;

	NavNclType INavValueMetadata.NclType => FieldNclType;

	int INavValueMetadata.NavDefinedLengthMetadata => FieldDefinedLength;

	NCLOptionMetadata INavValueMetadata.NavOptionMetadata
	{
		get
		{
			if (FieldNclType == NavNclType.NavOption)
			{
				return FieldOptionMetadata;
			}
			throw new NotSupportedException();
		}
	}

	public int ColumnIndex => fieldIndex;

	public string DescriptiveName => FieldName;

	public string DescriptiveParentName => parent.TableName;

	public bool IsStringType
	{
		get
		{
			bool result = fieldNclType == NavNclType.NavBigText || fieldNclType == NavNclType.NavText || fieldNclType == NavNclType.NavOemText || fieldNclType == NavNclType.NavDateFormula;
			if (fieldNclType == NavNclType.NavOemCode || fieldNclType == NavNclType.NavCode)
			{
				result = sqlType == SqlDataType.VarChar || sqlType == SqlDataType.Default || sqlType == SqlDataType.Variant;
			}
			return result;
		}
	}

	public FilterExpressionContext ExpressionContext
	{
		get
		{
			if (expressionContext == null)
			{
				if (IsStringType)
				{
					SqlSortingProperties sqlSortingProperties = NavGlobal.AppDatabase.SqlSortingProperties;
					expressionContext = FilterExpressionContext.CreateForTextField(this, sqlSortingProperties.DatabaseCollationCulture, sqlSortingProperties.DatabaseComparisonStyle, sqlSortingProperties.IsCollationCompatibleWithWindows);
				}
				else
				{
					expressionContext = FilterExpressionContext.CreateForNonTextField(this);
				}
			}
			return expressionContext;
		}
	}

	public SqlDataType SqlDataType
	{
		get
		{
			if (FieldClass == FieldClass.FlowFilter)
			{
				return flowFilterSqlDataType.Value;
			}
			return sqlType;
		}
	}

	public SqlDataType SqlDataTypeWithDefaultMapping
	{
		get
		{
			if (fieldNclType != NavNclType.NavCode && fieldNclType != NavNclType.NavOemCode)
			{
				return SqlDataType;
			}
			if (SqlDataType != SqlDataType.Default)
			{
				return SqlDataType;
			}
			return SqlDataType.VarChar;
		}
	}

	public int SqlColumnIndex => sqlColumnIndex;

	public int ApproximateByteSize => SizableObject.ApproximateByteSizeForNclType(FieldNclType, FieldDefinedLength);

	public SortOrder Order => SortOrder.Ascending;

	public INavFieldMetadata Field => this;

	internal NCLMetaField(int fieldNo, NavNclType fieldNclType, SqlDataType sqlType, int fieldDefinedLength, FieldClass fieldClass, string fieldName, bool fieldActive, bool fieldAutoIncrement, bool fieldCompressed, bool fieldIsPartOfPrimaryKey, NCLOptionMetadata fieldOptionMetadata, MetaCalcFormula metaCalculationFormula, bool validateRelations, bool testRelations, List<MetaFieldRelation> metaFieldRelations, string initialValue, NCLMetaTable parent, int fieldIndex, int sqlColumnIndex, string externalName = null, string externalType = null, ExternalAccess externalAccess = ExternalAccess.Full, bool userdefinedTimestamp = false, int[] externalOptionOrdinalValues = null, BlobSubType blobSubType = BlobSubType.UserDefined, string sourceAppId = null, int sourceExtensionType = 0, ObsoleteState obsoleteState = ObsoleteState.No, string obsoleteReason = "", ALDataClassification dataClassification = ALDataClassification.ToBeClassified, ExtendedDatatype extendedDataType = ExtendedDatatype.Undefined)
	{
		NCLMetaField referencingField = this;
		this.parent = parent;
		this.fieldNo = fieldNo;
		this.fieldNclType = fieldNclType;
		this.sqlType = sqlType;
		this.fieldDefinedLength = fieldDefinedLength;
		this.fieldClass = fieldClass;
		this.fieldName = fieldName;
		if (Guid.TryParse(sourceAppId, out var result))
		{
			this.sourceAppId = result;
		}
		SourceExtensionType = (NavExtensionType)sourceExtensionType;
		if (fieldActive)
		{
			fieldFlags |= FieldFlags.Active;
		}
		if (fieldAutoIncrement)
		{
			fieldFlags |= FieldFlags.AutoIncrement;
		}
		if (fieldCompressed)
		{
			fieldFlags |= FieldFlags.Compressed;
		}
		if (fieldIsPartOfPrimaryKey)
		{
			fieldFlags |= FieldFlags.IsPartOfPrimaryKey;
		}
		if (validateRelations)
		{
			fieldFlags |= FieldFlags.ValidateRelations;
		}
		if (testRelations)
		{
			fieldFlags |= FieldFlags.TestRelations;
		}
		if (userdefinedTimestamp)
		{
			fieldFlags |= FieldFlags.UserDefinedTimestamp;
		}
		this.fieldOptionMetadata = fieldOptionMetadata;
		if (fieldClass == FieldClass.FlowField)
		{
			calculationFormula = ((metaCalculationFormula != null) ? NCLMetaCalculationFormula.CreateFromMetaCalculationFormula(metaCalculationFormula, parent) : NCLMetaCalculationFormula.EmptyFormula);
		}
		if (metaFieldRelations != null)
		{
			fieldRelations = new ReadOnlyCollection<NCLMetaFieldRelation>(metaFieldRelations.Select((MetaFieldRelation metaFieldRelation) => NCLMetaFieldRelation.CreateFromMetaFieldRelation(metaFieldRelation, parent, referencingField)).ToArray());
		}
		initialValueText = initialValue;
		this.fieldIndex = fieldIndex;
		this.sqlColumnIndex = sqlColumnIndex;
		externalMetadata = ExternalMetadata.Create(externalName, externalType, externalAccess, externalOptionOrdinalValues, blobSubType);
		flowFilterSqlDataType = new Lazy<SqlDataType>(GetFlowFilterSqlDataType);
		ObsoleteState = obsoleteState;
		ObsoleteReason = obsoleteReason;
		DataClassification = dataClassification;
		ExtendedDataType = extendedDataType;
	}

	internal static SqlDataType GetSqlDataTypeFromText(string sqlDataType)
	{
		if (string.IsNullOrEmpty(sqlDataType))
		{
			return SqlDataType.Default;
		}
		return sqlDataType switch
		{
			"Variant" => SqlDataType.Variant, 
			"Varchar" => SqlDataType.VarChar, 
			"Integer" => SqlDataType.Integer, 
			"BigInteger" => SqlDataType.BigInt, 
			_ => throw new NotSupportedException(), 
		};
	}

	internal static NCLMetaField CreateFromMetaField(NCLMetaTable parentTable, MetaField metaField, bool isPartOfPrimaryKey, int index, int sqlFieldIndex)
	{
		SqlDataType sqlDataType = SqlDataType.Default;
		NavNclType nclTypeFromNavType = NavNclTypeHelper.GetNclTypeFromNavType(metaField.Type);
		int num = metaField.Length;
		NCLOptionMetadata nCLOptionMetadata = null;
		switch (nclTypeFromNavType)
		{
		case NavNclType.NavDateFormula:
			num = 32;
			break;
		case NavNclType.NavBoolean:
		case NavNclType.NavInteger:
		case NavNclType.NavDate:
		case NavNclType.NavTime:
			num = 4;
			break;
		case NavNclType.NavDuration:
		case NavNclType.NavDateTime:
		case NavNclType.NavBlob:
			num = 8;
			break;
		case NavNclType.NavBigInteger:
			sqlDataType = ((!metaField.Timestamp) ? SqlDataType.BigInt : SqlDataType.Timestamp);
			num = 8;
			break;
		case NavNclType.NavDecimal:
			num = 12;
			break;
		case NavNclType.NavMedia:
		case NavNclType.NavMediaSet:
		case NavNclType.NavGuid:
			num = 16;
			break;
		case NavNclType.NavTableFilter:
			num = 504;
			break;
		case NavNclType.NavRecordId:
			num = 448;
			break;
		case NavNclType.NavOption:
			num = 4;
			if (metaField.EnumTypeName == null)
			{
				if (parentTable.TableId == 2000000041 && metaField.Id == 5)
				{
					nCLOptionMetadata = new NCLOptionMetadataTable2000000041Field5(metaField.OptionString);
				}
				else if (parentTable.ObjectId != 2000000071 && parentTable.ObjectId != 2000000053)
				{
					nCLOptionMetadata = ((parentTable.ObjectId != 2000000007 || metaField.Id != 1) ? ((NCLOptionMetadata)new NCLOptionMetadataWithCaptions(metaField.OptionString, NCLCaptionStrings.CreateNCLCaptionStrings(metaField.MergedOptionCaptionML ?? string.Empty, string.Empty), parentTable.ObjectId, metaField.Id)) : ((NCLOptionMetadata)new NCLOptionMetadataDateTablePeriodTypeField(metaField.OptionString)));
				}
				else
				{
					nCLOptionMetadata = NCLOptionMetadata.Create(metaField.OptionString);
				}
			}
			else
			{
				nCLOptionMetadata = ((metaField.OptionString != null) ? NCLLegacyFieldEnumMetadata.Create(metaField.EnumTypeId, metaField.EnumTypeName, metaField.OptionString, metaField.OptionCaptionML) : NCLFieldEnumMetadata.Create(metaField.EnumTypeId));
			}
			break;
		case NavNclType.NavOemCode:
		case NavNclType.NavCode:
			sqlDataType = GetSqlDataTypeFromText(metaField.SqlDataType);
			break;
		}
		return new NCLMetaField(metaField.Id, nclTypeFromNavType, sqlDataType, num, metaField.FieldClass, metaField.Name, metaField.Enabled, metaField.AutoIncrement, metaField.Compressed, isPartOfPrimaryKey, nCLOptionMetadata, metaField.CalcFormula, metaField.ValidateRelation, metaField.TestRelation, metaField.Relations, metaField.InitValue, parentTable, index, sqlFieldIndex, metaField.ExternalName, metaField.ExternalType, metaField.ExternalAccess, metaField.Timestamp, metaField.OptionOrdinalValues, metaField.BlobSubType, metaField.SourceAppId, metaField.SourceExtensionType, metaField.ObsoleteState, metaField.ObsoleteReason, metaField.DataClassification, metaField.ExtendedDatatype);
	}

	private NCLCaptionStrings CreateCaptionStringsForSpecialTables(int languageAppGroupId)
	{
		return NCLCaptionStrings.CreateNCLCaptionStrings(string.Empty, FieldName);
	}

	private NCLCaptionStrings CreateCaptionStrings(int languageAppGroupId)
	{
		if (fieldNo == 0)
		{
			return NCLCaptionStrings.CreateNCLCaptionStrings(string.Empty, FieldName);
		}
		MetaTable metaTableOriginal = parent.GetMetaTableOriginal();
		if (metaTableOriginal == null)
		{
			return NCLCaptionStrings.CreateNCLCaptionStrings(string.Empty, fieldName);
		}
		MetaField metaField = (MetaField)metaTableOriginal.GetFieldsById(FieldNo).Clone();
		metaField.MergeMultiLanguageTexts(parent.ObjectId, languageAppGroupId);
		return NCLCaptionStrings.CreateNCLCaptionStrings(metaField.MergedCaptionMultiLanguage ?? string.Empty, metaField.Name);
	}

	public bool FieldBelongsInPhysicalTable(NavAppTableContext tableContext)
	{
		bool fieldIsTimestamp = FieldIsTimestamp;
		fieldIsTimestamp |= FieldIsPartOfPrimaryKey;
		return fieldIsTimestamp | OriginatingTableContext.Equals(tableContext);
	}

	public int MapExternalOptionValueToNavOptionValue(int externalValue)
	{
		if (externalMetadata == null || externalMetadata.OptionOrdinalValues == null)
		{
			return externalValue;
		}
		return externalMetadata.OptionOrdinalValues.IndexOf(externalValue, EqualityComparer<int>.Default);
	}

	public int MapNavOptionValueToExternalOptionValue(int navValue)
	{
		return externalMetadata.OptionOrdinalValues[navValue];
	}

	public override int GetHashCode()
	{
		return HashCodeHelper.CombineHashCodes(parent.GetHashCode(), FieldNo);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} (Number {1}, Index {2})", FieldName, FieldNo, ColumnIndex);
	}

	public bool IsEventSubscribed(NavTriggerEventType triggerEventType)
	{
		return GetEventScope(triggerEventType)?.HasSubscribers ?? false;
	}

	public NavEventScope GetEventScope(NavTriggerEventType triggerEventType, EventScopeGetOption getOption = EventScopeGetOption.None)
	{
		if (eventScopes == null)
		{
			if (getOption != EventScopeGetOption.CreateIfNotFound)
			{
				return null;
			}
			lock (this)
			{
				if (eventScopes == null)
				{
					eventScopes = new NavEventScope[2];
				}
			}
		}
		int num;
		switch (triggerEventType)
		{
		case NavTriggerEventType.OnBeforeValidateEvent:
			num = 0;
			break;
		case NavTriggerEventType.OnAfterValidateEvent:
			num = 1;
			break;
		default:
			if (getOption == EventScopeGetOption.CreateIfNotFound)
			{
				throw new InvalidOperationException();
			}
			return null;
		}
		NavEventScope navEventScope = eventScopes[num];
		if (navEventScope == null && getOption == EventScopeGetOption.CreateIfNotFound)
		{
			lock (this)
			{
				if (eventScopes[num] == null)
				{
					eventScopes[num] = new NavEventScope();
				}
				navEventScope = eventScopes[num];
			}
		}
		if (navEventScope != null && (getOption == EventScopeGetOption.RemoveIfFound || getOption == EventScopeGetOption.SetStaleIfFound))
		{
			lock (this)
			{
				navEventScope = eventScopes[num];
				if (navEventScope.ClearEventScope())
				{
					eventScopes[num] = ((getOption == EventScopeGetOption.RemoveIfFound) ? null : NavEventScope.StaleEventScope);
				}
			}
		}
		return navEventScope;
	}

	private SqlDataType GetFlowFilterSqlDataType()
	{
		if (FieldClass != FieldClass.FlowFilter || (FieldNclType != NavNclType.NavCode && FieldNclType != NavNclType.NavOemCode) || FieldRelations == null || FieldRelations.Count == 0)
		{
			return sqlType;
		}
		SqlDataType? sqlDataType = null;
		for (int i = 0; i < FieldRelations.Count; i++)
		{
			NCLMetaField sourceField = FieldRelations[i].SourceField;
			if (sourceField == null || sourceField == this)
			{
				continue;
			}
			SqlDataType sqlDataTypeWithDefaultMapping = sourceField.SqlDataTypeWithDefaultMapping;
			if (sqlDataTypeWithDefaultMapping == SqlDataType.Variant)
			{
				return SqlDataType.Variant;
			}
			if (!sqlDataType.HasValue)
			{
				sqlDataType = sqlDataTypeWithDefaultMapping;
			}
			else if (sqlDataType.Value == sqlDataTypeWithDefaultMapping)
			{
				continue;
			}
			if (sqlDataType == SqlDataType.BigInt || sqlDataType == SqlDataType.Integer)
			{
				if (sqlDataTypeWithDefaultMapping != SqlDataType.BigInt && sqlDataTypeWithDefaultMapping != SqlDataType.Integer)
				{
					return SqlDataType.Variant;
				}
				if (sqlDataTypeWithDefaultMapping == SqlDataType.BigInt)
				{
					sqlDataType = SqlDataType.BigInt;
				}
			}
			else if (sqlDataType == SqlDataType.VarChar && sqlDataTypeWithDefaultMapping != SqlDataType.VarChar)
			{
				return SqlDataType.Variant;
			}
		}
		return sqlDataType ?? sqlType;
	}
}
