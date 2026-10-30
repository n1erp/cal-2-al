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
public class ChartTableDefinition
{
	private ChartFilterDefinition[] filtersField;

	private int idField;

	private string nameField;

	private string keyField;

	[XmlArrayItem("Filter", IsNullable = false)]
	public ChartFilterDefinition[] Filters
	{
		get
		{
			return filtersField;
		}
		set
		{
			filtersField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(0)]
	public int ID
	{
		get
		{
			return idField;
		}
		set
		{
			idField = value;
		}
	}

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
	public string Key
	{
		get
		{
			return keyField;
		}
		set
		{
			keyField = value;
		}
	}

	public ChartTableDefinition()
	{
		idField = 0;
	}
}
