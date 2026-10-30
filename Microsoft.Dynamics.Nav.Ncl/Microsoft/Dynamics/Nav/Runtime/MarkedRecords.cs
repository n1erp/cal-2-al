using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class MarkedRecords : ISizableObject
{
	private class PrimaryKeyFieldTreeNode
	{
		private readonly FilterExpression valueExpression;

		private readonly List<PrimaryKeyFieldTreeNode> childNodes;

		private readonly FilterExpressionContext filterExpressionContext;

		private bool IsLeaf
		{
			get
			{
				if (!IsRoot)
				{
					return childNodes.Count == 0;
				}
				return false;
			}
		}

		private bool IsRoot => valueExpression == null;

		public FilterExpression SubExpression
		{
			get
			{
				if (IsLeaf)
				{
					return valueExpression;
				}
				if (childNodes[0].IsLeaf)
				{
					return CombineEntriesToExpression(childNodes);
				}
				IEnumerable<IGrouping<FilterExpression, PrimaryKeyFieldTreeNode>> source = from e in childNodes
					group e by e.SubExpression;
				return FilterExpression.Or(source.Select((IGrouping<FilterExpression, PrimaryKeyFieldTreeNode> g) => FilterExpression.And(CombineEntriesToExpression(g.ToList()), g.Key)));
			}
		}

		public PrimaryKeyFieldTreeNode(NCLMetaField field, NavValue value, IEnumerable<PrimaryKeyFieldTreeNode> childNodes)
		{
			filterExpressionContext = field.ExpressionContext;
			valueExpression = FilterExpression.Equal(value, filterExpressionContext);
			this.childNodes = childNodes.ToList();
		}

		public PrimaryKeyFieldTreeNode(IEnumerable<PrimaryKeyFieldTreeNode> childNodes)
		{
			this.childNodes = childNodes.ToList();
		}

		private static FilterExpression CombineEntriesToExpression(List<PrimaryKeyFieldTreeNode> orderedNodes)
		{
			FilterExpression filterExpression = FilterExpression.BalancedOr(orderedNodes.Select((PrimaryKeyFieldTreeNode sv) => sv.valueExpression));
			if (orderedNodes[0].filterExpressionContext.IsDiscrete)
			{
				filterExpression = filterExpression.Simplify();
			}
			return filterExpression;
		}
	}

	private class SortedSetWrapper : ISizableObject, IEnumerable<NavRecordId>, IEnumerable, IEquatable<SortedSetWrapper>
	{
		private SortedSet<NavRecordId> sortedSet;

		private NavRecordId[] sortedArray;

		private int arrayIndex;

		private int arrayCount;

		private readonly RecordIdComparer comparer;

		public int ApproximateByteSize => ((sortedSet != null) ? (sortedSet.Count * 448) : 0) + ((sortedArray != null) ? (sortedArray.Length * 448) : 0) + SizableObject.GetApproximateByteSize(5) + 24;

		public int Count
		{
			get
			{
				if (sortedSet == null)
				{
					return arrayCount;
				}
				return sortedSet.Count;
			}
		}

		public NavRecordId Min
		{
			get
			{
				if (sortedArray == null)
				{
					return sortedSet.Min;
				}
				if (arrayCount != 0)
				{
					return sortedArray[arrayIndex];
				}
				return null;
			}
		}

		public NavRecordId Max
		{
			get
			{
				if (sortedArray == null)
				{
					return sortedSet.Max;
				}
				if (arrayCount != 0)
				{
					return sortedArray[arrayIndex + arrayCount - 1];
				}
				return null;
			}
		}

		public SortedSetWrapper(SortedSet<NavRecordId> sortedSet, RecordIdComparer comparer)
		{
			if (sortedSet == null)
			{
				throw new ArgumentNullException("sortedSet");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}
			this.sortedSet = sortedSet;
			this.comparer = comparer;
		}

		private SortedSetWrapper(NavRecordId[] sortedArray, int arrayIndex, int arrayCount, RecordIdComparer comparer)
		{
			if (sortedArray == null)
			{
				throw new ArgumentNullException("sortedArray");
			}
			this.sortedArray = sortedArray;
			this.arrayIndex = arrayIndex;
			this.arrayCount = arrayCount;
			this.comparer = comparer;
		}

		public SortedSetWrapper(IEnumerable<NavRecordId> elements, RecordIdComparer comparer)
		{
			if (elements == null)
			{
				throw new ArgumentNullException("elements");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}
			sortedArray = elements.OrderBy((NavRecordId id) => id, comparer).ToArray();
			arrayCount = sortedArray.Length;
			this.comparer = comparer;
		}

		public bool Add(NavRecordId item)
		{
			if (sortedSet == null)
			{
				sortedSet = new SortedSet<NavRecordId>(sortedArray.Skip(arrayIndex).Take(arrayCount), comparer);
			}
			if (sortedSet.Add(item))
			{
				sortedArray = null;
				return true;
			}
			return false;
		}

		public bool Remove(NavRecordId item)
		{
			if (sortedSet == null)
			{
				sortedSet = new SortedSet<NavRecordId>(sortedArray.Skip(arrayIndex).Take(arrayCount), comparer);
			}
			if (sortedSet.Remove(item))
			{
				sortedArray = null;
				return true;
			}
			return false;
		}

		public bool Contains(NavRecordId item)
		{
			if (sortedSet == null)
			{
				return Array.BinarySearch(sortedArray, arrayIndex, arrayCount, item, comparer) >= 0;
			}
			return sortedSet.Contains(item);
		}

		public void Partition(out SortedSetWrapper partition1, out SortedSetWrapper partition2)
		{
			if (sortedArray == null)
			{
				sortedArray = new NavRecordId[sortedSet.Count];
				sortedSet.CopyTo(sortedArray);
				arrayIndex = 0;
				arrayCount = sortedSet.Count;
			}
			int num = (arrayCount + 1) / 2;
			partition1 = new SortedSetWrapper(sortedArray, arrayIndex, num, comparer);
			partition2 = new SortedSetWrapper(sortedArray, arrayIndex + num, arrayCount - num, comparer);
		}

		public SortedSetWrapper Clone()
		{
			if (sortedSet != null)
			{
				return new SortedSetWrapper(new SortedSet<NavRecordId>(sortedSet, comparer), comparer);
			}
			return new SortedSetWrapper((NavRecordId[])sortedArray.Clone(), arrayIndex, arrayCount, comparer);
		}

		public IEnumerator<NavRecordId> GetEnumerator()
		{
			if (sortedSet != null)
			{
				return sortedSet.GetEnumerator();
			}
			return GetArrayEnumerable().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private IEnumerable<NavRecordId> GetArrayEnumerable()
		{
			for (int i = arrayIndex; i < arrayIndex + arrayCount; i++)
			{
				yield return sortedArray[i];
			}
		}

		public bool Equals(SortedSetWrapper other)
		{
			if (other == null)
			{
				return false;
			}
			if (Count != other.Count)
			{
				return false;
			}
			IEnumerator<NavRecordId> enumerator = GetEnumerator();
			IEnumerator<NavRecordId> enumerator2 = other.GetEnumerator();
			while (enumerator.MoveNext() && enumerator2.MoveNext())
			{
				if (!comparer.Equals(enumerator.Current, enumerator2.Current))
				{
					return false;
				}
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as NavRecordId);
		}

		public override int GetHashCode()
		{
			int count = Count;
			return this.Take(8).Aggregate(count, (int current, NavRecordId navRecordId) => HashCodeHelper.CombineHashCodes(current, navRecordId.GetHashCode()));
		}
	}

	internal enum MarkedRecordsKind
	{
		SimpleListOfMarks,
		SimpleListOfExclusions,
		Complex
	}

	private const int UnknownExpressionSize = -1;

	private static int maximumExpressionSize = 1500;

	private readonly bool allMarkedWithinFilter;

	private readonly FilterFieldDictionary allMarksFilter;

	private readonly RecordIdComparer recordIdComparer;

	private readonly NCLMetaTable metaTable;

	private readonly bool filteringOnlyOnNonPrimaryKeys;

	private SortedSetWrapper marks;

	private SortedSetWrapper exclusions;

	private FilterExpression completeExpression;

	private bool hashCodeCalculated;

	private int hashCode;

	private bool isInReadonlyMode;

	private readonly FilterExpression additionalAllMarksFilter;

	private int estimatedExpressionSize = -1;

	public int ApproximateByteSize => SizableObject.GetApproximateByteSize(allMarksFilter, GetCompleteExpression(), additionalAllMarksFilter, marks, exclusions) + SizableObject.GetApproximateByteSize(13) + 24;

	public int IndividualMarksAndExlusionsCount => ((marks != null) ? marks.Count : 0) + ((exclusions != null) ? exclusions.Count : 0);

	public bool ReadOnly
	{
		get
		{
			return isInReadonlyMode;
		}
		set
		{
			if (isInReadonlyMode != value)
			{
				ThrowIfReadOnly();
				isInReadonlyMode = true;
			}
		}
	}

	public NCLMetaTable MetaTable => metaTable;

	public IEnumerable<NavRecordId> MarkedRecordIds
	{
		get
		{
			if (allMarkedWithinFilter)
			{
				throw new NotSupportedException("All records within a filter are marked. We do not support turning this into a list of individual record IDs. This should never be necessary.");
			}
			IEnumerable<NavRecordId> enumerable = marks;
			return enumerable ?? new NavRecordId[0];
		}
	}

	internal MarkedRecordsKind Kind
	{
		get
		{
			if (additionalAllMarksFilter != null)
			{
				return MarkedRecordsKind.Complex;
			}
			if (allMarkedWithinFilter)
			{
				if (marks == null || marks.Count == 0)
				{
					return MarkedRecordsKind.SimpleListOfExclusions;
				}
				return MarkedRecordsKind.Complex;
			}
			return MarkedRecordsKind.SimpleListOfMarks;
		}
	}

	public FilterFieldDictionary AllMarksFilter
	{
		[DebuggerStepThrough]
		get
		{
			return allMarksFilter;
		}
	}

	public bool IsCompleteExpressionLarge
	{
		get
		{
			ThrowIfNotReadOnly();
			if (estimatedExpressionSize > maximumExpressionSize)
			{
				return true;
			}
			if (IndividualMarksAndExlusionsCount == 2)
			{
				return false;
			}
			if (IndividualMarksAndExlusionsCount * metaTable.PrimaryKey.KeyFieldCount <= maximumExpressionSize)
			{
				return false;
			}
			return (estimatedExpressionSize = CountLeafNodes(GetCompleteExpression())) > maximumExpressionSize;
		}
	}

	private MarkedRecords(NCLMetaTable metaTable, FilterFieldDictionary allMarksFilter, SortedSetWrapper marks, SortedSetWrapper exclusions, bool allMarkedWithinFilter)
		: this(new RecordIdComparer(metaTable), metaTable, allMarksFilter, marks, exclusions, allMarkedWithinFilter, null, -1)
	{
	}

	private MarkedRecords(RecordIdComparer recordIdComparer, NCLMetaTable metaTable, FilterFieldDictionary allMarksFilter, SortedSetWrapper marks, SortedSetWrapper exclusions, bool allMarkedWithinFilter, FilterExpression additionalAllMarksFilter, int estimatedExpressionSize)
	{
		if (!allMarkedWithinFilter && allMarksFilter != null)
		{
			throw new ArgumentException("allMarksFilter can only be non-null when allMarkedWithinFilter is true");
		}
		this.recordIdComparer = recordIdComparer;
		this.metaTable = metaTable;
		this.allMarksFilter = allMarksFilter;
		this.marks = marks;
		this.exclusions = exclusions;
		this.allMarkedWithinFilter = allMarkedWithinFilter;
		this.additionalAllMarksFilter = additionalAllMarksFilter;
		this.estimatedExpressionSize = estimatedExpressionSize;
		if (this.allMarksFilter != null && this.allMarksFilter.Items.Length != 0)
		{
			filteringOnlyOnNonPrimaryKeys = this.allMarksFilter.Items.All((Tuple<INavFieldMetadata, FilterExpression> t) => !((NCLMetaField)t.Item1).FieldIsPartOfPrimaryKey);
		}
	}

	public static MarkedRecords CreateEmpty(NCLMetaTable metaTable)
	{
		return new MarkedRecords(metaTable, null, null, null, allMarkedWithinFilter: false);
	}

	public static MarkedRecords CreateWithMarks(NCLMetaTable metaTable, IEnumerable<NavRecordId> markedRecords)
	{
		RecordIdComparer comparer = new RecordIdComparer(metaTable);
		return new MarkedRecords(comparer, metaTable, null, new SortedSetWrapper(markedRecords, comparer), null, allMarkedWithinFilter: false, null, -1);
	}

	public static MarkedRecords CreateWithExclusions(NCLMetaTable metaTable, FilterFieldDictionary allMarksFilter, NavRecordId[] exclusions)
	{
		RecordIdComparer comparer = new RecordIdComparer(metaTable);
		SortedSetWrapper sortedSetWrapper = null;
		if (!allMarksFilter.IsNullOrEmpty() && exclusions.Length != 0)
		{
			List<Tuple<int, NCLMetaField, FilterExpression>> primaryKeyFilters = null;
			for (int i = 0; i < allMarksFilter.Items.Length; i++)
			{
				NCLMetaField nCLMetaField = (NCLMetaField)allMarksFilter.Items[i].Item1;
				if (nCLMetaField.FieldIsPartOfPrimaryKey)
				{
					int item = metaTable.PrimaryKey.KeyFieldsList.IndexOf(nCLMetaField);
					FilterExpression item2 = allMarksFilter.Items[i].Item2;
					if (primaryKeyFilters == null)
					{
						primaryKeyFilters = new List<Tuple<int, NCLMetaField, FilterExpression>>();
					}
					primaryKeyFilters.Add(Tuple.Create(item, nCLMetaField, item2));
				}
			}
			if (primaryKeyFilters != null)
			{
				SortedSet<NavRecordId> sortedSet = new SortedSet<NavRecordId>(exclusions, comparer);
				sortedSet.RemoveWhere((NavRecordId id) => primaryKeyFilters.Any((Tuple<int, NCLMetaField, FilterExpression> tuple) => !tuple.Item3.Evaluate(id.FieldValueByIndex(tuple.Item2, tuple.Item1))));
				sortedSetWrapper = new SortedSetWrapper(sortedSet, comparer);
			}
		}
		if (sortedSetWrapper == null)
		{
			sortedSetWrapper = new SortedSetWrapper(exclusions, comparer);
		}
		return new MarkedRecords(comparer, metaTable, allMarksFilter, null, sortedSetWrapper, allMarkedWithinFilter: true, null, -1);
	}

	public void Partition(out MarkedRecords partition1, out MarkedRecords partition2)
	{
		SortedSetWrapper partition3 = null;
		SortedSetWrapper partition4 = null;
		SortedSetWrapper partition5 = null;
		SortedSetWrapper partition6 = null;
		FilterExpression filterExpression = null;
		if (marks != null)
		{
			marks.Partition(out partition3, out partition4);
		}
		FilterExpression filterExpression3;
		bool flag;
		if (exclusions != null && exclusions.Count > 0)
		{
			exclusions.Partition(out partition5, out partition6);
			FilterExpression filterExpression2 = FilterExpression.And(DataProviderHelper.BuildPositioningFilterFromRecordBuffer(BuildRecordBufferForRecordId(partition5.Min), metaTable.PrimaryKey.DefinedSortingFields, includeCurrent: true), DataProviderHelper.BuildPositioningFilterFromRecordBuffer(BuildRecordBufferForRecordId(partition5.Max), metaTable.PrimaryKey.DefinedSortingFields.Reversed, includeCurrent: true));
			filterExpression3 = filterExpression2;
			filterExpression = FilterExpression.And(additionalAllMarksFilter, filterExpression2.Negate());
			flag = true;
		}
		else
		{
			filterExpression3 = additionalAllMarksFilter;
			flag = false;
		}
		partition1 = new MarkedRecords(recordIdComparer, metaTable, allMarksFilter, partition3, partition5, allMarkedWithinFilter, filterExpression3, estimatedExpressionSize / 2)
		{
			ReadOnly = true
		};
		partition2 = new MarkedRecords(recordIdComparer, metaTable, flag ? allMarksFilter : null, partition4, partition6, flag, filterExpression, estimatedExpressionSize / 2)
		{
			ReadOnly = true
		};
	}

	private MutableRecordBuffer BuildRecordBufferForRecordId(NavRecordId recordId)
	{
		NavValue[] array = new NavValue[metaTable.FieldCount];
		NCLMetaKey primaryKey = metaTable.PrimaryKey;
		for (int i = 0; i < primaryKey.KeyFieldCount; i++)
		{
			NCLMetaField keyFieldByIndex = primaryKey.GetKeyFieldByIndex(i);
			array[keyFieldByIndex.FieldIndex] = recordId.FieldValueByIndex(keyFieldByIndex, i);
		}
		return new MutableRecordBuffer(new ReadOnlyRecordBuffer(metaTable, array));
	}

	public MarkedRecords Clone()
	{
		return new MarkedRecords(recordIdComparer, metaTable, allMarksFilter, (marks == null) ? null : marks.Clone(), (exclusions == null) ? null : exclusions.Clone(), allMarkedWithinFilter, additionalAllMarksFilter, estimatedExpressionSize);
	}

	public bool SetMarked(IRecordBuffer recordBuffer, bool marked)
	{
		ThrowIfReadOnly();
		if (!allMarkedWithinFilter)
		{
			NavRecordId recordId = recordBuffer.GetRecordId();
			if (marked)
			{
				EnsureSetCreated(ref marks);
				return marks.Add(recordId);
			}
			if (marks != null)
			{
				return marks.Remove(recordId);
			}
			return false;
		}
		bool flag = allMarksFilter == null || recordBuffer.PassesFieldFilters(allMarksFilter);
		if (marked)
		{
			NavRecordId recordId = recordBuffer.GetRecordId();
			if (flag && exclusions != null && !filteringOnlyOnNonPrimaryKeys)
			{
				return exclusions.Remove(recordId);
			}
			EnsureSetCreated(ref marks);
			bool flag2 = marks.Add(recordId);
			if (flag && exclusions != null && filteringOnlyOnNonPrimaryKeys)
			{
				flag2 |= exclusions.Remove(recordId);
			}
			return flag2;
		}
		if (flag || filteringOnlyOnNonPrimaryKeys)
		{
			NavRecordId recordId = recordBuffer.GetRecordId();
			EnsureSetCreated(ref exclusions);
			bool flag3 = exclusions.Add(recordId);
			if (marks != null)
			{
				flag3 |= marks.Remove(recordId);
			}
			return flag3;
		}
		return false;
	}

	public bool IsMarked(IRecordBuffer recordBuffer, bool includeFlowFields = true)
	{
		if (allMarkedWithinFilter && recordBuffer.PassesFieldFilters(allMarksFilter, includeFlowFields) && recordBuffer.PassesFilter(additionalAllMarksFilter))
		{
			if (exclusions != null)
			{
				return !exclusions.Contains(recordBuffer.GetRecordId());
			}
			return true;
		}
		if (marks != null)
		{
			return marks.Contains(recordBuffer.GetRecordId());
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		ThrowIfNotReadOnly();
		if (!(obj is MarkedRecords markedRecords))
		{
			return false;
		}
		if (this == markedRecords)
		{
			return true;
		}
		if (GetHashCode() != markedRecords.GetHashCode() || allMarkedWithinFilter != markedRecords.allMarkedWithinFilter || marks == null != (markedRecords.marks == null) || exclusions == null != (markedRecords.exclusions == null) || metaTable != markedRecords.metaTable || !object.Equals(allMarksFilter, markedRecords.allMarksFilter) || !object.Equals(additionalAllMarksFilter, markedRecords.additionalAllMarksFilter))
		{
			return false;
		}
		if (marks != null && !marks.Equals(markedRecords.marks))
		{
			return false;
		}
		if (exclusions != null && !exclusions.Equals(markedRecords.exclusions))
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		ThrowIfNotReadOnly();
		if (!hashCodeCalculated)
		{
			int h = HashCodeHelper.CombineHashCodes(allMarkedWithinFilter.GetHashCode(), (!(allMarksFilter == null)) ? allMarksFilter.GetHashCode() : 0, (additionalAllMarksFilter != null) ? additionalAllMarksFilter.GetHashCode() : 0);
			if (marks != null)
			{
				h = HashCodeHelper.CombineHashCodes(h, marks.Count);
				h = marks.Take(8).Aggregate(h, (int current, NavRecordId navRecordId) => HashCodeHelper.CombineHashCodes(current, navRecordId.GetHashCode()));
			}
			if (exclusions != null)
			{
				h = HashCodeHelper.CombineHashCodes(h, exclusions.Count);
				h = exclusions.Take(8).Aggregate(h, (int current, NavRecordId navRecordId) => HashCodeHelper.CombineHashCodes(current, navRecordId.GetHashCode()));
			}
			hashCode = h;
			hashCodeCalculated = true;
		}
		return hashCode;
	}

	public FilterExpression GetCompleteExpression()
	{
		ThrowIfNotReadOnly();
		if (completeExpression == null)
		{
			FilterExpression left;
			if (allMarkedWithinFilter)
			{
				left = ((allMarksFilter == null) ? null : allMarksFilter.ConcatenatedFilterNormalFields);
				if (additionalAllMarksFilter != null)
				{
					left = FilterExpression.And(left, additionalAllMarksFilter);
				}
				if (exclusions != null && exclusions.Count > 0)
				{
					FilterExpression right = GetExpressionFromRecordIdSet(exclusions).Negate();
					left = FilterExpression.And(left, right);
				}
				if (marks != null && marks.Count > 0)
				{
					FilterExpression expressionFromRecordIdSet = GetExpressionFromRecordIdSet(marks);
					left = FilterExpression.Or(left, expressionFromRecordIdSet);
				}
			}
			else
			{
				left = ((marks == null || marks.Count == 0) ? FilterExpression.BooleanConstant(value: false) : GetExpressionFromRecordIdSet(marks));
			}
			completeExpression = left;
		}
		return completeExpression;
	}

	private static int CountLeafNodes(FilterExpression expression)
	{
		switch (expression.ExpressionType)
		{
		case FilterExpressionType.And:
		case FilterExpressionType.Or:
		{
			BinaryFilterExpression binaryFilterExpression = (BinaryFilterExpression)expression;
			return CountLeafNodes(binaryFilterExpression.Left) + CountLeafNodes(binaryFilterExpression.Right);
		}
		case FilterExpressionType.RangeBetweenInclusive:
			return 2;
		default:
			return 1;
		}
	}

	private FilterExpression GetExpressionFromRecordIdSet(SortedSetWrapper setOfIds)
	{
		return new PrimaryKeyFieldTreeNode(GroupByKeyField(setOfIds, 0)).SubExpression;
	}

	private IEnumerable<PrimaryKeyFieldTreeNode> GroupByKeyField(IEnumerable<NavRecordId> orderedRecordIds, int keyFieldIndex)
	{
		NavValue navValue = null;
		List<NavRecordId> list = new List<NavRecordId>();
		NCLMetaField keyField = metaTable.PrimaryKey.GetKeyFieldByIndex(keyFieldIndex);
		foreach (NavRecordId id in orderedRecordIds)
		{
			if (navValue == null)
			{
				navValue = id.FieldValueByIndex(keyField, keyFieldIndex);
				list.Add(id);
				continue;
			}
			if (keyField.ExpressionContext.Compare(navValue, id.FieldValueByIndex(keyField, keyFieldIndex)) == 0)
			{
				list.Add(id);
				continue;
			}
			if (keyFieldIndex < metaTable.PrimaryKey.KeyFieldCount - 1)
			{
				yield return new PrimaryKeyFieldTreeNode(keyField, navValue, GroupByKeyField(list, keyFieldIndex + 1));
			}
			else
			{
				yield return new PrimaryKeyFieldTreeNode(keyField, navValue, new PrimaryKeyFieldTreeNode[0]);
			}
			navValue = id.FieldValueByIndex(keyField, keyFieldIndex);
			list = new List<NavRecordId> { id };
		}
		if (list.Count > 0)
		{
			if (keyFieldIndex < metaTable.PrimaryKey.KeyFieldCount - 1)
			{
				yield return new PrimaryKeyFieldTreeNode(keyField, navValue, GroupByKeyField(list, keyFieldIndex + 1));
			}
			else
			{
				yield return new PrimaryKeyFieldTreeNode(keyField, navValue, new PrimaryKeyFieldTreeNode[0]);
			}
		}
	}

	private void ThrowIfReadOnly()
	{
		if (isInReadonlyMode)
		{
			throw new InvalidOperationException("Cannot modify a MarkedRecords instance which has been set to readonly");
		}
	}

	private void ThrowIfNotReadOnly()
	{
		if (!isInReadonlyMode)
		{
			throw new InvalidOperationException("MarkedRecords must be in read-only mode for this operation");
		}
	}

	private void EnsureSetCreated(ref SortedSetWrapper set)
	{
		if (set == null)
		{
			set = new SortedSetWrapper(new SortedSet<NavRecordId>(), recordIdComparer);
		}
	}
}
