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
[XmlRoot(Namespace = "http://schemas.microsoft.com/locstudio/2006/6/lcx", IsNullable = false)]
public class LCX
{
	private LCXCmt[] ownedCommentsField;

	private LCXItem[] itemField;

	private decimal schemaVersionField;

	private string nameField;

	private byte psrIdField;

	private string srcCulField;

	private string tgtCulField;

	[XmlArrayItem("Cmt", IsNullable = false)]
	public LCXCmt[] OwnedComments
	{
		get
		{
			return ownedCommentsField;
		}
		set
		{
			ownedCommentsField = value;
		}
	}

	[XmlElement("Item")]
	public LCXItem[] Item
	{
		get
		{
			return itemField;
		}
		set
		{
			itemField = value;
		}
	}

	[XmlAttribute]
	public decimal SchemaVersion
	{
		get
		{
			return schemaVersionField;
		}
		set
		{
			schemaVersionField = value;
		}
	}

	[XmlAttribute]
	public string Name
	{
		get
		{
			return nameField;
		}
		set
		{
			nameField = value;
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
	public string SrcCul
	{
		get
		{
			return srcCulField;
		}
		set
		{
			srcCulField = value;
		}
	}

	[XmlAttribute]
	public string TgtCul
	{
		get
		{
			return tgtCulField;
		}
		set
		{
			tgtCulField = value;
		}
	}
}
