using System.Collections.Generic;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal interface INavXmlConverterDeltaRetriever
{
	IEnumerable<IMetadataRecord> GetDeltasToConvert();
}
