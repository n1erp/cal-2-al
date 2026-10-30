using System;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavXmlConverterMetadataProviderAdapter : INavXmlConverterMetadataProvider
{
	private readonly MetadataProvider provider;

	public NavXmlConverterMetadataProviderAdapter(MetadataProvider provider)
	{
		this.provider = provider ?? throw new ArgumentNullException("provider");
	}

	public MasterPage GetMasterPage(PageType type)
	{
		return provider.GetMasterPage(type);
	}

	public PageDefinition GetPageDefinition(int pageId)
	{
		return provider.GetPageDefinition(pageId, 0);
	}

	public MasterPage GetMasterPageWithoutConfiguration(int pageId)
	{
		return provider.GetMasterPageWithoutConfiguration(pageId);
	}

	public MasterNavigation GetMasterNavigationFrame(Guid userSid)
	{
		NavProfileKey userProfile = NavConfigurationProfile.GetUserProfile(userSid);
		return provider.GetNavigationFrame(applyPersonalization: false, userProfile);
	}

	public MasterNavigation GetMasterNavigationFrame(string profileId)
	{
		return provider.GetNavigationFrame(applyPersonalization: false, new NavProfileKey(profileId));
	}
}
