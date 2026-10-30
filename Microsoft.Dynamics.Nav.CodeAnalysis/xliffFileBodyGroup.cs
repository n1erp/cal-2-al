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
public class xliffFileBodyGroup
{
	private xliffFileBodyGroupTransunit[] transunitField;

	private string idField;

	[XmlElement("trans-unit")]
	public xliffFileBodyGroupTransunit[] transunit
	{
		get
		{
			return transunitField;
		}
		set
		{
			transunitField = value;
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
}
