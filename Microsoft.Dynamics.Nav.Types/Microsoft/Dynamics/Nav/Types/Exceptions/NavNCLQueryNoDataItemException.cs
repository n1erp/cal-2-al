using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavNCLQueryNoDataItemException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLQueryNoDataItemException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLQueryNoDataItemException(string message)
		: base(message)
	{
	}

	public NavNCLQueryNoDataItemException(string reportName, int tableId, string tableName)
		: base(CreateMessage(reportName, tableId, tableName))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLQueryNoDataItemException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavNCLQueryNoDataItemException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string reportName, int tableId, string tableName)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang._00095_50072, reportName, tableId, tableName);
	}
}
