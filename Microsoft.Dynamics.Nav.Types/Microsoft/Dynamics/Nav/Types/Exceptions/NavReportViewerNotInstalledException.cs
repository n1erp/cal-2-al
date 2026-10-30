using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavReportViewerNotInstalledException : NavBaseException
{
	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavReportViewerNotInstalledException()
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavReportViewerNotInstalledException(string message)
		: base(message)
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavReportViewerNotInstalledException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	public NavReportViewerNotInstalledException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavReportViewerNotInstalledException(Exception innerException)
		: base(string.Format(CultureInfo.CurrentCulture, Lang.ReportViewerNotInstalled, ProductNames.ProductNameShort), innerException)
	{
	}
}
