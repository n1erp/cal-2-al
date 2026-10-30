using System;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface INavXmlConverterMetadataProvider
{
	MasterPage GetMasterPage(PageType type);

	PageDefinition GetPageDefinition(int pageId);

	MasterPage GetMasterPageWithoutConfiguration(int pageId);

	MasterNavigation GetMasterNavigationFrame(Guid userSid);

	MasterNavigation GetMasterNavigationFrame(string profileId);
}
