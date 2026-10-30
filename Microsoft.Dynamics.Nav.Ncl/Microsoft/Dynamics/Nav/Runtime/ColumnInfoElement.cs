namespace Microsoft.Dynamics.Nav.Runtime;

internal class ColumnInfoElement
{
	public string ColumnName { get; set; }

	public string DisplayName { get; set; }

	public int FieldNumber { get; set; }

	public string SourceExpression { get; set; }

	public int PreferredWidth { get; set; }

	public string DecimalFormatString { get; set; }

	public ColumnInfoElement()
	{
		ColumnName = string.Empty;
		DisplayName = string.Empty;
		FieldNumber = -1;
		SourceExpression = string.Empty;
		PreferredWidth = 0;
		DecimalFormatString = string.Empty;
	}
}
