using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[XmlInclude(typeof(ContentAreaDefinition))]
[XmlInclude(typeof(InfopartsAreaDefinition))]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[KnownType(typeof(ContentAreaDefinition))]
[KnownType(typeof(InfopartsAreaDefinition))]
[DataContract(Name = "ABD0", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class AreaBaseDefinition : ElementDefinition
{
	private List<ControlBaseDefinition> controlsField;

	[XmlElement("Controls")]
	[DataMember(Name = "ABD0_1", EmitDefaultValue = false, IsRequired = false)]
	public List<ControlBaseDefinition> Controls
	{
		get
		{
			return controlsField;
		}
		set
		{
			controlsField = value;
		}
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (controlsField != null)
		{
			for (int i = 0; i < controlsField.Count; i++)
			{
				controlsField[i].SetLanguage(language, primaryLanguage);
			}
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (controlsField != null)
		{
			for (int i = 0; i < controlsField.Count; i++)
			{
				controlsField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
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
		if (controlsField != null)
		{
			for (int i = 0; i < controlsField.Count; i++)
			{
				elementDefinition = controlsField[i].Find(match);
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
		if (controlsField != null)
		{
			for (int i = 0; i < controlsField.Count; i++)
			{
				controlsField[i].FindAllCore(result, match);
			}
		}
	}

	public override object Clone()
	{
		AreaBaseDefinition areaBaseDefinition = (AreaBaseDefinition)base.Clone();
		if (controlsField != null)
		{
			areaBaseDefinition.Controls = new List<ControlBaseDefinition>();
			for (int i = 0; i < controlsField.Count; i++)
			{
				areaBaseDefinition.Controls.Add((ControlBaseDefinition)controlsField[i].Clone());
			}
		}
		return areaBaseDefinition;
	}
}
