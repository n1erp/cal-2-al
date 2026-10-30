using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNotSupportedPageViewModeException : NavBaseException
{
	public NavNotSupportedPageViewModeException(string viewMode, int pageId, Exception innerException)
		: base(string.Format(CultureInfo.CurrentCulture, Lang.NavNotSupportedPageViewModeException, viewMode, pageId), innerException)
	{
	}

	public NavNotSupportedPageViewModeException()
	{
	}

	public NavNotSupportedPageViewModeException(string message)
		: base(message)
	{
	}

	public NavNotSupportedPageViewModeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNotSupportedPageViewModeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
