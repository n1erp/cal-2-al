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
public class LCXItem
{
	private LCXItemStr strField;

	private LCXItemDisp dispField;

	private LCXItemModified modifiedField;

	private LCXItemCmts cmtsField;

	private LCXItemStr1[] propsField;

	private string itemIdField;

	private string itemTypeField;

	private byte psrIdField;

	private bool instFlgField;

	private bool leafField;

	public LCXItemStr Str
	{
		get
		{
			return strField;
		}
		set
		{
			strField = value;
		}
	}

	public LCXItemDisp Disp
	{
		get
		{
			return dispField;
		}
		set
		{
			dispField = value;
		}
	}

	public LCXItemModified Modified
	{
		get
		{
			return modifiedField;
		}
		set
		{
			modifiedField = value;
		}
	}

	public LCXItemCmts Cmts
	{
		get
		{
			return cmtsField;
		}
		set
		{
			cmtsField = value;
		}
	}

	[XmlArrayItem("Str", IsNullable = false)]
	public LCXItemStr1[] Props
	{
		get
		{
			return propsField;
		}
		set
		{
			propsField = value;
		}
	}

	[XmlAttribute]
	public string ItemId
	{
		get
		{
			return itemIdField;
		}
		set
		{
			itemIdField = value;
		}
	}

	[XmlAttribute]
	public string ItemType
	{
		get
		{
			return itemTypeField;
		}
		set
		{
			itemTypeField = value;
		}
	}

	[XmlAttribute]
	public byte PsrId
	{
		get
		{
			return psrIdField;
		}
		set
		{
			psrIdField = value;
		}
	}

	[XmlAttribute]
	public bool InstFlg
	{
		get
		{
			return instFlgField;
		}
		set
		{
			instFlgField = value;
		}
	}

	[XmlAttribute]
	public bool Leaf
	{
		get
		{
			return leafField;
		}
		set
		{
			leafField = value;
		}
	}
}
