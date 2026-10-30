using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

[CompilerGenerated]
internal static class NavAppManifestCompatibility
{
	internal static ImmutableDictionary<string, ManifestPropertyTypeInfo> ManifestPropertiesInfos { get; }

	internal static ImmutableHashSet<string> ManifestPropertiesName { get; }

	internal static ImmutableDictionary<string, ManifestPropertyValueTypeInfo> ManifestPropertyValuesInfos { get; }

	static NavAppManifestCompatibility()
	{
		ManifestPropertiesInfos = GetManifestPropertiesInfos();
		ManifestPropertiesName = GetManifestPropertiesName();
		ManifestPropertyValuesInfos = GetManifestPropertyValuesInfos();
	}

	private static ImmutableDictionary<string, ManifestPropertyTypeInfo> GetManifestPropertiesInfos()
	{
		return ImmutableDictionary.CreateRange(new KeyValuePair<string, ManifestPropertyTypeInfo>[34]
		{
			new KeyValuePair<string, ManifestPropertyTypeInfo>("ID", new ManifestPropertyTypeInfo("ID", null, null, isRequired: true)),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("NAME", new ManifestPropertyTypeInfo("NAME", null, null, isRequired: true)),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("VERSION", new ManifestPropertyTypeInfo("VERSION", null, null, isRequired: true)),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("PUBLISHER", new ManifestPropertyTypeInfo("PUBLISHER", null, null, isRequired: true)),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("DESCRIPTION", new ManifestPropertyTypeInfo("DESCRIPTION")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("BRIEF", new ManifestPropertyTypeInfo("BRIEF")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("CAPABILITIES", new ManifestPropertyTypeInfo("CAPABILITIES", null, null, isRequired: false, isArray: true)),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("PRIVACYSTATEMENT", new ManifestPropertyTypeInfo("PRIVACYSTATEMENT")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("EULA", new ManifestPropertyTypeInfo("EULA")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("HELP", new ManifestPropertyTypeInfo("HELP")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("URL", new ManifestPropertyTypeInfo("URL")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("LOGO", new ManifestPropertyTypeInfo("LOGO")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("SCREENSHOTS", new ManifestPropertyTypeInfo("SCREENSHOTS", null, null, isRequired: false, isArray: true)),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("TARGET", new ManifestPropertyTypeInfo("TARGET")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("SHOWMYCODE", new ManifestPropertyTypeInfo("SHOWMYCODE")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("PLATFORM", new ManifestPropertyTypeInfo("PLATFORM")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("APPLICATION", new ManifestPropertyTypeInfo("APPLICATION")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("TEST", new ManifestPropertyTypeInfo("TEST")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("RUNTIME", new ManifestPropertyTypeInfo("RUNTIME")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("IDRANGES", new ManifestPropertyTypeInfo("IDRANGES", null, VersionCompatibility.Parse("3.0"), isRequired: false, isArray: true)),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("IDRANGES_FROM", new ManifestPropertyTypeInfo("IDRANGES_FROM", null, VersionCompatibility.Parse("3.0"))),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("IDRANGES_TO", new ManifestPropertyTypeInfo("IDRANGES_TO", null, VersionCompatibility.Parse("3.0"))),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("IDRANGE", new ManifestPropertyTypeInfo("IDRANGE")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("IDRANGE_FROM", new ManifestPropertyTypeInfo("IDRANGE_FROM")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("IDRANGE_TO", new ManifestPropertyTypeInfo("IDRANGE_TO")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("DEPENDENCIES", new ManifestPropertyTypeInfo("DEPENDENCIES", null, null, isRequired: false, isArray: true)),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("DEPENDENCIES_APPID", new ManifestPropertyTypeInfo("DEPENDENCIES_APPID")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("DEPENDENCIES_NAME", new ManifestPropertyTypeInfo("DEPENDENCIES_NAME")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("DEPENDENCIES_PUBLISHER", new ManifestPropertyTypeInfo("DEPENDENCIES_PUBLISHER")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("DEPENDENCIES_VERSION", new ManifestPropertyTypeInfo("DEPENDENCIES_VERSION")),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("FEATURES", new ManifestPropertyTypeInfo("FEATURES", null, null, isRequired: false, isArray: true)),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("HELPBASEURL", new ManifestPropertyTypeInfo("HELPBASEURL", null, VersionCompatibility.Parse("2.0"))),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("SUPPORTEDLOCALES", new ManifestPropertyTypeInfo("SUPPORTEDLOCALES", null, VersionCompatibility.Parse("2.0"), isRequired: false, isArray: true)),
			new KeyValuePair<string, ManifestPropertyTypeInfo>("CONTEXTSENSITIVEHELPURL", new ManifestPropertyTypeInfo("CONTEXTSENSITIVEHELPURL", null, VersionCompatibility.Parse("3.0")))
		});
	}

	private static ImmutableHashSet<string> GetManifestPropertiesName()
	{
		return ImmutableHashSet.Create<string>("id", "name", "version", "publisher", "description", "brief", "capabilities", "privacyStatement", "EULA", "help", "url", "logo", "screenshots", "target", "showMyCode", "platform", "application", "test", "runtime", "idRanges", "idRanges_from", "idRanges_to", "idRange", "idRange_from", "idRange_to", "dependencies", "dependencies_appId", "dependencies_name", "dependencies_publisher", "dependencies_version", "features", "helpBaseUrl", "supportedLocales", "contextSensitiveHelpUrl");
	}

	private static ImmutableDictionary<string, ManifestPropertyValueTypeInfo> GetManifestPropertyValuesInfos()
	{
		return ImmutableDictionary.CreateRange(new KeyValuePair<string, ManifestPropertyValueTypeInfo>[0]);
	}
}
