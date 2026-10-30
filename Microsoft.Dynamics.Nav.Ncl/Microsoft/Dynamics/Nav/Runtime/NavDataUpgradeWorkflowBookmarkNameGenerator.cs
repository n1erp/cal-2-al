using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavDataUpgradeWorkflowBookmarkNameGenerator
{
	public static string CreateForCodeunitExecutionActivity(int codeunitId, string methodName, string companyName)
	{
		return string.Join("_", codeunitId.ToString(CultureInfo.InvariantCulture), methodName, companyName);
	}

	public static string CreateForBaseUpgradeActivity(string uniqueTag)
	{
		return "BASE_" + uniqueTag;
	}

	public static string CreateForNavAppUpgradeActivity(INavAppReference app, string uniqueTag)
	{
		return string.Join("_", new string[4]
		{
			app.Name,
			app.Publisher,
			app.Version.ToString(),
			uniqueTag
		}.Where((string s) => s != null));
	}
}
