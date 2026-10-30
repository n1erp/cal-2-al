using System;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaQueryColumn : INavFieldMetadata, INavValueMetadata
{
	private const int NextAvailableColumnIndex = -1;

	private readonly ExtendedDatatype extendedDataType;

	private readonly NCLMetaQueryDataItem parentDataItem;

	private readonly AggregationType aggregationType;

	private readonly DateMethodType dateMethod;

	private readonly int columnIndex;

	private readonly NCLCaptionStrings columnCaptions;

	private readonly NavType navType;

	private readonly NavNclType nclType;

	private readonly int length;

	private readonly int id;

	private readonly string name;

	private readonly int fieldNo;

	private readonly QueryColumnType columnType;

	private readonly bool reverseSign;

	private readonly NavValue constValue;

	private NavValue emptyValue;

	private NCLMetaQueryColumn sourceColumn;

	private FilterExpressionContext expressionContext;

	private string sqlColumnName;

	public NCLMetaQueryColumn SourceColumn
	{
		get
		{
			if (sourceColumn == null)
			{
				sourceColumn = ((parentDataItem.SubQueryDefinition == null) ? this : parentDataItem.SubQueryDefinition.DefinedColumns.FirstOrDefault((NCLMetaQueryColumn column) => column.id == fieldNo).SourceColumn);
			}
			return sourceColumn;
		}
	}

	public NCLMetaField SourceTableField
	{
		get
		{
			if (columnType == QueryColumnType.ConstValue)
			{
				throw new NotSupportedException();
			}
			if (SourceColumn.aggregationType == AggregationType.Count)
			{
				return SourceColumn.ParentDataItem.MetaTable.PrimaryKey.GetKeyFieldByIndex(0);
			}
			return SourceColumn.ParentDataItem.MetaTable.GetFieldByNo(SourceColumn.FieldNo);
		}
	}

	public NCLMetaQueryDataItem ParentDataItem => parentDataItem;

	public int Id => id;

	public string Name => name;

	public int FieldNo => fieldNo;

	public QueryColumnType ColumnType => columnType;

	public NavValue ConstValue => constValue;

	public bool ReverseSign => reverseSign;

	public string Caption
	{
		get
		{
			if (columnCaptions != null)
			{
				return columnCaptions.GetValueOrDefault();
			}
			if (columnType != QueryColumnType.ConstValue && aggregationType == AggregationType.None && dateMethod == DateMethodType.None)
			{
				return SourceTableField.FieldCaption;
			}
			return Name;
		}
	}

	public bool IsAggregated => aggregationType != AggregationType.None;

	public bool IsDateFunction => dateMethod != DateMethodType.None;

	public AggregationType AggregationType => aggregationType;

	public DateMethodType DateMethod => dateMethod;

	public string SqlColumnName
	{
		get
		{
			if (sqlColumnName == null)
			{
				sqlColumnName = NavSqlStatementHelper.ConvertToSqlIdentifier(name);
			}
			return sqlColumnName;
		}
	}

	private bool InheritsDataTypeFromSource => navType == NavType.None;

	public NavType NavType
	{
		get
		{
			if (SourceColumn == this)
			{
				if (!InheritsDataTypeFromSource)
				{
					return navType;
				}
				return SourceTableField.FieldNavType;
			}
			if (!InheritsDataTypeFromSource)
			{
				return navType;
			}
			return SourceColumn.NavType;
		}
	}

	public NavNclType NclType
	{
		get
		{
			if (SourceColumn == this)
			{
				if (!InheritsDataTypeFromSource)
				{
					return nclType;
				}
				return SourceTableField.FieldNclType;
			}
			if (!InheritsDataTypeFromSource)
			{
				return nclType;
			}
			return SourceColumn.NclType;
		}
	}

	public ExtendedDatatype ExtendedDataType
	{
		get
		{
			if (SourceColumn == this)
			{
				if (!InheritsDataTypeFromSource)
				{
					return extendedDataType;
				}
				return SourceTableField.ExtendedDataType;
			}
			if (!InheritsDataTypeFromSource)
			{
				return extendedDataType;
			}
			return SourceColumn.ExtendedDataType;
		}
	}

	private bool IsStringAsumingColumnIsSource
	{
		get
		{
			if (nclType != NavNclType.NavBigText && nclType != NavNclType.NavText && nclType != NavNclType.NavOemText)
			{
				return nclType == NavNclType.NavDateFormula;
			}
			return true;
		}
	}

	public bool IsStringType
	{
		get
		{
			if (SourceColumn == this)
			{
				if (!InheritsDataTypeFromSource)
				{
					return IsStringAsumingColumnIsSource;
				}
				return SourceTableField.IsStringType;
			}
			if (!InheritsDataTypeFromSource)
			{
				return IsStringAsumingColumnIsSource;
			}
			return SourceColumn.IsStringType;
		}
	}

	public int NavDefinedLengthMetadata
	{
		get
		{
			if (SourceColumn == this)
			{
				if (InheritsDataTypeFromSource)
				{
					return SourceTableField.FieldDefinedLength;
				}
			}
			else if (InheritsDataTypeFromSource)
			{
				return SourceColumn.NavDefinedLengthMetadata;
			}
			return length;
		}
	}

	public NCLOptionMetadata NavOptionMetadata
	{
		get
		{
			if (SourceColumn == this)
			{
				if (SourceTableField != null)
				{
					return SourceTableField.FieldOptionMetadata;
				}
				throw new NotSupportedException();
			}
			return SourceColumn.NavOptionMetadata;
		}
	}

	public int ColumnIndex => columnIndex;

	public string DescriptiveName => name;

	public string DescriptiveParentName => parentDataItem.Name;

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

	public FieldClass FieldClass => FieldClass.Normal;

	public SqlDataType SqlDataType
	{
		get
		{
			if (columnType != QueryColumnType.ConstValue)
			{
				return SourceTableField.SqlDataType;
			}
			return SqlDataType.Default;
		}
	}

	public NavValue InitValue => EmptyValue;

	public NavValue EmptyValue
	{
		get
		{
			if (NclType == NavNclType.NavBlob)
			{
				return NavBigInteger.Default;
			}
			if (emptyValue == null)
			{
				if (SourceColumn == this)
				{
					if (InheritsDataTypeFromSource)
					{
						return SourceTableField.EmptyValue;
					}
				}
				else if (InheritsDataTypeFromSource)
				{
					return SourceColumn.EmptyValue;
				}
				emptyValue = NavValue.GetDefaultNavValue(this);
			}
			return emptyValue;
		}
	}

	private NCLMetaQueryColumn(NCLMetaQueryDataItem parentDataItem, string name, int columnIndex, NCLCaptionStrings columnCaptions, AggregationType aggregationType, DateMethodType dateMethod, NavType navType, int length, int id, QueryColumnType columnType, bool reverseSign, ExtendedDatatype extendedDataType)
	{
		this.aggregationType = aggregationType;
		this.dateMethod = dateMethod;
		this.parentDataItem = parentDataItem;
		this.name = name;
		this.columnCaptions = columnCaptions;
		if (columnType != QueryColumnType.FilterOnly)
		{
			NCLMetaQueryDefinition parentQueryDefinition = parentDataItem.ParentQueryDefinition;
			if (columnIndex == -1)
			{
				this.columnIndex = parentQueryDefinition.NextColumnIndex;
			}
			else
			{
				this.columnIndex = columnIndex;
			}
			if (this.columnIndex >= parentQueryDefinition.NextColumnIndex)
			{
				parentQueryDefinition.NextColumnIndex = this.columnIndex + 1;
			}
		}
		this.navType = navType;
		nclType = NavNclTypeHelper.GetNclTypeFromNavType(navType);
		this.length = length;
		this.id = id;
		this.columnType = columnType;
		this.reverseSign = reverseSign;
		this.extendedDataType = extendedDataType;
	}

	private NCLMetaQueryColumn(NCLMetaQueryDataItem parentDataItem, NCLMetaField sourceField, string name, int columnIndex, NCLCaptionStrings columnCaptions, AggregationType aggregationType, DateMethodType dateMethod, NavType navType, int length, int id, QueryColumnType columnType, bool reverseSign)
		: this(parentDataItem, name, columnIndex, columnCaptions, aggregationType, dateMethod, navType, length, id, columnType, reverseSign, sourceField.ExtendedDataType)
	{
		fieldNo = sourceField.FieldNo;
	}

	private NCLMetaQueryColumn(NCLMetaQueryDataItem parentDataItem, int fieldNo, string name, int columnIndex, NCLCaptionStrings columnCaptions, AggregationType aggregationType, DateMethodType dateMethod, NavType navType, int length, int id, QueryColumnType columnType, bool reverseSign)
		: this(parentDataItem, name, columnIndex, columnCaptions, aggregationType, dateMethod, navType, length, id, columnType, reverseSign, ExtendedDatatype.None)
	{
		this.fieldNo = fieldNo;
	}

	private NCLMetaQueryColumn(NCLMetaQueryDataItem parentDataItem, string name, int columnIndex, NCLCaptionStrings columnCaptions, AggregationType aggregationType, NavType navType, int length, int id, QueryColumnType columnType, NavValue constValue)
		: this(parentDataItem, name, columnIndex, columnCaptions, aggregationType, DateMethodType.None, navType, length, id, columnType, reverseSign: false, ExtendedDatatype.None)
	{
		this.constValue = constValue;
	}

	public static NCLMetaQueryColumn CreateAggregate(NCLMetaQueryDataItem dataItem, int sourceFieldNo, string name, AggregationType aggregationType, int columnId, bool reverseSign, bool filterOnly, int columnIndex = -1)
	{
		return new NCLMetaQueryColumn(dataItem, sourceFieldNo, name, columnIndex, null, aggregationType, DateMethodType.None, NavType.None, 0, columnId, filterOnly ? QueryColumnType.FilterOnly : QueryColumnType.Normal, reverseSign);
	}

	public static NCLMetaQueryColumn CreateAggregate(NCLMetaQueryDataItem dataItem, int sourceFieldNo, string name, AggregationType aggregationType, NavType navType, int length, int columnId, bool reverseSign)
	{
		return new NCLMetaQueryColumn(dataItem, sourceFieldNo, name, -1, null, aggregationType, DateMethodType.None, navType, length, columnId, QueryColumnType.Normal, reverseSign);
	}

	public static NCLMetaQueryColumn CreateConstValue(NCLMetaQueryDataItem dataItem, string name, NavType navType, int length, int columnId, NavValue constValue)
	{
		return new NCLMetaQueryColumn(dataItem, name, -1, null, AggregationType.None, navType, length, columnId, QueryColumnType.ConstValue, constValue);
	}

	public static NCLMetaQueryColumn CreateCount(NCLMetaQueryDataItem dataItem, string name, int columnId)
	{
		return new NCLMetaQueryColumn(dataItem, 0, name, -1, null, AggregationType.Count, DateMethodType.None, NavType.Integer, 0, columnId, QueryColumnType.Normal, reverseSign: false);
	}

	public static NCLMetaQueryColumn Create(NCLMetaQueryDataItem dataItem, int sourceFieldNo, string name, int columnId, bool filterOnly, int columnIndex = -1, bool reverseSign = false)
	{
		return new NCLMetaQueryColumn(dataItem, sourceFieldNo, name, columnIndex, null, AggregationType.None, DateMethodType.None, NavType.None, 0, columnId, filterOnly ? QueryColumnType.FilterOnly : QueryColumnType.Normal, reverseSign);
	}

	public static NCLMetaQueryColumn Create(NCLMetaQueryDataItem dataItem, int sourceFieldNo, NavType navType, int length, string name, int columnId, bool reverseSign = false)
	{
		return new NCLMetaQueryColumn(dataItem, sourceFieldNo, name, -1, null, AggregationType.None, DateMethodType.None, navType, length, columnId, QueryColumnType.Normal, reverseSign);
	}

	public static NCLMetaQueryColumn Create(NCLMetaQueryDataItem dataItem, NCLMetaField sourceField, bool filterOnly, int columnIndex = -1, bool reverseSign = false)
	{
		return new NCLMetaQueryColumn(dataItem, sourceField, sourceField.FieldName, columnIndex, null, AggregationType.None, DateMethodType.None, NavType.None, 0, sourceField.FieldNo, filterOnly ? QueryColumnType.FilterOnly : QueryColumnType.Normal, reverseSign);
	}

	public static NCLMetaQueryColumn CreateWithDateMethod(NCLMetaQueryDataItem dataItem, int sourceFieldNo, string name, DateMethodType dateMethod, int columnId, bool filterOnly, int columnIndex = -1)
	{
		return new NCLMetaQueryColumn(dataItem, sourceFieldNo, name, columnIndex, null, AggregationType.None, dateMethod, NavType.Integer, 0, columnId, filterOnly ? QueryColumnType.FilterOnly : QueryColumnType.Normal, reverseSign: false);
	}

	public static NCLMetaQueryColumn CreateFromDesignMetadata(NCLMetaQueryDataItem dataItem, MetaQueryColumn metaColumn)
	{
		NCLCaptionStrings nCLCaptionStrings = null;
		if (!string.IsNullOrEmpty(metaColumn.CaptionML))
		{
			nCLCaptionStrings = NCLCaptionStrings.CreateNCLCaptionStrings(metaColumn.CaptionML, metaColumn.Name);
		}
		return new NCLMetaQueryColumn(dataItem, metaColumn.FieldNo, metaColumn.Name, metaColumn.QueryColumnIndex, nCLCaptionStrings, metaColumn.FieldTotalingMethod, metaColumn.FieldDateMethod, (metaColumn.FieldTotalingMethod == AggregationType.Count || metaColumn.FieldDateMethod != DateMethodType.None) ? NavType.Integer : NavType.None, 0, metaColumn.Id, metaColumn.FilterOnly ? QueryColumnType.FilterOnly : QueryColumnType.Normal, metaColumn.ReverseSign);
	}

	private int GetValueAgnosticHashCode()
	{
		int num = HashCodeHelper.CombineHashCodes(fieldNo, id, (int)aggregationType, (int)dateMethod, (int)ColumnType);
		if (columnType == QueryColumnType.ConstValue)
		{
			num = HashCodeHelper.CombineHashCodes(num, (int)nclType);
		}
		return num;
	}

	public override int GetHashCode()
	{
		int num = GetValueAgnosticHashCode();
		if (columnType == QueryColumnType.ConstValue)
		{
			num = HashCodeHelper.CombineHashCodes(num, constValue.GetHashCode());
		}
		return num;
	}

	public int GetValueInsensitiveHashCode()
	{
		return GetValueAgnosticHashCode();
	}

	private bool ValueAgnosticEquals(object other)
	{
		if (this == other)
		{
			return true;
		}
		if (!(other is NCLMetaQueryColumn nCLMetaQueryColumn))
		{
			return false;
		}
		if (id != nCLMetaQueryColumn.id || aggregationType != nCLMetaQueryColumn.aggregationType || dateMethod != nCLMetaQueryColumn.dateMethod || fieldNo != nCLMetaQueryColumn.fieldNo || columnType != nCLMetaQueryColumn.columnType || (columnType == QueryColumnType.ConstValue && nclType != nCLMetaQueryColumn.nclType) || reverseSign != nCLMetaQueryColumn.reverseSign)
		{
			return false;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (!ValueAgnosticEquals(obj))
		{
			return false;
		}
		NCLMetaQueryColumn nCLMetaQueryColumn = obj as NCLMetaQueryColumn;
		if (columnType == QueryColumnType.ConstValue && !constValue.Equals(nCLMetaQueryColumn.constValue))
		{
			return false;
		}
		return true;
	}

	public bool ValueInsensitiveEquals(object other)
	{
		return ValueAgnosticEquals(other);
	}
}
