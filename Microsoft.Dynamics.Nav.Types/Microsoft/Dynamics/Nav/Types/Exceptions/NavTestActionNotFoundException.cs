using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestActionNotFoundException : NavTestBaseException
{
	public NavTestActionNotFoundException()
	{
	}

	public NavTestActionNotFoundException(string message)
		: base(message)
	{
	}

	public NavTestActionNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestActionNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavTestActionNotFoundException Create(CultureInfo culture, int actionId)
	{
		return new NavTestActionNotFoundException(string.Format(culture, Lang.TestActionNotFound, actionId));
	}

	public static NavTestActionNotFoundException Create(CultureInfo culture, FormResult formResult)
	{
		return new NavTestActionNotFoundException(string.Format(culture, Lang.TestBuiltInActionNotFound, formResult));
	}
}
