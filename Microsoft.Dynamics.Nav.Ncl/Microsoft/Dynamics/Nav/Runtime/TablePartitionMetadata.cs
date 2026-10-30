namespace Microsoft.Dynamics.Nav.Runtime;

public class TablePartitionMetadata
{
	public static readonly TablePartitionMetadata CompanyIdPartitionMetadata = new TablePartitionMetadata
	{
		PartitionSchemaName = "CompanyIdPartitionScheme",
		PartitionColumnName = "$companyId"
	};

	public string PartitionSchemaName { get; set; }

	public string PartitionColumnName { get; set; }
}
