using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NCLMetaFilter
{
	private static ReadOnlyCollection<NCLMetaFilter> emptyFilters = new ReadOnlyCollection<NCLMetaFilter>(new List<NCLMetaFilter>());

	private readonly NCLMetaFilterType filterType;

	private readonly NCLMetaFilterCollection parentFilterCollection;

	private readonly AppGroupAwareLookup<NCLMetaField> sourceFieldLookup;

	private readonly int sourceFieldId;

	public virtual FilterExpression Expression => null;

	public static ReadOnlyCollection<NCLMetaFilter> EmptyFilters => emptyFilters;

	public NCLMetaFilterType FilterType => filterType;

	public int SourceFieldId => sourceFieldId;

	public NCLMetaField SourceField => sourceFieldLookup.GetReferenceUsingCurrentAppGroup();

	protected NCLMetaFilter(NCLMetaFilterCollection parentFilterCollection, NCLMetaFilterType filterType, int sourceFieldId)
	{
		this.parentFilterCollection = parentFilterCollection;
		this.filterType = filterType;
		this.sourceFieldId = sourceFieldId;
		sourceFieldLookup = AppGroupAwareLookup<NCLMetaField>.CreateWithDefaultRefreshLogic(ResolveSourceField);
	}

	protected NCLMetaFilter(NCLMetaFilterCollection parentFilterCollection, NCLMetaFilterType filterType, NCLMetaField sourceField)
	{
		this.parentFilterCollection = parentFilterCollection;
		this.filterType = filterType;
		sourceFieldLookup = AppGroupAwareLookup<NCLMetaField>.CreateWithDefaultRefreshLogic(ResolveSourceField);
		sourceFieldLookup.SetKnownReference(0, sourceField);
	}

	private NCLMetaField ResolveSourceField(NavAppGroup appGroup)
	{
		return parentFilterCollection.GetSourceTableForGroup(appGroup).GetFieldByNo(sourceFieldId);
	}

	public virtual bool CheckIsStale()
	{
		IEnumerable<NCLMetaField> source = from r in sourceFieldLookup.GetAllKnownReferences()
			select r.Item2;
		return source.Where((NCLMetaField t) => t != null).Any((NCLMetaField t) => t.Parent.Stale);
	}

	internal static NCLMetaFilter CreateFromMetaFilter(MetaFilter metaFilter, NCLMetaFilterCollection parentFilterCollection, NCLMetaTable parentTable)
	{
		return metaFilter.FilterType switch
		{
			Microsoft.Dynamics.Nav.Types.Metadata.FilterType.CONST => (NCLMetaFilter)NCLMetaFilterConst.CreateFromMetaFilter(metaFilter, parentFilterCollection), 
			Microsoft.Dynamics.Nav.Types.Metadata.FilterType.FILTER => NCLMetaFilterExpression.CreateFromMetaFilter(parentFilterCollection, metaFilter), 
			Microsoft.Dynamics.Nav.Types.Metadata.FilterType.FIELD => NCLMetaFilterField.CreateFromMetaFilter(parentFilterCollection, metaFilter, parentTable), 
			_ => throw new NotSupportedException(), 
		};
	}

	internal static NCLMetaFilter CreateFromMetaCondition(MetaCondition metaCondition, NCLMetaFilterCollection parentFilterCollection)
	{
		return metaCondition.ConditionType switch
		{
			Microsoft.Dynamics.Nav.Types.Metadata.FilterType.CONST => (NCLMetaFilter)NCLMetaFilterConst.CreateFromMetaCondition(metaCondition, parentFilterCollection), 
			Microsoft.Dynamics.Nav.Types.Metadata.FilterType.FILTER => NCLMetaFilterExpression.CreateFromMetaCondition(parentFilterCollection, metaCondition), 
			_ => throw new NotSupportedException(), 
		};
	}
}
