using System;

namespace Microsoft.Dynamics.Nav.Diagnostic;

public interface ITelemetryProvider
{
	ITraceScope CreateScope(Guid clientActivityId, string name);

	IMetricFromProvider CreateMetric(string metricName, string[] dimensionNames);

	void SendTrace(string tag, Verbosity verbosity, string message);
}
