using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

[Serializable]
internal class ControlAddInEntry
{
	[NonSerialized]
	private byte[] resource;

	public string Name { get; set; }

	public string Version { get; set; }

	public string Token { get; set; }

	public string FullName => string.Join(", ", new string[3]
	{
		string.IsNullOrEmpty(Name) ? "null" : Name,
		string.IsNullOrEmpty(Version) ? null : ("Version=" + Version),
		string.IsNullOrEmpty(Token) ? null : ("PublicKeyToken=" + Token)
	}.Where((string v) => v != null).ToArray());

	public string FileName { get; set; }

	public string InnerPath { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Avoids needless conversion")]
	public byte[] Resource
	{
		get
		{
			return resource;
		}
		set
		{
			resource = value;
		}
	}

	public ControlAddInCategory Category { get; set; }

	public XElement ToXml()
	{
		return new XElement(ControlAddInXmlPart.ControlAddIn.ToString(), new XAttribute(ControlAddInXmlPart.Name.ToString(), Name), new XAttribute(ControlAddInXmlPart.Version.ToString(), Version), new XAttribute(ControlAddInXmlPart.Token.ToString(), Token), new XAttribute(ControlAddInXmlPart.FileName.ToString(), FileName), new XAttribute(ControlAddInXmlPart.Type.ToString(), Category.ToString()));
	}

	public static ControlAddInEntry FromXml(XElement element)
	{
		return new ControlAddInEntry
		{
			Name = element.GetAttribute<string>(ControlAddInXmlPart.Name.ToString(), null),
			Version = element.GetAttribute<string>(ControlAddInXmlPart.Version.ToString(), null),
			Token = element.GetAttribute<string>(ControlAddInXmlPart.Token.ToString(), null),
			FileName = element.GetAttribute<string>(ControlAddInXmlPart.FileName.ToString(), null),
			Category = (ControlAddInCategory)Enum.Parse(typeof(ControlAddInCategory), element.GetAttribute<string>(ControlAddInXmlPart.Type.ToString(), null))
		};
	}
}
