using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Apps;

[Serializable]
public class NavNCLExtensionFieldNotFoundException : NavNCLFieldNotFoundException
{
	public NavNCLExtensionFieldNotFoundException()
	{
	}

	public NavNCLExtensionFieldNotFoundException(string message)
		: base(message)
	{
	}

	public NavNCLExtensionFieldNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLExtensionFieldNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLExtensionFieldNotFoundException Create(CultureInfo culture, int extensionId, int field, string table)
	{
		return new NavNCLExtensionFieldNotFoundException(string.Format(culture, Lang.NavAppFieldNotFound, field, extensionId, table));
	}
}
