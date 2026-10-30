using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;

[Serializable]
[GeneratedCode("xsd", "4.6.1590.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
public class xliffFileBodyGroupTransunit
{
	private string sourceField;

	private xliffFileBodyGroupTransunitTarget targetField;

	private xliffFileBodyGroupTransunitNote[] noteField;

	private string idField;

	private ushort maxwidthField;

	private string sizeunitField;

	private string translateField;

	private string spaceField;

	public string source
	{
		get
		{
			return sourceField;
		}
		set
		{
			sourceField = value;
		}
	}

	public xliffFileBodyGroupTransunitTarget target
	{
		get
		{
			return targetField;
		}
		set
		{
			targetField = value;
		}
	}

	[XmlElement("note")]
	public xliffFileBodyGroupTransunitNote[] note
	{
		get
		{
			return noteField;
		}
		set
		{
			noteField = value;
		}
	}

	[XmlAttribute]
	public string id
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
	public ushort maxwidth
	{
		get
		{
			return maxwidthField;
		}
		set
		{
			maxwidthField = value;
		}
	}

	[XmlAttribute("size-unit")]
	public string sizeunit
	{
		get
		{
			return sizeunitField;
		}
		set
		{
			sizeunitField = value;
		}
	}

	[XmlAttribute]
	public string translate
	{
		get
		{
			return translateField;
		}
		set
		{
			translateField = value;
		}
	}

	[XmlAttribute(Form = XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
	public string space
	{
		get
		{
			return spaceField;
		}
		set
		{
			spaceField = value;
		}
	}
}
