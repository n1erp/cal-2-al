using System.Collections.Generic;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavXmlPersonalizationConverterDeltaRetriever : INavXmlConverterDeltaRetriever
{
	private readonly INavV1PageConfiguration sourceRepository;

	private readonly NavXmlPersonalizationConversionOptions options;

	public NavXmlPersonalizationConverterDeltaRetriever(NavXmlPersonalizationConversionOptions options, INavV1PageConfiguration sourceRepository)
	{
		this.sourceRepository = sourceRepository;
		this.options = options;
	}

	public IEnumerable<IMetadataRecord> GetDeltasToConvert()
	{
		return sourceRepository.LoadPersonalizationDeltasForUser(options.UserSid, options.PersonalizationId);
	}
}
