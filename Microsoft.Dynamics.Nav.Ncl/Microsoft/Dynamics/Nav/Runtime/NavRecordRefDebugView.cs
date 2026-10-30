using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavRecordRefDebugView : NavTypeDebuggerView
{
	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	[DebuggerDisplay("{viewedObject.ALGetFilterGroup}")]
	public FilterGroupInfo FilterGroup => GetFilterGroupProperties();

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	[DebuggerDisplay("{viewedObject.ALGetTableName}")]
	public TableInfo Table => GetTableInfo();

	[DebuggerDisplay("{viewedObject.ALGetFilters}")]
	public ReadOnlyCollection<FilterInfo> Filters => GetFiltersProperties();

	[DebuggerDisplay("Number of Fields = {viewedObject.ALFieldCount}")]
	public ReadOnlyCollection<FieldInfo> Fields => GetFieldInfo();

	[DebuggerDisplay("Number of Keys = {viewedObject.ALKeyCount}")]
	public ReadOnlyCollection<KeyInfo> Keys => GetKeysProperties();

	public NavRecordRefDebugView(NavRecordRef navRecordRef)
	{
		SetViewedObject(navRecordRef.SafeRecord);
	}
}
