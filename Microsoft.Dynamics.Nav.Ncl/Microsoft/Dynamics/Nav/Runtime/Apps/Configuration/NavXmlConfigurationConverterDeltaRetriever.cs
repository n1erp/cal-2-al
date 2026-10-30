using System.Collections.Generic;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.Configuration;

internal class NavXmlConfigurationConverterDeltaRetriever : INavXmlConverterDeltaRetriever
{
	private NavXmlConfigurationConversionOptions options;

	private INavV1PageConfiguration sourceRepository;

	public NavXmlConfigurationConverterDeltaRetriever(NavXmlConfigurationConversionOptions options, INavV1PageConfiguration source)
	{
		this.options = options;
		sourceRepository = source;
	}

	public IEnumerable<IMetadataRecord> GetDeltasToConvert()
	{
		return sourceRepository.LoadConfigurationDeltasForProfile(options.SourceProfileKey, options.ConfigurationId);
	}
}
