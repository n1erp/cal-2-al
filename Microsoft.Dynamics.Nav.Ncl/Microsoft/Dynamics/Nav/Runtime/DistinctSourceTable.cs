using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class DistinctSourceTable
{
	private List<NCLMetaField> numericFlowFields;

	private List<NCLMetaField> existsFlowFields;

	private List<NCLMetaField> lookupFlowFields;

	private List<NCLMetaField> minMaxFlowFields;

	public NCLMetaTable Table { get; private set; }

	internal FilterFieldDictionary Filters { get; private set; }

	public FieldList NumericFlowFields
	{
		get
		{
			if (numericFlowFields == null)
			{
				return null;
			}
			INavFieldMetadata[] fields = numericFlowFields.ToArray();
			return new FieldList(fields);
		}
	}

	public FieldList ExistsFlowFields
	{
		get
		{
			if (existsFlowFields == null)
			{
				return null;
			}
			INavFieldMetadata[] fields = existsFlowFields.ToArray();
			return new FieldList(fields);
		}
	}

	public FieldList LookupFlowFields
	{
		get
		{
			if (lookupFlowFields == null)
			{
				return null;
			}
			INavFieldMetadata[] fields = lookupFlowFields.ToArray();
			return new FieldList(fields);
		}
	}

	public FieldList MinMaxFlowFields
	{
		get
		{
			if (minMaxFlowFields == null)
			{
				return null;
			}
			INavFieldMetadata[] fields = minMaxFlowFields.ToArray();
			return new FieldList(fields);
		}
	}

	internal DistinctSourceTable(NCLMetaTable table, FilterFieldDictionary filters)
	{
		Table = table;
		Filters = filters;
	}

	internal DistinctSourceTable(NCLMetaTable table, FilterFieldDictionary filters, List<NCLMetaField> numericFlowFields, List<NCLMetaField> existsFlowFields, List<NCLMetaField> lookupFlowFields, List<NCLMetaField> minMaxFlowFields)
		: this(table, filters)
	{
		this.numericFlowFields = numericFlowFields;
		this.existsFlowFields = existsFlowFields;
		this.lookupFlowFields = lookupFlowFields;
		this.minMaxFlowFields = minMaxFlowFields;
	}

	public void AddField(NCLMetaField field)
	{
		List<NCLMetaField> list = null;
		FlowFieldsHelper.CheckFlowFieldProperties(field);
		switch (field.CalculationFormula.CalculationMethod)
		{
		case NCLMetaCalculationMethod.Sum:
		case NCLMetaCalculationMethod.Average:
		case NCLMetaCalculationMethod.Count:
			if (numericFlowFields == null)
			{
				numericFlowFields = new List<NCLMetaField>();
			}
			list = numericFlowFields;
			break;
		case NCLMetaCalculationMethod.Exists:
			if (existsFlowFields == null)
			{
				existsFlowFields = new List<NCLMetaField>();
			}
			list = existsFlowFields;
			break;
		case NCLMetaCalculationMethod.Min:
		case NCLMetaCalculationMethod.Max:
		case NCLMetaCalculationMethod.Lookup:
			if (field.CalculationFormula.CalculationMethod == NCLMetaCalculationMethod.Lookup)
			{
				if (lookupFlowFields == null)
				{
					lookupFlowFields = new List<NCLMetaField>();
				}
				list = lookupFlowFields;
			}
			else
			{
				if (minMaxFlowFields == null)
				{
					minMaxFlowFields = new List<NCLMetaField>();
				}
				list = minMaxFlowFields;
			}
			break;
		}
		if (!list.Contains(field))
		{
			list.Add(field);
		}
	}
}
