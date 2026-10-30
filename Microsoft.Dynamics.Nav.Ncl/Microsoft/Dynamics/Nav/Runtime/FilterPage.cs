using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class FilterPage : NavForm
{
	public IReadOnlyList<NavRecordRef> IndexedNavRecords { get; private set; }

	public FilterPage(ITreeObject parent, MasterPage metaForm, IReadOnlyList<NavRecordRef> records)
		: base(parent, metaForm)
	{
		IndexedNavRecords = records;
		InitializeForm();
		for (int i = 0; i < records.Count; i++)
		{
			string text = string.Format(CultureInfo.InvariantCulture, "DataItem{0}TableView", i);
			NavRecordRef navRecord = records[i];
			RegisterSourceExpression(text, text, typeof(byte[]), SourceExpressionType.DataItem, () => ALCompiler.ToNavValue(navRecord.Record.GetTableView().ToByteArray()), (NavValue setValue) =>
			{
				navRecord.Record.SetTableView(NavTableView.Create(setValue.GetBytes()));
			});
		}
		SetTableViewUsed = true;
	}

	protected override string GetAutoFormatString(int fieldNumber)
	{
		return string.Empty;
	}

	protected override string GetDecimalPlaces(int fieldNumber)
	{
		return string.Empty;
	}
}
