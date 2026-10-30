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
public class DimensionAxisDefinition : AxisDefinition
{
	private ChartFieldDefinition fieldField;

	private MultiLanguageTextDefinition[] captionField;

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
}
