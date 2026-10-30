using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Microsoft.Dynamics.Nav.Types.Channels;

internal class ClientMessageCorrelationInspector : IClientMessageInspector
{
	private const string HeaderClientSessionIdName = "ClientSessionId";

	private const string HeaderClientActivityIdName = "ClientActivityId";

	private const string HeaderNamespace = "Microsoft.Dynamics.Nav.Diagnostics";

	private Func<Guid> getClientSessionId;

	private Func<Guid> getClientActivityId;

	private Action<Guid, string> logClientRequestCallback;

	public ClientMessageCorrelationInspector(Func<Guid> getClientSessionId, Func<Guid> getClientActivityId, Action<Guid, string> logClientRequestCallback = null)
	{
		this.getClientSessionId = getClientSessionId;
		this.getClientActivityId = getClientActivityId;
		this.logClientRequestCallback = logClientRequestCallback;
	}

	public void AfterReceiveReply(ref Message reply, object correlationState)
	{
	}

	public object BeforeSendRequest(ref Message request, IClientChannel channel)
	{
		Guid guid = getClientSessionId?.Invoke() ?? Guid.NewGuid();
		AddHeader(request, "ClientSessionId", guid.ToString());
		Guid guid2 = getClientActivityId?.Invoke() ?? Guid.NewGuid();
		if (guid2 == Guid.Empty)
		{
			guid2 = Guid.NewGuid();
			if (logClientRequestCallback != null)
			{
				string arg = request.Headers.Action.Split('/').Last() ?? "unknown operation";
				logClientRequestCallback(guid2, arg);
			}
		}
		AddHeader(request, "ClientActivityId", guid2.ToString());
		return null;
	}

	private static void AddHeader(Message request, string headerName, string value)
	{
		if (value != null && request.Headers.FindHeader(headerName, "Microsoft.Dynamics.Nav.Diagnostics") == -1)
		{
			MessageHeader header = MessageHeader.CreateHeader(headerName, "Microsoft.Dynamics.Nav.Diagnostics", value);
			request.Headers.Add(header);
		}
	}
}
