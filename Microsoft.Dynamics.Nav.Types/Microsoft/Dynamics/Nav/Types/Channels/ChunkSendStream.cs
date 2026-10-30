using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Channels;

internal class ChunkSendStream : Stream
{
	private class BufferBodyWriter : BodyWriter
	{
		private readonly byte[] data;

		private readonly int length;

		public BufferBodyWriter(byte[] data, int length)
			: base(isBuffered: true)
		{
			this.data = data;
			this.length = length;
		}

		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("Chunk", "http://www.microsoft.com/Dynamics/Channel");
			writer.WriteBase64(data, 0, length);
			writer.WriteEndElement();
		}
	}

	private static readonly MessageHeader chunkEndHeader = MessageHeader.CreateHeader("ChunkEnd", "http://www.microsoft.com/Dynamics/Channel", null, mustUnderstand: true);

	private readonly IDuplexSessionChannel innerChannel;

	private readonly Message originalMessage;

	private readonly int chunkSize;

	private readonly TimeSpan timeout;

	private readonly byte[] data;

	private int position;

	private readonly MessageHeader chunkSequenceIdHeader;

	private int chunkNumber = 1;

	private readonly Guid chunkSequenceId;

	private bool isCancelled;

	private bool isCompleted;

	public Guid ChunkSequenceId
	{
		[DebuggerStepThrough]
		get
		{
			return chunkSequenceId;
		}
	}

	public override bool CanRead => false;

	public override bool CanSeek => false;

	public override bool CanWrite => true;

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

	public override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public ChunkSendStream(IDuplexSessionChannel innerChannel, Message originalMessage, int chunkSize, TimeSpan timeout)
	{
		this.innerChannel = innerChannel;
		this.originalMessage = originalMessage;
		this.chunkSize = chunkSize;
		this.timeout = timeout;
		data = ChunkingHelper.BufferManager.TakeBuffer(chunkSize);
		chunkSequenceId = Guid.NewGuid();
		chunkSequenceIdHeader = MessageHeader.CreateHeader("ChunkId", "http://www.microsoft.com/Dynamics/Channel", chunkSequenceId.ToString(), mustUnderstand: true);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		int num;
		while ((num = Math.Min(chunkSize - position, count)) > 0)
		{
			Buffer.BlockCopy(buffer, offset, data, position, num);
			position += num;
			offset += num;
			count -= num;
			if (position >= chunkSize)
			{
				SendChunk(isLast: false);
			}
		}
	}

	public override void Close()
	{
		if (isCompleted)
		{
			SendChunk(isLast: true);
		}
		ChunkingHelper.BufferManager.ReturnBuffer(data);
	}

	public void SetCompleted()
	{
		isCompleted = true;
		isCancelled = false;
	}

	public void Cancel()
	{
		isCancelled = true;
	}

	private void SendChunk(bool isLast)
	{
		BufferBodyWriter body = new BufferBodyWriter(data, position);
		Message message = Message.CreateMessage(originalMessage.Version, "http://www.microsoft.com/Dynamics/Channel/RealAction", body);
		string addressingNamespace = ChunkingMessageInfo.GetAddressingNamespace(originalMessage.Version);
		foreach (MessageHeader header3 in originalMessage.Headers)
		{
			MessageHeader header = header3;
			if (header3.Name == "Action" && header3.Namespace == addressingNamespace)
			{
				string action = originalMessage.Headers.Action;
				header = MessageHeader.CreateHeader("RealAction", "http://www.microsoft.com/Dynamics/Channel", action);
			}
			message.Headers.Add(header);
		}
		message.Headers.Add(chunkSequenceIdHeader);
		MessageHeader header2 = MessageHeader.CreateHeader("ChunkNumber", "http://www.microsoft.com/Dynamics/Channel", chunkNumber, mustUnderstand: true);
		message.Headers.Add(header2);
		chunkNumber++;
		if (isLast || isCancelled)
		{
			message.Headers.Add(chunkEndHeader);
		}
		innerChannel.Send(message, timeout);
		position = 0;
		if (isCancelled)
		{
			throw new CanceledChunkSequenceException();
		}
	}

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
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
}
