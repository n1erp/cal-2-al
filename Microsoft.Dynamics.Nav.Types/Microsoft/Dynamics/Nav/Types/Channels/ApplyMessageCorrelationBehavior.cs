using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.Dynamics.Nav.Types.Channels;

public class ApplyMessageCorrelationBehavior : IEndpointBehavior
{
	private ClientMessageCorrelationInspector inspector;

	public ApplyMessageCorrelationBehavior(Func<Guid> getClientSessionId, Func<Guid> getClientActivityId, Action<Guid, string> logClientRequestCallback = null)
	{
		inspector = new ClientMessageCorrelationInspector(getClientSessionId, getClientActivityId, logClientRequestCallback);
	}

	public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
	{
	}

	public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
	{
		clientRuntime.MessageInspectors.Add(inspector);
	}

	public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
	{
	}

	public void Validate(ServiceEndpoint endpoint)
	{
	}
}
