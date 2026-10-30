using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavXmlPersonalizationConversionOptions
{
	public Guid UserSid { get; }

	public string PersonalizationId { get; }

	public NavXmlPersonalizationConversionOptions(Guid userSid)
		: this(userSid, null)
	{
	}

	public NavXmlPersonalizationConversionOptions(Guid userSid, string personalizationId)
	{
		UserSid = userSid;
		PersonalizationId = personalizationId;
	}
}
