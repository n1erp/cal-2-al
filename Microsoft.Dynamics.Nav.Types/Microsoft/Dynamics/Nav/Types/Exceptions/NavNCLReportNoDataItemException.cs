using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavNCLReportNoDataItemException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLReportNoDataItemException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLReportNoDataItemException(string message)
		: base(message)
	{
	}

	public NavNCLReportNoDataItemException(string reportName, int tableId, string tableName)
		: base(CreateMessage(reportName, tableId, tableName))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLReportNoDataItemException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavNCLReportNoDataItemException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string reportName, int tableId, string tableName)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang._00095_50071, reportName, tableId, tableName);
	}
}
