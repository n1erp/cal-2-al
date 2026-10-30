using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AppInsightAzureLicenseUsageLogger : ILogger
{
	public const string InstrumentationKey = "e5aa87d8-42b0-43cd-a22f-3c4f0025910f";

	private TelemetryClient telemetryClient;

	public AppInsightAzureLicenseUsageLogger()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		telemetryClient = new TelemetryClient();
		telemetryClient.InstrumentationKey = "e5aa87d8-42b0-43cd-a22f-3c4f0025910f";
	}

	public void Log(Dictionary<string, string> payload)
	{
		if (payload == null)
		{
			throw new ArgumentNullException("payload");
		}
		telemetryClient.TrackTrace("System information", (SeverityLevel)1, (IDictionary<string, string>)payload);
	}

	public void Flush()
	{
		telemetryClient.Flush();
	}
}
