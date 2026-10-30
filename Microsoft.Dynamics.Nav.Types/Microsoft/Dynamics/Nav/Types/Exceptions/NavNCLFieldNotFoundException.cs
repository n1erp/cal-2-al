using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLFieldNotFoundException : NavNCLException
{
	public NavNCLFieldNotFoundException()
	{
	}

	public NavNCLFieldNotFoundException(string message)
		: base(message)
	{
	}

	public NavNCLFieldNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLFieldNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLFieldNotFoundException Create(CultureInfo culture, string field, string table)
	{
		return new NavNCLFieldNotFoundException(string.Format(culture, Lang._00095_50022, field, table));
	}

	public static NavNCLFieldNotFoundException Create(CultureInfo culture, int field, string table)
	{
		return new NavNCLFieldNotFoundException(string.Format(culture, Lang._00095_50023, field, table));
	}
}
