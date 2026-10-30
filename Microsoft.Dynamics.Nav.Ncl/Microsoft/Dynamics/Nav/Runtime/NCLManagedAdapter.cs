using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NCLManagedAdapter
{
	private static byte[] oem2AnsiTable;

	private static byte[] ansi2OemTable;

	private static int maxBytesPerChar;

	private static Encoding defaultEncoding;

	private static void Initialize()
	{
		oem2AnsiTable = new byte[256];
		ansi2OemTable = new byte[256];
		for (int i = 0; i < 256; i++)
		{
			oem2AnsiTable[i] = (byte)i;
			ansi2OemTable[i] = (byte)i;
		}
		NativeMethods.OemToCharBuff(oem2AnsiTable, oem2AnsiTable, oem2AnsiTable.Length);
		NativeMethods.CharToOemBuff(ansi2OemTable, ansi2OemTable, ansi2OemTable.Length);
		int num = 255;
		int num2 = 255;
		while (true)
		{
			if (ansi2OemTable[oem2AnsiTable[num]] == num && num != 0)
			{
				num--;
				continue;
			}
			while (oem2AnsiTable[ansi2OemTable[num2]] == num2 && num2 != 0)
			{
				num2--;
			}
			if (num == 0)
			{
				break;
			}
			oem2AnsiTable[num] = (byte)num2;
			ansi2OemTable[num2] = (byte)num;
			num--;
			num2--;
		}
		if (defaultEncoding.IsSingleByte)
		{
			maxBytesPerChar = 1;
		}
		else
		{
			maxBytesPerChar = 2;
		}
	}

	private static void ChangeDefaultEncoding(Encoding newEncoding)
	{
		defaultEncoding = newEncoding;
		Initialize();
	}

	private static void RestoreDefaultEncoding()
	{
		defaultEncoding = Encoding.Default;
		Initialize();
	}

	static NCLManagedAdapter()
	{
		defaultEncoding = Encoding.Default;
		Initialize();
	}

	internal static string ByteToText(byte[] oemBytes)
	{
		return ByteToText(oemBytes, TextEncoding.MSDos);
	}

	internal static string ByteToText(byte[] oemBytes, TextEncoding encoding)
	{
		return ByteToText(oemBytes, 0, oemBytes.Length, encoding);
	}

	internal static char ByteToTextChar(byte oemByte)
	{
		return ByteToTextChar(oemByte, TextEncoding.MSDos);
	}

	internal static char ByteToTextChar(byte oemByte, TextEncoding encoding)
	{
		if (Encoding.Default.IsSingleByte && encoding == TextEncoding.MSDos)
		{
			return defaultEncoding.GetChars(new byte[1] { oem2AnsiTable[oemByte] })[0];
		}
		return defaultEncoding.GetChars(new byte[1] { oemByte })[0];
	}

	internal static string ByteToText(byte[] oemBytes, int startIndex, int length)
	{
		return ByteToText(oemBytes, startIndex, length, TextEncoding.MSDos);
	}

	internal static string ByteToText(byte[] oemBytes, int startIndex, int length, TextEncoding encoding)
	{
		if (oemBytes == null)
		{
			throw new NavNCLArgumentException(Lang._00095_50082);
		}
		if (startIndex < 0 || startIndex > oemBytes.Length)
		{
			throw new NavNCLIndexOutOfBoundsException();
		}
		if (length < 0 || startIndex + length > oemBytes.Length)
		{
			throw new NavNCLIndexOutOfBoundsException();
		}
		for (int i = startIndex; i < startIndex + length; i++)
		{
			if (oemBytes[i] == 0)
			{
				length = i - startIndex;
				break;
			}
		}
		if (length == 0)
		{
			return string.Empty;
		}
		if (defaultEncoding.IsSingleByte && encoding == TextEncoding.MSDos)
		{
			byte[] array = new byte[length];
			for (int j = 0; j < length; j++)
			{
				array[j] = oem2AnsiTable[oemBytes[startIndex++]];
			}
			return defaultEncoding.GetString(array);
		}
		return defaultEncoding.GetString(oemBytes, startIndex, length);
	}

	internal unsafe static string ByteToText(IntPtr lpzOemStr)
	{
		byte* ptr = (byte*)lpzOemStr.ToPointer();
		if (*ptr == 0)
		{
			return string.Empty;
		}
		byte[] array = new byte[1050];
		int num = 0;
		for (; *ptr != 0; ptr++)
		{
			if (num >= array.Length)
			{
				byte* ptr2;
				for (ptr2 = ptr; *ptr2 != 0; ptr2++)
				{
				}
				int num2 = array.Length + (int)ptr2 - (int)ptr;
				byte[] array2 = new byte[num2];
				Array.Copy(array, array2, num2);
				array = array2;
			}
			if (defaultEncoding.IsSingleByte)
			{
				array[num++] = oem2AnsiTable[*ptr];
			}
			else
			{
				array[num++] = *ptr;
			}
		}
		return defaultEncoding.GetString(array, 0, num);
	}

	internal static byte[] TextToByte(string ansiString)
	{
		return TextToByte(ansiString, TextEncoding.MSDos);
	}

	internal static byte[] TextToByte(string ansiString, TextEncoding encoding)
	{
		byte[] array = new byte[(ansiString.Length + 1) * maxBytesPerChar];
		int bytes = defaultEncoding.GetBytes(ansiString, 0, ansiString.Length, array, 0);
		if (!defaultEncoding.IsSingleByte)
		{
			Array.Resize(ref array, bytes + 1);
		}
		else if (encoding != TextEncoding.Windows)
		{
			for (int i = 0; i < array.Length - 1; i++)
			{
				array[i] = ansi2OemTable[array[i]];
			}
		}
		return array;
	}

	internal static byte TextCharToByte(char ansiChar)
	{
		return TextCharToByte(ansiChar, TextEncoding.MSDos);
	}

	internal static byte TextCharToByte(char ansiChar, TextEncoding encoding)
	{
		byte b = defaultEncoding.GetBytes(new char[1] { ansiChar })[0];
		if (defaultEncoding.IsSingleByte && encoding == TextEncoding.MSDos)
		{
			return ansi2OemTable[b];
		}
		return b;
	}

	internal unsafe static void TextToByte(string ansiString, IntPtr dest, int destMaxSize)
	{
		byte[] array = new byte[ansiString.Length * maxBytesPerChar];
		int bytes = defaultEncoding.GetBytes(ansiString, 0, ansiString.Length, array, 0);
		if (bytes > destMaxSize)
		{
			throw new NavNCLStringLengthExceededException(destMaxSize - 1, bytes, ansiString);
		}
		byte* ptr = (byte*)dest.ToPointer();
		if (defaultEncoding.IsSingleByte)
		{
			for (int i = 0; i < array.Length; i++)
			{
				*(ptr++) = ansi2OemTable[array[i]];
			}
		}
		else
		{
			for (int j = 0; j < bytes; j++)
			{
				*(ptr++) = array[j];
			}
		}
		*ptr = 0;
	}

	internal unsafe static void CopyFromBytesToIntPtr(byte[] source, IntPtr destination)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == IntPtr.Zero)
		{
			throw new ArgumentNullException("destination");
		}
		if (source.Length < 3400)
		{
			byte* ptr = (byte*)destination.ToPointer();
			for (int i = 0; i < source.Length; i++)
			{
				*(ptr++) = source[i];
			}
		}
		else
		{
			Marshal.Copy(source, 0, destination, source.Length);
		}
	}
}
