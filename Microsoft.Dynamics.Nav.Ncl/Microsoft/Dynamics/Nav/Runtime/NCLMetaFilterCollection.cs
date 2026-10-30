using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaFilterCollection : IEnumerable<NCLMetaFilter>, IEnumerable
{
	private readonly ReadOnlyCollection<NCLMetaFilter> filters;

	private readonly int sourceTableId;

	private readonly AppGroupAwareLookup<NCLMetaTable> sourceTableLookup;

	public static readonly NCLMetaFilterCollection Empty = new NCLMetaFilterCollection(0, (NCLMetaFilterCollection filterCollection) => new List<NCLMetaFilter>());

	public int Count => filters.Count;

	public NCLMetaFilter this[int index] => filters[index];

	public int SourceTableId => sourceTableId;

	public NCLMetaTable SourceTable => sourceTableLookup.GetReferenceUsingCurrentAppGroup();

	public NCLMetaFilterCollection(int sourceTableId, Func<NCLMetaFilterCollection, IList<NCLMetaFilter>> filterCreator)
	{
		if (filterCreator == null)
		{
			throw new ArgumentNullException("filterCreator");
		}
		this.sourceTableId = sourceTableId;
		sourceTableLookup = AppGroupAwareLookup<NCLMetaTable>.CreateWithDefaultRefreshLogic(ResolveSourceTable);
		filters = new ReadOnlyCollection<NCLMetaFilter>(filterCreator(this));
	}

	public NCLMetaFilterCollection(NCLMetaTable sourceTable, Func<NCLMetaFilterCollection, IList<NCLMetaFilter>> filterCreator)
	{
		if (filterCreator == null)
		{
			throw new ArgumentNullException("filterCreator");
		}
		sourceTableLookup = AppGroupAwareLookup<NCLMetaTable>.CreateWithDefaultRefreshLogic(ResolveSourceTable);
		sourceTableLookup.SetKnownReference(0, sourceTable);
		sourceTableId = sourceTable.TableId;
		filters = new ReadOnlyCollection<NCLMetaFilter>(filterCreator(this));
	}

	private NCLMetaTable ResolveSourceTable(NavAppGroup appGroup)
	{
		return NavGlobal.NCLMetadata.GetMetaTableById(sourceTableId, appGroup.GroupId, requireCompiled: false);
	}

	public static NCLMetaFilterCollection CreateFromMetaFilter(IEnumerable<MetaFilter> metaFilters, int sourceTableId, NCLMetaTable parentTable)
	{
		return new NCLMetaFilterCollection(sourceTableId, (NCLMetaFilterCollection filterCollection) => metaFilters.Select((MetaFilter metaFilter) => NCLMetaFilter.CreateFromMetaFilter(metaFilter, filterCollection, parentTable)).ToArray());
	}

	public bool CheckIsStale()
	{
		IEnumerable<NCLMetaTable> source = from r in sourceTableLookup.GetAllKnownReferences()
			select r.Item2;
		if (!source.Where((NCLMetaTable t) => t != null).Any((NCLMetaTable t) => t.Stale))
		{
			return filters.Any((NCLMetaFilter filter) => filter.CheckIsStale());
		}
		return true;
	}

	public NCLMetaTable GetSourceTableForGroup(NavAppGroup appGroup)
	{
		return sourceTableLookup.GetReferenceUsingSpecificAppGroup(appGroup);
	}

	public IEnumerator<NCLMetaFilter> GetEnumerator()
	{
		return filters.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
