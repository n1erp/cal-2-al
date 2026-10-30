using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavFilterPageBuilderFormatter : NavValueFormatter
{
	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		if (!(value is NavFilterPageBuilder navFilterPageBuilder))
		{
			return string.Empty;
		}
		switch (formatNumber)
		{
		case 0:
		case 1:
		case 2:
			return FormatTextDocument(navFilterPageBuilder);
		case 9:
			return FormatXmlDocument(navFilterPageBuilder);
		default:
			return value.ToString();
		}
	}

	private static string FormatTextDocument(NavFilterPageBuilder navFilterPageBuilder)
	{
		return navFilterPageBuilder.ToString(invariant: false);
	}

	private static string FormatXmlDocument(NavFilterPageBuilder navFilterPageBuilder)
	{
		return navFilterPageBuilder.ToString(invariant: true);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		throw new NotImplementedException();
	}

	internal override bool PadLeft(NavValue value)
	{
		return false;
	}
}
