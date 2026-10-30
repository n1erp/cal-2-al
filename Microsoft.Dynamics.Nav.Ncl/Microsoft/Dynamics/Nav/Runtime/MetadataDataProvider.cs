using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class MetadataDataProvider : RangeBasedComputedDataProvider
{
	private readonly NCLMetadata nclMetadata;

	private readonly MetadataProvider metadataProvider;

	protected NCLMetadata NclMetadata
	{
		[DebuggerStepThrough]
		get
		{
			return nclMetadata;
		}
	}

	public MetadataProvider MetadataProvider
	{
		[DebuggerStepThrough]
		get
		{
			return metadataProvider;
		}
	}

	protected MetadataDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
	{
		if (nclMetadata == null)
		{
			throw new ArgumentNullException("nclMetadata");
		}
		if (metadataProvider == null)
		{
			throw new ArgumentNullException("metadataProvider");
		}
		this.nclMetadata = nclMetadata;
		this.metadataProvider = metadataProvider;
	}

	protected IEnumerable<ObjectType> GetObjectTypesWithinRange(Range filteredRange, RangeList permittedRangeList, SortOrder sortOrder)
	{
		RangeList rangeList = RangeList.Intersect(RangeList.Create(filteredRange), permittedRangeList);
		IEnumerable<Range> source;
		if (sortOrder != SortOrder.Ascending)
		{
			source = rangeList.Ranges.Reverse();
		}
		else
		{
			IEnumerable<Range> ranges = rangeList.Ranges;
			source = ranges;
		}
		return from range in source
			from objectType in GetObjectTypesWithinRange(range, sortOrder)
			select objectType;
	}

	protected IEnumerable<ObjectType> GetObjectTypesWithinRange(Range range, SortOrder sortOrder, bool includeObsoleteTables = true)
	{
		if (!range.GetInclusiveIntegerBounds(0, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> lastReadObjectIds = nclMetadata.GetSnapshotOfAllObjects();
		for (int i = ((sortOrder != SortOrder.Ascending) ? (lastReadObjectIds.Count - 1) : 0); i >= 0 && i < lastReadObjectIds.Count; i += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
		{
			ObjectType objectType = lastReadObjectIds.Keys[i];
			if (objectType == ObjectType.Table && !includeObsoleteTables)
			{
				bool flag = true;
				foreach (KeyValuePair<int, AllObjectSnapshotEntry> item in lastReadObjectIds.Values[i])
				{
					if (nclMetadata.TryGetMetaTableById(item.Key, out var nCLMetaTable, -1, requireCompiled: false) && nCLMetaTable.ObsoleteState != ObsoleteState.Removed)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			if (sortOrder == SortOrder.Ascending && objectType == ObjectType.Table && 0 >= low && 0 <= high)
			{
				yield return ObjectType.TableData;
			}
			if ((int)objectType >= low && (int)objectType <= high)
			{
				yield return objectType;
			}
			if (sortOrder == SortOrder.Descending && objectType == ObjectType.Table && 0 >= low && 0 <= high)
			{
				yield return ObjectType.TableData;
			}
		}
	}

	protected IEnumerable<KeyValuePair<int, AllObjectSnapshotEntry>> GetObjectNumberAndInfoWithinRange(ObjectType objectType, Range range, SortOrder sortOrder, bool needNames, bool includeObsoleteTables = true)
	{
		if (!range.GetInclusiveIntegerBounds(0, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		if (objectType == ObjectType.TableData)
		{
			objectType = ObjectType.Table;
		}
		SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> snapshotOfAllObjects = nclMetadata.GetSnapshotOfAllObjects();
		if (!snapshotOfAllObjects.TryGetValue(objectType, out var objects))
		{
			yield break;
		}
		int num = objects.Keys.BinarySearch((sortOrder == SortOrder.Ascending) ? low : high);
		if (num < 0)
		{
			num = ~num;
			if (sortOrder == SortOrder.Descending)
			{
				num--;
			}
		}
		for (int i = num; i >= 0 && i < objects.Count; i += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
		{
			int num2 = objects.Keys[i];
			if (num2 >= low && num2 <= high)
			{
				AllObjectSnapshotEntry allObjectSnapshotEntry = objects.Values[i];
				string text = (needNames ? allObjectSnapshotEntry.Name : null);
				bool flag = true;
				if (objectType == ObjectType.Table && (!includeObsoleteTables || (needNames && string.IsNullOrEmpty(text))) && nclMetadata.TryGetMetaTableById(num2, out var nCLMetaTable, -1, requireCompiled: false))
				{
					flag = includeObsoleteTables || nCLMetaTable.ObsoleteState != ObsoleteState.Removed;
					if (flag)
					{
						text = ((needNames && string.IsNullOrEmpty(text)) ? nCLMetaTable.TableName : text);
						allObjectSnapshotEntry = new AllObjectSnapshotEntry(text, allObjectSnapshotEntry.AppPackageId);
					}
				}
				if (flag)
				{
					yield return new KeyValuePair<int, AllObjectSnapshotEntry>(num2, allObjectSnapshotEntry);
				}
			}
			else if ((sortOrder == SortOrder.Ascending) ? (num2 > high) : (num2 < low))
			{
				break;
			}
		}
	}

	internal string GetObjectCaption(ObjectType type, int objectNumber, string objectName)
	{
		switch (type)
		{
		case ObjectType.TableData:
		case ObjectType.Table:
			return nclMetadata.GetMetaTableById(objectNumber, -1, requireCompiled: false).TableCaptionSafe;
		case ObjectType.Report:
			try
			{
				return nclMetadata.GetMetaReportById(objectNumber, -1, requireCompiled: false).Caption;
			}
			catch (NavReportVersionObsoleteException)
			{
				return objectName;
			}
		case ObjectType.XmlPort:
			return nclMetadata.GetMetaXmlPortById(objectNumber, -1, requireCompiled: false).Caption;
		case ObjectType.Page:
			return nclMetadata.GetMetaFormById(objectNumber, -1, requireCompiled: false).Caption;
		case ObjectType.Query:
		{
			NCLMetaQuery metaQueryById = nclMetadata.GetMetaQueryById(objectNumber, -1, requireCompiled: false);
			string caption = metaQueryById.Caption;
			if (!string.IsNullOrEmpty(caption))
			{
				return caption;
			}
			return objectName;
		}
		case ObjectType.System:
			return SystemObjects.GetSystemObjectCaption(objectNumber);
		default:
			return objectName;
		}
	}

	protected int CountObjectsWithinRange(ObjectType objectType, Range range, bool includeObsoleteTables = true)
	{
		if (!range.GetInclusiveIntegerBounds(0, int.MaxValue, out var low, out var high))
		{
			return 0;
		}
		if (objectType == ObjectType.TableData)
		{
			objectType = ObjectType.Table;
		}
		IList<int> keys;
		if (objectType == ObjectType.System)
		{
			SortedList<int, string> systemObjectAndNames = NCLMetadata.SystemObjectAndNames;
			keys = systemObjectAndNames.Keys;
		}
		else
		{
			SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> snapshotOfAllObjects = nclMetadata.GetSnapshotOfAllObjects();
			if (!snapshotOfAllObjects.TryGetValue(objectType, out var value))
			{
				return 0;
			}
			keys = value.Keys;
		}
		int num = keys.BinarySearch(low);
		if (num < 0)
		{
			num = ~num;
		}
		int num2 = keys.BinarySearch(high);
		if (num2 < 0)
		{
			num2 = ~num2 - 1;
		}
		if ((objectType != ObjectType.Table) | includeObsoleteTables)
		{
			return num2 - num + 1;
		}
		int num3 = 0;
		for (int i = num; i < num2 + 1; i++)
		{
			int tableId = keys[i];
			if (!NclMetadata.TryGetMetaTableById(tableId, out var nCLMetaTable) || nCLMetaTable.ObsoleteState != ObsoleteState.Removed)
			{
				num3++;
			}
		}
		return num3;
	}
}
