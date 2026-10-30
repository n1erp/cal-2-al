using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class GlobalFilters
{
	private int globalFilterBumperToken;

	private Dictionary<int, FilterFieldDictionary> globalFilters;

	public bool AnyGlobalFilters => globalFilters != null;

	public int GlobalFilterBumperToken => globalFilterBumperToken;

	public bool SetGlobalFilter(NCLMetaField field, FilterExpression expression)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		int tableId = field.Parent.TableId;
		FilterFieldDictionary value;
		if (globalFilters == null)
		{
			if (expression == null)
			{
				return false;
			}
			globalFilters = new Dictionary<int, FilterFieldDictionary>();
			value = new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(field, expression));
			globalFilters.Add(tableId, value);
			GlobalFilterChanged();
			return true;
		}
		if (!globalFilters.TryGetValue(tableId, out value))
		{
			if (expression == null)
			{
				return false;
			}
			value = new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(field, expression));
			globalFilters.Add(tableId, value);
			GlobalFilterChanged();
			return true;
		}
		value.TryGetValue(field, out var value2);
		if (expression == null)
		{
			if (value2 == null)
			{
				return false;
			}
			if (value.Items.Length == 1)
			{
				if (globalFilters.Count == 1)
				{
					globalFilters = null;
					GlobalFilterChanged();
					return true;
				}
				globalFilters.Remove(tableId);
				GlobalFilterChanged();
				return true;
			}
		}
		else if (expression.Equals(value2))
		{
			return false;
		}
		IEnumerable<Tuple<INavFieldMetadata, FilterExpression>> enumerable = value.Items;
		if (value2 != null)
		{
			enumerable = enumerable.Where((Tuple<INavFieldMetadata, FilterExpression> t) => t.Item1 != field);
		}
		if (expression != null)
		{
			enumerable = enumerable.Concat(new Tuple<INavFieldMetadata, FilterExpression>[1]
			{
				new Tuple<INavFieldMetadata, FilterExpression>(field, expression)
			});
		}
		globalFilters[tableId] = new FilterFieldDictionary(enumerable);
		GlobalFilterChanged();
		return true;
	}

	public FilterFieldDictionary GetGlobalFilters(int tableId)
	{
		if (globalFilters == null)
		{
			return null;
		}
		if (globalFilters.TryGetValue(tableId, out var value))
		{
			return value;
		}
		return null;
	}

	public void ClearGlobalFilters(int tableId)
	{
		if (globalFilters != null && globalFilters.Remove(tableId))
		{
			if (globalFilters.Count == 0)
			{
				globalFilters = null;
			}
			GlobalFilterChanged();
		}
	}

	public void ClearGlobalFilters()
	{
		if (globalFilters != null)
		{
			globalFilters = null;
			GlobalFilterChanged();
		}
	}

	private void GlobalFilterChanged()
	{
		globalFilterBumperToken++;
	}
}
