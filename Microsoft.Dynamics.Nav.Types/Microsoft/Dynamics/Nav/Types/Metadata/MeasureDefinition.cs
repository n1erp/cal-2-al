using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("xsd", "2.0.50727.42")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
public class MeasureDefinition
{
	private ChartFieldDefinition fieldField;

	private MultiLanguageTextDefinition[] captionField;

	private int valueIndexField;

	private ChartMeasureOperator operatorField;

	private bool reverseSignField;

	private ChartType seriesChartTypeField;

	public ChartFieldDefinition Field
	{
		get
		{
			return fieldField;
		}
		set
		{
			fieldField = value;
		}
	}

	[XmlArrayItem("Text", IsNullable = false)]
	public MultiLanguageTextDefinition[] Caption
	{
		get
		{
			return captionField;
		}
		set
		{
			captionField = value;
		}
	}

	[DefaultValue(0)]
	public int ValueIndex
	{
		get
		{
			return valueIndexField;
		}
		set
		{
			valueIndexField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(ChartMeasureOperator.Sum)]
	public ChartMeasureOperator Operator
	{
		get
		{
			return operatorField;
		}
		set
		{
			operatorField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	public bool ReverseSign
	{
		get
		{
			return reverseSignField;
		}
		set
		{
			reverseSignField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(ChartType.Column)]
	public ChartType SeriesChartType
	{
		get
		{
			return seriesChartTypeField;
		}
		set
		{
			seriesChartTypeField = value;
		}
	}

	public MeasureDefinition()
	{
		valueIndexField = 0;
		operatorField = ChartMeasureOperator.Sum;
		reverseSignField = false;
		seriesChartTypeField = ChartType.Column;
	}
}
