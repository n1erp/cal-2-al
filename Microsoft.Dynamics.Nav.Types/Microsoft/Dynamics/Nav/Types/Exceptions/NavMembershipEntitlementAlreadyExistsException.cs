using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavMembershipEntitlementAlreadyExistsException : NavException
{
	[Obsolete("Do not use this constructor. Use a factory method instead")]
	public NavMembershipEntitlementAlreadyExistsException()
	{
	}

	public NavMembershipEntitlementAlreadyExistsException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use a factory method instead")]
	public NavMembershipEntitlementAlreadyExistsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[Obsolete("Do not use this constructor. Use a factory method instead")]
	protected NavMembershipEntitlementAlreadyExistsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavMembershipEntitlementAlreadyExistsException Create(string type, string id, string entitlementSetId)
	{
		return new NavMembershipEntitlementAlreadyExistsException(string.Format(CultureInfo.CurrentCulture, Lang.MembershipEntitlementAlreadyExistsError, type, id, entitlementSetId));
	}
}
