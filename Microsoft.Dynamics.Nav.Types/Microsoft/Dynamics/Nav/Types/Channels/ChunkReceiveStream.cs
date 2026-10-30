using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Channels;

internal class ChunkReceiveStream : Stream
{
	private readonly ReceiveCorrelator receiveCorrelator;

	private readonly IDuplexSessionChannel innerChannel;

	private readonly Guid chunkSequenceId;

	private readonly byte[] data;

	private readonly object syncRoot = new object();

	private int currentChunkLength;

	private XmlDictionaryReader currentreader;

	private bool isLastChunk;

	private int streamReadPosition;

	private int currentChunkNumber = 1;

	private bool disposed;

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override long Position
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public ChunkReceiveStream(ChunkingMessageInfo firstChunkMessageInfo, ReceiveCorrelator receiveCorrelator, int chunkSize, IDuplexSessionChannel innerChannel)
	{
		this.receiveCorrelator = receiveCorrelator;
		this.innerChannel = innerChannel;
		data = ChunkingHelper.BufferManager.TakeBuffer(Math.Max(chunkSize, 256));
		chunkSequenceId = firstChunkMessageInfo.ChunkSequenceId;
		isLastChunk = firstChunkMessageInfo.IsLastchunk;
		_ = isLastChunk;
		GetDataFromChunk(firstChunkMessageInfo.Message);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (!AdvanceIfNecessary())
		{
			return 0;
		}
		int num = Math.Min(count, currentChunkLength - streamReadPosition);
		if (num > 0)
		{
			Buffer.BlockCopy(data, streamReadPosition, buffer, offset, num);
			streamReadPosition += num;
		}
		return num;
	}

	public override int ReadByte()
	{
		if (!AdvanceIfNecessary() || streamReadPosition == currentChunkLength)
		{
			return -1;
		}
		return data[streamReadPosition++];
	}

	private bool AdvanceIfNecessary()
	{
		if (streamReadPosition == currentChunkLength)
		{
			if (currentreader == null)
			{
				return false;
			}
			ReadFromCurrentMessage();
		}
		return true;
	}

	private void ReadFromCurrentMessage()
	{
		currentChunkLength = currentreader.ReadContentAsBase64(data, 0, data.Length);
		streamReadPosition = 0;
		if (currentChunkLength == 0)
		{
			currentreader.Close();
			currentreader = null;
			if (isLastChunk)
			{
				ChunkingHelper.BufferManager.ReturnBuffer(data);
			}
			else
			{
				FetchNextMessage();
			}
		}
	}

	private void FetchNextMessage()
	{
		ChunkingMessageInfo message;
		lock (syncRoot)
		{
			if (disposed)
			{
				return;
			}
			currentChunkNumber++;
			receiveCorrelator.TryGetChunkMessage(chunkSequenceId, currentChunkNumber, out message);
			if (message == null)
			{
				throw new CommunicationObjectAbortedException();
			}
			isLastChunk = message.IsLastchunk;
		}
		_ = isLastChunk;
		GetDataFromChunk(message.Message);
	}

	private void GetDataFromChunk(Message message)
	{
		if (currentreader == null)
		{
			currentreader = message.GetReaderAtBodyContents();
			currentreader.ReadStartElement("Chunk", "http://www.microsoft.com/Dynamics/Channel");
		}
		ReadFromCurrentMessage();
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposing)
			{
				return;
			}
			lock (syncRoot)
			{
				if (disposed)
				{
					return;
				}
				disposed = true;
				if (innerChannel.State == CommunicationState.Opened)
				{
					if (!isLastChunk)
					{
						SendSequenceCancellationMessage();
					}
					ChunkingMessageInfo message;
					while (!isLastChunk && receiveCorrelator.TryGetChunkMessage(chunkSequenceId, ++currentChunkNumber, out message) && message != null)
					{
						isLastChunk = message.IsLastchunk;
					}
				}
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	private void SendSequenceCancellationMessage()
	{
		Message message = Message.CreateMessage(MessageVersion.Default, "http://www.microsoft.com/Dynamics/Channel/CancelChunkSequence");
		message.Headers.Add(MessageHeader.CreateHeader("ChunkId", "http://www.microsoft.com/Dynamics/Channel", chunkSequenceId.ToString(), mustUnderstand: true));
		innerChannel.BeginSend(message, (IAsyncResult asyncResult) =>
		{
			try
			{
				innerChannel.EndSend(asyncResult);
			}
			catch (CommunicationException)
			{
			}
			catch (ObjectDisposedException)
			{
			}
		}, null);
	}

	public override void Flush()
	{
		throw new NotSupportedException();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}
}
