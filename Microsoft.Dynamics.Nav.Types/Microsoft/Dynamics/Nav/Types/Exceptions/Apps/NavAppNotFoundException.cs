using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Apps;

[Serializable]
public class NavAppNotFoundException : NavAppException
{
	public static NavAppNotFoundException Create(string appName, string tenant)
	{
		return new NavAppNotFoundException(string.Format(CultureInfo.CurrentCulture, Lang.AppNotInstalledForTenant, appName, tenant));
	}

	public NavAppNotFoundException()
	{
	}

	public NavAppNotFoundException(string message)
		: base(message)
	{
	}

	public NavAppNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavAppNotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
