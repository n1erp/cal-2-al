using System;

namespace Microsoft.Dynamics.Nav.EventSubscription;

[Flags]
public enum NavSubscriberParameterMatchInfo
{
	SubscriberIsByRef = 1,
	OriginalIsByRef = 2,
	ParameterIsGlobalVar = 0x10,
	ParameterIsSender = 0x20,
	ParameterMismatch = 0x80
}
