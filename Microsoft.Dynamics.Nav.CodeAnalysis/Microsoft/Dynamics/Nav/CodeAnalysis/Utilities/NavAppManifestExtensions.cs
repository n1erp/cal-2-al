using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

public static class NavAppManifestExtensions
{
	public static string ToJsonString(this NavAppManifest manifest)
	{
		return GenerateAppJsonContent(manifest)?.ToString();
	}

	private static JObject GenerateAppJsonContent(NavAppManifest manifest)
	{
		if (manifest == null)
		{
			return null;
		}
		return new JObject(new JProperty("id", SanitizeJsonValue(manifest.AppId.ToString())), new JProperty("name", SanitizeJsonValue(manifest.AppName)), new JProperty("publisher", SanitizeJsonValue(manifest.AppPublisher)), new JProperty("brief", SanitizeJsonValue(manifest.AppBrief)), new JProperty("description", SanitizeJsonValue(manifest.AppDescription)), new JProperty("version", SanitizeJsonValue(manifest.AppVersion.ToString())), new JProperty("compatibilityId", SanitizeJsonValue(manifest.AppCompatibilityId.ToString())), new JProperty("privacyStatement", SanitizeJsonValue(manifest.AppPrivacyStatement)), new JProperty("EULA", SanitizeJsonValue(manifest.AppEula)), new JProperty("help", SanitizeJsonValue(manifest.AppHelp)), new JProperty("helpBaseUrl", SanitizeJsonValue(manifest.AppHelpBaseUrl)), new JProperty("contextSensitiveHelpUrl", SanitizeJsonValue(manifest.ContextSensitiveHelpUrl)), new JProperty("supportedLocales", new JArray()), new JProperty("url", SanitizeJsonValue(manifest.AppUrl)), new JProperty("logo", SanitizeJsonValue(manifest.AppLogo)), new JProperty("capabilities", new JArray()), new JProperty("dependencies", new JArray()), new JProperty("screenshots", new JArray()), new JProperty("platform", SanitizeJsonValue(manifest.Platform?.ToString())), new JProperty("application", SanitizeJsonValue(manifest.Application?.ToString())));
	}

	private static string SanitizeJsonValue(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return string.Empty;
		}
		return input;
	}
}
