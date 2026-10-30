using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
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
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
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
[DataContract(Name = "UIED", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class UIElementDefinition : NamedElementDefinition
{
	private VisibleMode visibleModeField;

	[DataMember(Name = "UIED_2", EmitDefaultValue = false, IsRequired = false)]
	private string visibleField;

	[DataMember(Name = "UIED_3", EmitDefaultValue = false, IsRequired = false)]
	private string personalizedPropertiesField;

	[DataMember(Name = "UIED_4", EmitDefaultValue = false, IsRequired = false)]
	private string controlGUIDField;

	[DataMember(Name = "UIED_5", EmitDefaultValue = false, IsRequired = false)]
	private string enabledField;

	[XmlAttribute]
	[DefaultValue(VisibleMode.Always)]
	[DataMember(Name = "UIED_1", EmitDefaultValue = false, IsRequired = false)]
	public VisibleMode VisibleMode
	{
		get
		{
			return visibleModeField;
		}
		set
		{
			visibleModeField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("true")]
	public string Visible
	{
		get
		{
			if (visibleField == null)
			{
				return "true";
			}
			return visibleField;
		}
		set
		{
			visibleField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string PersonalizedProperties
	{
		get
		{
			if (personalizedPropertiesField == null)
			{
				return "";
			}
			return personalizedPropertiesField;
		}
		set
		{
			personalizedPropertiesField = value;
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

	[XmlAttribute]
	[DefaultValue("true")]
	public string Enabled
	{
		get
		{
			if (enabledField == null)
			{
				return "true";
			}
			return enabledField;
		}
		set
		{
			enabledField = value;
		}
	}

	protected internal UIElementDefinition()
	{
		visibleModeField = VisibleMode.Always;
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
		return (UIElementDefinition)base.Clone();
	}
}
