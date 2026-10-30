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
public class xliffFile
{
	private xliffFileHeader headerField;

	private xliffFileBody bodyField;

	private string datatypeField;

	private string sourcelanguageField;

	private string targetlanguageField;

	private string originalField;

	private string toolidField;

	private string productnameField;

	private string productversionField;

	private string buildnumField;

	public xliffFileHeader header
	{
		get
		{
			return headerField;
		}
		set
		{
			headerField = value;
		}
	}

	public xliffFileBody body
	{
		get
		{
			return bodyField;
		}
		set
		{
			bodyField = value;
		}
	}

	[XmlAttribute]
	public string datatype
	{
		get
		{
			return datatypeField;
		}
		set
		{
			datatypeField = value;
		}
	}

	[XmlAttribute("source-language")]
	public string sourcelanguage
	{
		get
		{
			return sourcelanguageField;
		}
		set
		{
			sourcelanguageField = value;
		}
	}

	[XmlAttribute("target-language")]
	public string targetlanguage
	{
		get
		{
			return targetlanguageField;
		}
		set
		{
			targetlanguageField = value;
		}
	}

	[XmlAttribute]
	public string original
	{
		get
		{
			return originalField;
		}
		set
		{
			originalField = value;
		}
	}

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

	[XmlAttribute("product-name")]
	public string productname
	{
		get
		{
			return productnameField;
		}
		set
		{
			productnameField = value;
		}
	}

	[XmlAttribute("product-version")]
	public string productversion
	{
		get
		{
			return productversionField;
		}
		set
		{
			productversionField = value;
		}
	}

	[XmlAttribute("build-num")]
	public string buildnum
	{
		get
		{
			return buildnumField;
		}
		set
		{
			buildnumField = value;
		}
	}
}
