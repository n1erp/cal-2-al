using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Diagnostic;

public sealed class IfxTimingScope : ITraceScope, IDisposable
{
	private readonly NavMetric metric;

	private string[] dimensionValues;

	private readonly Stopwatch executionTimer;

	public IfxTimingScope(NavMetric metric, params string[] dimensionValues)
	{
		this.metric = metric;
		this.dimensionValues = dimensionValues;
		executionTimer = new Stopwatch();
		executionTimer.Start();
	}

	public void SetDimensionValues(params string[] newDimensionValues)
	{
		dimensionValues = newDimensionValues;
	}

	public void Dispose()
	{
		executionTimer.Stop();
		metric.LogValue(executionTimer.ElapsedMilliseconds, dimensionValues);
	}

	public void SetResult(NavDiagnosticsScopeResult result)
	{
		throw new NotImplementedException();
	}

	public void SetResult(Exception exception)
	{
		throw new NotImplementedException();
	}
}
