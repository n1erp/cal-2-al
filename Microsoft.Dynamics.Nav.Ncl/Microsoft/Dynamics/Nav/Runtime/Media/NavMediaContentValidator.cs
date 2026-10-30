using System;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal static class NavMediaContentValidator
{
	private const ushort ImageDosSignature = 23117;

	private const ushort ImageOs2Signature = 17742;

	private const ushort ImageOs2SignatureLe = 17740;

	private const uint ImageNtSignature = 17744u;

	private const int PeMagicOffset = 60;

	private const int FileBufferLength = 1024;

	internal static void VerifyStreamData(Stream s, string mimeType)
	{
		long position = s.Position;
		if (position != 0L && s.CanSeek)
		{
			s.Seek(0L, SeekOrigin.Begin);
		}
		byte[] array = new byte[1024];
		int num = s.Read(array, 0, 1024);
		s.Seek(position, SeekOrigin.Begin);
		if (num < 2)
		{
			return;
		}
		ushort num2 = BitConverter.ToUInt16(array, 0);
		if (num2 == 17740 || num2 == 17742 || num2 == 23117)
		{
			CheckPeSignature(array, mimeType);
		}
		switch (num2)
		{
		case 332:
		case 448:
		case 452:
		case 512:
		case 34404:
		case 43620:
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000027H", Category.Runtime, new NavMediaUnsupportedFormatException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidMediaContentDetected, "COFF", mimeType)));
		}
		string text = ((num >= "!<arch>\n".Length) ? Encoding.ASCII.GetString(array, 0, 8) : string.Empty);
		if (!text.Equals("!<arch>\n"))
		{
			return;
		}
		throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000027I", Category.Runtime, new NavMediaUnsupportedFormatException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidMediaContentDetected, "Archive File", mimeType)));
	}

	internal static void ValidateOfficeContentHeader(Stream content, string mimeType)
	{
		long position = content.Position;
		if (position != 0L && content.CanSeek)
		{
			content.Seek(0L, SeekOrigin.Begin);
		}
		int num = 8;
		byte[] array = new byte[num];
		int num2 = content.Read(array, 0, num);
		content.Seek(position, SeekOrigin.Begin);
		if (num2 != num)
		{
			return;
		}
		uint num3 = EndianSwap(BitConverter.ToUInt32(array, 0));
		if (num3 != 1347093252)
		{
			uint num4 = EndianSwap(BitConverter.ToUInt32(array, 4));
			if (num3 != 3503231456u && num4 != 2712738529u)
			{
				throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000027J", Category.Runtime, new NavMediaUnsupportedFormatException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidMediaOfficeDocContent, mimeType)));
			}
		}
	}

	private static uint EndianSwap(uint source)
	{
		return (source >> 24) | ((source << 8) & 0xFF0000) | ((source >> 8) & 0xFF00) | (source << 24);
	}

	private static void CheckPeSignature(byte[] buffer, string mimeType)
	{
		uint num = BitConverter.ToUInt32(buffer, 60);
		if (num < buffer.Length - 4)
		{
			uint num2 = BitConverter.ToUInt32(buffer, (int)num);
			if (num2 == 17744)
			{
				throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000027K", Category.Runtime, new NavMediaUnsupportedFormatException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidMediaContentDetected, "Executable file", mimeType)));
			}
		}
	}
}
