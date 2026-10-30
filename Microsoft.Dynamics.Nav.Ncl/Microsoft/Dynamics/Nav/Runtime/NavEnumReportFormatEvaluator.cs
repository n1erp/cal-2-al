using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavEnumReportFormatEvaluator : NavEnumTypeEvaluator<NavReportFormat, NavEnumReportFormat>
{
	private static readonly NavEnumReportFormatEvaluator instance = new NavEnumReportFormatEvaluator();

	internal static NavEnumReportFormatEvaluator Instance => instance;

	private NavEnumReportFormatEvaluator()
		: base((Func<NavReportFormat, NavEnumReportFormat>)((NavReportFormat objectType) => new NavEnumReportFormat(objectType)))
	{
	}

	protected override NavEnumReportFormat GetDefaultValue(INavValueMetadata metadata)
	{
		return NavEnumReportFormat.Default;
	}
}
