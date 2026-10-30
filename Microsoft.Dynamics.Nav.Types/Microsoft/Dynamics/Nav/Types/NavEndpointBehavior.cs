using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.Dynamics.Nav.Types;

public class NavEndpointBehavior : IEndpointBehavior
{
	private int maxFaultSize;

	public NavEndpointBehavior(long maxFaultSize)
	{
		if (maxFaultSize < 0 || maxFaultSize > int.MaxValue)
		{
			this.maxFaultSize = int.MaxValue;
		}
		else
		{
			this.maxFaultSize = (int)maxFaultSize;
		}
	}

	public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
	{
	}

	public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
	{
		if (clientRuntime != null)
		{
			clientRuntime.MaxFaultSize = maxFaultSize;
		}
	}

	public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
	{
	}

	public void Validate(ServiceEndpoint endpoint)
	{
	}
}
