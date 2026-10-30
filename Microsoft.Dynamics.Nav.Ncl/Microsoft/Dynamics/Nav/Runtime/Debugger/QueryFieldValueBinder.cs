using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

public class QueryFieldValueBinder : ValueBinder
{
	private static readonly MethodInfo getValueMethodInfo = typeof(QueryFieldValueBinder).GetMethod("GetValue", new Type[3]
	{
		typeof(NavQuery),
		typeof(NCLMetaQueryColumn),
		typeof(bool)
	});

	private readonly NCLMetaQueryColumn metaField;

	public QueryFieldValueBinder(NCLMetaQueryColumn metaField)
	{
		if (metaField == null)
		{
			throw new ArgumentNullException("metaField");
		}
		this.metaField = metaField;
	}

	public override object GetValue(object parent)
	{
		return GetValue((NavQuery)parent, metaField);
	}

	public override Expression BuildGetValueExpression(Expression input)
	{
		if (!typeof(NavQuery).IsAssignableFrom(input.Type))
		{
			input = Expression.Convert(input, typeof(NavQuery));
		}
		return Expression.Convert(Expression.Call(getValueMethodInfo, input, Expression.Constant(metaField), Expression.Constant(true)), metaField.EmptyValue.GetType());
	}

	public static object GetValue(NavQuery query, NCLMetaQueryColumn column, bool returnEmptyValueOnUninitialized = false)
	{
		if (query == null || (!query.IsInitialized | (query.CurrentDataRow == null)))
		{
			if (!returnEmptyValueOnUninitialized)
			{
				return null;
			}
			return column.EmptyValue;
		}
		return query.GetColumnValueSafe(column.Id, column.NavType);
	}
}
