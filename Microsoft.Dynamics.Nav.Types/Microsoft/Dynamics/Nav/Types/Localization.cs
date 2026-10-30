using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public static class Localization
{
	private const string DescriptionPropertyName = "{0}_{1}_Description";

	private const string DisplayNamePropertyName = "{0}_{1}_DisplayName";

	public static string GetDescription(this PropertyInfo property)
	{
		return GetDescription(property.DeclaringType, property.Name);
	}

	public static string GetDescription(Type type, string name)
	{
		return GetDescription(type.FullName, name);
	}

	public static string GetDescription(string typeFullName, string name)
	{
		return GetNonLocalizableString(string.Format(CultureInfo.CurrentCulture, "{0}_{1}_Description", typeFullName, name)) ?? string.Empty;
	}

	public static string GetDisplayName(this PropertyInfo property)
	{
		return GetDisplayName(property.DeclaringType, property.Name);
	}

	public static string GetDisplayName(Type type, string name)
	{
		return GetNonLocalizableString(string.Format(CultureInfo.InvariantCulture, "{0}_{1}_DisplayName", type.FullName, name)) ?? name;
	}

	public static string GetDisplayName(string typeFullName, string name)
	{
		return GetNonLocalizableString(string.Format(CultureInfo.InvariantCulture, "{0}_{1}_DisplayName", typeFullName, name)) ?? name;
	}

	public static string GetNonLocalizableString(string nonLocalizableStringName)
	{
		return (string)typeof(NonLocalizable).GetField(nonLocalizableStringName.Replace('.', '_'))?.GetValue(null);
	}
}
