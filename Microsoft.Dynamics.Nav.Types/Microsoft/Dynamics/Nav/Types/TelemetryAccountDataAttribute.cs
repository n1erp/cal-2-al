using System;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class TelemetryAccountDataAttribute : Attribute, ITelemetryPrivacyClassification
{
	public PrivacyClassification Classification => PrivacyClassification.AccountData;
}
