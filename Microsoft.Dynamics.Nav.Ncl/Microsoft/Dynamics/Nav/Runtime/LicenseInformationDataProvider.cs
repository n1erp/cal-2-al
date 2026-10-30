using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class LicenseInformationDataProvider : EagerVirtualDataProvider
{
	public static readonly LicenseInformationDataProvider Instance = new LicenseInformationDataProvider();

	public override int TableId => 2000000040;

	private LicenseInformationDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = true;
		return GetAllLicenseInformationLines();
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetAllLicenseInformationLines()
	{
		string[] array = NavCurrentThread.Session.License.Header.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
		ReadOnlyRecordBuffer[] array2 = new ReadOnlyRecordBuffer[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(i + 1), new NavText(MetaTable.GetFieldByNo(2).FieldDefinedLength, array[i]));
		}
		return array2;
	}
}
