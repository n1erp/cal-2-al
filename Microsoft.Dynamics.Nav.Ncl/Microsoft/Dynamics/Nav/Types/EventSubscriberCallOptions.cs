using System;

namespace Microsoft.Dynamics.Nav.Types;

[Flags]
public enum EventSubscriberCallOptions
{
	SkipOnMissingLicense = 1,
	SkipOnMissingPermission = 2
}
