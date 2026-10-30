using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavOptionFormatter : NavValueFormatter
{
	private readonly bool useOptionString;

	private static readonly ReadOnlyCollection<string> OptionFieldNames = new ReadOnlyCollection<string>(new string[2] { "TEXT", "NUMBER" });

	private bool UseOptionString
	{
		get
		{
			if (useOptionString)
			{
				return NavCurrentThread.Session.ClientConnectionType == ConnectionType.WebServiceClient;
			}
			return false;
		}
	}

	internal NavOptionFormatter()
	{
		useOptionString = ServerUserSettings.Instance.ServicesOptionFormat.Value == ServicesOptionFormat.OptionString;
	}

	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		int index = value.ToInt32();
		if ((uint)formatNumber > 1u)
		{
			if (formatNumber != 2 && formatNumber != 9)
			{
				throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Option);
			}
		}
		else
		{
			NavOption navOption = (NavOption)value;
			if (navOption.NavOptionMetadata != null)
			{
				string text = (UseOptionString ? navOption.NavOptionMetadata.GetOptionFromIndex(index) : navOption.NavOptionMetadata.GetCaptionFromIndex(index));
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
			}
		}
		return index.ToString(NavCurrentThread.Session.WindowsCulture);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return FormatSettings.OptionStdFormatStrings[formatNumber];
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Char);
	}

	internal override ReadOnlyCollection<string> GetFieldNames()
	{
		return OptionFieldNames;
	}

	internal override bool PadLeft(NavValue value)
	{
		return false;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		NavOption navOption = (NavOption)value;
		NavFormatEvaluateHelper.GetFieldFormatFromString(options, format, ref pos);
		switch (options.Field)
		{
		case 3:
			if (navOption.NavOptionMetadata != null && !string.IsNullOrEmpty(navOption.NavOptionMetadata.GetOptionFromIndex(value.ToInt32())))
			{
				return navOption.NavOptionMetadata.GetCaptionFromIndex(value.ToInt32());
			}
			return value.ToInt32().ToString(NavCurrentThread.Session.WindowsCulture);
		case 4:
		case 5:
			return value.ToInt32().ToString(NavCurrentThread.Session.WindowsCulture);
		default:
			throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, value.NclType.ToString());
		}
	}
}
