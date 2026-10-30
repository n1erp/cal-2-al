using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal abstract class CommonMessageProvider
{
	public abstract string CodePrefix { get; }

	public abstract Type ErrorCodeType { get; }

	public abstract DiagnosticSeverity GetSeverity(int code);

	public abstract string LoadMessage(int code, CultureInfo language);

	public abstract LocalizableString GetTitle(int code);

	public abstract LocalizableString GetDescription(int code);

	public abstract LocalizableString GetMessageFormat(int code);

	public abstract string GetHelpLink(int code);

	public abstract string GetCategory(int code);

	public abstract int GetWarningLevel(int code);

	public Diagnostic CreateDiagnostic(int code, Location location)
	{
		return CreateDiagnostic(code, location, SpecializedCollections.EmptyObjects);
	}

	public abstract Diagnostic CreateDiagnostic(int code, Location location, params object[] args);

	public abstract string GetMessagePrefix(string id, DiagnosticSeverity severity, bool isWarningAsError, CultureInfo culture);

	public string GetIdForErrorCode(int errorCode)
	{
		return CodePrefix + errorCode.ToString("0000", CultureInfo.CurrentCulture);
	}

	public abstract ReportDiagnostic GetDiagnosticReport(DiagnosticInfo diagnosticInfo, CompilationOptions Options);

	public DiagnosticInfo FilterDiagnosticInfo(DiagnosticInfo diagnosticInfo, CompilationOptions options)
	{
		return GetDiagnosticReport(diagnosticInfo, options) switch
		{
			ReportDiagnostic.Error => diagnosticInfo.GetInstanceWithSeverity(DiagnosticSeverity.Error), 
			ReportDiagnostic.Warn => diagnosticInfo.GetInstanceWithSeverity(DiagnosticSeverity.Warning), 
			ReportDiagnostic.Info => diagnosticInfo.GetInstanceWithSeverity(DiagnosticSeverity.Info), 
			ReportDiagnostic.Hidden => diagnosticInfo.GetInstanceWithSeverity(DiagnosticSeverity.Hidden), 
			ReportDiagnostic.Suppress => null, 
			_ => diagnosticInfo, 
		};
	}
}
