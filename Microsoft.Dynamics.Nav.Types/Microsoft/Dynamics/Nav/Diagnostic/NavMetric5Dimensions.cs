namespace Microsoft.Dynamics.Nav.Diagnostic;

public class NavMetric5Dimensions : NavMetric
{
	public NavMetric5Dimensions(string metricName, string dimension1Name, string dimension2Name, string dimension3Name, string dimension4Name, string dimension5Name)
		: base(metricName, dimension1Name, dimension2Name, dimension3Name, dimension4Name, dimension5Name)
	{
	}

	public void LogValue(long value, string dimension1Value, string dimension2Value, string dimension3Value, string dimension4Value, string dimension5Value)
	{
		base.LogValue(value, dimension1Value, dimension2Value, dimension3Value, dimension4Value, dimension5Value);
	}
}
