using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NCLMetaFilterConst : NCLMetaFilter
{
	private readonly byte[] valueBytes;

	private readonly string filterTextValue;

	private NavValue value;

	private UnaryFilterExpression expression;

	public string FilterTextValue => filterTextValue;

	public NavValue Value
	{
		get
		{
			if (value == null)
			{
				if (valueBytes != null)
				{
					value = NavValue.CreateNavValueFromBytes(SourceField, valueBytes);
				}
				else
				{
					value = ((string.Compare(filterTextValue, "''", StringComparison.Ordinal) == 0) ? SourceField.EmptyValue : ALSystemVariable.EvaluateIntoNavValue(DataError.ThrowError, SourceField, filterTextValue, 9));
				}
			}
			return value;
		}
	}

	public override FilterExpression Expression
	{
		get
		{
			if (expression == null)
			{
				expression = new UnaryFilterExpression(FilterExpressionType.Equal, Value, SourceField.ExpressionContext);
			}
			return expression;
		}
	}

	public NCLMetaFilterConst(NCLMetaFilterCollection parentFilterCollection, int sourceFieldId, byte[] valueBytes)
		: base(parentFilterCollection, NCLMetaFilterType.Const, sourceFieldId)
	{
		this.valueBytes = valueBytes;
	}

	public NCLMetaFilterConst(NCLMetaFilterCollection parentFilterCollection, NCLMetaField sourceField, string filterTextValue)
		: base(parentFilterCollection, NCLMetaFilterType.Const, sourceField)
	{
		this.filterTextValue = filterTextValue;
	}

	public NCLMetaFilterConst(NCLMetaFilterCollection parentFilterCollection, int sourceFieldId, string filterTextValue)
		: base(parentFilterCollection, NCLMetaFilterType.Const, sourceFieldId)
	{
		this.filterTextValue = filterTextValue;
	}

	internal static NCLMetaFilterConst CreateMetaFilterConst(NCLMetaFilterCollection parentFilterCollection, NCLMetaField sourceField, string filterValue)
	{
		return new NCLMetaFilterConst(parentFilterCollection, sourceField, filterValue);
	}

	internal static NCLMetaFilterConst CreateFromMetaFilter(MetaFilter metaFilter, NCLMetaFilterCollection parentFilterCollection)
	{
		return new NCLMetaFilterConst(parentFilterCollection, metaFilter.FieldId, metaFilter.FilterValue);
	}

	internal new static NCLMetaFilterConst CreateFromMetaCondition(MetaCondition metaCondition, NCLMetaFilterCollection parentFilterCollection)
	{
		return new NCLMetaFilterConst(parentFilterCollection, metaCondition.FieldId, metaCondition.ConditionValue);
	}
}
