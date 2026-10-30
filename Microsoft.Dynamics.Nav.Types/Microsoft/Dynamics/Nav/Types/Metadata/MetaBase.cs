using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Dynamics.Nav.AL.Common;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class MetaBase : ElementDefinition
{
	private const string UndefinedValue = "undefined";

	protected MetaBase()
	{
	}

	protected MetaBase(XmlNode root)
	{
		if (root == null)
		{
			throw new ArgumentNullException("root");
		}
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		throw new NotSupportedException();
	}

	public override object Clone()
	{
		return (MetaBase)base.Clone();
	}

	public static int Int32Value(string value)
	{
		if (string.IsNullOrEmpty(value) || value == "undefined")
		{
			return 0;
		}
		return int.Parse(value, CultureInfo.InvariantCulture);
	}

	public static bool BooleanValue(string value)
	{
		if (value == null)
		{
			return false;
		}
		if (string.Compare(value, "yes", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(value, "true", StringComparison.OrdinalIgnoreCase) != 0)
		{
			return string.Compare(value, "1", StringComparison.Ordinal) == 0;
		}
		return true;
	}

	public static int GetId(XmlNode root, string propertyId)
	{
		int result = 0;
		if (root.Attributes[propertyId] != null)
		{
			result = Int32Value(root.Attributes[propertyId].Value);
		}
		return result;
	}

	public static string ExternalizeName(string navName)
	{
		return NameTransformations.MangleIntoValidXmlIdentifier(navName);
	}
}
