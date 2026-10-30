using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "VDC", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ViewDefinitionChart : ElementDefinition
{
	[DataMember(Name = "VDC_1", EmitDefaultValue = false, IsRequired = false)]
	private string visualizationDefinitionField;

	private bool showAsChartField;

	[XmlAttribute]
	public string VisualizationDefinition
	{
		get
		{
			return visualizationDefinitionField;
		}
		set
		{
			visualizationDefinitionField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "VDC_2", EmitDefaultValue = false, IsRequired = false)]
	public bool ShowAsChart
	{
		get
		{
			return showAsChartField;
		}
		set
		{
			showAsChartField = value;
		}
	}

	public ViewDefinitionChart()
	{
		showAsChartField = false;
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
		return (ViewDefinitionChart)base.Clone();
	}
}
