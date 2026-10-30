using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis;

internal class CvtResFile
{
	private const ushort RT_DLGINCLUDE = 17;

	internal static List<RESOURCE> ReadResFile(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream, Encoding.Unicode);
		List<RESOURCE> list = new List<RESOURCE>();
		long position = stream.Position;
		if (binaryReader.ReadUInt32() != 0)
		{
			throw new ResourceException("Stream does not begin with a null resource and is not in .RES format.");
		}
		stream.Position = position;
		while (stream.Position < stream.Length)
		{
			uint num = binaryReader.ReadUInt32();
			uint num2 = binaryReader.ReadUInt32();
			if (num2 < 8)
			{
				throw new ResourceException($"Resource header beginning at offset 0x{stream.Position - 8:x} is malformed.");
			}
			if (num == 0)
			{
				stream.Position += num2 - 8;
				continue;
			}
			RESOURCE rESOURCE = new RESOURCE
			{
				HeaderSize = num2,
				DataSize = num
			};
			rESOURCE.pstringType = ReadStringOrID(binaryReader);
			rESOURCE.pstringName = ReadStringOrID(binaryReader);
			stream.Position = (stream.Position + 3) & -4;
			rESOURCE.DataVersion = binaryReader.ReadUInt32();
			rESOURCE.MemoryFlags = binaryReader.ReadUInt16();
			rESOURCE.LanguageId = binaryReader.ReadUInt16();
			rESOURCE.Version = binaryReader.ReadUInt32();
			rESOURCE.Characteristics = binaryReader.ReadUInt32();
			rESOURCE.data = new byte[rESOURCE.DataSize];
			binaryReader.Read(rESOURCE.data, 0, rESOURCE.data.Length);
			stream.Position = (stream.Position + 3) & -4;
			if (rESOURCE.pstringType.theString != null || rESOURCE.pstringType.Ordinal != 17)
			{
				list.Add(rESOURCE);
			}
		}
		return list;
	}

	private static RESOURCE_STRING ReadStringOrID(BinaryReader fhIn)
	{
		RESOURCE_STRING rESOURCE_STRING = new RESOURCE_STRING();
		char c = fhIn.ReadChar();
		if (c == '\uffff')
		{
			rESOURCE_STRING.Ordinal = fhIn.ReadUInt16();
		}
		else
		{
			rESOURCE_STRING.Ordinal = ushort.MaxValue;
			StringBuilder stringBuilder = new StringBuilder();
			char c2 = c;
			do
			{
				stringBuilder.Append(c2);
				c2 = fhIn.ReadChar();
			}
			while (c2 != '\0');
			rESOURCE_STRING.theString = stringBuilder.ToString();
		}
		return rESOURCE_STRING;
	}
}
