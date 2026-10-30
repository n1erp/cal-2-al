using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public static class LabelPropertyHelper
{
	public const string Comment = "Comment";

	public const string Locked = "Locked";

	public const string MaxLength = "MaxLength";

	private const string Boolean = "Boolean";

	private const string Integer = "Integer";

	private const string Text = "Text";

	public static string GetPropertyReturnType(string propertyName)
	{
		if (string.Equals(propertyName, "Comment", StringComparison.OrdinalIgnoreCase))
		{
			return "Text";
		}
		if (string.Equals(propertyName, "Locked", StringComparison.OrdinalIgnoreCase))
		{
			return "Boolean";
		}
		if (string.Equals(propertyName, "MaxLength", StringComparison.OrdinalIgnoreCase))
		{
			return "Integer";
		}
		return null;
	}

	public static bool IsBooleanProperty(string propertyName)
	{
		return GetPropertyReturnType(propertyName) == "Boolean";
	}

	public static string[] GetAllLabelProperties()
	{
		return new string[3] { "Comment", "Locked", "MaxLength" };
	}

	public static string GetAllLabelPropertiesString()
	{
		return FormattableString.Invariant(FormattableStringFactory.Create("{0}, {1}, {2}", "Comment", "Locked", "MaxLength"));
	}
}
