namespace Microsoft.Dynamics.Nav.Diagnostic;

public class NavMetric2Dimensions : NavMetric
{
	public NavMetric2Dimensions(string metricName, string dimension1Name, string dimension2Name)
		: base(metricName, dimension1Name, dimension2Name)
	{
	}

	public void LogValue(long value, string dimension1Value, string dimension2Value)
	{
		base.LogValue(value, dimension1Value, dimension2Value);
	}
}
