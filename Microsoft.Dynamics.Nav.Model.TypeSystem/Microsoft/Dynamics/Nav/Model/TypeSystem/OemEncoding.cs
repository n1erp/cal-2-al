using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class OemEncoding : Encoding
{
	private static class NativeMethods
	{
		[DllImport("user32", EntryPoint = "OemToCharBuffA")]
		internal static extern int OemToCharBuff(byte[] source, byte[] dest, int bytesize);

		[DllImport("user32", EntryPoint = "CharToOemBuffA")]
		internal static extern int CharToOemBuff(byte[] source, byte[] dest, int bytesize);
	}

	private static byte[] oem2AnsiTable;

	private static byte[] ansi2OemTable;

	private static int maxBytesPerChar;

	private static Encoding ansiEncoding;

	private static readonly Encoding instance = new OemEncoding();

	public static Encoding Instance => instance;

	private OemEncoding()
	{
		ansiEncoding = Encoding.Default;
		Initialize();
	}

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
		maxBytesPerChar = (ansiEncoding.IsSingleByte ? 1 : 2);
	}

	public override int GetByteCount(string s)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		return ansiEncoding.GetBytes(s).Length;
	}

	public override int GetByteCount(char[] chars, int index, int count)
	{
		throw new NotImplementedException();
	}

	public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		int bytes2 = ansiEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
		if (ansiEncoding.IsSingleByte)
		{
			for (int i = 0; i < bytes2; i++)
			{
				bytes[byteIndex + i] = ansi2OemTable[bytes[byteIndex + i]];
			}
		}
		return bytes2;
	}

	public override int GetCharCount(byte[] bytes, int index, int count)
	{
		throw new NotImplementedException();
	}

	public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
	{
		for (int i = byteIndex; i < byteIndex + byteCount; i++)
		{
			if (bytes[i] == 0)
			{
				byteCount = i - byteIndex;
				break;
			}
		}
		if (byteCount == 0)
		{
			return 0;
		}
		if (ansiEncoding.IsSingleByte)
		{
			byte[] array = new byte[byteCount];
			for (int j = 0; j < byteCount; j++)
			{
				array[j] = oem2AnsiTable[bytes[byteIndex++]];
			}
			return ansiEncoding.GetChars(array, 0, byteCount, chars, charIndex);
		}
		return ansiEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
	}

	public override int GetMaxByteCount(int charCount)
	{
		return maxBytesPerChar * charCount;
	}

	public override int GetMaxCharCount(int byteCount)
	{
		return byteCount;
	}
}
