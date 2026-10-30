using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public class ExtensionEmitValues
{
	public Guid AppId { get; }

	public Version AppVersion { get; }

	public ExtensionEmitValues(Guid appId, Version appVersion)
	{
		if (appVersion == null)
		{
			throw new ArgumentNullException("appVersion");
		}
		AppId = appId;
		AppVersion = appVersion;
	}
}
