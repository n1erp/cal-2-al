using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class CSharpDiagnosticFilter
{
	private static readonly ErrorCode[] s_alinkWarnings = new ErrorCode[3]
	{
		ErrorCode.WRN_ConflictingMachineAssembly,
		ErrorCode.WRN_RefCultureMismatch,
		ErrorCode.WRN_InvalidVersionFormat
	};

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
		ReportDiagnostic reportAction = ((!s_alinkWarnings.Contains((ErrorCode)d.Code) || !specificDiagnosticOptions.Keys.Contains(MessageProvider.Instance.GetIdForErrorCode(1607))) ? GetDiagnosticReport(d.Severity, d.IsEnabledByDefault, d.Id, d.WarningLevel, d.Location, d.Category, warningLevelOption, generalDiagnosticOption, specificDiagnosticOptions, out var hasPragmaSuppression) : GetDiagnosticReport(ErrorFacts.GetSeverity(ErrorCode.WRN_ALinkWarn), d.IsEnabledByDefault, MessageProvider.Instance.GetIdForErrorCode(1607), ErrorFacts.GetWarningLevel(ErrorCode.WRN_ALinkWarn), d.Location, d.Category, warningLevelOption, generalDiagnosticOption, specificDiagnosticOptions, out hasPragmaSuppression));
		if (hasPragmaSuppression)
		{
			d = d.WithIsSuppressed(isSuppressed: true);
		}
		return d.WithReportDiagnostic(reportAction);
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
		if (location != null && location.SourceTree != null && location.SourceTree.GetPragmaDirectiveWarningState(id, location.SourceSpan.Start) == ReportDiagnostic.Suppress)
		{
			hasPragmaSuppression = true;
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
