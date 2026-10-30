using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NCLMetaCalculationFormula
{
	public static readonly NCLMetaCalculationFormula EmptyFormula = new NCLMetaCalculationFormula(0, 0, NCLMetaCalculationMethod.None, negateResult: false, null);

	private readonly int sourceTableId;

	private readonly int sourceFieldId;

	private readonly AppGroupAwareLookup<NCLMetaField> sourceFieldLookup;

	private readonly NCLMetaCalculationMethod calculationMethod;

	private readonly bool negateResult;

	private readonly NCLMetaFilterCollection filters;

	private ReadOnlyCollection<NCLMetaField> nonstandardJoinCriteriaFields;

	private bool nonstandardJoinCriteriaFieldsCalculated;

	public int TableId => sourceTableId;

	public int FieldId => sourceFieldId;

	public NCLMetaField SourceField
	{
		get
		{
			NCLMetaField referenceUsingCurrentAppGroup = sourceFieldLookup.GetReferenceUsingCurrentAppGroup();
			ObsolescenceGuard.ThrowIfObsoleted(referenceUsingCurrentAppGroup);
			return referenceUsingCurrentAppGroup;
		}
	}

	public NCLMetaCalculationMethod CalculationMethod => calculationMethod;

	public bool NegateResult => negateResult;

	public NCLMetaFilterCollection Filters => filters;

	public bool ContainsNestedFlowFields
	{
		get
		{
			if (!filters.Any((NCLMetaFilter filter) =>
			{
				NCLMetaFilterField nCLMetaFilterField = filter as NCLMetaFilterField;
				return filter.SourceField.FieldClass == FieldClass.FlowField || (nCLMetaFilterField != null && nCLMetaFilterField.ValueField.FieldClass == FieldClass.FlowField);
			}))
			{
				return SourceField.FieldClass == FieldClass.FlowField;
			}
			return true;
		}
	}

	public ReadOnlyCollection<NCLMetaField> NonstandardJoinCriteriaFields
	{
		get
		{
			if (!nonstandardJoinCriteriaFieldsCalculated)
			{
				IEnumerable<NCLMetaFilterField> fieldFilters = from filter in filters
					where filter.FilterType == NCLMetaFilterType.Field
					select (NCLMetaFilterField)filter;
				NCLMetaField[] array = (from filter in fieldFilters.Where((NCLMetaFilterField filter) =>
					{
						if ((filter.FilterModes & NCLMetaFilterModes.ValueIsFilter) == NCLMetaFilterModes.ValueIsFilter && filter.ValueField.FieldClass != FieldClass.FlowFilter)
						{
							return true;
						}
						return fieldFilters.Any((NCLMetaFilterField otherFilter) => otherFilter != filter && (otherFilter.FilterModes & NCLMetaFilterModes.ValueIsFilter) != NCLMetaFilterModes.ValueIsFilter && otherFilter.SourceFieldId == filter.SourceFieldId) ? true : false;
					})
					select (NCLMetaField)filter.ValueField).ToArray();
				if (array.Length != 0)
				{
					nonstandardJoinCriteriaFields = new ReadOnlyCollection<NCLMetaField>(array);
				}
				nonstandardJoinCriteriaFieldsCalculated = true;
			}
			return nonstandardJoinCriteriaFields;
		}
	}

	public NCLMetaCalculationFormula(int sourceTableId, int sourceFieldId, NCLMetaCalculationMethod calculationMethod, bool negateResult, NCLMetaFilterCollection filters)
	{
		this.sourceTableId = sourceTableId;
		this.sourceFieldId = sourceFieldId;
		this.calculationMethod = calculationMethod;
		this.negateResult = negateResult;
		this.filters = filters;
		sourceFieldLookup = AppGroupAwareLookup<NCLMetaField>.CreateWithDefaultRefreshLogic(ResolveSourceField);
	}

	private NCLMetaField ResolveSourceField(NavAppGroup appGroup)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(sourceTableId, appGroup.GroupId, requireCompiled: false);
		return metaTableById.GetFieldByNo(sourceFieldId);
	}

	public bool CheckIsStale()
	{
		IEnumerable<NCLMetaField> source = from r in sourceFieldLookup.GetAllKnownReferences()
			select r.Item2;
		if (!source.Where((NCLMetaField t) => t != null).Any((NCLMetaField t) => t.Parent.Stale))
		{
			if (filters != null)
			{
				return filters.CheckIsStale();
			}
			return false;
		}
		return true;
	}

	public NavValue NegateValue(NavValue value)
	{
		return SourceField.FieldNavType switch
		{
			NavType.Decimal => NavDecimal.Create(-((NavDecimal)value).Value), 
			NavType.BigInteger => NavBigInteger.Create(-((NavBigInteger)value).Value), 
			NavType.Integer => NavInteger.Create(-((NavInteger)value).Value), 
			_ => value, 
		};
	}

	internal static NCLMetaCalculationFormula CreateFromMetaCalculationFormula(MetaCalcFormula metaCalcFormula, NCLMetaTable parentTable)
	{
		return new NCLMetaCalculationFormula(calculationMethod: metaCalcFormula.FlowFieldType.ToUpperInvariant() switch
		{
			"SUM" => NCLMetaCalculationMethod.Sum, 
			"AVERAGE" => NCLMetaCalculationMethod.Average, 
			"EXIST" => NCLMetaCalculationMethod.Exists, 
			"COUNT" => NCLMetaCalculationMethod.Count, 
			"MIN" => NCLMetaCalculationMethod.Min, 
			"MAX" => NCLMetaCalculationMethod.Max, 
			"LOOKUP" => NCLMetaCalculationMethod.Lookup, 
			_ => throw new ArgumentException(metaCalcFormula.FlowFieldType), 
		}, sourceTableId: metaCalcFormula.TableId, sourceFieldId: metaCalcFormula.FieldId, negateResult: metaCalcFormula.ReverseSign, filters: NCLMetaFilterCollection.CreateFromMetaFilter(metaCalcFormula.Filters, metaCalcFormula.TableId, parentTable));
	}
}
