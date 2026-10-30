namespace Microsoft.Dynamics.Nav.Types.Metadata;

public interface IFilterDefinition
{
	int FieldId { get; }

	int FilterGroup { get; }

	FilterType FilterType { get; }

	string FilterValue { get; }

	bool ValueIsFilter { get; }
}
