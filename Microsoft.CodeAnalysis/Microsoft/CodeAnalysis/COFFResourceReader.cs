using System;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal static class COFFResourceReader
{
	private static void ConfirmSectionValues(SectionHeader hdr, long fileSize)
	{
		if ((long)hdr.PointerToRawData + (long)hdr.SizeOfRawData > fileSize)
		{
			throw new ResourceException(CodeAnalysisResources.CoffResourceInvalidSectionSize);
		}
	}

	internal static ResourceSection ReadWin32ResourcesFromCOFF(Stream stream)
	{
		PEHeaders pEHeaders = new PEHeaders(stream);
		SectionHeader hdr = default;
		SectionHeader hdr2 = default;
		int num = 0;
		foreach (SectionHeader sectionHeader in pEHeaders.SectionHeaders)
		{
			if (sectionHeader.Name == ".rsrc$01")
			{
				hdr = sectionHeader;
				num++;
			}
			else if (sectionHeader.Name == ".rsrc$02")
			{
				hdr2 = sectionHeader;
				num++;
			}
		}
		if (num != 2)
		{
			throw new ResourceException(CodeAnalysisResources.CoffResourceMissingSection);
		}
		ConfirmSectionValues(hdr, stream.Length);
		ConfirmSectionValues(hdr2, stream.Length);
		byte[] array;
		uint[] array2;
		uint[] array3;
		BinaryReader binaryReader;
		MemoryStream memoryStream;
		BinaryWriter binaryWriter;
		checked
		{
			array = new byte[hdr.SizeOfRawData + hdr2.SizeOfRawData];
			stream.Seek(hdr.PointerToRawData, SeekOrigin.Begin);
			stream.TryReadAll(array, 0, hdr.SizeOfRawData);
			stream.Seek(hdr2.PointerToRawData, SeekOrigin.Begin);
			stream.TryReadAll(array, hdr.SizeOfRawData, hdr2.SizeOfRawData);
			try
			{
				if (hdr.PointerToRelocations + hdr.NumberOfRelocations * 10 > stream.Length)
				{
					throw new ResourceException(CodeAnalysisResources.CoffResourceInvalidRelocation);
				}
			}
			catch (OverflowException)
			{
				throw new ResourceException(CodeAnalysisResources.CoffResourceInvalidRelocation);
			}
			array2 = new uint[hdr.NumberOfRelocations];
			array3 = new uint[hdr.NumberOfRelocations];
			binaryReader = new BinaryReader(stream, Encoding.Unicode);
			stream.Position = hdr.PointerToRelocations;
			for (int i = 0; i < hdr.NumberOfRelocations; i = unchecked(i + 1))
			{
				array2[i] = binaryReader.ReadUInt32();
				array3[i] = binaryReader.ReadUInt32();
				binaryReader.ReadUInt16();
			}
			stream.Position = pEHeaders.CoffHeader.PointerToSymbolTable;
			try
			{
				if (pEHeaders.CoffHeader.PointerToSymbolTable + unchecked((long)pEHeaders.CoffHeader.NumberOfSymbols) * 18L > stream.Length)
				{
					throw new ResourceException(CodeAnalysisResources.CoffResourceInvalidSymbol);
				}
			}
			catch (OverflowException)
			{
				throw new ResourceException(CodeAnalysisResources.CoffResourceInvalidSymbol);
			}
			memoryStream = new MemoryStream(array);
			binaryWriter = new BinaryWriter(memoryStream);
		}
		for (int j = 0; j < array3.Length; j++)
		{
			if (array3[j] > pEHeaders.CoffHeader.NumberOfSymbols)
			{
				throw new ResourceException(CodeAnalysisResources.CoffResourceInvalidRelocation);
			}
			long position = pEHeaders.CoffHeader.PointerToSymbolTable + array3[j] * 18;
			stream.Position = position;
			stream.Position += 8L;
			uint num2 = binaryReader.ReadUInt32();
			short num3 = binaryReader.ReadInt16();
			if (binaryReader.ReadUInt16() != 0 || num3 != 3)
			{
				throw new ResourceException(CodeAnalysisResources.CoffResourceInvalidSymbol);
			}
			memoryStream.Position = array2[j];
			binaryWriter.Write((uint)(num2 + hdr.SizeOfRawData));
		}
		return new ResourceSection(array, array2);
	}
}
