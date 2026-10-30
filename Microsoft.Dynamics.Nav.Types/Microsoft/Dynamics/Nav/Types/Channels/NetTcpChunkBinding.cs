using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.Dynamics.Nav.Types.Channels;

public class NetTcpChunkBinding : NetTcpBinding
{
	private readonly ChunkingBindingElement bindingElement;

	public NetTcpChunkBinding(int chunkSize)
	{
		bindingElement = new ChunkingBindingElement(chunkSize);
	}

	public NetTcpChunkBinding()
		: this(28672)
	{
	}

	public override BindingElementCollection CreateBindingElements()
	{
		BindingElementCollection bindingElementCollection = base.CreateBindingElements();
		bindingElementCollection.Insert(0, bindingElement);
		return bindingElementCollection;
	}
}
