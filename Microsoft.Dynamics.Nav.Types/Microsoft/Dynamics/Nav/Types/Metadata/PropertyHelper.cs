using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public static class PropertyHelper
{
	public const string FalseAsString = "0";

	public const string TrueAsString = "1";

	private const string IdentifierSeperator = "\"";

	public const string StringValueYes = "YES";

	public const string StringValueTrue = "TRUE";

	private const string StringValueNo = "NO";

	public const string StringValueFalse = "FALSE";

	public static bool ConvertStringToBool(string property)
	{
		if (string.IsNullOrEmpty(property))
		{
			return false;
		}
		property = property.Trim();
		if (!property.AreEqual("YES") && !property.AreEqual("TRUE"))
		{
			return property.AreEqual("1");
		}
		return true;
	}

	public static string ConvertBoolToYesNoString(bool propertyValue)
	{
		if (!propertyValue)
		{
			return "NO";
		}
		return "YES";
	}

	public static bool PropertyIsFalse(string property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		property = property.Trim();
		if (!property.AreEqual("NO") && !property.AreEqual("FALSE"))
		{
			return property.AreEqual("0");
		}
		return true;
	}

	public static bool PropertyIsTrue(string property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		return ConvertStringToBool(property);
	}

	public static string CleanAndTrimProperty(string property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		if (property.Contains("\""))
		{
			property = property.Replace("\"", string.Empty);
		}
		property = property.Trim();
		return property;
	}

	public static string ConvertIntToString(int number)
	{
		return number.ToString(CultureInfo.InvariantCulture);
	}

	public static bool IsDynamic(string expression)
	{
		if (string.IsNullOrEmpty(expression))
		{
			return false;
		}
		if (!expression.AreEqual("YES") && !expression.AreEqual("NO") && !expression.AreEqual("TRUE") && !expression.AreEqual("FALSE") && !expression.AreEqual("1"))
		{
			return !expression.AreEqual("0");
		}
		return false;
	}

	public static bool IsDynamic(string expression, out bool booleanValue)
	{
		if (string.IsNullOrEmpty(expression))
		{
			booleanValue = false;
			return false;
		}
		if (PropertyIsTrue(expression))
		{
			booleanValue = true;
			return false;
		}
		if (PropertyIsFalse(expression))
		{
			booleanValue = false;
			return false;
		}
		booleanValue = false;
		return true;
	}

	public static bool GetDefaultBooleanPropertyValue(string expression)
	{
		if (!IsDynamic(expression))
		{
			return ConvertStringToBool(expression);
		}
		return true;
	}

	private static bool AreEqual(this string a, string b)
	{
		return string.Compare(a, b, StringComparison.OrdinalIgnoreCase) == 0;
	}
}
