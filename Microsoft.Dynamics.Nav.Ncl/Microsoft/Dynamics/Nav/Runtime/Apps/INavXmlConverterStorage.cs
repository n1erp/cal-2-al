using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal interface INavXmlConverterStorage
{
	void SaveConvertedDeltas(IEnumerable<NavXmlDeltaPageConversionResult> convertedPages);
}
