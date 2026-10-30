using Microsoft.Dynamics.Nav.Apps.Runtime;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class SpecificNavAppObjectMetadataSummaryPair
{
	public NavAppRuntimeMetadata RuntimeMetadata { get; private set; }

	public NavAppObjectMetadataSummary Summary { get; private set; }

	public SpecificNavAppObjectMetadataSummaryPair(NavAppRuntimeMetadata runtimeMetadata, NavAppObjectMetadataSummary summary)
	{
		RuntimeMetadata = runtimeMetadata;
		Summary = summary;
	}
}
