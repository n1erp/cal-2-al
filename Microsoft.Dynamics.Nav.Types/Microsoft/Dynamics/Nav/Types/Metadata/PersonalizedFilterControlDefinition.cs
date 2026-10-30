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
[DataContract(Name = "PFCD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class PersonalizedFilterControlDefinition : ElementDefinition
{
	private List<UserFilterDefinition> userFilterField;

	private ViewDefinitionChart chartField;

	[DataMember(Name = "PFCD_3", EmitDefaultValue = false, IsRequired = false)]
	private string currentSortingKeyField;

	[DataMember(Name = "PFCD_4", EmitDefaultValue = false, IsRequired = false)]
	private string sortingOrderField;

	[XmlElement("UserFilter")]
	[DataMember(Name = "PFCD_1", EmitDefaultValue = false, IsRequired = false)]
	public List<UserFilterDefinition> UserFilter
	{
		get
		{
			return userFilterField;
		}
		set
		{
			userFilterField = value;
		}
	}

	[DataMember(Name = "PFCD_2", EmitDefaultValue = false, IsRequired = false)]
	public ViewDefinitionChart Chart
	{
		get
		{
			return chartField;
		}
		set
		{
			chartField = value;
		}
	}

	[XmlAttribute]
	public string CurrentSortingKey
	{
		get
		{
			return currentSortingKeyField;
		}
		set
		{
			currentSortingKeyField = value;
		}
	}

	[XmlAttribute]
	public string SortingOrder
	{
		get
		{
			return sortingOrderField;
		}
		set
		{
			sortingOrderField = value;
		}
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (userFilterField != null)
		{
			for (int i = 0; i < userFilterField.Count; i++)
			{
				userFilterField[i].SetLanguage(language, primaryLanguage);
			}
		}
		if (chartField != null)
		{
			chartField.SetLanguage(language, primaryLanguage);
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (userFilterField != null)
		{
			for (int i = 0; i < userFilterField.Count; i++)
			{
				userFilterField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
			}
		}
		if (chartField != null)
		{
			chartField.MergeMultiLanguage(objType, id, languageAppGroupId);
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
		if (chartField != null)
		{
			elementDefinition = chartField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (userFilterField != null)
		{
			for (int i = 0; i < userFilterField.Count; i++)
			{
				elementDefinition = userFilterField[i].Find(match);
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
		if (chartField != null)
		{
			chartField.FindAllCore(result, match);
		}
		if (userFilterField != null)
		{
			for (int i = 0; i < userFilterField.Count; i++)
			{
				userFilterField[i].FindAllCore(result, match);
			}
		}
	}

	public override object Clone()
	{
		PersonalizedFilterControlDefinition personalizedFilterControlDefinition = (PersonalizedFilterControlDefinition)base.Clone();
		if (chartField != null)
		{
			personalizedFilterControlDefinition.Chart = (ViewDefinitionChart)chartField.Clone();
		}
		if (userFilterField != null)
		{
			personalizedFilterControlDefinition.UserFilter = new List<UserFilterDefinition>();
			for (int i = 0; i < userFilterField.Count; i++)
			{
				personalizedFilterControlDefinition.UserFilter.Add((UserFilterDefinition)userFilterField[i].Clone());
			}
		}
		return personalizedFilterControlDefinition;
	}
}
