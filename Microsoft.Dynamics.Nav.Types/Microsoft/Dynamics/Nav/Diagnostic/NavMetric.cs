using System;

namespace Microsoft.Dynamics.Nav.Diagnostic;

public abstract class NavMetric
{
	private IMetricFromProvider ifxMetric;

	private readonly string metricName;

	private static readonly object createLock = new object();

	private readonly string[] dimensionNames;

	protected NavMetric(string metricName, params string[] dimensionNames)
	{
		this.metricName = metricName ?? throw new ArgumentNullException("metricName");
		this.dimensionNames = dimensionNames ?? new string[0];
	}

	private void InitializeMetric()
	{
		try
		{
			if (NavDiagnostics.IfxTelemetryProvider != null)
			{
				ifxMetric = NavDiagnostics.IfxTelemetryProvider.CreateMetric(metricName, dimensionNames);
			}
		}
		catch (Exception exception)
		{
			NavDiagnostics.GetMostSpecificInstance?.SendExceptionTag("00001FG", Category.Telemetry, exception, Verbosity.Error, DiagnosticParameter.SystemMetadata(FormattableString.Invariant($"Cannot create metric {metricName}.")));
		}
	}

	public void LogValue(long value, params string[] dimensionValues)
	{
		_ = dimensionValues?.Length;
		if (ifxMetric == null && NavDiagnostics.IfxTelemetryProvider != null)
		{
			lock (createLock)
			{
				if (ifxMetric == null)
				{
					InitializeMetric();
				}
			}
		}
		try
		{
			ifxMetric?.LogValue(value, dimensionValues);
		}
		catch (Exception exception)
		{
			NavDiagnostics.GetMostSpecificInstance?.SendExceptionTag("00001FH", Category.Telemetry, exception, Verbosity.Error, DiagnosticParameter.SystemMetadata(FormattableString.Invariant($"Cannot log value {value} for metric {metricName}.")));
		}
	}
}
