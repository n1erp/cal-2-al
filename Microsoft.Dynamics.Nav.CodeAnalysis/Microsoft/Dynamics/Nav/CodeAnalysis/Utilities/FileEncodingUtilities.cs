using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class FileEncodingUtilities
{
	internal static int PreambleOffset(byte[] bytes, Encoding encoding)
	{
		byte[] preamble = encoding.GetPreamble();
		int result = 0;
		if (bytes.Length >= preamble.Length)
		{
			bool flag = true;
			for (int i = 0; i < preamble.Length; i++)
			{
				if (bytes[i] != preamble[i])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				result = preamble.Length;
			}
		}
		return result;
	}
}
