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
[XmlRoot(Namespace = "urn:oasis:names:tc:xliff:document:1.2", IsNullable = false)]
public class xliff
{
	private xliffFile fileField;

	private decimal versionField;

	public xliffFile file
	{
		get
		{
			return fileField;
		}
		set
		{
			fileField = value;
		}
	}

	[XmlAttribute]
	public decimal version
	{
		get
		{
			return versionField;
		}
		set
		{
			versionField = value;
		}
	}
}
