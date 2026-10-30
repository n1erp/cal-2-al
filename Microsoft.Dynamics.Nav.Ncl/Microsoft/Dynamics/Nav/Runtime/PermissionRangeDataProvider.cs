using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class PermissionRangeDataProvider : RangeBasedComputedDataProvider
{
	public static readonly PermissionRangeDataProvider Instance = new PermissionRangeDataProvider();

	public override int TableId => 2000000044;

	private PermissionRangeDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		return keyField.FieldNo switch
		{
			1 => EnumerateTypes(range, sortOrder), 
			2 => EnumerateIndexInType((NavOption)precedingKeyFieldValues[1], range, sortOrder, nonPrimaryKeyFilters), 
			_ => throw new NotSupportedException(), 
		};
	}

	private IEnumerable<ReadOnlyRecordBuffer> EnumerateTypes(Range range, SortOrder sortOrder)
	{
		INavLicense license = NavCurrentThread.Session.License;
		if (license == null || !range.GetInclusiveIntegerBounds(0, 12, out var low, out var high))
		{
			yield break;
		}
		ushort i = (ushort)((sortOrder == SortOrder.Ascending) ? low : high);
		LicenseSearchDirection direction = ((sortOrder == SortOrder.Ascending) ? LicenseSearchDirection.GreaterThan : LicenseSearchDirection.LessThan);
		if (!license.GetNextType(LicenseSearchDirection.Equals, ref i) && !license.GetNextType(direction, ref i))
		{
			yield break;
		}
		do
		{
			if (i < low || i > high)
			{
				if ((sortOrder == SortOrder.Ascending && i > high) || (sortOrder == SortOrder.Descending && i < low))
				{
					break;
				}
			}
			else
			{
				yield return new ReadOnlyRecordBuffer(VirtualDataProvider.VirtualTimeStamp, GetOptionValue(1, i));
			}
		}
		while (license.GetNextType(direction, ref i));
	}

	private IEnumerable<ReadOnlyRecordBuffer> EnumerateIndexInType(NavOption type, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		if (!range.GetInclusiveIntegerBounds(0, 32767, out var low, out var high))
		{
			yield break;
		}
		INavLicense license = NavCurrentThread.Session.License;
		short i = (short)((sortOrder == SortOrder.Ascending) ? low : high);
		LicenseSearchDirection direction = ((sortOrder == SortOrder.Ascending) ? LicenseSearchDirection.GreaterThan : LicenseSearchDirection.LessThan);
		if (!license.GetNextIndex(LicenseSearchDirection.Equals, type.ToByte(), ref i) && !license.GetNextIndex(direction, type.ToByte(), ref i))
		{
			yield break;
		}
		do
		{
			if (i < low || i > high)
			{
				if ((sortOrder == SortOrder.Ascending && i > high) || (sortOrder == SortOrder.Descending && i < low))
				{
					break;
				}
				continue;
			}
			license.GetIndexData(type.ToByte(), i, out var from, out var to);
			PermissionMask licensePermissionMask = license.GetLicensePermissionMask((ObjectType)type.ToByte(), (int)from);
			ReadOnlyRecordBuffer readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, CreateNavValuesFromPermissionMask(type, i, (int)from, (int)to, licensePermissionMask));
			if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				yield return readOnlyRecordBuffer;
			}
		}
		while (license.GetNextIndex(direction, type.ToByte(), ref i));
	}

	private NavValue[] CreateNavValuesFromPermissionMask(NavOption type, int index, int from, int to, PermissionMask mask)
	{
		ObjectType objectType = (ObjectType)type.ToByte();
		if (objectType != ObjectType.LimitedUsageTableData)
		{
			return new NavValue[11]
			{
				VirtualDataProvider.VirtualTimeStamp,
				type,
				NavInteger.Create(index),
				NavInteger.Create(from),
				NavInteger.Create(to),
				GetOptionValue(10, LicensePermissionDataProvider.MapPermissionMaskToPemissionOption(mask, PermissionMask.Read, PermissionMask.IndirectRead)),
				GetOptionValue(11, LicensePermissionDataProvider.MapPermissionMaskToPemissionOption(mask, PermissionMask.Insert, PermissionMask.IndirectInsert)),
				GetOptionValue(12, LicensePermissionDataProvider.MapPermissionMaskToPemissionOption(mask, PermissionMask.Modify, PermissionMask.IndirectModify)),
				GetOptionValue(13, LicensePermissionDataProvider.MapPermissionMaskToPemissionOption(mask, PermissionMask.Delete, PermissionMask.IndirectDelete)),
				GetOptionValue(14, LicensePermissionDataProvider.MapPermissionMaskToPemissionOption(mask, PermissionMask.Execute, PermissionMask.IndirectExecute)),
				GetOptionValue(15, 0)
			};
		}
		return new NavValue[11]
		{
			VirtualDataProvider.VirtualTimeStamp,
			type,
			NavInteger.Create(index),
			NavInteger.Create(from),
			NavInteger.Create(to),
			GetOptionValue(10, 0),
			GetOptionValue(11, 0),
			GetOptionValue(12, 0),
			GetOptionValue(13, 0),
			GetOptionValue(14, 0),
			GetOptionValue(15, LicensePermissionDataProvider.MapPermissionMaskToLimitedUsagePermissionOption(mask))
		};
	}
}
