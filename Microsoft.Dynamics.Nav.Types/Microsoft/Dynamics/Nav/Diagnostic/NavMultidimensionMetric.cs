namespace Microsoft.Dynamics.Nav.Diagnostic;

public class NavMultidimensionMetric : NavMetric
{
	public NavMultidimensionMetric(string metricName, params string[] dimensionNames)
		: base(metricName, dimensionNames)
	{
	}

	public void LogValues(long value, string[] dimensionValues)
	{
		LogValue(value, dimensionValues);
	}
}
