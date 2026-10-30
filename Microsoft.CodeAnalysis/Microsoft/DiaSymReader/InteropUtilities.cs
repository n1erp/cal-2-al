using System;
using System.Runtime.InteropServices;

namespace Microsoft.DiaSymReader;

internal class InteropUtilities
{
	private static readonly IntPtr s_ignoreIErrorInfo = new IntPtr(-1);

	internal static T[] NullToEmpty<T>(T[] items)
	{
		if (items != null)
		{
			return items;
		}
		return EmptyArray<T>.Instance;
	}

	internal static void ThrowExceptionForHR(int hr)
	{
		if (hr < 0 && hr != -2147467259 && hr != -2147467263)
		{
			Marshal.ThrowExceptionForHR(hr, s_ignoreIErrorInfo);
		}
	}

	internal unsafe static void CopyQualifiedTypeName(char* qualifiedName, int qualifiedNameBufferLength, int* qualifiedNameLength, string namespaceStr, string nameStr)
	{
		if (namespaceStr == null)
		{
			namespaceStr = string.Empty;
		}
		if (qualifiedNameLength != null)
		{
			int num = ((namespaceStr.Length > 0) ? (namespaceStr.Length + 1) : 0) + nameStr.Length;
			if (qualifiedName != null)
			{
				*qualifiedNameLength = Math.Min(num, Math.Max(0, qualifiedNameBufferLength - 1));
			}
			else
			{
				*qualifiedNameLength = num;
			}
		}
		if (qualifiedName == null || qualifiedNameBufferLength <= 0)
		{
			return;
		}
		char* ptr = qualifiedName;
		char* ptr2 = ptr + qualifiedNameBufferLength - 1;
		if (namespaceStr.Length > 0)
		{
			for (int i = 0; i < namespaceStr.Length; i++)
			{
				if (ptr >= ptr2)
				{
					break;
				}
				*ptr = namespaceStr[i];
				ptr++;
			}
			if (ptr < ptr2)
			{
				*ptr = '.';
				ptr++;
			}
		}
		for (int j = 0; j < nameStr.Length; j++)
		{
			if (ptr >= ptr2)
			{
				break;
			}
			*ptr = nameStr[j];
			ptr++;
		}
		*ptr = '\0';
	}
}
