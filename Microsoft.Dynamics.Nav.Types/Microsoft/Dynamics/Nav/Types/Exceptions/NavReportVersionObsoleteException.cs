using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavReportVersionObsoleteException : NavException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavReportVersionObsoleteException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavReportVersionObsoleteException(string message)
		: base(message)
	{
	}

	public NavReportVersionObsoleteException(int reportId)
		: base(CreateMessage(reportId))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavReportVersionObsoleteException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavReportVersionObsoleteException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(int reportId)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.ReportVersionObsolete, reportId);
	}
}
