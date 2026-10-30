using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Channels;

internal class ChunkingDuplexSessionChannel : ChannelBase, IDuplexSessionChannel, IDuplexChannel, IInputChannel, IChannel, ICommunicationObject, IOutputChannel, ISessionChannel<IDuplexSession>
{
	private readonly IDuplexSessionChannel innerChannel;

	private readonly int chunkSize;

	private readonly HashSet<string> chunkedActions;

	private readonly ReceiveCorrelator receiveCorrelator;

	private readonly Dictionary<Guid, ChunkSendStream> currentSendStreams = new Dictionary<Guid, ChunkSendStream>();

	public EndpointAddress RemoteAddress => innerChannel.RemoteAddress;

	public Uri Via => innerChannel.Via;

	public IDuplexSession Session => innerChannel.Session;

	public EndpointAddress LocalAddress => innerChannel.LocalAddress;

	public ChunkingDuplexSessionChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel, int chunkSize, HashSet<string> chunkedActions)
		: base(channelManager)
	{
		if (innerChannel == null)
		{
			throw new ArgumentNullException("innerChannel");
		}
		this.innerChannel = innerChannel;
		this.innerChannel.Faulted += OnInnerChannelFaulted;
		this.chunkSize = chunkSize;
		this.chunkedActions = chunkedActions;
		receiveCorrelator = new ReceiveCorrelator(this, this.innerChannel);
	}

	protected override void OnAbort()
	{
		innerChannel.Abort();
	}

	protected override void OnOpen(TimeSpan timeout)
	{
		innerChannel.Open(timeout);
	}

	private void OnInnerChannelFaulted(object s, EventArgs e)
	{
		Fault();
	}

	protected override void OnClose(TimeSpan timeout)
	{
		try
		{
			innerChannel.Faulted -= OnInnerChannelFaulted;
			innerChannel.Close();
		}
		catch (CommunicationException ex)
		{
			ChunkingHelper.MapException(ex);
			throw;
		}
	}

	public void Send(Message message)
	{
		Send(message, DefaultSendTimeout);
	}

	public void Send(Message message, TimeSpan timeout)
	{
		ThrowIfDisposedOrNotOpen();
		bool flag = true;
		try
		{
			if (message.IsFault || chunkSize == 0 || !chunkedActions.Contains(message.Headers.Action))
			{
				innerChannel.Send(message, timeout);
			}
			else
			{
				ChunkSendStream chunkSendStream = new ChunkSendStream(innerChannel, message, chunkSize, timeout);
				lock (currentSendStreams)
				{
					currentSendStreams.Add(chunkSendStream.ChunkSequenceId, chunkSendStream);
				}
				try
				{
					using (message)
					{
						using XmlDictionaryWriter xmlDictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(chunkSendStream);
						try
						{
							message.WriteStartEnvelope(xmlDictionaryWriter);
							message.WriteStartBody(xmlDictionaryWriter);
							message.WriteBodyContents(xmlDictionaryWriter);
							xmlDictionaryWriter.WriteEndElement();
							xmlDictionaryWriter.WriteEndElement();
						}
						catch (CanceledChunkSequenceException)
						{
						}
						chunkSendStream.SetCompleted();
					}
				}
				finally
				{
					lock (currentSendStreams)
					{
						currentSendStreams.Remove(chunkSendStream.ChunkSequenceId);
					}
				}
			}
			flag = false;
		}
		catch (CommunicationException ex2)
		{
			ChunkingHelper.MapException(ex2);
			throw;
		}
		finally
		{
			if (flag)
			{
				Fault();
			}
		}
	}

	public void CancelSendingChunkSequence(Guid chunkSequenceId)
	{
		ChunkSendStream value;
		lock (currentSendStreams)
		{
			currentSendStreams.TryGetValue(chunkSequenceId, out value);
		}
		value?.Cancel();
	}

	public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
	{
		return receiveCorrelator.BeginTryReceive(timeout, callback, state);
	}

	public bool EndTryReceive(IAsyncResult result, out Message message)
	{
		bool flag = ReceiveCorrelator.EndTryReceive(result, out var message2);
		ref Message reference = ref message;
		object obj;
		if (message2 == null || !flag)
		{
			obj = null;
		}
		else
		{
			obj = (message2.IsChunk ? GetOriginalMessageFromFirstChunk(message2) : message2.Message);
		}
		reference = (Message)obj;
		return flag;
	}

	private Message GetOriginalMessageFromFirstChunk(ChunkingMessageInfo messageInfo)
	{
		if (messageInfo == null)
		{
			return null;
		}
		if (messageInfo.Message.Headers.Action != "http://www.microsoft.com/Dynamics/Channel/RealAction")
		{
			return messageInfo.Message;
		}
		ChunkReceiveStream receiveStream = new ChunkReceiveStream(messageInfo, receiveCorrelator, chunkSize, innerChannel);
		ChunkingMessage chunkingMessage = new ChunkingMessage(messageInfo.Message.Version, messageInfo.RealAction, receiveStream);
		string addressingNamespace = ChunkingMessageInfo.GetAddressingNamespace(messageInfo.Message.Version);
		for (int i = 0; i < messageInfo.Message.Headers.Count; i++)
		{
			MessageHeaderInfo messageHeaderInfo = messageInfo.Message.Headers[i];
			if (messageHeaderInfo.Namespace != "http://www.microsoft.com/Dynamics/Channel" && (!(messageHeaderInfo.Name == "Action") || !(messageHeaderInfo.Namespace == addressingNamespace)))
			{
				chunkingMessage.Headers.CopyHeaderFrom(messageInfo.Message.Headers, i);
			}
		}
		chunkingMessage.Properties.CopyProperties(messageInfo.Message.Properties);
		return chunkingMessage;
	}

	public bool WaitForMessage(TimeSpan timeout)
	{
		throw new NotSupportedException();
	}

	public Message Receive()
	{
		throw new NotSupportedException();
	}

	public Message Receive(TimeSpan timeout)
	{
		throw new NotSupportedException();
	}

	public bool TryReceive(TimeSpan timeout, out Message message)
	{
		throw new NotSupportedException();
	}

	protected override void OnEndClose(IAsyncResult result)
	{
		throw new NotSupportedException();
	}

	protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
	{
		throw new NotSupportedException();
	}

	protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
	{
		throw new NotSupportedException();
	}

	protected override void OnEndOpen(IAsyncResult result)
	{
		throw new NotSupportedException();
	}

	public IAsyncResult BeginReceive(AsyncCallback callback, object state)
	{
		throw new NotSupportedException();
	}

	public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
	{
		throw new NotSupportedException();
	}

	public Message EndReceive(IAsyncResult result)
	{
		throw new NotSupportedException();
	}

	public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
	{
		throw new NotSupportedException();
	}

	public bool EndWaitForMessage(IAsyncResult result)
	{
		throw new NotSupportedException();
	}

	public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
	{
		return BeginSend(message, DefaultSendTimeout, callback, state);
	}

	public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
	{
		Send(message, timeout);
		return CompletedAsyncResult.WithState(state);
	}

	public void EndSend(IAsyncResult result)
	{
	}
}
