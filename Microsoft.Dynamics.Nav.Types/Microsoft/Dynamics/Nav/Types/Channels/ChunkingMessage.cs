using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Channels;

public class ChunkingMessage : Message
{
	private readonly MessageVersion version;

	private readonly ChunkReceiveStream receiveStream;

	private readonly MessageProperties properties;

	private readonly MessageHeaders headers;

	public override MessageHeaders Headers => headers;

	public override MessageProperties Properties => properties;

	public override MessageVersion Version => version;

	internal ChunkingMessage(MessageVersion version, string action, ChunkReceiveStream receiveStream)
	{
		this.version = version;
		this.receiveStream = receiveStream;
		properties = new MessageProperties();
		headers = new MessageHeaders(version)
		{
			Action = action
		};
	}

	protected override XmlDictionaryReader OnGetReaderAtBodyContents()
	{
		XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateBinaryReader(receiveStream, XmlDictionaryReaderQuotas.Max);
		xmlDictionaryReader.ReadStartElement();
		xmlDictionaryReader.ReadStartElement();
		return xmlDictionaryReader;
	}

	protected override void OnClose()
	{
		receiveStream.Close();
	}

	protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
	{
	}
}
