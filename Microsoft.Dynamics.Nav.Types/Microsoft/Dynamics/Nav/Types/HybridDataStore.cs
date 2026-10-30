using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract]
public class HybridDataStore
{
	[DataMember]
	private byte[] data;

	[DataMember]
	private bool isCompressed;

	public byte[] Data
	{
		get
		{
			return data;
		}
		internal set
		{
			data = value;
		}
	}

	internal bool IsCompressed
	{
		get
		{
			return isCompressed;
		}
		set
		{
			isCompressed = value;
		}
	}

	internal HybridDataStore(byte[] data, bool isCompressed)
	{
		Data = data;
		IsCompressed = isCompressed;
	}

	public HybridMemoryDecompressStream DecompressStream()
	{
		return new HybridMemoryDecompressStream(this);
	}
}
