using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaFieldRelation
{
	private readonly NCLMetaFilterCollection conditions;

	private readonly int sourceTableId;

	private readonly int sourceFieldId;

	private readonly int referencingTableId;

	private readonly int referencingFieldId;

	private readonly NCLMetaFilterCollection sourceTableFilters;

	private readonly AppGroupAwareLookup<NCLMetaTable> sourceTableLookup;

	private readonly AppGroupAwareLookup<NCLMetaTable> referencingTableLookup;

	private FilterFieldDictionary expressionDictionary;

	public NCLMetaFilterCollection Conditions => conditions;

	internal FilterFieldDictionary ExpressionDictionary
	{
		get
		{
			if (expressionDictionary == null)
			{
				expressionDictionary = DataProviderHelper.MergeFilterFieldDictionaries(conditions.Select((NCLMetaFilter c) => new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(c.SourceField, c.Expression))).ToArray()) ?? FilterFieldDictionary.Empty;
			}
			return expressionDictionary;
		}
	}

	public int SourceTableId => sourceTableId;

	public NCLMetaTable SourceTable => sourceTableLookup.GetReferenceUsingCurrentAppGroup();

	public int SourceFieldId => sourceFieldId;

	public NCLMetaField SourceField
	{
		get
		{
			if (sourceFieldId == 0)
			{
				return SourceTable?.PrimaryKey.GetKeyFieldByIndex(0);
			}
			return SourceTable?.GetFieldByNo(sourceFieldId);
		}
	}

	public int ReferencingTableId => referencingTableId;

	public NCLMetaTable ReferencingTable => referencingTableLookup.GetReferenceUsingCurrentAppGroup();

	public int ReferencingFieldId => referencingFieldId;

	public NCLMetaField ReferencingField => ReferencingTable.GetFieldByNo(referencingFieldId);

	public NCLMetaFilterCollection SourceTableFilters => sourceTableFilters;

	public NCLMetaFieldRelation(NCLMetaFilterCollection conditions, int sourceTableId, int sourceFieldId, int referencingTableId, int referencingFieldId, NCLMetaFilterCollection sourceTableFilters)
	{
		this.sourceTableId = sourceTableId;
		this.sourceFieldId = sourceFieldId;
		this.referencingTableId = referencingTableId;
		this.referencingFieldId = referencingFieldId;
		this.conditions = conditions;
		this.sourceTableFilters = sourceTableFilters;
		sourceTableLookup = AppGroupAwareLookup<NCLMetaTable>.CreateWithDefaultRefreshLogic(ResolveSourceTable);
		referencingTableLookup = AppGroupAwareLookup<NCLMetaTable>.CreateWithDefaultRefreshLogic(ResolveReferencingTable);
	}

	private NCLMetaTable ResolveSourceTable(NavAppGroup appGroup)
	{
		return NavGlobal.NCLMetadata.GetMetaTableById(sourceTableId, appGroup.GroupId, requireCompiled: false);
	}

	private NCLMetaTable ResolveReferencingTable(NavAppGroup appGroup)
	{
		return NavGlobal.NCLMetadata.GetMetaTableById(referencingTableId, appGroup.GroupId, requireCompiled: false);
	}

	public bool CheckIsStale()
	{
		IEnumerable<NCLMetaTable> source = from r in referencingTableLookup.GetAllKnownReferences()
			select r.Item2;
		if (!source.Where((NCLMetaTable t) => t != null).Any((NCLMetaTable t) => t.Stale) && !conditions.CheckIsStale())
		{
			return sourceTableFilters.CheckIsStale();
		}
		return true;
	}

	internal static NCLMetaFieldRelation CreateFromMetaFieldRelation(MetaFieldRelation metaFieldRelation, NCLMetaTable filterTable, NCLMetaField referencingField)
	{
		NCLMetaFilterCollection nCLMetaFilterCollection = new NCLMetaFilterCollection(filterTable.TableId, (NCLMetaFilterCollection filterCollection) => metaFieldRelation.Conditions.Select((MetaCondition condition) => NCLMetaFilter.CreateFromMetaCondition(condition, filterCollection)).ToArray());
		NCLMetaFilterCollection nCLMetaFilterCollection2 = new NCLMetaFilterCollection(metaFieldRelation.TableId, (NCLMetaFilterCollection filterCollection) => metaFieldRelation.Filters.Select((MetaFilter filter) => NCLMetaFilter.CreateFromMetaFilter(filter, filterCollection, filterTable)).ToArray());
		return new NCLMetaFieldRelation(nCLMetaFilterCollection, metaFieldRelation.TableId, metaFieldRelation.FieldId, referencingField.Parent.TableId, referencingField.FieldNo, nCLMetaFilterCollection2);
	}
}
