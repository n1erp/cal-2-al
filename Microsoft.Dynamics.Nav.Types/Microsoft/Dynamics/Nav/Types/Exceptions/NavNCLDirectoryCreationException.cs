using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLDirectoryCreationException : NavNCLException
{
	public NavNCLDirectoryCreationException()
	{
	}

	public NavNCLDirectoryCreationException(string message)
		: base(message)
	{
	}

	public NavNCLDirectoryCreationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLDirectoryCreationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLDirectoryCreationException Create(CultureInfo culture, string path, Exception innerException)
	{
		return new NavNCLDirectoryCreationException(string.Format(culture, Lang.UnableToCreateDirectory, path), innerException);
	}
}
