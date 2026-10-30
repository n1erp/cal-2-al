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
public class LCXItemCmts
{
	private LCXItemCmtsCmt cmtField;

	public LCXItemCmtsCmt Cmt
	{
		get
		{
			return cmtField;
		}
		set
		{
			cmtField = value;
		}
	}
}
