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
public class xliffFileHeaderTool
{
	private string toolidField;

	private string toolnameField;

	private string toolversionField;

	private string toolcompanyField;

	[XmlAttribute("tool-id")]
	public string toolid
	{
		get
		{
			return toolidField;
		}
		set
		{
			toolidField = value;
		}
	}

	[XmlAttribute("tool-name")]
	public string toolname
	{
		get
		{
			return toolnameField;
		}
		set
		{
			toolnameField = value;
		}
	}

	[XmlAttribute("tool-version")]
	public string toolversion
	{
		get
		{
			return toolversionField;
		}
		set
		{
			toolversionField = value;
		}
	}

	[XmlAttribute("tool-company")]
	public string toolcompany
	{
		get
		{
			return toolcompanyField;
		}
		set
		{
			toolcompanyField = value;
		}
	}
}
