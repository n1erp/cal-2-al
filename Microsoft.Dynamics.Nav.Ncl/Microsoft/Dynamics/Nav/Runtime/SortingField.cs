namespace Microsoft.Dynamics.Nav.Runtime;

public class SortingField : INavSortingField
{
	public SortOrder Order { get; private set; }

	public INavFieldMetadata Field { get; private set; }

	internal SortingField(INavFieldMetadata field, SortOrder order)
	{
		Field = field;
		Order = order;
	}
}
