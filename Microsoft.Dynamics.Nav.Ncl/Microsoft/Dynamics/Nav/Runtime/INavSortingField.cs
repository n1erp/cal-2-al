namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavSortingField
{
	SortOrder Order { get; }

	INavFieldMetadata Field { get; }
}
