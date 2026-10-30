namespace Microsoft.Dynamics.Nav.Runtime;

internal class PageTableFieldData
{
	internal int FieldNo { get; set; }

	internal int TableNo { get; set; }

	internal NavOption FieldType { get; set; }

	internal int FieldLength { get; set; }

	internal string Caption { get; set; }

	internal bool IsTableField { get; set; }

	internal string DataColumnName { get; set; }
}
