using System.Collections.Generic;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface INavMetadataConverter
{
	NavXmlDeltaConversionResult Convert(IEnumerable<IMetadataRecord> deltas);
}
