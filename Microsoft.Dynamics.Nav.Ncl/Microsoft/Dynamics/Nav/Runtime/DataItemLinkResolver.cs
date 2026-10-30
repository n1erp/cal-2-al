namespace Microsoft.Dynamics.Nav.Runtime;

internal class DataItemLinkResolver : TableFilterResolver
{
	private readonly NCLMetaTable referencedTable;

	protected override string Description => "{0} {1}";

	public DataItemLinkResolver(NCLMetaTable table, NCLMetaTable referencedTable, string input)
		: base(table, input)
	{
		this.referencedTable = referencedTable;
	}

	protected override INavFieldMetadata ResolveReferencedField(string field)
	{
		return referencedTable.FindFieldMatch(field);
	}

	protected override void ValidateFilterType(NCLMetaFilterType filterType)
	{
		if (filterType != NCLMetaFilterType.Field)
		{
			throw GetInvalidExpressionException();
		}
	}
}
