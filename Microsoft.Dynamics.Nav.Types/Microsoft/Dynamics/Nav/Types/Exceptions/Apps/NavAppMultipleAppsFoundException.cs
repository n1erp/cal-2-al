using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Apps;

[Serializable]
public class NavAppMultipleAppsFoundException : NavAppException
{
	public static NavAppMultipleAppsFoundException Create()
	{
		return new NavAppMultipleAppsFoundException(string.Format(CultureInfo.CurrentCulture, Lang.MultiplePublishedAppsMatch_WhereUniquenessIsNecessary));
	}

	public NavAppMultipleAppsFoundException()
	{
	}

	public NavAppMultipleAppsFoundException(string message)
		: base(message)
	{
	}

	public NavAppMultipleAppsFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavAppMultipleAppsFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
