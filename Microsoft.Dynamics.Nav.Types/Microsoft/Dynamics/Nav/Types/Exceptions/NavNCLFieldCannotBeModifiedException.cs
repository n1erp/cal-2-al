using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLFieldCannotBeModifiedException : NavNCLException
{
	public NavNCLFieldCannotBeModifiedException()
	{
	}

	public NavNCLFieldCannotBeModifiedException(string message)
		: base(message)
	{
	}

	public NavNCLFieldCannotBeModifiedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLFieldCannotBeModifiedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLFieldCannotBeModifiedException Create(CultureInfo culture, string tableName, string fieldName)
	{
		return new NavNCLFieldCannotBeModifiedException(string.Format(culture, Lang.FieldCannotBeModified, tableName, fieldName));
	}
}
