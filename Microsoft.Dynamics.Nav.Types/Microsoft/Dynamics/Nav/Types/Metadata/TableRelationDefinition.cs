using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "TRD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class TableRelationDefinition : ElementDefinition
{
	private List<ConditionDefinition> conditionsField;

	private List<FilterDefinition> filtersField;

	private int tableIDField;

	[DataMember(Name = "TRD_4", EmitDefaultValue = false, IsRequired = false)]
	private string tableNameField;

	private int fieldIDField;

	[XmlElement("Conditions")]
	[DataMember(Name = "TRD_1", EmitDefaultValue = false, IsRequired = false)]
	public List<ConditionDefinition> Conditions
	{
		get
		{
			return conditionsField;
		}
		set
		{
			conditionsField = value;
		}
	}

	[XmlElement("Filters")]
	[DataMember(Name = "TRD_2", EmitDefaultValue = false, IsRequired = false)]
	public List<FilterDefinition> Filters
	{
		get
		{
			return filtersField;
		}
		set
		{
			filtersField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "TRD_3", EmitDefaultValue = false, IsRequired = false)]
	public int TableID
	{
		get
		{
			return tableIDField;
		}
		set
		{
			tableIDField = value;
			TableIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool TableIDSpecified
	{
		get
		{
			return GetSpecified(0);
		}
		set
		{
			SetSpecified(0, value);
		}
	}

	[XmlAttribute]
	public string TableName
	{
		get
		{
			return tableNameField;
		}
		set
		{
			tableNameField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "TRD_5", EmitDefaultValue = false, IsRequired = false)]
	public int FieldID
	{
		get
		{
			return fieldIDField;
		}
		set
		{
			fieldIDField = value;
			FieldIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool FieldIDSpecified
	{
		get
		{
			return GetSpecified(1);
		}
		set
		{
			SetSpecified(1, value);
		}
	}

	protected override int MaxSpecifiedBits => 2;

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (conditionsField != null)
		{
			for (int i = 0; i < conditionsField.Count; i++)
			{
				conditionsField[i].SetLanguage(language, primaryLanguage);
			}
		}
		if (filtersField != null)
		{
			for (int i = 0; i < filtersField.Count; i++)
			{
				filtersField[i].SetLanguage(language, primaryLanguage);
			}
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (conditionsField != null)
		{
			for (int i = 0; i < conditionsField.Count; i++)
			{
				conditionsField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
			}
		}
		if (filtersField != null)
		{
			for (int i = 0; i < filtersField.Count; i++)
			{
				filtersField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
			}
		}
	}

	public override ElementDefinition Find(Predicate<ElementDefinition> match)
	{
		if (match(this))
		{
			return this;
		}
		ElementDefinition elementDefinition = base.Find(match);
		if (elementDefinition != null)
		{
			return elementDefinition;
		}
		if (conditionsField != null)
		{
			for (int i = 0; i < conditionsField.Count; i++)
			{
				elementDefinition = conditionsField[i].Find(match);
				if (elementDefinition != null)
				{
					return elementDefinition;
				}
			}
		}
		if (filtersField != null)
		{
			for (int i = 0; i < filtersField.Count; i++)
			{
				elementDefinition = filtersField[i].Find(match);
				if (elementDefinition != null)
				{
					return elementDefinition;
				}
			}
		}
		return elementDefinition;
	}

	public override void FindAllCore(List<ElementDefinition> result, Predicate<ElementDefinition> match)
	{
		base.FindAllCore(result, match);
		if (conditionsField != null)
		{
			for (int i = 0; i < conditionsField.Count; i++)
			{
				conditionsField[i].FindAllCore(result, match);
			}
		}
		if (filtersField != null)
		{
			for (int i = 0; i < filtersField.Count; i++)
			{
				filtersField[i].FindAllCore(result, match);
			}
		}
	}

	public override object Clone()
	{
		TableRelationDefinition tableRelationDefinition = (TableRelationDefinition)base.Clone();
		if (conditionsField != null)
		{
			tableRelationDefinition.Conditions = new List<ConditionDefinition>();
			for (int i = 0; i < conditionsField.Count; i++)
			{
				tableRelationDefinition.Conditions.Add((ConditionDefinition)conditionsField[i].Clone());
			}
		}
		if (filtersField != null)
		{
			tableRelationDefinition.Filters = new List<FilterDefinition>();
			for (int j = 0; j < filtersField.Count; j++)
			{
				tableRelationDefinition.Filters.Add((FilterDefinition)filtersField[j].Clone());
			}
		}
		return tableRelationDefinition;
	}
}
