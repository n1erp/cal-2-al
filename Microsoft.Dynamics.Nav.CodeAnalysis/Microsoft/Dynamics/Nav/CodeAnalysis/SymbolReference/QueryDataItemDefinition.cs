using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class QueryDataItemDefinition : LanguageElementWithProperties
{
	public string RelatedTable { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public QueryDataItemDefinition[] DataItems { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public QueryColumnDefinition[] Columns { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public QueryFilterDefinition[] Filters { get; set; }
}
