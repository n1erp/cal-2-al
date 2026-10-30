using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.Dynamics.Nav.Types.Channels;

internal class ChunkingChannelFactory : ChannelFactoryBase<IDuplexSessionChannel>
{
	private readonly IChannelFactory<IDuplexSessionChannel> innerChannelFactory;

	private readonly int chunkSize;

	private readonly HashSet<string> chunkedActions;

	private IChannelFactory<IDuplexSessionChannel> InnerChannelFactory => innerChannelFactory;

	internal ChunkingChannelFactory(IDefaultCommunicationTimeouts timeouts, IChannelFactory<IDuplexSessionChannel> innerChannelFactory, int chunkSize, HashSet<string> chunkedActions)
		: base(timeouts)
	{
		this.innerChannelFactory = innerChannelFactory;
		this.chunkSize = chunkSize;
		this.chunkedActions = chunkedActions;
	}

	protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
	{
		return InnerChannelFactory.BeginOpen(timeout, callback, state);
	}

	protected override void OnEndOpen(IAsyncResult result)
	{
		InnerChannelFactory.EndOpen(result);
	}

	protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
	{
		return InnerChannelFactory.BeginClose(timeout, callback, state);
	}

	protected override void OnEndClose(IAsyncResult result)
	{
		InnerChannelFactory.EndClose(result);
	}

	protected override void OnOpen(TimeSpan timeout)
	{
		InnerChannelFactory.Open(timeout);
	}

	protected override void OnAbort()
	{
		InnerChannelFactory.Abort();
	}

	protected override void OnClose(TimeSpan timeout)
	{
		InnerChannelFactory.Close(timeout);
	}

	protected override IDuplexSessionChannel OnCreateChannel(EndpointAddress address, Uri via)
	{
		IDuplexSessionChannel innerChannel = InnerChannelFactory.CreateChannel(address, via);
		return new ChunkingDuplexSessionChannel(this, innerChannel, chunkSize, chunkedActions);
	}

	public override T GetProperty<T>()
	{
		return InnerChannelFactory.GetProperty<T>();
	}
}
