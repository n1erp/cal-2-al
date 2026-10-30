namespace Microsoft.Dynamics.Nav.Diagnostic;

public class NavMetric0Dimensions : NavMetric
{
	public NavMetric0Dimensions(string metricName)
		: base(metricName, (string[])null)
	{
	}

	public void LogValue(long value)
	{
		LogValue(value, (string[])null);
	}
}
