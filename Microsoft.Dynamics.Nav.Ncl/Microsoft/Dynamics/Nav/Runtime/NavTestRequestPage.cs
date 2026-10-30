using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTestRequestPage : NavTestPage
{
	internal NavTestRequestPage(ITreeObject parent, ApplicationObjectId reportUnderTestId, ITestPage testPage)
		: base(parent, reportUnderTestId, testPage)
	{
	}

	protected override PageDefinition LoadMetadata()
	{
		return Tree.Session.MetadataProvider.GetReportMetadata(ObjectId.ObjectNumber).RequestPageDefinition;
	}
}
