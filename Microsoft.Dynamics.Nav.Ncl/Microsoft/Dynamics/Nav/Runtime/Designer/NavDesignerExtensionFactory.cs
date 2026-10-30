using System;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerExtensionFactory : INavDesignerExtensionFactory
{
	public NavDevelopmentDesignerExtension CreateDevelopmentDesignerExtension(AppId appId = null, Version appVersion = null)
	{
		return new NavDevelopmentDesignerExtension(new NavDesignerExtensionStorage(), new NavDesignerCompiler(), appId, appVersion);
	}

	public NavPersonalizationDesignerExtension CreatePersonalizationDesignerExtension()
	{
		return new NavPersonalizationDesignerExtension(new DatabaseNavPageCustomizationRepository(), new NavDesignerCompiler());
	}

	public NavConfigurationDesignerExtension CreateConfigurationDesignerExtension(NavProfileKey profileKey, ConfigurationDesignerMode mode = ConfigurationDesignerMode.Tenant)
	{
		return new NavConfigurationDesignerExtension(new DatabaseNavPageCustomizationRepository(), new NavDesignerCompiler(), profileKey, mode);
	}
}
