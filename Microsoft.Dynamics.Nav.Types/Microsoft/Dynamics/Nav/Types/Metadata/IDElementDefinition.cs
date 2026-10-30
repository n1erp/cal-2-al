using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[XmlInclude(typeof(NamedElementDefinition))]
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
[KnownType(typeof(NamedElementDefinition))]
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
[DataContract(Name = "IDED", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class IDElementDefinition : ElementDefinition
{
	private int idField;

	private int clonedFromIDField;

	[XmlAttribute]
	[DataMember(Name = "IDED_1", EmitDefaultValue = false, IsRequired = false)]
	public int ID
	{
		get
		{
			return idField;
		}
		set
		{
			idField = value;
			IDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool IDSpecified
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

	[XmlAttribute]
	[DataMember(Name = "IDED_2", EmitDefaultValue = false, IsRequired = false)]
	public int ClonedFromID
	{
		get
		{
			return clonedFromIDField;
		}
		set
		{
			clonedFromIDField = value;
			ClonedFromIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ClonedFromIDSpecified
	{
		get
		{
			return GetSpecified(1);
		}
		set
		{
			SetSpecified(1, value);
		}
	}

	protected override int MaxSpecifiedBits => 2;

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
		return (IDElementDefinition)base.Clone();
	}
}
