using System.IO.Compression;
using Microsoft.Dynamics.Nav.Runtime.Apps.Configuration;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavXmlConverterFactory
{
	public static INavXmlConverter CreateForPersonalization(NavXmlPersonalizationConversionOptions options, INavXmlConverterMetadataProvider provider, INavV1PageConfiguration source, DatabaseNavPageCustomizationRepository destination)
	{
		return new NavXmlConverter(new NavXmlPersonalizationConverter(provider, new NavXmlDeltaPersonalizationConverterAdapter()), new NavXmlPersonalizationConverterDeltaRetriever(options, source), new NavXmlPersonalizationConverterStorage(destination, options));
	}

	public static INavXmlConverter CreateForConfiguration(NavXmlConfigurationConversionOptions options, INavXmlConverterMetadataProvider provider, INavV1PageConfiguration source, DatabaseNavPageCustomizationRepository destination, bool downloadResult, ZipArchive archive)
	{
		return new NavXmlConverter(new NavXmlConfigurationConverter(provider, new NavXmlDeltaPersonalizationConverterAdapter()), new NavXmlConfigurationConverterDeltaRetriever(options, source), new NavXmlConfigurationConverterStorage(destination, options, downloadResult, archive));
	}
}
