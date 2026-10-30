using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTestPart : NavTestPageBase
{
	internal NavTestPart(ITreeObject parent, ITestPage testPage)
		: base(parent, new ApplicationObjectId(ObjectType.Page, 0), testPage)
	{
	}

	protected override PageDefinition LoadMetadata()
	{
		return Tree.Session.MetadataProvider.GetPageDefinition(TestPage.PageId);
	}
}
