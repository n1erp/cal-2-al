using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime.Data.Graph;

internal static class ExtensionsHelper
{
	public enum ExtensionType
	{
		OpenTypeExtension,
		SingleValueExtendedProperties
	}

	public const string ExtensionOperation = "Extensions";

	public const string ExtensionNameProperty = "ExtensionName";

	public const string OpenTypeExtensionName = "OpenTypeExtension";

	public const string OpenTypeExtensionIdFieldName = "Id";

	public const string SingleValueExtendedProperties = "SingleValueExtendedProperties";

	public const string SingleValueExtendedPropertiesIdFieldName = "PropertyId";

	public static string GetExpandedExtensionQuery(ICollection<string> ids, ExtensionType extensionType)
	{
		if (ids.Count == 0)
		{
			return string.Empty;
		}
		string text = null;
		string text2 = null;
		switch (extensionType)
		{
		case ExtensionType.OpenTypeExtension:
			text = "Extensions";
			text2 = "Id";
			break;
		case ExtensionType.SingleValueExtendedProperties:
			text = "SingleValueExtendedProperties";
			text2 = "PropertyId";
			break;
		}
		string text3 = "$filter = " + text2 + " eq '" + string.Join("' or " + text2 + " eq '", ids) + "'";
		return text + "(" + text3 + ")";
	}

	public static bool IsOpenTypeExtension(NCLMetaField metaField)
	{
		return "OpenTypeExtension".Equals(metaField.ExternalType, StringComparison.Ordinal);
	}

	public static bool IsSingleValueExtendedProperties(NCLMetaField metaField)
	{
		return "SingleValueExtendedProperties".Equals(metaField.ExternalType, StringComparison.Ordinal);
	}
}
