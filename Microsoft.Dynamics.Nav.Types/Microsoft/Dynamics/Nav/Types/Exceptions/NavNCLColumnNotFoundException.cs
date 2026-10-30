using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLColumnNotFoundException : NavNCLException
{
	public NavNCLColumnNotFoundException()
	{
	}

	public NavNCLColumnNotFoundException(string message)
		: base(message)
	{
	}

	public NavNCLColumnNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLColumnNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLColumnNotFoundException Create(CultureInfo culture, int field)
	{
		return new NavNCLColumnNotFoundException(string.Format(culture, Lang.QueryColumnCouldNotBeFound, field));
	}

	public static NavNCLColumnNotFoundException Create(CultureInfo culture, string columnName)
	{
		return new NavNCLColumnNotFoundException(string.Format(culture, Lang.QueryColumnCouldNotBeFound, columnName));
	}
}
