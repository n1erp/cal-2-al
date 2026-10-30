using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.ServiceModel;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTenantAlreadyExistsException : NavException
{
	public const string TenantAlreadyExistsFaultCode = "TenantAlreadyExistsFaultCode";

	public NavTenantAlreadyExistsException()
	{
	}

	public NavTenantAlreadyExistsException(string message)
		: base(message)
	{
	}

	public NavTenantAlreadyExistsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTenantAlreadyExistsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static FaultException CreateFaultException(string reason)
	{
		FaultCode code = FaultCode.CreateSenderFaultCode(new FaultCode("TenantAlreadyExistsFaultCode"));
		return new FaultException(reason, code);
	}

	public static NavTenantAlreadyExistsException Create(string tenantId, string tenantDatabaseId)
	{
		return new NavTenantAlreadyExistsException(string.Format(CultureInfo.CurrentCulture, Lang.TenantAlreadyExistsInDatabase, tenantId, tenantDatabaseId));
	}
}
