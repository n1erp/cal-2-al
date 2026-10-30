using Microsoft.Dynamics.Nav.Common;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALProductName
{
	public static string ALProductNameFull()
	{
		return ProductNames.ProductName;
	}

	public static string ALProductNameShort()
	{
		return ProductNames.ProductNameShort;
	}

	public static string ALMarketingName()
	{
		return ProductNames.MarketingName;
	}
}
