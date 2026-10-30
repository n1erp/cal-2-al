using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLReportSourceExpressionException : NavNCLException
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLReportSourceExpressionException()
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLReportSourceExpressionException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLReportSourceExpressionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLReportSourceExpressionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLReportSourceExpressionException CreateMessage(string metaFieldName, string errorMessage)
	{
		return new NavNCLReportSourceExpressionException(string.Format(CultureInfo.CurrentCulture, Lang.ReportInvalidSourceExprException, metaFieldName, errorMessage));
	}
}
