using System.Collections.Generic;
using Microsoft.Dynamics.Nav.OpenXml.Spreadsheet;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Office;

internal interface IExcelWriter
{
	int ObjectId();

	int RowCount();

	string ObjectCaption();

	ColumnInfoElement CreateColumnInfoElement(ControlDefinition control);

	IEnumerable<List<ICellValueTypeFormula>> GenerateData(List<ColumnInfoElement> pageColumns);

	void ApplyFilters(List<ColumnInfoElement> pageColumns);

	IOfficeDownloadHandler DownloadHandler();

	List<ElementDefinition> ControlDefinitions();
}
