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
[DataContract(Name = "CD2", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ContentDefinition : ElementDefinition
{
	private List<ControlContainerDefinition> containersField;

	[XmlElement("Containers")]
	[DataMember(Name = "CD2_1", EmitDefaultValue = false, IsRequired = false)]
	public List<ControlContainerDefinition> Containers
	{
		get
		{
			return containersField;
		}
		set
		{
			containersField = value;
		}
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (containersField != null)
		{
			for (int i = 0; i < containersField.Count; i++)
			{
				containersField[i].SetLanguage(language, primaryLanguage);
			}
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (containersField != null)
		{
			for (int i = 0; i < containersField.Count; i++)
			{
				containersField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
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
		if (containersField != null)
		{
			for (int i = 0; i < containersField.Count; i++)
			{
				elementDefinition = containersField[i].Find(match);
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
		if (containersField != null)
		{
			for (int i = 0; i < containersField.Count; i++)
			{
				containersField[i].FindAllCore(result, match);
			}
		}
	}

	public override object Clone()
	{
		ContentDefinition contentDefinition = (ContentDefinition)base.Clone();
		if (containersField != null)
		{
			contentDefinition.Containers = new List<ControlContainerDefinition>();
			for (int i = 0; i < containersField.Count; i++)
			{
				contentDefinition.Containers.Add((ControlContainerDefinition)containersField[i].Clone());
			}
		}
		return contentDefinition;
	}
}
