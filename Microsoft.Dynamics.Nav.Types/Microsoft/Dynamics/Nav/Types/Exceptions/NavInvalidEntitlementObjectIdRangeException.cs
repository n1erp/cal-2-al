using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavInvalidEntitlementObjectIdRangeException : NavException
{
	[Obsolete("Do not use this constructor. Use a factory method instead")]
	public NavInvalidEntitlementObjectIdRangeException()
	{
	}

	public NavInvalidEntitlementObjectIdRangeException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use a factory method instead")]
	public NavInvalidEntitlementObjectIdRangeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[Obsolete("Do not use this constructor. Use a factory method instead")]
	protected NavInvalidEntitlementObjectIdRangeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavInvalidEntitlementObjectIdRangeException Create(string setId, string type, int start, int end)
	{
		return new NavInvalidEntitlementObjectIdRangeException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidEntitlementObjectIdRange, setId, type, start, end));
	}
}
