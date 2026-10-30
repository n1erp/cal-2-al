using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestMethodNotSupportedInPageTestException : NavTestBaseException
{
	public NavTestMethodNotSupportedInPageTestException()
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestMethodNotSupportedInPageTestException(string message)
		: base(message)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestMethodNotSupportedInPageTestException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestMethodNotSupportedInPageTestException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavTestMethodNotSupportedInPageTestException Create(CultureInfo culture, string methodName)
	{
		return new NavTestMethodNotSupportedInPageTestException(string.Format(culture, Lang.TestPageMethodNotSupported, methodName));
	}
}
