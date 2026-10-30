using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALCompanyProperty
{
	public static string ALDisplayName()
	{
		NavSession session = NavCurrentThread.Session;
		if (session.IsOpen)
		{
			return session.Company.CompanyDisplayName;
		}
		return string.Empty;
	}

	public static string ALUrlName()
	{
		string stringToEscape = string.Empty;
		NavSession session = NavCurrentThread.Session;
		if (session.IsOpen)
		{
			stringToEscape = session.CompanyName;
		}
		return Uri.EscapeDataString(stringToEscape);
	}
}
