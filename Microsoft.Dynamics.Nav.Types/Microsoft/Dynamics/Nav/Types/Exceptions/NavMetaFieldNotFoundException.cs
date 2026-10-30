using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavMetaFieldNotFoundException : NavBuilderException
{
	public NavMetaFieldNotFoundException()
	{
	}

	public NavMetaFieldNotFoundException(string message)
		: base(message)
	{
	}

	public NavMetaFieldNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavMetaFieldNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavMetaFieldNotFoundException Create(CultureInfo culture, int field, int table)
	{
		return new NavMetaFieldNotFoundException(string.Format(culture, Lang._00095_50022, field, table));
	}
}
