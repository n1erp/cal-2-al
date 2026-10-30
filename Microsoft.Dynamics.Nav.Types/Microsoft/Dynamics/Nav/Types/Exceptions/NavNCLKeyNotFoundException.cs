using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLKeyNotFoundException : NavNCLException
{
	public NavNCLKeyNotFoundException()
	{
	}

	public NavNCLKeyNotFoundException(string propertyKey)
		: base(string.Format(CultureInfo.CurrentCulture, Lang.DictionaryKeyNotFoundException, propertyKey))
	{
	}

	protected NavNCLKeyNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavNCLKeyNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
