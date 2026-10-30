using System.IO;
using Microsoft.IO;

namespace Microsoft.Dynamics.Nav.Types;

public static class MemoryStreamManager
{
	private const int SmallBlockSize = 8192;

	private const int LargeBufferMultiple = 1048576;

	private const int MaximumBufferSize = 134217728;

	private static readonly RecyclableMemoryStreamManager instance = new RecyclableMemoryStreamManager(8192, 1048576, 134217728);

	public static MemoryStream GetStream()
	{
		if (FeatureSwitches.Instance.DisableRecyclableMemoryStream)
		{
			return new MemoryStream();
		}
		return instance.GetStream();
	}

	public static MemoryStream GetStream(byte[] buffer)
	{
		if (FeatureSwitches.Instance.DisableRecyclableMemoryStream)
		{
			return new MemoryStream(buffer);
		}
		return instance.GetStream(string.Empty, buffer, 0, buffer.Length);
	}

	public static MemoryStream GetStream(int capacity)
	{
		if (FeatureSwitches.Instance.DisableRecyclableMemoryStream)
		{
			return new MemoryStream(capacity);
		}
		return instance.GetStream(string.Empty, capacity);
	}

	static MemoryStreamManager()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected Obj, but got Unknown
	}
}
