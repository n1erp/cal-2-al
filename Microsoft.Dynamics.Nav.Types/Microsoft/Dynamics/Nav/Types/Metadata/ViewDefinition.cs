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
[DataContract(Name = "VD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ViewDefinition : ElementDefinition
{
	private ViewDefinitionSorting sortingField;

	private List<FilterDefinition> tableFiltersField;

	[DataMember(Name = "VD_1", EmitDefaultValue = false, IsRequired = false)]
	public ViewDefinitionSorting Sorting
	{
		get
		{
			return sortingField;
		}
		set
		{
			sortingField = value;
		}
	}

	[XmlElement("TableFilters")]
	[DataMember(Name = "VD_2", EmitDefaultValue = false, IsRequired = false)]
	public List<FilterDefinition> TableFilters
	{
		get
		{
			return tableFiltersField;
		}
		set
		{
			tableFiltersField = value;
		}
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (tableFiltersField != null)
		{
			for (int i = 0; i < tableFiltersField.Count; i++)
			{
				tableFiltersField[i].SetLanguage(language, primaryLanguage);
			}
		}
		if (sortingField != null)
		{
			sortingField.SetLanguage(language, primaryLanguage);
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (tableFiltersField != null)
		{
			for (int i = 0; i < tableFiltersField.Count; i++)
			{
				tableFiltersField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
			}
		}
		if (sortingField != null)
		{
			sortingField.MergeMultiLanguage(objType, id, languageAppGroupId);
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
		if (sortingField != null)
		{
			elementDefinition = sortingField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (tableFiltersField != null)
		{
			for (int i = 0; i < tableFiltersField.Count; i++)
			{
				elementDefinition = tableFiltersField[i].Find(match);
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
		if (sortingField != null)
		{
			sortingField.FindAllCore(result, match);
		}
		if (tableFiltersField != null)
		{
			for (int i = 0; i < tableFiltersField.Count; i++)
			{
				tableFiltersField[i].FindAllCore(result, match);
			}
		}
	}

	public override object Clone()
	{
		ViewDefinition viewDefinition = (ViewDefinition)base.Clone();
		if (sortingField != null)
		{
			viewDefinition.Sorting = (ViewDefinitionSorting)sortingField.Clone();
		}
		if (tableFiltersField != null)
		{
			viewDefinition.TableFilters = new List<FilterDefinition>();
			for (int i = 0; i < tableFiltersField.Count; i++)
			{
				viewDefinition.TableFilters.Add((FilterDefinition)tableFiltersField[i].Clone());
			}
		}
		return viewDefinition;
	}
}
