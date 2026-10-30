using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class DiagnosticFilter
{
	public static Diagnostic Filter(Diagnostic d, int warningLevelOption, ReportDiagnostic generalDiagnosticOption, IDictionary<string, ReportDiagnostic> specificDiagnosticOptions)
	{
		if (d == null)
		{
			return d;
		}
		if (d.IsNotConfigurable())
		{
			if (d.IsEnabledByDefault)
			{
				return d;
			}
			return null;
		}
		if (d.Severity == (DiagnosticSeverity)(-2))
		{
			return null;
		}
		ReportDiagnostic diagnosticReport = GetDiagnosticReport(d.Severity, d.IsEnabledByDefault, d.Id, d.WarningLevel, d.Location, d.Category, warningLevelOption, generalDiagnosticOption, specificDiagnosticOptions, out var hasPragmaSuppression);
		if (hasPragmaSuppression)
		{
			d = d.WithIsSuppressed(isSuppressed: true);
		}
		return d.WithReportDiagnostic(diagnosticReport);
	}

	internal static ReportDiagnostic GetDiagnosticReport(DiagnosticSeverity severity, bool isEnabledByDefault, string id, int diagnosticWarningLevel, Location location, string category, int warningLevelOption, ReportDiagnostic generalDiagnosticOption, IDictionary<string, ReportDiagnostic> specificDiagnosticOptions, out bool hasPragmaSuppression)
	{
		hasPragmaSuppression = false;
		ReportDiagnostic value = ReportDiagnostic.Default;
		bool flag = specificDiagnosticOptions.TryGetValue(id, out value);
		if (!flag)
		{
			value = ((!isEnabledByDefault) ? ReportDiagnostic.Suppress : ReportDiagnostic.Default);
		}
		if (diagnosticWarningLevel > warningLevelOption || value == ReportDiagnostic.Suppress)
		{
			return ReportDiagnostic.Suppress;
		}
		if (value == ReportDiagnostic.Default)
		{
			switch (generalDiagnosticOption)
			{
			case ReportDiagnostic.Error:
				if (severity == DiagnosticSeverity.Warning && !flag && value == ReportDiagnostic.Default)
				{
					return ReportDiagnostic.Error;
				}
				break;
			case ReportDiagnostic.Suppress:
				if (severity == DiagnosticSeverity.Warning || severity == DiagnosticSeverity.Info)
				{
					return ReportDiagnostic.Suppress;
				}
				break;
			}
		}
		return value;
	}
}
