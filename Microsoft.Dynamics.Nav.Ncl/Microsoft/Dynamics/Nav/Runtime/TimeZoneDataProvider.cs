using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TimeZoneDataProvider : EagerVirtualDataProvider
{
	public static readonly TimeZoneDataProvider Instance = new TimeZoneDataProvider();

	public override int TableId => 2000000164;

	private TimeZoneDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = true;
		return GetAllTimeZones();
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetAllTimeZones()
	{
		int maxIdLength = MetaTable.GetFieldByNo(1).FieldDefinedLength;
		int maxDisplayNameLength = MetaTable.GetFieldByNo(2).FieldDefinedLength;
		int timeZoneNo = 1;
		foreach (TimeZoneInfo systemTimeZone in TimeZoneInfo.GetSystemTimeZones())
		{
			yield return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(timeZoneNo), NavText.CreateTruncated(maxIdLength, systemTimeZone.Id), NavText.CreateTruncated(maxDisplayNameLength, systemTimeZone.DisplayName));
			timeZoneNo++;
		}
	}
}
