using System;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.Configuration;

public class NavXmlConfigurationConversionOptions
{
	public string ConfigurationId { get; }

	public NavProfileKey SourceProfileKey { get; }

	public NavProfileKey DestinationProfileKey { get; }

	public NavXmlConfigurationConversionOptions(string profileId, bool overwriteExisting = false)
		: this(new NavProfileKey(profileId), null, overwriteExisting)
	{
	}

	public NavXmlConfigurationConversionOptions(string profileId, string personalizationId, bool overwriteExisting = false)
		: this(new NavProfileKey(profileId), personalizationId, overwriteExisting)
	{
	}

	public NavXmlConfigurationConversionOptions(NavProfileKey profileKey, string personalizationId = null, bool overwriteExisting = false)
	{
		SourceProfileKey = profileKey;
		if (SourceProfileKey == null || !SourceProfileKey.ProfileExists())
		{
			throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.ConversionSourceProfileNullOrNotExist, profileKey.ProfileId));
		}
		DestinationProfileKey = new NavProfileKey(profileKey.ProfileId, null, ProfileScope.Tenant);
		if (DestinationProfileKey.ProfileExists() && !overwriteExisting)
		{
			throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.ConversionDestinationProfileExist, profileKey.ProfileId));
		}
		ConfigurationId = personalizationId;
	}
}
