using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

public class ControlAddInManifestParser
{
	private const string ScriptUrlXPath = "/Manifest/ScriptUrls/ScriptUrl";

	private const string StyleSheetUrlXPath = "/Manifest/StyleSheetUrls/StyleSheetUrl";

	private const string ScriptResourceXPath = "/Manifest/Resources/Script";

	private const string ImageResourceXPath = "/Manifest/Resources/Image";

	private const string StyleSheetResourceXPath = "/Manifest/Resources/StyleSheet";

	private const string ScriptXPath = "/Manifest/Script";

	private const string RecreateScriptXPath = "/Manifest/RecreateScript";

	private const string RefreshScriptXPath = "/Manifest/RefreshScript";

	private const string RequestedHeight = "RequestedHeight";

	private const string RequestedHeightXPath = "/Manifest/RequestedHeight";

	private const string RequestedWidth = "RequestedWidth";

	private const string RequestedWidthXPath = "/Manifest/RequestedWidth";

	private const string MinimumHeight = "MinimumHeight";

	private const string MinimumHeightXPath = "/Manifest/MinimumHeight";

	private const string MinimumWidth = "MinimumWidth";

	private const string MinimumWidthXPath = "/Manifest/MinimumWidth";

	private const string MaximumHeight = "MaximumHeight";

	private const string MaximumHeightXPath = "/Manifest/MaximumHeight";

	private const string MaximumWidth = "MaximumWidth";

	private const string MaximumWidthXPath = "/Manifest/MaximumWidth";

	private const string VerticalShrink = "VerticalShrink";

	private const string VerticalShrinkXPath = "/Manifest/VerticalShrink";

	private const string HorizontalShrink = "HorizontalShrink";

	private const string HorizontalShrinkXPath = "/Manifest/HorizontalShrink";

	private const string VerticalStretch = "VerticalStretch";

	private const string VerticalStretchXPath = "/Manifest/VerticalStretch";

	private const string HorizontalStretch = "HorizontalStretch";

	private const string HorizontalStretchXPath = "/Manifest/HorizontalStretch";

	private const string AllowCaption = "AllowCaption";

	private const string AllowCaptionXPath = "/Manifest/AllowCaption";

	private const string Version = "Version";

	private const string VersionXPath = "/Manifest/Version";

	private readonly byte[] controlAddInManifest;

	public ControlAddInManifest Manifest { get; private set; }

	public string ErrorMessage { get; private set; }

	public ControlAddInManifestParser(byte[] controlAddInManifest)
	{
		if (controlAddInManifest == null)
		{
			throw new ArgumentNullException("controlAddInManifest");
		}
		this.controlAddInManifest = controlAddInManifest;
	}

	public bool Parse()
	{
		ErrorMessage = null;
		Manifest = null;
		ControlAddInManifest controlAddInManifest = new ControlAddInManifest();
		try
		{
			XPathNavigator navigator = new XPathDocument(XmlHelper.SafeXmlReader(new MemoryStream(this.controlAddInManifest))).CreateNavigator();
			AddRange(controlAddInManifest.ScriptUrls, Nodes(navigator, "/Manifest/ScriptUrls/ScriptUrl", valueIsPath: true));
			AddRange(controlAddInManifest.StyleSheetUrls, Nodes(navigator, "/Manifest/StyleSheetUrls/StyleSheetUrl", valueIsPath: true));
			AddRange(controlAddInManifest.ImageResourceNames, Nodes(navigator, "/Manifest/Resources/Image", valueIsPath: true));
			AddRange(controlAddInManifest.ScriptResourceNames, Nodes(navigator, "/Manifest/Resources/Script", valueIsPath: true));
			AddRange(controlAddInManifest.StyleSheetResourceNames, Nodes(navigator, "/Manifest/Resources/StyleSheet", valueIsPath: true));
			controlAddInManifest.Script = ParseNode(navigator, "/Manifest/Script", valueIsPath: false, string.Empty).Trim();
			controlAddInManifest.RecreateScript = ParseNode(navigator, "/Manifest/RecreateScript", valueIsPath: false, string.Empty).Trim();
			controlAddInManifest.RefreshScript = ParseNode(navigator, "/Manifest/RefreshScript", valueIsPath: false, string.Empty).Trim();
			controlAddInManifest.RequestedHeight = ParseNode(navigator, "/Manifest/RequestedHeight", valueIsPath: false, -1);
			controlAddInManifest.RequestedWidth = ParseNode(navigator, "/Manifest/RequestedWidth", valueIsPath: false, -1);
			controlAddInManifest.MinimumHeight = ParseNode(navigator, "/Manifest/MinimumHeight", valueIsPath: false, -1);
			controlAddInManifest.MinimumWidth = ParseNode(navigator, "/Manifest/MinimumWidth", valueIsPath: false, -1);
			controlAddInManifest.MaximumHeight = ParseNode(navigator, "/Manifest/MaximumHeight", valueIsPath: false, -1);
			controlAddInManifest.MaximumWidth = ParseNode(navigator, "/Manifest/MaximumWidth", valueIsPath: false, -1);
			controlAddInManifest.VerticalShrink = ParseNode(navigator, "/Manifest/VerticalShrink", valueIsPath: false, defaultValue: false);
			controlAddInManifest.HorizontalShrink = ParseNode(navigator, "/Manifest/HorizontalShrink", valueIsPath: false, defaultValue: false);
			controlAddInManifest.VerticalStretch = ParseNode(navigator, "/Manifest/VerticalStretch", valueIsPath: false, defaultValue: false);
			controlAddInManifest.HorizontalStretch = ParseNode(navigator, "/Manifest/HorizontalStretch", valueIsPath: false, defaultValue: false);
			controlAddInManifest.AllowCaption = ParseNode(navigator, "/Manifest/AllowCaption", valueIsPath: false, defaultValue: false);
			controlAddInManifest.Version = ParseNode(navigator, "/Manifest/Version", valueIsPath: false, 1);
			ValidateManifest(controlAddInManifest);
		}
		catch (XmlException ex)
		{
			ErrorMessage = string.Format(CultureInfo.CurrentCulture, Lang.ManifestParser_InvalidXml, ex.Message.TrimEnd('.'));
		}
		if (ErrorMessage == null)
		{
			Manifest = controlAddInManifest;
			return true;
		}
		return false;
	}

	public static string ParseOverride(string controlAddInManifestOverride, ControlAddInManifestDefinition controlAddInManifest)
	{
		if (string.IsNullOrEmpty(controlAddInManifestOverride))
		{
			return null;
		}
		string[] array = controlAddInManifestOverride.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			int num = text.IndexOf("=", StringComparison.OrdinalIgnoreCase);
			if (num == -1)
			{
				return string.Format(CultureInfo.CurrentCulture, Lang.ManifestParser_InvalidOverride, text);
			}
			string text2 = text.Substring(0, num).Trim();
			string text3 = text.Substring(num + 1).Trim();
			try
			{
				if (!OverrideManifestProperty(controlAddInManifest, text2, text3))
				{
					return string.Format(CultureInfo.CurrentCulture, Lang.ManifestParser_InvalidOverride, text2);
				}
			}
			catch (FormatException)
			{
				return string.Format(CultureInfo.CurrentCulture, Lang.ManifestParser_InvalidOverrideValue, text3);
			}
		}
		return null;
	}

	private static bool OverrideManifestProperty(ControlAddInManifestDefinition controlAddInManifest, string overrideName, string overrideValue)
	{
		switch (overrideName)
		{
		case "RequestedHeight":
			controlAddInManifest.RequestedHeight = Convert.ToInt32(overrideValue, CultureInfo.InvariantCulture);
			return true;
		case "RequestedWidth":
			controlAddInManifest.RequestedWidth = Convert.ToInt32(overrideValue, CultureInfo.InvariantCulture);
			return true;
		case "MinimumHeight":
			controlAddInManifest.MinimumHeight = Convert.ToInt32(overrideValue, CultureInfo.InvariantCulture);
			return true;
		case "MinimumWidth":
			controlAddInManifest.MinimumWidth = Convert.ToInt32(overrideValue, CultureInfo.InvariantCulture);
			return true;
		case "MaximumHeight":
			controlAddInManifest.MaximumHeight = Convert.ToInt32(overrideValue, CultureInfo.InvariantCulture);
			return true;
		case "MaximumWidth":
			controlAddInManifest.MaximumWidth = Convert.ToInt32(overrideValue, CultureInfo.InvariantCulture);
			return true;
		case "VerticalShrink":
			controlAddInManifest.VerticalShrink = Convert.ToBoolean(overrideValue, CultureInfo.InvariantCulture);
			return true;
		case "HorizontalShrink":
			controlAddInManifest.HorizontalShrink = Convert.ToBoolean(overrideValue, CultureInfo.InvariantCulture);
			return true;
		case "VerticalStretch":
			controlAddInManifest.VerticalStretch = Convert.ToBoolean(overrideValue, CultureInfo.InvariantCulture);
			return true;
		case "HorizontalStretch":
			controlAddInManifest.HorizontalStretch = Convert.ToBoolean(overrideValue, CultureInfo.InvariantCulture);
			return true;
		case "AllowCaption":
			controlAddInManifest.AllowCaption = Convert.ToBoolean(overrideValue, CultureInfo.InvariantCulture);
			return true;
		case "Height":
			controlAddInManifest.RequestedHeight = Convert.ToInt32(overrideValue, CultureInfo.InvariantCulture);
			controlAddInManifest.VerticalShrink = false;
			controlAddInManifest.VerticalStretch = false;
			return true;
		case "Width":
			controlAddInManifest.RequestedWidth = Convert.ToInt32(overrideValue, CultureInfo.InvariantCulture);
			controlAddInManifest.HorizontalShrink = false;
			controlAddInManifest.HorizontalStretch = false;
			return true;
		default:
			return false;
		}
	}

	private static void AddRange<T>(ICollection<T> destination, IEnumerable<T> source)
	{
		foreach (T item in source)
		{
			destination.Add(item);
		}
	}

	private static T ParseNode<T>(XPathNavigator navigator, string xpath, bool valueIsPath, T defaultValue = default(T))
	{
		List<string> list = Nodes(navigator, xpath, valueIsPath).ToList();
		if (list.Count == 0)
		{
			return defaultValue;
		}
		if (list.Count > 1)
		{
			throw new XmlException(string.Format(CultureInfo.CurrentCulture, Lang.ManifestParser_InvalidXml, xpath));
		}
		try
		{
			return (T)Convert.ChangeType(list[0], typeof(T), CultureInfo.InvariantCulture);
		}
		catch (Exception innerException)
		{
			throw new XmlException(string.Format(CultureInfo.CurrentCulture, Lang.ManifestParser_InvalidValue, list[0]), innerException);
		}
	}

	private static IEnumerable<string> Nodes(XPathNavigator navigator, string xpath, bool valueIsPath)
	{
		XPathNodeIterator nodeIterator = navigator.Select(xpath);
		while (nodeIterator.MoveNext())
		{
			string text = nodeIterator.Current.Value.Trim();
			if (valueIsPath)
			{
				text = text.Replace('\\', '/');
			}
			if (!string.IsNullOrEmpty(text))
			{
				yield return text;
			}
		}
	}

	private static void ValidateManifest(ControlAddInManifest manifest)
	{
		if (manifest.RequestedHeight != -1)
		{
			if (manifest.MinimumHeight != -1 && manifest.MinimumHeight > manifest.RequestedHeight)
			{
				throw new XmlException(string.Format(CultureInfo.CurrentCulture, Lang.ManifestParser_DimensionOutOfRange, GetName("/Manifest/MinimumHeight")));
			}
			if (manifest.MaximumHeight != -1 && manifest.MaximumHeight < manifest.RequestedHeight)
			{
				throw new XmlException(string.Format(CultureInfo.CurrentCulture, Lang.ManifestParser_DimensionOutOfRange, GetName("/Manifest/MaximumHeight")));
			}
		}
		if (manifest.RequestedWidth != -1)
		{
			if (manifest.MinimumWidth != -1 && manifest.MinimumWidth > manifest.RequestedWidth)
			{
				throw new XmlException(string.Format(CultureInfo.CurrentCulture, Lang.ManifestParser_DimensionOutOfRange, GetName("/Manifest/MinimumWidth")));
			}
			if (manifest.MaximumWidth != -1 && manifest.MaximumWidth < manifest.RequestedWidth)
			{
				throw new XmlException(string.Format(CultureInfo.CurrentCulture, Lang.ManifestParser_DimensionOutOfRange, GetName("/Manifest/MaximumWidth")));
			}
		}
		if (string.IsNullOrEmpty(manifest.Script) && manifest.ScriptResourceNames.Count == 0 && manifest.ScriptUrls.Count == 0)
		{
			throw new XmlException(Lang.ManifestParser_MissingScript);
		}
	}

	private static string GetName(string fullName)
	{
		return fullName.Substring(fullName.LastIndexOf("/", StringComparison.Ordinal) + 1);
	}
}
