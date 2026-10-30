using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NCLMetaFilterExpression : NCLMetaFilter
{
	private readonly string filterTextExpression;

	private FilterExpression expression;

	public string FilterTextExpression => filterTextExpression;

	public override FilterExpression Expression
	{
		get
		{
			if (expression == null)
			{
				FilterExpression result = FilterExpressionParser.Parse(filterTextExpression, SourceField.ExpressionContext, true, out var hasVariables);
				if (hasVariables)
				{
					return result;
				}
				expression = result;
			}
			return expression;
		}
	}

	private NCLMetaFilterExpression(NCLMetaFilterCollection parentFilterCollection, NCLMetaField sourceField, string filterTextExpression)
		: base(parentFilterCollection, NCLMetaFilterType.Expression, sourceField)
	{
		this.filterTextExpression = filterTextExpression;
	}

	private NCLMetaFilterExpression(NCLMetaFilterCollection parentFilterCollection, int sourceFieldId, string filterTextExpression)
		: base(parentFilterCollection, NCLMetaFilterType.Expression, sourceFieldId)
	{
		this.filterTextExpression = filterTextExpression;
	}

	internal static NCLMetaFilterExpression CreateMetaFilterExpression(NCLMetaFilterCollection parentFilterCollection, NCLMetaField sourceField, string filterValue)
	{
		return new NCLMetaFilterExpression(parentFilterCollection, sourceField, filterValue);
	}

	internal static NCLMetaFilterExpression CreateFromMetaFilter(NCLMetaFilterCollection parentFilterCollection, MetaFilter metaFilter)
	{
		return new NCLMetaFilterExpression(parentFilterCollection, metaFilter.FieldId, metaFilter.FilterValue);
	}

	internal static NCLMetaFilterExpression CreateFromMetaCondition(NCLMetaFilterCollection parentFilterCollection, MetaCondition metaCondition)
	{
		return new NCLMetaFilterExpression(parentFilterCollection, metaCondition.FieldId, metaCondition.ConditionValue);
	}
}
