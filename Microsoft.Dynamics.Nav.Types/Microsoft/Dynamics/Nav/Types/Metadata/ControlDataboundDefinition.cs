using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[XmlInclude(typeof(FilterControlDefinition))]
[XmlInclude(typeof(ControlDefinition))]
[XmlInclude(typeof(ProgressControlDefinition))]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[KnownType(typeof(FilterControlDefinition))]
[KnownType(typeof(ControlDefinition))]
[KnownType(typeof(ProgressControlDefinition))]
[DataContract(Name = "CDD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class ControlDataboundDefinition : ControlBaseDefinition
{
	private List<TableRelationDefinition> tableRelationsField;

	private List<RelatedObjectDefinition> relatedObjectsField;

	private List<ControlTriggerDefinition> triggersField;

	private Style styleField;

	[DataMember(Name = "CDD_5", EmitDefaultValue = false, IsRequired = false)]
	private string styleExprField;

	private ExtendedDatatype extendedDatatypeField;

	private int widthField;

	[DataMember(Name = "CDD_8", EmitDefaultValue = false, IsRequired = false)]
	private string showMandatoryField;

	[DataMember(Name = "CDD_9", EmitDefaultValue = false, IsRequired = false)]
	private string oDataEDMTypeField;

	[DataMember(Name = "CDD_10", EmitDefaultValue = false, IsRequired = false)]
	private string dataColumnNameField;

	[DataMember(Name = "CDD_11", EmitDefaultValue = false, IsRequired = false)]
	private string editableField;

	private bool sourceExpressionIsAssignableField;

	[DataMember(Name = "CDD_13", EmitDefaultValue = false, IsRequired = false)]
	private string minValueField;

	[DataMember(Name = "CDD_14", EmitDefaultValue = false, IsRequired = false)]
	private string maxValueField;

	[DataMember(Name = "CDD_15", EmitDefaultValue = false, IsRequired = false)]
	private string imageField;

	public const string StackCueImageName = "Stack";

	public const string NoCueImageName = "None";

	public const string CueCustomImagePrefix = "Cue_";

	public const string CueCustomImageSuffix = "_56x46";

	[XmlElement("HasOnValidateEventSubscriber")]
	[DataMember(Name = "HasOnValidateEventSubscriber", EmitDefaultValue = false, IsRequired = false)]
	public bool HasOnValidateEventSubscriber { get; set; }

	[XmlElement("TableRelations")]
	[DataMember(Name = "CDD_1", EmitDefaultValue = false, IsRequired = false)]
	public List<TableRelationDefinition> TableRelations
	{
		get
		{
			return tableRelationsField;
		}
		set
		{
			tableRelationsField = value;
		}
	}

	[XmlElement("RelatedObjects")]
	[DataMember(Name = "CDD_2", EmitDefaultValue = false, IsRequired = false)]
	public List<RelatedObjectDefinition> RelatedObjects
	{
		get
		{
			return relatedObjectsField;
		}
		set
		{
			relatedObjectsField = value;
		}
	}

	[XmlElement("Triggers")]
	[DataMember(Name = "CDD_3", EmitDefaultValue = false, IsRequired = false)]
	public List<ControlTriggerDefinition> Triggers
	{
		get
		{
			return triggersField;
		}
		set
		{
			triggersField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CDD_4", EmitDefaultValue = false, IsRequired = false)]
	public Style Style
	{
		get
		{
			return styleField;
		}
		set
		{
			styleField = value;
			StyleSpecified = true;
		}
	}

	[XmlIgnore]
	public bool StyleSpecified
	{
		get
		{
			return GetSpecified(9);
		}
		set
		{
			SetSpecified(9, value);
		}
	}

	[XmlAttribute]
	public string StyleExpr
	{
		get
		{
			return styleExprField;
		}
		set
		{
			styleExprField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CDD_6", EmitDefaultValue = false, IsRequired = false)]
	public ExtendedDatatype ExtendedDatatype
	{
		get
		{
			return extendedDatatypeField;
		}
		set
		{
			extendedDatatypeField = value;
			ExtendedDatatypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ExtendedDatatypeSpecified
	{
		get
		{
			return GetSpecified(10);
		}
		set
		{
			SetSpecified(10, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CDD_7", EmitDefaultValue = false, IsRequired = false)]
	public int Width
	{
		get
		{
			return widthField;
		}
		set
		{
			widthField = value;
			WidthSpecified = true;
		}
	}

	[XmlIgnore]
	public bool WidthSpecified
	{
		get
		{
			return GetSpecified(11);
		}
		set
		{
			SetSpecified(11, value);
		}
	}

	[XmlAttribute]
	public string ShowMandatory
	{
		get
		{
			return showMandatoryField;
		}
		set
		{
			showMandatoryField = value;
		}
	}

	[XmlAttribute]
	public string ODataEDMType
	{
		get
		{
			return oDataEDMTypeField;
		}
		set
		{
			oDataEDMTypeField = value;
		}
	}

	[XmlAttribute]
	public string DataColumnName
	{
		get
		{
			return dataColumnNameField;
		}
		set
		{
			dataColumnNameField = value;
		}
	}

	[XmlAttribute]
	public string Editable
	{
		get
		{
			return editableField;
		}
		set
		{
			editableField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(true)]
	[DataMember(Name = "CDD_12", EmitDefaultValue = false, IsRequired = false)]
	public bool SourceExpressionIsAssignable
	{
		get
		{
			return sourceExpressionIsAssignableField;
		}
		set
		{
			sourceExpressionIsAssignableField = value;
		}
	}

	[XmlAttribute]
	public string MinValue
	{
		get
		{
			return minValueField;
		}
		set
		{
			minValueField = value;
		}
	}

	[XmlAttribute]
	public string MaxValue
	{
		get
		{
			return maxValueField;
		}
		set
		{
			maxValueField = value;
		}
	}

	[XmlAttribute]
	public string Image
	{
		get
		{
			return imageField;
		}
		set
		{
			imageField = value;
		}
	}

	protected override int MaxSpecifiedBits => 12;

	protected override ControlType ComponentType()
	{
		return ControlType.Control;
	}

	protected override int ComponentId()
	{
		return ID;
	}

	protected internal ControlDataboundDefinition()
	{
		sourceExpressionIsAssignableField = true;
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (tableRelationsField != null)
		{
			for (int i = 0; i < tableRelationsField.Count; i++)
			{
				tableRelationsField[i].SetLanguage(language, primaryLanguage);
			}
		}
		if (relatedObjectsField != null)
		{
			for (int i = 0; i < relatedObjectsField.Count; i++)
			{
				relatedObjectsField[i].SetLanguage(language, primaryLanguage);
			}
		}
		if (triggersField != null)
		{
			for (int i = 0; i < triggersField.Count; i++)
			{
				triggersField[i].SetLanguage(language, primaryLanguage);
			}
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (tableRelationsField != null)
		{
			for (int i = 0; i < tableRelationsField.Count; i++)
			{
				tableRelationsField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
			}
		}
		if (relatedObjectsField != null)
		{
			for (int i = 0; i < relatedObjectsField.Count; i++)
			{
				relatedObjectsField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
			}
		}
		if (triggersField != null)
		{
			for (int i = 0; i < triggersField.Count; i++)
			{
				triggersField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
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
		if (tableRelationsField != null)
		{
			for (int i = 0; i < tableRelationsField.Count; i++)
			{
				elementDefinition = tableRelationsField[i].Find(match);
				if (elementDefinition != null)
				{
					return elementDefinition;
				}
			}
		}
		if (relatedObjectsField != null)
		{
			for (int i = 0; i < relatedObjectsField.Count; i++)
			{
				elementDefinition = relatedObjectsField[i].Find(match);
				if (elementDefinition != null)
				{
					return elementDefinition;
				}
			}
		}
		if (triggersField != null)
		{
			for (int i = 0; i < triggersField.Count; i++)
			{
				elementDefinition = triggersField[i].Find(match);
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
		if (tableRelationsField != null)
		{
			for (int i = 0; i < tableRelationsField.Count; i++)
			{
				tableRelationsField[i].FindAllCore(result, match);
			}
		}
		if (relatedObjectsField != null)
		{
			for (int i = 0; i < relatedObjectsField.Count; i++)
			{
				relatedObjectsField[i].FindAllCore(result, match);
			}
		}
		if (triggersField != null)
		{
			for (int i = 0; i < triggersField.Count; i++)
			{
				triggersField[i].FindAllCore(result, match);
			}
		}
	}

	public override object Clone()
	{
		ControlDataboundDefinition controlDataboundDefinition = (ControlDataboundDefinition)base.Clone();
		if (tableRelationsField != null)
		{
			controlDataboundDefinition.TableRelations = new List<TableRelationDefinition>();
			for (int i = 0; i < tableRelationsField.Count; i++)
			{
				controlDataboundDefinition.TableRelations.Add((TableRelationDefinition)tableRelationsField[i].Clone());
			}
		}
		if (relatedObjectsField != null)
		{
			controlDataboundDefinition.RelatedObjects = new List<RelatedObjectDefinition>();
			for (int j = 0; j < relatedObjectsField.Count; j++)
			{
				controlDataboundDefinition.RelatedObjects.Add((RelatedObjectDefinition)relatedObjectsField[j].Clone());
			}
		}
		if (triggersField != null)
		{
			controlDataboundDefinition.Triggers = new List<ControlTriggerDefinition>();
			for (int k = 0; k < triggersField.Count; k++)
			{
				controlDataboundDefinition.Triggers.Add((ControlTriggerDefinition)triggersField[k].Clone());
			}
		}
		return controlDataboundDefinition;
	}

	public override void SolveProperties(MetaField field, MetaTable metaTable, MasterPage masterPage)
	{
		base.SolveProperties(field, metaTable, masterPage);
		PropertiesSolveHelper.SolveEditable(this, field, masterPage);
		PropertiesSolveHelper.SolveExtendedDatatype(this, field);
		PropertiesSolveHelper.SolveRelatedObjects(this, field);
		PropertiesSolveHelper.SolveDataboundControlImage(this);
	}
}
