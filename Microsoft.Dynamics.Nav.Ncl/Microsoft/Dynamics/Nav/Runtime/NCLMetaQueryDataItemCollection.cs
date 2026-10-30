using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaQueryDataItemCollection : ReadOnlyCollection<NCLMetaQueryDataItem>
{
	private int hashCode;

	private bool hashCodeCalculated;

	private int valueInsensitiveHashCode;

	private bool valueInsensitiveHashCodeCalculated;

	private NCLMetaQueryDataItemCollection(NCLMetaQueryDataItem dataItem)
		: base((IList<NCLMetaQueryDataItem>)new NCLMetaQueryDataItem[1] { dataItem })
	{
	}

	private NCLMetaQueryDataItemCollection(IList<NCLMetaQueryDataItem> dataItems)
		: base(dataItems)
	{
	}

	private NCLMetaQueryDataItemCollection(NCLMetaQueryDataItemCollection dataItems, NCLMetaQueryDataItem dataItem)
		: base((IList<NCLMetaQueryDataItem>)dataItems.Concat(new NCLMetaQueryDataItem[1] { dataItem }).ToArray())
	{
	}

	public static NCLMetaQueryDataItemCollection Create(IList<NCLMetaQueryDataItem> dataItems)
	{
		return new NCLMetaQueryDataItemCollection(dataItems);
	}

	public static NCLMetaQueryDataItemCollection Create(params NCLMetaQueryDataItem[] dataItems)
	{
		return new NCLMetaQueryDataItemCollection(dataItems);
	}

	public static NCLMetaQueryDataItemCollection Create(Func<IList<NCLMetaQueryDataItem>> dataItems)
	{
		return new NCLMetaQueryDataItemCollection(dataItems());
	}

	internal static NCLMetaQueryDataItemCollection Create(NCLMetaQueryDefinition parentQueryDefinition, int tableNo, DataLockState lockState, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(NCLMetaQueryDataItem.Create(parentQueryDefinition, tableNo, lockState, name, queryColumnsCreator, null, null, appGroup));
	}

	internal static NCLMetaQueryDataItemCollection Create(NCLMetaQueryDefinition parentQueryDefinition, NCLMetaTable table, DataLockState lockState, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, FiltersAndMarks> tableFiltersAndMarksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(NCLMetaQueryDataItem.Create(parentQueryDefinition, table, lockState, name, queryColumnsCreator, tableFiltersAndMarksCreator, null, appGroup));
	}

	internal static NCLMetaQueryDataItemCollection Create(NCLMetaQueryDefinition parentQueryDefinition, int tableNo, DataLockState lockState, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, FiltersAndMarks> tableFiltersAndMarksCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(NCLMetaQueryDataItem.Create(parentQueryDefinition, DataItemLinkType.None, tableNo, lockState, name, queryColumnsCreator, tableFiltersAndMarksCreator, dataItemLinksCreator, appGroup));
	}

	internal NCLMetaQueryDataItemCollection CrossApply(NCLMetaQueryDefinition parentQueryDefinition, Func<NCLMetaQueryDataItem, NCLMetaQueryDefinition> subQueryCreator, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(this, NCLMetaQueryDataItem.CreateWithSubQuery(parentQueryDefinition, DataItemLinkType.CrossApply, subQueryCreator, name, queryColumnsCreator, dataItemLinksCreator, appGroup));
	}

	internal NCLMetaQueryDataItemCollection OuterApply(NCLMetaQueryDefinition parentQueryDefinition, Func<NCLMetaQueryDataItem, NCLMetaQueryDefinition> subQueryCreator, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(this, NCLMetaQueryDataItem.CreateWithSubQuery(parentQueryDefinition, DataItemLinkType.OuterApply, subQueryCreator, name, queryColumnsCreator, dataItemLinksCreator, appGroup));
	}

	internal NCLMetaQueryDataItemCollection LeftOuterJoin(NCLMetaQueryDefinition parentQueryDefinition, Func<NCLMetaQueryDataItem, NCLMetaQueryDefinition> subQueryCreator, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(this, NCLMetaQueryDataItem.CreateWithSubQuery(parentQueryDefinition, DataItemLinkType.LeftOuterJoin, subQueryCreator, name, queryColumnsCreator, dataItemLinksCreator, appGroup));
	}

	internal NCLMetaQueryDataItemCollection LeftOuterJoin(NCLMetaQueryDefinition parentQueryDefinition, int tableNo, DataLockState lockState, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, FiltersAndMarks> tableFiltersAndMarksCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(this, NCLMetaQueryDataItem.Create(parentQueryDefinition, DataItemLinkType.LeftOuterJoin, tableNo, lockState, name, queryColumnsCreator, tableFiltersAndMarksCreator, dataItemLinksCreator, appGroup));
	}

	internal NCLMetaQueryDataItemCollection CrossJoin(NCLMetaQueryDefinition parentQueryDefinition, Func<NCLMetaQueryDataItem, NCLMetaQueryDefinition> subQueryCreator, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(this, NCLMetaQueryDataItem.CreateWithSubQuery(parentQueryDefinition, DataItemLinkType.CrossJoin, subQueryCreator, name, queryColumnsCreator, null, appGroup));
	}

	internal NCLMetaQueryDataItemCollection CrossJoin(NCLMetaQueryDefinition parentQueryDefinition, int tableNo, DataLockState lockState, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, FiltersAndMarks> tableFiltersAndMarksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(this, NCLMetaQueryDataItem.Create(parentQueryDefinition, DataItemLinkType.CrossJoin, tableNo, lockState, name, queryColumnsCreator, tableFiltersAndMarksCreator, null, appGroup));
	}

	internal NCLMetaQueryDataItemCollection FullOuterJoin(NCLMetaQueryDefinition parentQueryDefinition, Func<NCLMetaQueryDataItem, NCLMetaQueryDefinition> subQueryCreator, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(this, NCLMetaQueryDataItem.CreateWithSubQuery(parentQueryDefinition, DataItemLinkType.FullOuterJoin, subQueryCreator, name, queryColumnsCreator, dataItemLinksCreator, appGroup));
	}

	internal NCLMetaQueryDataItemCollection FullOuterJoin(NCLMetaQueryDefinition parentQueryDefinition, int tableNo, DataLockState lockState, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, FiltersAndMarks> tableFiltersAndMarksCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(this, NCLMetaQueryDataItem.Create(parentQueryDefinition, DataItemLinkType.FullOuterJoin, tableNo, lockState, name, queryColumnsCreator, tableFiltersAndMarksCreator, dataItemLinksCreator, appGroup));
	}

	internal NCLMetaQueryDataItemCollection InnerJoin(NCLMetaQueryDefinition parentQueryDefinition, Func<NCLMetaQueryDataItem, NCLMetaQueryDefinition> subQueryCreator, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(this, NCLMetaQueryDataItem.CreateWithSubQuery(parentQueryDefinition, DataItemLinkType.InnerJoin, subQueryCreator, name, queryColumnsCreator, dataItemLinksCreator, appGroup));
	}

	internal NCLMetaQueryDataItemCollection InnerJoin(NCLMetaQueryDefinition parentQueryDefinition, int tableNo, DataLockState lockState, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, FiltersAndMarks> tableFiltersAndMarksCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(this, NCLMetaQueryDataItem.Create(parentQueryDefinition, DataItemLinkType.InnerJoin, tableNo, lockState, name, queryColumnsCreator, tableFiltersAndMarksCreator, dataItemLinksCreator, appGroup));
	}

	internal NCLMetaQueryDataItemCollection RightOuterJoin(NCLMetaQueryDefinition parentQueryDefinition, Func<NCLMetaQueryDataItem, NCLMetaQueryDefinition> subQueryCreator, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(this, NCLMetaQueryDataItem.CreateWithSubQuery(parentQueryDefinition, DataItemLinkType.RightOuterJoin, subQueryCreator, name, queryColumnsCreator, dataItemLinksCreator, appGroup));
	}

	internal NCLMetaQueryDataItemCollection RightOuterJoin(NCLMetaQueryDefinition parentQueryDefinition, int tableNo, DataLockState lockState, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, FiltersAndMarks> tableFiltersAndMarksCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItemCollection(this, NCLMetaQueryDataItem.Create(parentQueryDefinition, DataItemLinkType.RightOuterJoin, tableNo, lockState, name, queryColumnsCreator, tableFiltersAndMarksCreator, dataItemLinksCreator, appGroup));
	}

	public override int GetHashCode()
	{
		if (!hashCodeCalculated)
		{
			using (IEnumerator<NCLMetaQueryDataItem> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					NCLMetaQueryDataItem current = enumerator.Current;
					hashCode = HashCodeHelper.CombineHashCodes(hashCode, current.GetHashCode());
				}
			}
			hashCodeCalculated = true;
		}
		return hashCode;
	}

	public int GetValueInsensitiveHashCode()
	{
		if (!valueInsensitiveHashCodeCalculated)
		{
			using (IEnumerator<NCLMetaQueryDataItem> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					NCLMetaQueryDataItem current = enumerator.Current;
					valueInsensitiveHashCode = HashCodeHelper.CombineHashCodes(valueInsensitiveHashCode, current.GetValueInsensitiveHashCode());
				}
			}
			valueInsensitiveHashCodeCalculated = true;
		}
		return valueInsensitiveHashCode;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is NCLMetaQueryDataItemCollection nCLMetaQueryDataItemCollection) || Count != nCLMetaQueryDataItemCollection.Count || GetHashCode() != nCLMetaQueryDataItemCollection.GetHashCode())
		{
			return false;
		}
		IEnumerator<NCLMetaQueryDataItem> enumerator = nCLMetaQueryDataItemCollection.GetEnumerator();
		using (IEnumerator<NCLMetaQueryDataItem> enumerator2 = GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				NCLMetaQueryDataItem current = enumerator2.Current;
				enumerator.MoveNext();
				if (!current.Equals(enumerator.Current))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool ValueInsensitiveEquals(object other)
	{
		if (this == other)
		{
			return true;
		}
		if (!(other is NCLMetaQueryDataItemCollection nCLMetaQueryDataItemCollection) || GetValueInsensitiveHashCode() != nCLMetaQueryDataItemCollection.GetValueInsensitiveHashCode())
		{
			return false;
		}
		IEnumerator<NCLMetaQueryDataItem> enumerator = nCLMetaQueryDataItemCollection.GetEnumerator();
		using (IEnumerator<NCLMetaQueryDataItem> enumerator2 = GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				NCLMetaQueryDataItem current = enumerator2.Current;
				enumerator.MoveNext();
				if (!current.ValueInsensitiveEquals(enumerator.Current))
				{
					return false;
				}
			}
		}
		return true;
	}
}
