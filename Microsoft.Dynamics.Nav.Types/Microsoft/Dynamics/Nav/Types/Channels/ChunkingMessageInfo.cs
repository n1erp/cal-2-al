using System;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Channels;

internal class ChunkingMessageInfo
{
	private readonly Message message;

	private readonly Guid chunkSequenceId;

	private readonly bool isLastchunk;

	private readonly int chunkNumber;

	private readonly string realAction;

	private readonly bool isCancellation;

	public Message Message
	{
		[DebuggerStepThrough]
		get
		{
			return message;
		}
	}

	public Guid ChunkSequenceId
	{
		[DebuggerStepThrough]
		get
		{
			return chunkSequenceId;
		}
	}

	public bool IsLastchunk
	{
		[DebuggerStepThrough]
		get
		{
			return isLastchunk;
		}
	}

	public int ChunkNumber
	{
		[DebuggerStepThrough]
		get
		{
			return chunkNumber;
		}
	}

	public string RealAction
	{
		[DebuggerStepThrough]
		get
		{
			return realAction;
		}
	}

	public bool IsChunk
	{
		[DebuggerStepThrough]
		get
		{
			return chunkNumber != 0;
		}
	}

	public bool IsCancellation
	{
		[DebuggerStepThrough]
		get
		{
			return isCancellation;
		}
	}

	public ChunkingMessageInfo(Message message)
	{
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		this.message = message;
		bool flag;
		if (this.message.Headers.Action == "http://www.microsoft.com/Dynamics/Channel/RealAction")
		{
			flag = true;
		}
		else if (this.message.Headers.Action == "http://www.microsoft.com/Dynamics/Channel/CancelChunkSequence")
		{
			flag = true;
			isCancellation = true;
		}
		else
		{
			flag = false;
		}
		if (flag)
		{
			ScanMessageHeaders(message, ref chunkSequenceId, ref isLastchunk, ref chunkNumber, ref realAction);
		}
	}

	internal static string GetAddressingNamespace(MessageVersion version)
	{
		if (version.Addressing != AddressingVersion.WSAddressing10)
		{
			return "http://schemas.xmlsoap.org/ws/2004/08/addressing";
		}
		return "http://www.w3.org/2005/08/addressing";
	}

	private static void ScanMessageHeaders(Message message, ref Guid chunkId, ref bool isLastChunk, ref int chunkNumber, ref string realAction)
	{
		int num = 0;
		MessageHeaders headers = message.Headers;
		for (int i = 0; i < headers.Count; i++)
		{
			if (num >= 4)
			{
				break;
			}
			XmlDictionaryReader readerAtHeader = message.Headers.GetReaderAtHeader(i);
			if (!(readerAtHeader.NamespaceURI != "http://www.microsoft.com/Dynamics/Channel"))
			{
				switch (readerAtHeader.LocalName)
				{
				case "ChunkId":
					chunkId = readerAtHeader.ReadElementContentAsGuid();
					num++;
					break;
				case "ChunkEnd":
					isLastChunk = true;
					num++;
					break;
				case "ChunkNumber":
					chunkNumber = readerAtHeader.ReadElementContentAsInt();
					num++;
					break;
				case "RealAction":
					realAction = readerAtHeader.ReadElementContentAsString();
					num++;
					break;
				}
			}
		}
	}
}
