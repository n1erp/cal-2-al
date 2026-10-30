using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[XmlInclude(typeof(UIElementDefinition))]
[XmlInclude(typeof(CommandBarDefinition))]
[XmlInclude(typeof(ControlBaseDefinition))]
[XmlInclude(typeof(InfopartBaseDefinition))]
[XmlInclude(typeof(InfopartChartDefinition))]
[XmlInclude(typeof(InfopartSystemDefinition))]
[XmlInclude(typeof(InfopartPageDefinition))]
[XmlInclude(typeof(ControlContainerPlaceHolder))]
[XmlInclude(typeof(ControlGroupBaseDefinition))]
[XmlInclude(typeof(StackGroupDefinition))]
[XmlInclude(typeof(RepeaterDefinition))]
[XmlInclude(typeof(GridLayoutDefinition))]
[XmlInclude(typeof(ColumnLayoutDefinition))]
[XmlInclude(typeof(ControlGroupDefinition))]
[XmlInclude(typeof(ReportDefinition))]
[XmlInclude(typeof(RecordLinksControlDefinition))]
[XmlInclude(typeof(OutlookOverviewDefinition))]
[XmlInclude(typeof(ControlDataboundDefinition))]
[XmlInclude(typeof(FilterControlDefinition))]
[XmlInclude(typeof(ControlDefinition))]
[XmlInclude(typeof(ProgressControlDefinition))]
[XmlInclude(typeof(NotesControlDefinition))]
[XmlInclude(typeof(ActionBaseDefinition))]
[XmlInclude(typeof(ActionDefinition))]
[XmlInclude(typeof(ActionContainerPlaceHolder))]
[XmlInclude(typeof(ActionGroupBaseDefinition))]
[XmlInclude(typeof(NavigationGroupDefinition))]
[XmlInclude(typeof(ActionGroupDefinition))]
[XmlInclude(typeof(SeparatorDefinition))]
[XmlInclude(typeof(NavigationPaneDefinition))]
[XmlInclude(typeof(ConceptDefinition))]
[XmlInclude(typeof(PageDefinition))]
[XmlInclude(typeof(NavigationDefinition))]
[XmlInclude(typeof(MasterPage))]
[XmlInclude(typeof(MasterNavigation))]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[KnownType(typeof(UIElementDefinition))]
[KnownType(typeof(CommandBarDefinition))]
[KnownType(typeof(ControlBaseDefinition))]
[KnownType(typeof(InfopartBaseDefinition))]
[KnownType(typeof(InfopartChartDefinition))]
[KnownType(typeof(InfopartSystemDefinition))]
[KnownType(typeof(InfopartPageDefinition))]
[KnownType(typeof(ControlContainerPlaceHolder))]
[KnownType(typeof(ControlGroupBaseDefinition))]
[KnownType(typeof(StackGroupDefinition))]
[KnownType(typeof(RepeaterDefinition))]
[KnownType(typeof(GridLayoutDefinition))]
[KnownType(typeof(ColumnLayoutDefinition))]
[KnownType(typeof(ControlGroupDefinition))]
[KnownType(typeof(ReportDefinition))]
[KnownType(typeof(RecordLinksControlDefinition))]
[KnownType(typeof(OutlookOverviewDefinition))]
[KnownType(typeof(ControlDataboundDefinition))]
[KnownType(typeof(FilterControlDefinition))]
[KnownType(typeof(ControlDefinition))]
[KnownType(typeof(ProgressControlDefinition))]
[KnownType(typeof(NotesControlDefinition))]
[KnownType(typeof(ActionBaseDefinition))]
[KnownType(typeof(ActionDefinition))]
[KnownType(typeof(ActionContainerPlaceHolder))]
[KnownType(typeof(ActionGroupBaseDefinition))]
[KnownType(typeof(NavigationGroupDefinition))]
[KnownType(typeof(ActionGroupDefinition))]
[KnownType(typeof(SeparatorDefinition))]
[KnownType(typeof(NavigationPaneDefinition))]
[KnownType(typeof(ConceptDefinition))]
[KnownType(typeof(PageDefinition))]
[KnownType(typeof(NavigationDefinition))]
[KnownType(typeof(MasterPage))]
[KnownType(typeof(MasterNavigation))]
[DataContract(Name = "NED", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class NamedElementDefinition : IDElementDefinition
{
	[DataMember(Name = "NED_1", EmitDefaultValue = false, IsRequired = false)]
	private string nameField;

	[DataMember(Name = "NED_2", EmitDefaultValue = false, IsRequired = false)]
	private string captionField;

	[DataMember(Name = "NED_3", EmitDefaultValue = false, IsRequired = false)]
	private string captionMLField;

	[DataMember(Name = "NED_4", EmitDefaultValue = false, IsRequired = false)]
	private string resourceIdentifierField;

	[XmlAttribute]
	public string Name
	{
		get
		{
			return nameField;
		}
		set
		{
			nameField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string Caption
	{
		get
		{
			if (captionField == null)
			{
				return "";
			}
			return captionField;
		}
		set
		{
			captionField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string CaptionML
	{
		get
		{
			if (captionMLField == null)
			{
				return "";
			}
			return captionMLField;
		}
		set
		{
			captionMLField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string ResourceIdentifier
	{
		get
		{
			if (resourceIdentifierField == null)
			{
				return "";
			}
			return resourceIdentifierField;
		}
		set
		{
			resourceIdentifierField = value;
		}
	}

	internal string CachedALIdentifier { get; set; }

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (!string.IsNullOrEmpty(captionMLField))
		{
			captionField = MultiLanguage.GetLanguageValue(language, primaryLanguage, captionMLField);
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		captionMLField = MetaPage.MultiLanguageTextMerge(captionMLField, objType, id, ComponentType(), ComponentId(), 0, languageAppGroupId);
	}

	public override ElementDefinition Find(Predicate<ElementDefinition> match)
	{
		if (match(this))
		{
			return this;
		}
		return base.Find(match);
	}

	public override object Clone()
	{
		return (NamedElementDefinition)base.Clone();
	}

	public override void SolveProperties(MetaField field, MetaTable metaTable, MasterPage masterPage)
	{
		PropertiesSolveHelper.SolveCaption(this, field);
		SolveName(field);
	}

	public virtual void SolveName(MetaField field)
	{
		PropertiesSolveHelper.SolveName(this, field);
	}
}
