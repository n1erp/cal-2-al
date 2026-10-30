using System.Linq;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaDataItemLink
{
	private readonly NCLMetaFilterType linkType;

	private readonly NCLMetaFilterModes filterMode;

	private readonly string sourceDataItemName;

	private readonly int sourceFieldNo;

	private readonly int destinationFieldNo;

	private readonly NCLMetaQueryDataItem parentDataItem;

	private readonly NavValue constValue;

	private readonly FilterExpression expressionValue;

	private NCLMetaQueryColumn sourceColumn;

	private NCLMetaQueryColumn destinationColumn;

	private NCLMetaQueryDataItem sourceDataItem;

	private FilterExpression expression;

	private int? hashCode;

	private int? valueInsensitiveHashCode;

	public NCLMetaFilterType LinkType => linkType;

	public NCLMetaQueryColumn SourceColumn
	{
		get
		{
			if (sourceColumn == null)
			{
				sourceColumn = SourceDataItem.QueryColumns.FirstOrDefault((NCLMetaQueryColumn queryColumn) => queryColumn.ColumnType != QueryColumnType.ConstValue && queryColumn.AggregationType == AggregationType.None && queryColumn.DateMethod == DateMethodType.None && queryColumn.SourceTableField.FieldNo == sourceFieldNo);
				if (sourceColumn == null)
				{
					sourceColumn = NCLMetaQueryColumn.Create(SourceDataItem, sourceFieldNo, string.Empty, 0, filterOnly: true);
				}
			}
			return sourceColumn;
		}
	}

	public NCLMetaQueryColumn DestinationColumn
	{
		get
		{
			if (destinationColumn == null)
			{
				destinationColumn = parentDataItem.QueryColumns.FirstOrDefault((NCLMetaQueryColumn queryColumn) => queryColumn.ColumnType != QueryColumnType.ConstValue && queryColumn.AggregationType == AggregationType.None && queryColumn.DateMethod == DateMethodType.None && queryColumn.SourceTableField.FieldNo == destinationFieldNo);
				if (destinationColumn == null)
				{
					destinationColumn = NCLMetaQueryColumn.Create(parentDataItem, destinationFieldNo, string.Empty, 0, filterOnly: true);
				}
			}
			return destinationColumn;
		}
	}

	public FilterExpression Expression
	{
		get
		{
			if (expression == null)
			{
				switch (linkType)
				{
				case NCLMetaFilterType.Const:
					expression = new UnaryFilterExpression(FilterExpressionType.Equal, constValue, DestinationColumn.ExpressionContext);
					break;
				case NCLMetaFilterType.Expression:
					expression = expressionValue.TransferToDifferentContext(DestinationColumn.ExpressionContext);
					break;
				case NCLMetaFilterType.Field:
					expression = FilterExpression.FieldEqualsField(SourceColumn, DestinationColumn.ExpressionContext);
					break;
				}
			}
			return expression;
		}
	}

	public NCLMetaQueryDataItem SourceDataItem
	{
		get
		{
			if (sourceDataItem == null)
			{
				sourceDataItem = NCLMetaQueryDefinition.GetAllDataItems(parentDataItem.ParentQueryDefinition.RootQueryDefinition).First((NCLMetaQueryDataItem dataItem) => dataItem.Name.Equals(sourceDataItemName));
			}
			return sourceDataItem;
		}
	}

	public bool OnlyMaxLimit => (filterMode & NCLMetaFilterModes.OnlyMaxLimit) != 0;

	private NCLMetaDataItemLink(NCLMetaQueryDataItem parentDataItem, NCLMetaFilterType linkType, string sourceDataItemName, int sourceFieldNo, int destinationFieldNo, NavValue constValue, FilterExpression expressionValue, NCLMetaFilterModes filterMode)
	{
		this.linkType = linkType;
		this.parentDataItem = parentDataItem;
		this.sourceDataItemName = sourceDataItemName;
		this.sourceFieldNo = sourceFieldNo;
		this.destinationFieldNo = destinationFieldNo;
		this.constValue = constValue;
		this.expressionValue = expressionValue;
		this.filterMode = filterMode;
	}

	internal static NCLMetaDataItemLink CreateFieldEqualsField(NCLMetaQueryDataItem parentDataItem, string sourceDataItemName, int sourceFieldNo, int destinationFieldNo, NCLMetaFilterModes filterMode)
	{
		return new NCLMetaDataItemLink(parentDataItem, NCLMetaFilterType.Field, sourceDataItemName, sourceFieldNo, destinationFieldNo, null, null, filterMode);
	}

	internal static NCLMetaDataItemLink CreateConst(NCLMetaQueryDataItem parentDataItem, int destinationFieldNo, NavValue constValue)
	{
		return new NCLMetaDataItemLink(parentDataItem, NCLMetaFilterType.Const, string.Empty, 0, destinationFieldNo, constValue, null, NCLMetaFilterModes.None);
	}

	internal static NCLMetaDataItemLink CreateExpression(NCLMetaQueryDataItem parentDataItem, int destinationFieldNo, FilterExpression expression)
	{
		return new NCLMetaDataItemLink(parentDataItem, NCLMetaFilterType.Expression, string.Empty, 0, destinationFieldNo, null, expression, NCLMetaFilterModes.None);
	}

	private int GetValueAgnosticHashCode()
	{
		return HashCodeHelper.CombineHashCodes((int)linkType, (int)filterMode);
	}

	public override int GetHashCode()
	{
		if (!hashCode.HasValue)
		{
			hashCode = HashCodeHelper.CombineHashCodes(GetValueAgnosticHashCode(), Expression.GetHashCode());
		}
		return hashCode.Value;
	}

	public int GetValueInsensitiveHashCode()
	{
		if (!valueInsensitiveHashCode.HasValue)
		{
			valueInsensitiveHashCode = HashCodeHelper.CombineHashCodes(GetValueAgnosticHashCode(), ValueInsensitiveFilterExpressionEqualityComparer.Instance.GetHashCode(Expression));
		}
		return valueInsensitiveHashCode.Value;
	}

	private bool ValueAgnosticEquals(NCLMetaDataItemLink otherMetaDataItemLink)
	{
		if (otherMetaDataItemLink != null && linkType == otherMetaDataItemLink.linkType && filterMode == otherMetaDataItemLink.filterMode)
		{
			return string.Equals(sourceDataItemName, otherMetaDataItemLink.sourceDataItemName);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		NCLMetaDataItemLink nCLMetaDataItemLink = obj as NCLMetaDataItemLink;
		if (ValueAgnosticEquals(nCLMetaDataItemLink) && GetHashCode() == nCLMetaDataItemLink.GetHashCode())
		{
			return Expression.Equals(nCLMetaDataItemLink.Expression);
		}
		return false;
	}

	public bool ValueInsensitiveEquals(object other)
	{
		NCLMetaDataItemLink nCLMetaDataItemLink = other as NCLMetaDataItemLink;
		if (ValueAgnosticEquals(nCLMetaDataItemLink) && GetValueInsensitiveHashCode() == nCLMetaDataItemLink.GetValueInsensitiveHashCode())
		{
			return ValueInsensitiveFilterExpressionEqualityComparer.Instance.Equals(Expression, nCLMetaDataItemLink.Expression);
		}
		return false;
	}
}
