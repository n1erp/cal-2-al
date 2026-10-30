using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NCLMetaFilterField : NCLMetaFilter
{
	private readonly int valueFieldId;

	private readonly NCLMetaFilterModes filterMode;

	private readonly int valueTableId;

	private readonly AppGroupAwareLookup<NCLMetaTable> valueTableLookup;

	public int ValueFieldId => valueFieldId;

	public NCLMetaFilterModes FilterModes => filterMode;

	public NCLMetaTable ValueTable => valueTableLookup.GetReferenceUsingCurrentAppGroup();

	public INavFieldMetadata ValueField => ValueTable.GetFieldByNo(valueFieldId);

	public NCLMetaFilterField(NCLMetaFilterCollection parentFilterCollection, int sourceFieldId, NCLMetaTable valueTable, int valueFieldId, NCLMetaFilterModes filterMode)
		: base(parentFilterCollection, NCLMetaFilterType.Field, sourceFieldId)
	{
		valueTableId = valueTable.TableId;
		this.valueFieldId = valueFieldId;
		this.filterMode = filterMode;
		valueTableLookup = AppGroupAwareLookup<NCLMetaTable>.CreateWithDefaultRefreshLogic(ResolveValueTable);
		valueTableLookup.SetKnownReference(0, valueTable);
	}

	public NCLMetaFilterField(NCLMetaFilterCollection parentFilterCollection, int sourceFieldId, int valueTableId, int valueFieldId, NCLMetaFilterModes filterMode)
		: base(parentFilterCollection, NCLMetaFilterType.Field, sourceFieldId)
	{
		this.valueTableId = valueTableId;
		this.valueFieldId = valueFieldId;
		this.filterMode = filterMode;
		valueTableLookup = AppGroupAwareLookup<NCLMetaTable>.CreateWithDefaultRefreshLogic(ResolveValueTable);
	}

	private NCLMetaTable ResolveValueTable(NavAppGroup appGroup)
	{
		return NavGlobal.NCLMetadata.GetMetaTableById(valueTableId, appGroup.GroupId, requireCompiled: false);
	}

	public override bool CheckIsStale()
	{
		if (base.CheckIsStale())
		{
			return true;
		}
		IEnumerable<NCLMetaTable> source = from r in valueTableLookup.GetAllKnownReferences()
			select r.Item2;
		return source.Where((NCLMetaTable t) => t != null).Any((NCLMetaTable t) => t.Stale);
	}

	internal static NCLMetaFilterField CreateFromMetaFilter(NCLMetaFilterCollection parentFilterCollection, MetaFilter metaFilter, NCLMetaTable parentTable)
	{
		if (!int.TryParse(metaFilter.FilterValue, out var result))
		{
			throw new ArgumentException(metaFilter.FilterValue);
		}
		NCLMetaFilterModes nCLMetaFilterModes = (NCLMetaFilterModes)((metaFilter.OnlyMaxLimit ? 1 : 0) | (metaFilter.ValueIsFilter ? 2 : 0));
		return new NCLMetaFilterField(parentFilterCollection, metaFilter.FieldId, parentTable.TableId, result, nCLMetaFilterModes);
	}
}
