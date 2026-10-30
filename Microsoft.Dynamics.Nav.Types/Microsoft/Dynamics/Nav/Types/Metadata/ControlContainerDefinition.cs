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
[DataContract(Name = "CCD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ControlContainerDefinition : ElementDefinition
{
	private List<ControlBaseDefinition> controlsField;

	private ControlContainerType containerTypeField;

	[XmlElement("Controls")]
	[DataMember(Name = "CCD_1", EmitDefaultValue = false, IsRequired = false)]
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

	[XmlAttribute]
	[DataMember(Name = "CCD_2", EmitDefaultValue = false, IsRequired = false)]
	public ControlContainerType ContainerType
	{
		get
		{
			return containerTypeField;
		}
		set
		{
			containerTypeField = value;
			ContainerTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ContainerTypeSpecified
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

	protected override int MaxSpecifiedBits => 1;

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
		ControlContainerDefinition controlContainerDefinition = (ControlContainerDefinition)base.Clone();
		if (controlsField != null)
		{
			controlContainerDefinition.Controls = new List<ControlBaseDefinition>();
			for (int i = 0; i < controlsField.Count; i++)
			{
				controlContainerDefinition.Controls.Add((ControlBaseDefinition)controlsField[i].Clone());
			}
		}
		return controlContainerDefinition;
	}
}
