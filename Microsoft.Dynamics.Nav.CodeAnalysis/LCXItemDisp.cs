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
public class LCXItemDisp
{
	private string iconField;

	private bool expandField;

	private uint orderField;

	private string pathField;

	[XmlAttribute]
	public string Icon
	{
		get
		{
			return iconField;
		}
		set
		{
			iconField = value;
		}
	}

	[XmlAttribute]
	public bool Expand
	{
		get
		{
			return expandField;
		}
		set
		{
			expandField = value;
		}
	}

	[XmlAttribute]
	public uint Order
	{
		get
		{
			return orderField;
		}
		set
		{
			orderField = value;
		}
	}

	[XmlAttribute]
	public string Path
	{
		get
		{
			return pathField;
		}
		set
		{
			pathField = value;
		}
	}
}
