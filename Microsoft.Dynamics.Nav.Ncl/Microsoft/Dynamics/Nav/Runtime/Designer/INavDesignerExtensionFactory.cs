using System;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal interface INavDesignerExtensionFactory
{
	NavDevelopmentDesignerExtension CreateDevelopmentDesignerExtension(AppId appId = null, Version appVersion = null);

	NavPersonalizationDesignerExtension CreatePersonalizationDesignerExtension();

	NavConfigurationDesignerExtension CreateConfigurationDesignerExtension(NavProfileKey profileKey, ConfigurationDesignerMode mode = ConfigurationDesignerMode.Tenant);
}
