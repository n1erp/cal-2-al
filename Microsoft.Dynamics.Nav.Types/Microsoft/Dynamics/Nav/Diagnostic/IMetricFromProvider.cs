namespace Microsoft.Dynamics.Nav.Diagnostic;

public interface IMetricFromProvider
{
	void LogValue(long value, string[] dimensionValues);
}
