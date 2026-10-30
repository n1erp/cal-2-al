namespace Microsoft.Dynamics.Nav.Diagnostic;

public class NavMetric3Dimensions : NavMetric
{
	public NavMetric3Dimensions(string metricName, string dimension1Name, string dimension2Name, string dimension3Name)
		: base(metricName, dimension1Name, dimension2Name, dimension3Name)
	{
	}

	public void LogValue(long value, string dimension1Value, string dimension2Value, string dimension3Value)
	{
		base.LogValue(value, dimension1Value, dimension2Value, dimension3Value);
	}
}
