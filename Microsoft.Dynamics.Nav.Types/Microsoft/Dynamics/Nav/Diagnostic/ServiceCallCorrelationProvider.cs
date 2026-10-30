using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Diagnostic;

internal static class ServiceCallCorrelationProvider
{
	private static readonly Dictionary<CorrelationIdType, string> correlationNames = new Dictionary<CorrelationIdType, string>
	{
		[CorrelationIdType.ClientActivityId] = "ClientActivityId",
		[CorrelationIdType.ClientSessionId] = "ClientSessionId",
		[CorrelationIdType.ServerActivityId] = "ServerActivityId",
		[CorrelationIdType.ServerRequestId] = "ServerRequestId"
	};

	private const string HeaderNamespace = "Microsoft.Dynamics.Nav.Diagnostics";

	public static Guid GetCorrelationId(CorrelationIdType type)
	{
		OperationContext operationContext = TypesThreadLocalStorage.Current?.OperationContext ?? OperationContext.Current;
		if (operationContext == null)
		{
			return Guid.Empty;
		}
		int num = operationContext.IncomingMessageHeaders.FindHeader(correlationNames[type], "Microsoft.Dynamics.Nav.Diagnostics");
		if (num != -1)
		{
			return operationContext.IncomingMessageHeaders.GetHeader<Guid>(num);
		}
		return Guid.Empty;
	}
}
