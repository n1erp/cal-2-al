using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class PrinterDataProvider : EagerVirtualDataProvider
{
	public static readonly PrinterDataProvider Instance = new PrinterDataProvider();

	public override int TableId => 2000000039;

	private PrinterDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = false;
		IClientCallback clientCallbackOrNull = NavCurrentThread.Session.ClientCallbackOrNull;
		Printer[] array = ((clientCallbackOrNull != null) ? NavCurrentThread.Session.ClientCallback.GetPrinters() : PrinterHelper.GetPrinters());
		if (array == null)
		{
			return Enumerable.Empty<ReadOnlyRecordBuffer>();
		}
		ReadOnlyRecordBuffer[] array2 = new ReadOnlyRecordBuffer[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, GetTruncatedTextValue(1, array[i].Name), GetTruncatedTextValue(2, array[i].Name), GetTruncatedTextValue(3, array[i].Driver), GetTruncatedTextValue(4, array[i].Device));
		}
		return array2;
	}
}
