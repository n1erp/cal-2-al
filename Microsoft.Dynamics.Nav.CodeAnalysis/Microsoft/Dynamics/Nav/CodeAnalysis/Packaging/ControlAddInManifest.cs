using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal class ControlAddInManifest
{
	private const int DefaultVersion = 2;

	public static readonly Encoding ScriptEncoding = Encoding.UTF8;

	private const string ScriptResourceXPath = "/Resources/Script";

	private const string ImageResourceXPath = "/Resources/Image";

	private const string StyleSheetResourceXPath = "/Resources/StyleSheet";

	private const string ScriptUrlXPath = "/ScriptUrls/ScriptUrl";

	private const string StyleSheetUrlXPath = "/StyleSheetUrls/StyleSheetUrl";

	private List<string> scriptUrls;

	private List<string> styleSheetUrls;

	private List<string> imageResources;

	private List<string> scriptResources;

	private List<string> styleSheetResources;

	public const int NoValue = -1;

	public int Version { get; set; } = 2;

	public List<string> ScriptUrls => scriptUrls ?? (scriptUrls = new List<string>());

	public List<string> StyleSheetUrls => styleSheetUrls ?? (styleSheetUrls = new List<string>());

	public List<string> ImageResourceNames => imageResources ?? (imageResources = new List<string>());

	public List<string> ScriptResourceNames => scriptResources ?? (scriptResources = new List<string>());

	public List<string> StyleSheetResourceNames => styleSheetResources ?? (styleSheetResources = new List<string>());

	public string Script { get; set; }

	public string RecreateScript { get; set; }

	public string RefreshScript { get; set; }

	public int RequestedHeight { get; set; }

	public int RequestedWidth { get; set; }

	public int MinimumHeight { get; set; }

	public int MinimumWidth { get; set; }

	public int MaximumHeight { get; set; }

	public int MaximumWidth { get; set; }

	public bool VerticalShrink { get; set; }

	public bool HorizontalShrink { get; set; }

	public bool VerticalStretch { get; set; }

	public bool HorizontalStretch { get; set; }

	public string ToXml()
	{
		return new XDocument(new XElement("Manifest", new XElement("Resources", ScriptResourceNames.Select((string script) => new XElement("Script", script)), StyleSheetResourceNames.Select((string stylesheet) => new XElement("StyleSheet", stylesheet)), ImageResourceNames.Select((string image) => new XElement("Image", image))), NewElementIfNotDefault("Script", () => Script, cdata: true), new XElement("ScriptUrls", ScriptUrls.Select((string script) => new XElement("ScriptUrl", script))), new XElement("StyleSheetUrls", StyleSheetUrls.Select((string stylesheet) => new XElement("StyleSheetUrl", stylesheet))), NewElementIfNotDefault("RefreshScript", () => RefreshScript, cdata: true), NewElementIfNotDefault("RecreateScript", () => RecreateScript, cdata: true), NewElementIfNotDefault("RequestedHeight", () => RequestedHeight), NewElementIfNotDefault("RequestedWidth", () => RequestedWidth), NewElementIfNotDefault("MinimumHeight", () => MinimumHeight), NewElementIfNotDefault("MinimumWidth", () => MinimumWidth), NewElementIfNotDefault("MaximumHeight", () => MaximumHeight), NewElementIfNotDefault("MaximumWidth", () => MaximumWidth), NewElementIfNotDefault("VerticalShrink", () => VerticalShrink), NewElementIfNotDefault("VerticalStretch", () => VerticalStretch), NewElementIfNotDefault("HorizontalShrink", () => HorizontalShrink), NewElementIfNotDefault("HorizontalStretch", () => HorizontalStretch), NewElementIfNotDefault("Version", () => Version))).ToString();
	}

	public static ControlAddInManifest FromXml(string xml)
	{
		ControlAddInManifest controlAddInManifest = new ControlAddInManifest();
		XElement xElement = XElement.Parse(xml);
		controlAddInManifest.ScriptResourceNames.AddRange(ParseXmlRange(xElement, "/Resources/Script"));
		controlAddInManifest.StyleSheetResourceNames.AddRange(ParseXmlRange(xElement, "/Resources/StyleSheet"));
		controlAddInManifest.ImageResourceNames.AddRange(ParseXmlRange(xElement, "/Resources/Image"));
		controlAddInManifest.ScriptUrls.AddRange(ParseXmlRange(xElement, "/ScriptUrls/ScriptUrl"));
		controlAddInManifest.StyleSheetUrls.AddRange(ParseXmlRange(xElement, "/StyleSheetUrls/StyleSheetUrl"));
		controlAddInManifest.Script = ParseNode<string>(xElement, "Script");
		controlAddInManifest.RecreateScript = ParseNode<string>(xElement, "RecreateScript");
		controlAddInManifest.RefreshScript = ParseNode<string>(xElement, "RefreshScript");
		controlAddInManifest.RequestedHeight = ParseNode(xElement, "RequestedHeight", 0);
		controlAddInManifest.RequestedWidth = ParseNode(xElement, "RequestedWidth", 0);
		controlAddInManifest.MinimumHeight = ParseNode(xElement, "MinimumHeight", 0);
		controlAddInManifest.MinimumWidth = ParseNode(xElement, "MinimumWidth", 0);
		controlAddInManifest.MaximumHeight = ParseNode(xElement, "MaximumHeight", 0);
		controlAddInManifest.MaximumWidth = ParseNode(xElement, "MaximumWidth", 0);
		controlAddInManifest.VerticalShrink = ParseNode(xElement, "VerticalShrink", defaultValue: false);
		controlAddInManifest.VerticalStretch = ParseNode(xElement, "VerticalStretch", defaultValue: false);
		controlAddInManifest.HorizontalShrink = ParseNode(xElement, "HorizontalShrink", defaultValue: false);
		controlAddInManifest.HorizontalStretch = ParseNode(xElement, "HorizontalStretch", defaultValue: false);
		controlAddInManifest.Version = ParseNode(xElement, "Version", 2);
		return controlAddInManifest;
	}

	private static XElement NewElementIfNotDefault<T>(string nodeName, Func<T> accessor, bool cdata = false)
	{
		T x = accessor();
		if (EqualityComparer<T>.Default.Equals(x, default))
		{
			return null;
		}
		if (cdata)
		{
			return new XElement(nodeName, new XCData(x.ToString()));
		}
		return new XElement(nodeName, x.ToString());
	}

	private static IEnumerable<string> ParseXmlRange(XElement root, string xpath)
	{
		return from x in root.XPathSelectElements(xpath)
			select x.Value;
	}

	private static T ParseNode<T>(XElement parent, string nodeName, T defaultValue = default(T))
	{
		XElement xElement = parent.Element(nodeName);
		if (xElement == null)
		{
			return defaultValue;
		}
		try
		{
			return (T)Convert.ChangeType(xElement.Value, typeof(T), CultureInfo.InvariantCulture);
		}
		catch (Exception innerException)
		{
			throw new XmlException(string.Format(CultureInfo.CurrentCulture, CodeAnalysisResources.ControlAddInManifestWrongValue, xElement.Value), innerException);
		}
	}
}
