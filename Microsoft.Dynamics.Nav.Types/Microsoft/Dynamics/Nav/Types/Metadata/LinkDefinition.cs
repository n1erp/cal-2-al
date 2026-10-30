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
[DataContract(Name = "LD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class LinkDefinition : ElementDefinition
{
	private List<FilterDefinition> tableFiltersField;

	[XmlElement("TableFilters")]
	[DataMember(Name = "LD_1", EmitDefaultValue = false, IsRequired = false)]
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
		LinkDefinition linkDefinition = (LinkDefinition)base.Clone();
		if (tableFiltersField != null)
		{
			linkDefinition.TableFilters = new List<FilterDefinition>();
			for (int i = 0; i < tableFiltersField.Count; i++)
			{
				linkDefinition.TableFilters.Add((FilterDefinition)tableFiltersField[i].Clone());
			}
		}
		return linkDefinition;
	}
}
