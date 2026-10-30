using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[XmlInclude(typeof(StackGroupDefinition))]
[XmlInclude(typeof(RepeaterDefinition))]
[XmlInclude(typeof(GridLayoutDefinition))]
[XmlInclude(typeof(ColumnLayoutDefinition))]
[XmlInclude(typeof(ControlGroupDefinition))]
[XmlInclude(typeof(ReportDefinition))]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[KnownType(typeof(StackGroupDefinition))]
[KnownType(typeof(RepeaterDefinition))]
[KnownType(typeof(GridLayoutDefinition))]
[KnownType(typeof(ColumnLayoutDefinition))]
[KnownType(typeof(ControlGroupDefinition))]
[KnownType(typeof(ReportDefinition))]
[DataContract(Name = "CGBD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class ControlGroupBaseDefinition : ControlBaseDefinition
{
	private List<ControlBaseDefinition> controlsField;

	[DataMember(Name = "CGBD_2", EmitDefaultValue = false, IsRequired = false)]
	private string instructionalTextField;

	[DataMember(Name = "CGBD_3", EmitDefaultValue = false, IsRequired = false)]
	private string instructionalTextMLField;

	[DataMember(Name = "CGBD_4", EmitDefaultValue = false, IsRequired = false)]
	private string editableField;

	[XmlElement("Controls")]
	[DataMember(Name = "CGBD_1", EmitDefaultValue = false, IsRequired = false)]
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
	[DefaultValue("")]
	public string InstructionalText
	{
		get
		{
			if (instructionalTextField == null)
			{
				return "";
			}
			return instructionalTextField;
		}
		set
		{
			instructionalTextField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string InstructionalTextML
	{
		get
		{
			if (instructionalTextMLField == null)
			{
				return "";
			}
			return instructionalTextMLField;
		}
		set
		{
			instructionalTextMLField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("true")]
	public string Editable
	{
		get
		{
			if (editableField == null)
			{
				return "true";
			}
			return editableField;
		}
		set
		{
			editableField = value;
		}
	}

	protected override ControlType ComponentType()
	{
		return ControlType.Control;
	}

	protected override int ComponentId()
	{
		return ID;
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
		if (!string.IsNullOrEmpty(instructionalTextMLField))
		{
			instructionalTextField = MultiLanguage.GetLanguageValue(language, primaryLanguage, instructionalTextMLField);
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
		instructionalTextMLField = MetaPage.MultiLanguageTextMerge(instructionalTextMLField, objType, id, ComponentType(), ComponentId(), 1, languageAppGroupId);
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
		ControlGroupBaseDefinition controlGroupBaseDefinition = (ControlGroupBaseDefinition)base.Clone();
		if (controlsField != null)
		{
			controlGroupBaseDefinition.Controls = new List<ControlBaseDefinition>();
			for (int i = 0; i < controlsField.Count; i++)
			{
				controlGroupBaseDefinition.Controls.Add((ControlBaseDefinition)controlsField[i].Clone());
			}
		}
		return controlGroupBaseDefinition;
	}
}
