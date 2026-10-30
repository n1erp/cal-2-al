namespace Microsoft.Dynamics.Nav.Client;

public abstract class ServerInvocationHandler
{
	protected ServerInvocationHandler NextHandler { get; set; }

	protected ServerCallOptions ServerCallOptions { get; set; }

	public virtual T CallServer<T>(BeginCallServerMethod beginCallServerMethod, EndCallServerMethod<T> endCallServerMethod)
	{
		return NextHandler.CallServer(beginCallServerMethod, endCallServerMethod);
	}

	public virtual T CallServer<T>(BeginCallServerMethod beginCallServerMethod, EndCallServerMethod<T> endCallServerMethod, ServerCallOptions options)
	{
		return NextHandler.CallServer(beginCallServerMethod, endCallServerMethod, options);
	}

	public virtual void RunClientRequest(ProcessClientRequest clientRequest)
	{
		NextHandler.RunClientRequest(clientRequest);
	}
}
