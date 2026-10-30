namespace Microsoft.Dynamics.Nav.Types;

public sealed class ChartColumn
{
	public enum ChartColumnType
	{
		None,
		Dimension,
		Measure
	}

	public const string ColumnNameKey = "ChartColumnName";

	public const string ColumnTypeKey = "ChartColumnType";

	public const string ColumnMaskedKey = "ChartColumnMasked";

	public const string ColumnOperatorKey = "ChartColumnOperator";

	public const string ColumnReverseSignKey = "ChartColumnReverseSign";
}
