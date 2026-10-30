using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

[Serializable]
[GeneratedCode("xsd", "4.6.1590.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/locstudio/2006/6/lcx")]
public class LCXItemStrPrev
{
	private string valField;

	private string catField;

	private string cpField;

	public string Val
	{
		get
		{
			return valField;
		}
		set
		{
			valField = value;
		}
	}

	[XmlAttribute]
	public string Cat
	{
		get
		{
			return catField;
		}
		set
		{
			catField = value;
		}
	}

	[XmlAttribute]
	public string CP
	{
		get
		{
			return cpField;
		}
		set
		{
			cpField = value;
		}
	}
}
