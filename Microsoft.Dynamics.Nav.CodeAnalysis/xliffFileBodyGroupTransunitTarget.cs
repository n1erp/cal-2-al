using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

[Serializable]
[GeneratedCode("xsd", "4.6.1590.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
public class xliffFileBodyGroupTransunitTarget
{
	private string stateField;

	private string statequalifierField;

	private string valueField;

	[XmlAttribute]
	public string state
	{
		get
		{
			return stateField;
		}
		set
		{
			stateField = value;
		}
	}

	[XmlAttribute("state-qualifier")]
	public string statequalifier
	{
		get
		{
			return statequalifierField;
		}
		set
		{
			statequalifierField = value;
		}
	}

	[XmlText]
	public string Value
	{
		get
		{
			return valueField;
		}
		set
		{
			valueField = value;
		}
	}
}
