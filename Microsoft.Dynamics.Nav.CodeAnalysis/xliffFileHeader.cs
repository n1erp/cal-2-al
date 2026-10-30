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
public class xliffFileHeader
{
	private xliffFileHeaderTool toolField;

	public xliffFileHeaderTool tool
	{
		get
		{
			return toolField;
		}
		set
		{
			toolField = value;
		}
	}
}
