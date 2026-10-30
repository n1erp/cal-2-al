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
public class xliffFileBody
{
	private xliffFileBodyGroup groupField;

	public xliffFileBodyGroup group
	{
		get
		{
			return groupField;
		}
		set
		{
			groupField = value;
		}
	}
}
