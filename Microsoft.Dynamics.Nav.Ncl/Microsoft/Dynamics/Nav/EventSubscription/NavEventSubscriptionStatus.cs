using System;

namespace Microsoft.Dynamics.Nav.EventSubscription;

[Flags]
public enum NavEventSubscriptionStatus
{
	Resolved = 1,
	Disabled = 2,
	ManualBinding = 4,
	ErrorOriginalApplicationObjectNotFound = 0x100,
	ErrorOriginalMethodNotFound = 0x200,
	ErrorParameterMismatch = 0x400,
	ErrorGlobalVariableNotFound = 0x800,
	ErrorFieldNotFound = 0x1000,
	ErrorOriginalApplicationObjectTypeNotSupported = 0x2000,
	ErrorGlobalVariableAccessNotAllowed = 0x4000,
	ErrorIncludeSenderNotAllowed = 0x8000,
	ErrorSenderParameterIsWrongType = 0x10000,
	ErrorOnBeforeActionEventDisabledOnRunObject = 0x20000,
	ErrorMask = 0x7FFFFF00
}
