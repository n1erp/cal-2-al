using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestPageInvokedWithoutHandlerException : NavTestBaseException
{
	public NavTestPageInvokedWithoutHandlerException()
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestPageInvokedWithoutHandlerException(string message)
		: base(message)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestPageInvokedWithoutHandlerException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestPageInvokedWithoutHandlerException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavTestPageInvokedWithoutHandlerException Create(CultureInfo culture, int pageId)
	{
		return new NavTestPageInvokedWithoutHandlerException(string.Format(culture, Lang.TestPageInvokedWithoutHandler, pageId));
	}
}
