using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class LicensePermissionDataProvider : RangeBasedComputedDataProvider
{
	public static readonly LicensePermissionDataProvider Instance = new LicensePermissionDataProvider();

	public override int TableId => 2000000043;

	private LicensePermissionDataProvider()
	{
	}

	internal static int MapPermissionMaskToPemissionOption(PermissionMask licenseMask, PermissionMask yes, PermissionMask indirect)
	{
		if ((licenseMask & yes) != 0)
		{
			return 1;
		}
		if ((licenseMask & indirect) != 0)
		{
			return 2;
		}
		return 0;
	}

	internal static int MapPermissionMaskToLimitedUsagePermissionOption(PermissionMask licenseMask)
	{
		if ((licenseMask & PermissionMask.ExcludedLimitedUsage) != 0)
		{
			return 2;
		}
		if ((licenseMask & PermissionMask.IncludedLimitedUsage) != 0)
		{
			return 1;
		}
		return 3;
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		return keyField.FieldNo switch
		{
			1 => EnumerateTypes(range, sortOrder), 
			2 => EnumerateNoInType((NavOption)precedingKeyFieldValues[1], range, sortOrder, nonPrimaryKeyFilters), 
			_ => throw new NotSupportedException(), 
		};
	}

	private IEnumerable<ReadOnlyRecordBuffer> EnumerateTypes(Range range, SortOrder sortOrder)
	{
		INavLicense license = NavCurrentThread.Session.License;
		if (license != null && range.GetInclusiveIntegerBounds(0, 12, out var low, out var high))
		{
			for (int i = ((sortOrder == SortOrder.Ascending) ? low : high); i >= low && i <= high; i += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
			{
				yield return new ReadOnlyRecordBuffer(VirtualDataProvider.VirtualTimeStamp, GetOptionValue(1, i));
			}
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> EnumerateNoInType(NavOption type, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		if (!range.GetInclusiveIntegerBounds(0, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		INavLicense license = NavCurrentThread.Session.License;
		for (int i = ((sortOrder == SortOrder.Ascending) ? low : high); i >= low && i <= high; i += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
		{
			PermissionMask licensePermissionMask = license.GetLicensePermissionMask((ObjectType)type.ToByte(), i);
			ReadOnlyRecordBuffer readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, CreateNavValuesFromPermissionMask(type, i, licensePermissionMask));
			if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				yield return readOnlyRecordBuffer;
			}
			if (i == int.MaxValue && sortOrder == SortOrder.Ascending)
			{
				break;
			}
		}
	}

	private NavValue[] CreateNavValuesFromPermissionMask(NavOption type, int objectId, PermissionMask mask)
	{
		ObjectType objectType = (ObjectType)type.ToByte();
		if (objectType != ObjectType.LimitedUsageTableData)
		{
			return new NavValue[9]
			{
				VirtualDataProvider.VirtualTimeStamp,
				type,
				NavInteger.Create(objectId),
				GetOptionValue(10, MapPermissionMaskToPemissionOption(mask, PermissionMask.Read, PermissionMask.IndirectRead)),
				GetOptionValue(11, MapPermissionMaskToPemissionOption(mask, PermissionMask.Insert, PermissionMask.IndirectInsert)),
				GetOptionValue(12, MapPermissionMaskToPemissionOption(mask, PermissionMask.Modify, PermissionMask.IndirectModify)),
				GetOptionValue(13, MapPermissionMaskToPemissionOption(mask, PermissionMask.Delete, PermissionMask.IndirectDelete)),
				GetOptionValue(14, MapPermissionMaskToPemissionOption(mask, PermissionMask.Execute, PermissionMask.IndirectExecute)),
				GetOptionValue(15, 0)
			};
		}
		return new NavValue[9]
		{
			VirtualDataProvider.VirtualTimeStamp,
			type,
			NavInteger.Create(objectId),
			GetOptionValue(10, 0),
			GetOptionValue(11, 0),
			GetOptionValue(12, 0),
			GetOptionValue(13, 0),
			GetOptionValue(14, 0),
			GetOptionValue(15, MapPermissionMaskToLimitedUsagePermissionOption(mask))
		};
	}
}
