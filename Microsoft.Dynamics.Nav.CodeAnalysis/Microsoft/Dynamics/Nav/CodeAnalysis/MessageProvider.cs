using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class MessageProvider : CommonMessageProvider
{
	public static readonly MessageProvider Instance = new MessageProvider();

	public override string CodePrefix => "AL";

	public override Type ErrorCodeType => typeof(ErrorCode);

	private MessageProvider()
	{
	}

	public override DiagnosticSeverity GetSeverity(int code)
	{
		return ErrorFacts.GetSeverity((ErrorCode)code);
	}

	public override string LoadMessage(int code, CultureInfo language)
	{
		return ErrorFacts.GetMessage((ErrorCode)code, language);
	}

	public override LocalizableString GetMessageFormat(int code)
	{
		return ErrorFacts.GetMessageFormat((ErrorCode)code);
	}

	public override LocalizableString GetDescription(int code)
	{
		return ErrorFacts.GetDescription((ErrorCode)code);
	}

	public override LocalizableString GetTitle(int code)
	{
		return ErrorFacts.GetTitle((ErrorCode)code);
	}

	public override string GetHelpLink(int code)
	{
		return ErrorFacts.GetHelpLink((ErrorCode)code);
	}

	public override string GetCategory(int code)
	{
		return ErrorFacts.GetCategory((ErrorCode)code);
	}

	public override ReportDiagnostic GetDiagnosticReport(DiagnosticInfo diagnosticInfo, CompilationOptions options)
	{
		bool hasPragmaSuppression;
		return DiagnosticFilter.GetDiagnosticReport(diagnosticInfo.Severity, isEnabledByDefault: true, diagnosticInfo.MessageIdentifier, diagnosticInfo.WarningLevel, Location.None, diagnosticInfo.Category, options.WarningLevel, options.GeneralDiagnosticOption, options.SpecificDiagnosticOptions, out hasPragmaSuppression);
	}

	public override string GetMessagePrefix(string id, DiagnosticSeverity severity, bool isWarningAsError, CultureInfo culture)
	{
		return string.Format(culture, "{0} {1}", ((severity == DiagnosticSeverity.Error) | isWarningAsError) ? "error" : "warning", id);
	}

	public override int GetWarningLevel(int code)
	{
		return ErrorFacts.GetWarningLevel((ErrorCode)code);
	}

	public override Diagnostic CreateDiagnostic(int code, Location location, params object[] args)
	{
		return new NavDiagnostic(new DiagnosticInfo(this, code, args), location);
	}
}
