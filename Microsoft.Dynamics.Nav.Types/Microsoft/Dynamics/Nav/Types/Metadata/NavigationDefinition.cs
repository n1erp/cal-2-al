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
[XmlRoot(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects", IsNullable = false)]
[DataContract(Name = "ND", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavigationDefinition : ConceptDefinition
{
	private List<ActionContainerDefinition> actionContainersField;

	[XmlElement("ActionContainers")]
	[DataMember(Name = "ND_1", EmitDefaultValue = false, IsRequired = false)]
	public List<ActionContainerDefinition> ActionContainers
	{
		get
		{
			return actionContainersField;
		}
		set
		{
			actionContainersField = value;
		}
	}

	public override void MergeMenuSuiteMultiLanguage(int languageAppGroupId)
	{
		base.MergeMenuSuiteMultiLanguage(languageAppGroupId);
		if (actionContainersField != null)
		{
			for (int i = 0; i < actionContainersField.Count; i++)
			{
				actionContainersField[i].MergeMenuSuiteMultiLanguage(languageAppGroupId);
			}
		}
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (actionContainersField != null)
		{
			for (int i = 0; i < actionContainersField.Count; i++)
			{
				actionContainersField[i].SetLanguage(language, primaryLanguage);
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
		if (actionContainersField != null)
		{
			for (int i = 0; i < actionContainersField.Count; i++)
			{
				elementDefinition = actionContainersField[i].Find(match);
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
		if (actionContainersField != null)
		{
			for (int i = 0; i < actionContainersField.Count; i++)
			{
				actionContainersField[i].FindAllCore(result, match);
			}
		}
	}

	public override object Clone()
	{
		NavigationDefinition navigationDefinition = (NavigationDefinition)base.Clone();
		if (actionContainersField != null)
		{
			navigationDefinition.ActionContainers = new List<ActionContainerDefinition>();
			for (int i = 0; i < actionContainersField.Count; i++)
			{
				navigationDefinition.ActionContainers.Add((ActionContainerDefinition)actionContainersField[i].Clone());
			}
		}
		return navigationDefinition;
	}
}
