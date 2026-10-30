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
public class LCXItemStr
{
	private string valField;

	private LCXItemStrTgt tgtField;

	private LCXItemStrPrev prevField;

	private string catField;

	private string cpField;

	private bool usrLkField;

	private bool usrLkFieldSpecified;

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

	public LCXItemStrTgt Tgt
	{
		get
		{
			return tgtField;
		}
		set
		{
			tgtField = value;
		}
	}

	public LCXItemStrPrev Prev
	{
		get
		{
			return prevField;
		}
		set
		{
			prevField = value;
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

	[XmlAttribute]
	public bool UsrLk
	{
		get
		{
			return usrLkField;
		}
		set
		{
			usrLkField = value;
		}
	}

	[XmlIgnore]
	public bool UsrLkSpecified
	{
		get
		{
			return usrLkFieldSpecified;
		}
		set
		{
			usrLkFieldSpecified = value;
		}
	}
}
