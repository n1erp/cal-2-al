namespace Microsoft.Dynamics.Nav.Diagnostic;

public class NavMetric1Dimensions : NavMetric
{
	public NavMetric1Dimensions(string metricName, string dimensionName)
		: base(metricName, dimensionName)
	{
	}

	public void LogValue(long value, string dimensionValue)
	{
		base.LogValue(value, dimensionValue);
	}
}
