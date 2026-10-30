using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[XmlInclude(typeof(DimensionAxisDefinition))]
[XmlInclude(typeof(MeasureAxisDefinition))]
[GeneratedCode("xsd", "2.0.50727.42")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
public abstract class AxisDefinition
{
	private MultiLanguageTextDefinition[] titleField;

	private bool showTitleField;

	[XmlArrayItem("Text", IsNullable = false)]
	public MultiLanguageTextDefinition[] Title
	{
		get
		{
			return titleField;
		}
		set
		{
			titleField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(true)]
	public bool ShowTitle
	{
		get
		{
			return showTitleField;
		}
		set
		{
			showTitleField = value;
		}
	}

	public AxisDefinition()
	{
		showTitleField = true;
	}
}
