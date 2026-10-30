using System.IO;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class StreamExtensions
{
	public static int TryReadAll(this Stream stream, byte[] buffer, int offset, int count)
	{
		DebugAssertHelper.Assert(count > 0);
		int i;
		int num;
		for (i = 0; i < count; i += num)
		{
			num = stream.Read(buffer, offset + i, count - i);
			if (num == 0)
			{
				break;
			}
		}
		return i;
	}
}
