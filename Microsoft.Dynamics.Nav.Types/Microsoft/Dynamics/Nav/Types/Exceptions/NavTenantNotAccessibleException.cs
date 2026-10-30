using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.ServiceModel;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTenantNotAccessibleException : NavException
{
	public const string TenantNotAccessibleFaultCode = "TenantNotAccessibleFaultCode";

	public NavTenantNotAccessibleException()
	{
	}

	public NavTenantNotAccessibleException(string message)
		: base(message)
	{
	}

	public NavTenantNotAccessibleException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTenantNotAccessibleException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static FaultException CreateFaultException(string reason)
	{
		FaultCode code = FaultCode.CreateSenderFaultCode(new FaultCode("TenantNotAccessibleFaultCode"));
		return new FaultException(reason, code);
	}

	public static NavTenantNotAccessibleException Create(string tenantId)
	{
		return new NavTenantNotAccessibleException(string.Format(CultureInfo.CurrentCulture, Lang.TenantNotAccessible, tenantId))
		{
			NavTenantId = tenantId
		};
	}
}
