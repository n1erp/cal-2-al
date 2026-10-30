using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class RecordBufferComparer : IComparer<IRecordBuffer>
{
	private readonly INavSortingField[] orderByColumns;

	public RecordBufferComparer(IEnumerable<INavSortingField> orderByColumns)
	{
		if (orderByColumns == null)
		{
			throw new ArgumentNullException("orderByColumns");
		}
		this.orderByColumns = orderByColumns.ToArray();
	}

	public int Compare(IRecordBuffer x, IRecordBuffer y)
	{
		if (x == y)
		{
			return 0;
		}
		if (x == null)
		{
			return -1;
		}
		if (y == null)
		{
			return 1;
		}
		INavSortingField[] array = orderByColumns;
		foreach (INavSortingField navSortingField in array)
		{
			NavValue navValue = x[navSortingField.Field.ColumnIndex] ?? navSortingField.Field.InitValue;
			NavValue navValue2 = y[navSortingField.Field.ColumnIndex] ?? navSortingField.Field.InitValue;
			if (navValue == navValue2)
			{
				continue;
			}
			int num = navSortingField.Field.ExpressionContext.Compare(navValue, navValue2);
			if (num != 0)
			{
				if (navSortingField.Order != SortOrder.Descending)
				{
					return num;
				}
				return -num;
			}
		}
		return 0;
	}
}
