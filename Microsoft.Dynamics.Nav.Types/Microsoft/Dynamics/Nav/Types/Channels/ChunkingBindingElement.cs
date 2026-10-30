using System;
using System.ServiceModel.Channels;

namespace Microsoft.Dynamics.Nav.Types.Channels;

public class ChunkingBindingElement : BindingElement
{
	private readonly int chunkSize;

	public ChunkingBindingElement(int chunkSize)
	{
		this.chunkSize = ChunkingHelper.OptimalChunkSize(chunkSize);
	}

	public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
	{
		if (typeof(TChannel) == typeof(IDuplexSessionChannel))
		{
			return context.CanBuildInnerChannelFactory<TChannel>();
		}
		return false;
	}

	public override bool CanBuildChannelListener<TChannel>(BindingContext context)
	{
		if (typeof(TChannel) == typeof(IDuplexSessionChannel))
		{
			return context.CanBuildInnerChannelListener<TChannel>();
		}
		return false;
	}

	public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
	{
		if (!CanBuildChannelFactory<TChannel>(context))
		{
			throw new InvalidOperationException("ChunkingChannelUnsupportedChannelType");
		}
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		ChunkingBindingParameter chunkingBindingParameter = context.BindingParameters.Find<ChunkingBindingParameter>() ?? new ChunkingBindingParameter();
		return (IChannelFactory<TChannel>)new ChunkingChannelFactory(context.Binding, context.BuildInnerChannelFactory<IDuplexSessionChannel>(), chunkSize, chunkingBindingParameter.ChunkedActions);
	}

	public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
	{
		if (!CanBuildChannelListener<TChannel>(context))
		{
			throw new InvalidOperationException("ChunkingChannelUnsupportedChannelType");
		}
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		ChunkingBindingParameter chunkingBindingParameter = context.BindingParameters.Find<ChunkingBindingParameter>() ?? new ChunkingBindingParameter();
		return (IChannelListener<TChannel>)new ChunkingChannelListener(context.Binding, context.BuildInnerChannelListener<IDuplexSessionChannel>(), chunkSize, chunkingBindingParameter.ChunkedActions);
	}

	public override BindingElement Clone()
	{
		return new ChunkingBindingElement(chunkSize);
	}

	public override T GetProperty<T>(BindingContext context)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		return context.GetInnerProperty<T>();
	}
}
