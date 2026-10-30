using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using Roslyn.Utilities;

namespace Microsoft.Cci;

internal static class NativeResourceWriter
{
	private class Directory
	{
		internal readonly string Name;

		internal readonly int ID;

		internal ushort NumberOfNamedEntries;

		internal ushort NumberOfIdEntries;

		internal readonly List<object> Entries;

		internal Directory(string name, int id)
		{
			Name = name;
			ID = id;
			Entries = new List<object>();
		}
	}

	private static int CompareResources(IWin32Resource left, IWin32Resource right)
	{
		int num = CompareResourceIdentifiers(left.TypeId, left.TypeName, right.TypeId, right.TypeName);
		if (num != 0)
		{
			return num;
		}
		return CompareResourceIdentifiers(left.Id, left.Name, right.Id, right.Name);
	}

	private static int CompareResourceIdentifiers(int xOrdinal, string xString, int yOrdinal, string yString)
	{
		if (xString == null)
		{
			if (yString == null)
			{
				return xOrdinal - yOrdinal;
			}
			return 1;
		}
		if (yString == null)
		{
			return -1;
		}
		return string.Compare(xString, yString, StringComparison.OrdinalIgnoreCase);
	}

	internal static IEnumerable<IWin32Resource> SortResources(IEnumerable<IWin32Resource> resources)
	{
		return resources.OrderBy(CompareResources);
	}

	public static void SerializeWin32Resources(BlobBuilder builder, IEnumerable<IWin32Resource> theResources, int resourcesRva)
	{
		theResources = SortResources(theResources);
		Directory directory = new Directory(string.Empty, 0);
		Directory directory2 = null;
		Directory directory3 = null;
		int num = int.MinValue;
		string text = null;
		int num2 = int.MinValue;
		string text2 = null;
		uint num3 = 16u;
		foreach (IWin32Resource theResource in theResources)
		{
			int num4;
			if (theResource.TypeId >= 0 || !(theResource.TypeName != text))
			{
				num4 = ((theResource.TypeId > num) ? 1 : 0);
				if (num4 == 0)
				{
					goto IL_00c0;
				}
			}
			else
			{
				num4 = 1;
			}
			num = theResource.TypeId;
			text = theResource.TypeName;
			if (num < 0)
			{
				directory.NumberOfNamedEntries++;
			}
			else
			{
				directory.NumberOfIdEntries++;
			}
			num3 += 24;
			directory.Entries.Add(directory2 = new Directory(text, num));
			goto IL_00c0;
			IL_00c0:
			if (num4 != 0 || (theResource.Id < 0 && theResource.Name != text2) || theResource.Id > num2)
			{
				num2 = theResource.Id;
				text2 = theResource.Name;
				if (num2 < 0)
				{
					directory2.NumberOfNamedEntries++;
				}
				else
				{
					directory2.NumberOfIdEntries++;
				}
				num3 += 24;
				directory2.Entries.Add(directory3 = new Directory(text2, num2));
			}
			directory3.NumberOfIdEntries++;
			num3 += 8;
			directory3.Entries.Add(theResource);
		}
		BlobBuilder blobBuilder = new BlobBuilder();
		WriteDirectory(directory, builder, 0u, 0u, num3, resourcesRva, blobBuilder);
		builder.LinkSuffix(blobBuilder);
		builder.WriteByte(0);
		builder.Align(4);
	}

	private static void WriteDirectory(Directory directory, BlobBuilder writer, uint offset, uint level, uint sizeOfDirectoryTree, int virtualAddressBase, BlobBuilder dataWriter)
	{
		writer.WriteUInt32(0u);
		writer.WriteUInt32(0u);
		writer.WriteUInt32(0u);
		writer.WriteUInt16(directory.NumberOfNamedEntries);
		writer.WriteUInt16(directory.NumberOfIdEntries);
		uint count = (uint)directory.Entries.Count;
		uint num = offset + 16 + count * 8;
		for (int i = 0; i < count; i++)
		{
			uint num2 = (uint)dataWriter.Count + sizeOfDirectoryTree;
			uint num3 = num;
			Directory directory2 = directory.Entries[i] as Directory;
			int num4;
			string text;
			if (directory2 != null)
			{
				num4 = directory2.ID;
				text = directory2.Name;
				num = ((level != 0) ? (num + (uint)(16 + 8 * directory2.Entries.Count)) : (num + SizeOfDirectory(directory2)));
			}
			else
			{
				IWin32Resource win32Resource = (IWin32Resource)directory.Entries[i];
				num4 = level switch
				{
					1u => win32Resource.Id, 
					0u => win32Resource.TypeId, 
					_ => (int)win32Resource.LanguageId, 
				};
				text = level switch
				{
					1u => win32Resource.Name, 
					0u => win32Resource.TypeName, 
					_ => null, 
				};
				dataWriter.WriteUInt32((uint)(virtualAddressBase + sizeOfDirectoryTree + 16 + dataWriter.Count));
				byte[] array = new List<byte>(win32Resource.Data).ToArray();
				dataWriter.WriteUInt32((uint)array.Length);
				dataWriter.WriteUInt32(win32Resource.CodePage);
				dataWriter.WriteUInt32(0u);
				dataWriter.WriteBytes(array);
				while (dataWriter.Count % 4 != 0)
				{
					dataWriter.WriteByte(0);
				}
			}
			if (num4 >= 0)
			{
				writer.WriteInt32(num4);
			}
			else
			{
				if (text == null)
				{
					text = string.Empty;
				}
				writer.WriteUInt32(num2 | 0x80000000u);
				dataWriter.WriteUInt16((ushort)text.Length);
				dataWriter.WriteUTF16(text);
			}
			if (directory2 != null)
			{
				writer.WriteUInt32(num3 | 0x80000000u);
			}
			else
			{
				writer.WriteUInt32(num2);
			}
		}
		num = offset + 16 + count * 8;
		for (int j = 0; j < count; j++)
		{
			if (directory.Entries[j] is Directory directory3)
			{
				WriteDirectory(directory3, writer, num, level + 1, sizeOfDirectoryTree, virtualAddressBase, dataWriter);
				num = ((level != 0) ? (num + (uint)(16 + 8 * directory3.Entries.Count)) : (num + SizeOfDirectory(directory3)));
			}
		}
	}

	private static uint SizeOfDirectory(Directory directory)
	{
		uint count = (uint)directory.Entries.Count;
		uint num = 16 + 8 * count;
		for (int i = 0; i < count; i++)
		{
			if (directory.Entries[i] is Directory directory2)
			{
				num += (uint)(16 + 8 * directory2.Entries.Count);
			}
		}
		return num;
	}

	public static void SerializeWin32Resources(BlobBuilder builder, ResourceSection resourceSections, int resourcesRva)
	{
		BlobWriter blobWriter = new BlobWriter(builder.ReserveBytes(resourceSections.SectionBytes.Length));
		blobWriter.WriteBytes(resourceSections.SectionBytes);
		BinaryReader binaryReader = new BinaryReader(new MemoryStream(resourceSections.SectionBytes));
		uint[] relocations = resourceSections.Relocations;
		for (int i = 0; i < relocations.Length; i++)
		{
			int num = (blobWriter.Offset = (int)relocations[i]);
			binaryReader.BaseStream.Position = num;
			blobWriter.WriteUInt32(binaryReader.ReadUInt32() + (uint)resourcesRva);
		}
	}
}
