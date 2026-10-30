using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.Dynamics.Nav.Types.Channels;

internal class ChunkingChannelListener : ChannelListenerBase<IDuplexSessionChannel>
{
	private readonly IChannelListener<IDuplexSessionChannel> innerListener;

	private readonly int chunkSize;

	private readonly HashSet<string> chunkedActions;

	private IChannelListener<IDuplexSessionChannel> InnerListener => innerListener;

	public override Uri Uri => InnerListener.Uri;

	public ChunkingChannelListener(IDefaultCommunicationTimeouts timeouts, IChannelListener<IDuplexSessionChannel> innerListener, int chunkSize, HashSet<string> chunkedActions)
		: base(timeouts)
	{
		this.innerListener = innerListener;
		this.chunkSize = chunkSize;
		this.chunkedActions = chunkedActions;
	}

	public override T GetProperty<T>()
	{
		return InnerListener.GetProperty<T>();
	}

	protected override void OnOpen(TimeSpan timeout)
	{
		InnerListener.Open(timeout);
	}

	protected override void OnAbort()
	{
		InnerListener.Abort();
		Abort();
	}

	protected override void OnClose(TimeSpan timeout)
	{
		InnerListener.Close(timeout);
		Close();
	}

	protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
	{
		return InnerListener.BeginOpen(timeout, callback, state);
	}

	protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
	{
		return InnerListener.BeginClose(timeout, callback, state);
	}

	protected override void OnEndClose(IAsyncResult result)
	{
		InnerListener.EndClose(result);
	}

	protected override void OnEndOpen(IAsyncResult result)
	{
		InnerListener.EndOpen(result);
	}

	protected override IDuplexSessionChannel OnAcceptChannel(TimeSpan timeout)
	{
		IDuplexSessionChannel innerChannel = InnerListener.AcceptChannel();
		return WrapChannel(innerChannel);
	}

	protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
	{
		return InnerListener.BeginAcceptChannel(timeout, callback, state);
	}

	protected override IDuplexSessionChannel OnEndAcceptChannel(IAsyncResult result)
	{
		IDuplexSessionChannel innerChannel = InnerListener.EndAcceptChannel(result);
		return WrapChannel(innerChannel);
	}

	private IDuplexSessionChannel WrapChannel(IDuplexSessionChannel innerChannel)
	{
		if (innerChannel != null)
		{
			return new ChunkingDuplexSessionChannel(this, innerChannel, chunkSize, chunkedActions);
		}
		return null;
	}

	protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
	{
		return InnerListener.BeginWaitForChannel(timeout, callback, state);
	}

	protected override bool OnEndWaitForChannel(IAsyncResult result)
	{
		return InnerListener.EndWaitForChannel(result);
	}

	protected override bool OnWaitForChannel(TimeSpan timeout)
	{
		return InnerListener.WaitForChannel(timeout);
	}
}
