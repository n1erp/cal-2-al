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
public class xliffFileBodyGroupTransunitNote
{
	private string fromField;

	private string annotatesField;

	private byte priorityField;

	private string valueField;

	[XmlAttribute]
	public string from
	{
		get
		{
			return fromField;
		}
		set
		{
			fromField = value;
		}
	}

	[XmlAttribute]
	public string annotates
	{
		get
		{
			return annotatesField;
		}
		set
		{
			annotatesField = value;
		}
	}

	[XmlAttribute]
	public byte priority
	{
		get
		{
			return priorityField;
		}
		set
		{
			priorityField = value;
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
