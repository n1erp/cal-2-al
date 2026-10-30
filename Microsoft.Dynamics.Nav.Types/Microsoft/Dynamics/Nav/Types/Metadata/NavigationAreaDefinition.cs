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
[DataContract(Name = "NAD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavigationAreaDefinition : ElementDefinition
{
	private List<ActionBaseDefinition> actionsField;

	[DataMember(Name = "NAD_2", EmitDefaultValue = false, IsRequired = false)]
	private string controlGUIDField;

	[XmlElement("Actions")]
	[DataMember(Name = "NAD_1", EmitDefaultValue = false, IsRequired = false)]
	public List<ActionBaseDefinition> Actions
	{
		get
		{
			return actionsField;
		}
		set
		{
			actionsField = value;
		}
	}

	[XmlAttribute]
	public string ControlGUID
	{
		get
		{
			return controlGUIDField;
		}
		set
		{
			controlGUIDField = value;
		}
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (actionsField != null)
		{
			for (int i = 0; i < actionsField.Count; i++)
			{
				actionsField[i].SetLanguage(language, primaryLanguage);
			}
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (actionsField != null)
		{
			for (int i = 0; i < actionsField.Count; i++)
			{
				actionsField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
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
		if (actionsField != null)
		{
			for (int i = 0; i < actionsField.Count; i++)
			{
				elementDefinition = actionsField[i].Find(match);
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
		if (actionsField != null)
		{
			for (int i = 0; i < actionsField.Count; i++)
			{
				actionsField[i].FindAllCore(result, match);
			}
		}
	}

	public override object Clone()
	{
		NavigationAreaDefinition navigationAreaDefinition = (NavigationAreaDefinition)base.Clone();
		if (actionsField != null)
		{
			navigationAreaDefinition.Actions = new List<ActionBaseDefinition>();
			for (int i = 0; i < actionsField.Count; i++)
			{
				navigationAreaDefinition.Actions.Add((ActionBaseDefinition)actionsField[i].Clone());
			}
		}
		return navigationAreaDefinition;
	}
}
