using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis;

internal static class Win32ResourceConversions
{
	private struct ICONDIRENTRY
	{
		internal byte bWidth;

		internal byte bHeight;

		internal byte bColorCount;

		internal byte bReserved;

		internal ushort wPlanes;

		internal ushort wBitCount;

		internal uint dwBytesInRes;

		internal uint dwImageOffset;
	}

	private class VersionResourceSerializer
	{
		private readonly string _commentsContents;

		private readonly string _companyNameContents;

		private readonly string _fileDescriptionContents;

		private readonly string _fileVersionContents;

		private readonly string _internalNameContents;

		private readonly string _legalCopyrightContents;

		private readonly string _legalTrademarksContents;

		private readonly string _originalFileNameContents;

		private readonly string _productNameContents;

		private readonly string _productVersionContents;

		private readonly Version _assemblyVersionContents;

		private const string vsVersionInfoKey = "VS_VERSION_INFO";

		private const string varFileInfoKey = "VarFileInfo";

		private const string translationKey = "Translation";

		private const string stringFileInfoKey = "StringFileInfo";

		private readonly string _langIdAndCodePageKey;

		private const uint CP_WINUNICODE = 1200u;

		private const ushort sizeVS_FIXEDFILEINFO = 52;

		private readonly bool _isDll;

		private const uint VFT_APP = 1u;

		private const uint VFT_DLL = 2u;

		private const int HDRSIZE = 6;

		private uint FileType
		{
			get
			{
				if (!_isDll)
				{
					return 1u;
				}
				return 2u;
			}
		}

		internal VersionResourceSerializer(bool isDll, string comments, string companyName, string fileDescription, string fileVersion, string internalName, string legalCopyright, string legalTrademark, string originalFileName, string productName, string productVersion, Version assemblyVersion)
		{
			_isDll = isDll;
			_commentsContents = comments;
			_companyNameContents = companyName;
			_fileDescriptionContents = fileDescription;
			_fileVersionContents = fileVersion;
			_internalNameContents = internalName;
			_legalCopyrightContents = legalCopyright;
			_legalTrademarksContents = legalTrademark;
			_originalFileNameContents = originalFileName;
			_productNameContents = productName;
			_productVersionContents = productVersion;
			_assemblyVersionContents = assemblyVersion;
			_langIdAndCodePageKey = $"{0:x4}{1200u:x4}";
		}

		private IEnumerable<KeyValuePair<string, string>> GetVerStrings()
		{
			if (_commentsContents != null)
			{
				yield return new KeyValuePair<string, string>("Comments", _commentsContents);
			}
			if (_companyNameContents != null)
			{
				yield return new KeyValuePair<string, string>("CompanyName", _companyNameContents);
			}
			if (_fileDescriptionContents != null)
			{
				yield return new KeyValuePair<string, string>("FileDescription", _fileDescriptionContents);
			}
			yield return new KeyValuePair<string, string>("FileVersion", _fileVersionContents);
			if (_internalNameContents != null)
			{
				yield return new KeyValuePair<string, string>("InternalName", _internalNameContents);
			}
			if (_legalCopyrightContents != null)
			{
				yield return new KeyValuePair<string, string>("LegalCopyright", _legalCopyrightContents);
			}
			if (_legalTrademarksContents != null)
			{
				yield return new KeyValuePair<string, string>("LegalTrademarks", _legalTrademarksContents);
			}
			if (_originalFileNameContents != null)
			{
				yield return new KeyValuePair<string, string>("OriginalFilename", _originalFileNameContents);
			}
			if (_productNameContents != null)
			{
				yield return new KeyValuePair<string, string>("ProductName", _productNameContents);
			}
			yield return new KeyValuePair<string, string>("ProductVersion", _productVersionContents);
			if (_assemblyVersionContents != null)
			{
				yield return new KeyValuePair<string, string>("Assembly Version", _assemblyVersionContents.ToString());
			}
		}

		private void WriteVSFixedFileInfo(BinaryWriter writer)
		{
			VersionHelper.TryParse(_fileVersionContents, out var version);
			VersionHelper.TryParse(_productVersionContents, out var version2);
			writer.Write(4277077181u);
			writer.Write(65536u);
			writer.Write((uint)((version.Major << 16) | version.Minor));
			writer.Write((uint)((version.Build << 16) | version.Revision));
			writer.Write((uint)((version2.Major << 16) | version2.Minor));
			writer.Write((uint)((version2.Build << 16) | version2.Revision));
			writer.Write(63u);
			writer.Write(0u);
			writer.Write(4u);
			writer.Write(FileType);
			writer.Write(0u);
			writer.Write(0u);
			writer.Write(0u);
		}

		private static int PadKeyLen(int cb)
		{
			return PadToDword(cb + 6) - 6;
		}

		private static int PadToDword(int cb)
		{
			return (cb + 3) & -4;
		}

		private static ushort SizeofVerString(string lpszKey, string lpszValue)
		{
			int cb = (lpszKey.Length + 1) * 2;
			int num = (lpszValue.Length + 1) * 2;
			return checked((ushort)(PadKeyLen(cb) + num + 6));
		}

		private static void WriteVersionString(KeyValuePair<string, string> keyValuePair, BinaryWriter writer)
		{
			ushort value = SizeofVerString(keyValuePair.Key, keyValuePair.Value);
			int num = (keyValuePair.Key.Length + 1) * 2;
			_ = keyValuePair.Value.Length;
			_ = writer.BaseStream.Position;
			writer.Write(value);
			writer.Write((ushort)(keyValuePair.Value.Length + 1));
			writer.Write((ushort)1);
			writer.Write(keyValuePair.Key.ToCharArray());
			writer.Write((ushort)0);
			writer.Write(new byte[PadKeyLen(num) - num]);
			writer.Write(keyValuePair.Value.ToCharArray());
			writer.Write((ushort)0);
		}

		private static int KEYSIZE(string sz)
		{
			return PadKeyLen((sz.Length + 1) * 2) / 2;
		}

		private static int KEYBYTES(string sz)
		{
			return KEYSIZE(sz) * 2;
		}

		private int GetStringsSize()
		{
			int num = 0;
			foreach (KeyValuePair<string, string> verString in GetVerStrings())
			{
				num = (num + 3) & -4;
				num += SizeofVerString(verString.Key, verString.Value);
			}
			return num;
		}

		internal int GetDataSize()
		{
			int num = 34 + KEYBYTES("VS_VERSION_INFO") + KEYBYTES("VarFileInfo") + KEYBYTES("Translation") + KEYBYTES("StringFileInfo") + KEYBYTES(_langIdAndCodePageKey) + 52;
			return GetStringsSize() + num;
		}

		internal void WriteVerResource(BinaryWriter writer)
		{
			_ = writer.BaseStream.Position;
			int dataSize = GetDataSize();
			writer.Write((ushort)dataSize);
			writer.Write((ushort)52);
			writer.Write((ushort)0);
			writer.Write("VS_VERSION_INFO".ToCharArray());
			writer.Write(new byte[KEYBYTES("VS_VERSION_INFO") - "VS_VERSION_INFO".Length * 2]);
			WriteVSFixedFileInfo(writer);
			writer.Write((ushort)(16 + KEYBYTES("VarFileInfo") + KEYBYTES("Translation")));
			writer.Write((ushort)0);
			writer.Write((ushort)1);
			writer.Write("VarFileInfo".ToCharArray());
			writer.Write(new byte[KEYBYTES("VarFileInfo") - "VarFileInfo".Length * 2]);
			writer.Write((ushort)(10 + KEYBYTES("Translation")));
			writer.Write((ushort)4);
			writer.Write((ushort)0);
			writer.Write("Translation".ToCharArray());
			writer.Write(new byte[KEYBYTES("Translation") - "Translation".Length * 2]);
			writer.Write((ushort)0);
			writer.Write((ushort)1200);
			writer.Write((ushort)(12 + KEYBYTES("StringFileInfo") + KEYBYTES(_langIdAndCodePageKey) + GetStringsSize()));
			writer.Write((ushort)0);
			writer.Write((ushort)1);
			writer.Write("StringFileInfo".ToCharArray());
			writer.Write(new byte[KEYBYTES("StringFileInfo") - "StringFileInfo".Length * 2]);
			writer.Write((ushort)(6 + KEYBYTES(_langIdAndCodePageKey) + GetStringsSize()));
			writer.Write((ushort)0);
			writer.Write((ushort)1);
			writer.Write(_langIdAndCodePageKey.ToCharArray());
			writer.Write(new byte[KEYBYTES(_langIdAndCodePageKey) - _langIdAndCodePageKey.Length * 2]);
			_ = writer.BaseStream.Position;
			foreach (KeyValuePair<string, string> verString in GetVerStrings())
			{
				long position = writer.BaseStream.Position;
				writer.Write(new byte[((position + 3) & -4) - position]);
				WriteVersionString(verString, writer);
			}
		}
	}

	internal static void AppendIconToResourceStream(Stream resStream, Stream iconStream)
	{
		BinaryReader binaryReader = new BinaryReader(iconStream);
		if (binaryReader.ReadUInt16() != 0)
		{
			throw new ResourceException(CodeAnalysisResources.IconStreamUnexpectedFormat);
		}
		if (binaryReader.ReadUInt16() != 1)
		{
			throw new ResourceException(CodeAnalysisResources.IconStreamUnexpectedFormat);
		}
		ushort num = binaryReader.ReadUInt16();
		if (num == 0)
		{
			throw new ResourceException(CodeAnalysisResources.IconStreamUnexpectedFormat);
		}
		ICONDIRENTRY[] array = new ICONDIRENTRY[num];
		for (ushort num2 = 0; num2 < num; num2++)
		{
			array[num2].bWidth = binaryReader.ReadByte();
			array[num2].bHeight = binaryReader.ReadByte();
			array[num2].bColorCount = binaryReader.ReadByte();
			array[num2].bReserved = binaryReader.ReadByte();
			array[num2].wPlanes = binaryReader.ReadUInt16();
			array[num2].wBitCount = binaryReader.ReadUInt16();
			array[num2].dwBytesInRes = binaryReader.ReadUInt32();
			array[num2].dwImageOffset = binaryReader.ReadUInt32();
		}
		for (ushort num3 = 0; num3 < num; num3++)
		{
			iconStream.Position = array[num3].dwImageOffset;
			if (binaryReader.ReadUInt32() == 40)
			{
				iconStream.Position += 8L;
				array[num3].wPlanes = binaryReader.ReadUInt16();
				array[num3].wBitCount = binaryReader.ReadUInt16();
			}
		}
		BinaryWriter binaryWriter = new BinaryWriter(resStream);
		for (ushort num4 = 0; num4 < num; num4++)
		{
			resStream.Position = (resStream.Position + 3) & -4;
			binaryWriter.Write(array[num4].dwBytesInRes);
			binaryWriter.Write(32u);
			binaryWriter.Write(ushort.MaxValue);
			binaryWriter.Write((ushort)3);
			binaryWriter.Write(ushort.MaxValue);
			binaryWriter.Write((ushort)(num4 + 1));
			binaryWriter.Write(0u);
			binaryWriter.Write((ushort)4112);
			binaryWriter.Write((ushort)0);
			binaryWriter.Write(0u);
			binaryWriter.Write(0u);
			iconStream.Position = array[num4].dwImageOffset;
			binaryWriter.Write(binaryReader.ReadBytes(checked((int)array[num4].dwBytesInRes)));
		}
		resStream.Position = (resStream.Position + 3) & -4;
		binaryWriter.Write((uint)(6 + num * 14));
		binaryWriter.Write(32u);
		binaryWriter.Write(ushort.MaxValue);
		binaryWriter.Write((ushort)14);
		binaryWriter.Write(ushort.MaxValue);
		binaryWriter.Write((ushort)32512);
		binaryWriter.Write(0u);
		binaryWriter.Write((ushort)4144);
		binaryWriter.Write((ushort)0);
		binaryWriter.Write(0u);
		binaryWriter.Write(0u);
		binaryWriter.Write((ushort)0);
		binaryWriter.Write((ushort)1);
		binaryWriter.Write(num);
		for (ushort num5 = 0; num5 < num; num5++)
		{
			binaryWriter.Write(array[num5].bWidth);
			binaryWriter.Write(array[num5].bHeight);
			binaryWriter.Write(array[num5].bColorCount);
			binaryWriter.Write(array[num5].bReserved);
			binaryWriter.Write(array[num5].wPlanes);
			binaryWriter.Write(array[num5].wBitCount);
			binaryWriter.Write(array[num5].dwBytesInRes);
			binaryWriter.Write((ushort)(num5 + 1));
		}
	}

	internal static void AppendVersionToResourceStream(Stream resStream, bool isDll, string fileVersion, string originalFileName, string internalName, string productVersion, Version assemblyVersion, string fileDescription = " ", string legalCopyright = " ", string legalTrademarks = null, string productName = null, string comments = null, string companyName = null)
	{
		BinaryWriter binaryWriter = new BinaryWriter(resStream, Encoding.Unicode);
		resStream.Position = (resStream.Position + 3) & -4;
		VersionResourceSerializer versionResourceSerializer = new VersionResourceSerializer(isDll, comments, companyName, fileDescription, fileVersion, internalName, legalCopyright, legalTrademarks, originalFileName, productName, productVersion, assemblyVersion);
		_ = resStream.Position;
		int dataSize = versionResourceSerializer.GetDataSize();
		binaryWriter.Write((uint)dataSize);
		binaryWriter.Write(32u);
		binaryWriter.Write(ushort.MaxValue);
		binaryWriter.Write((ushort)16);
		binaryWriter.Write(ushort.MaxValue);
		binaryWriter.Write((ushort)1);
		binaryWriter.Write(0u);
		binaryWriter.Write((ushort)48);
		binaryWriter.Write((ushort)0);
		binaryWriter.Write(0u);
		binaryWriter.Write(0u);
		versionResourceSerializer.WriteVerResource(binaryWriter);
	}

	internal static void AppendManifestToResourceStream(Stream resStream, Stream manifestStream, bool isDll)
	{
		resStream.Position = (resStream.Position + 3) & -4;
		BinaryWriter binaryWriter = new BinaryWriter(resStream);
		binaryWriter.Write((uint)manifestStream.Length);
		binaryWriter.Write(32u);
		binaryWriter.Write(ushort.MaxValue);
		binaryWriter.Write((ushort)24);
		binaryWriter.Write(ushort.MaxValue);
		binaryWriter.Write((ushort)((!isDll) ? 1u : 2u));
		binaryWriter.Write(0u);
		binaryWriter.Write((ushort)4144);
		binaryWriter.Write((ushort)0);
		binaryWriter.Write(0u);
		binaryWriter.Write(0u);
		manifestStream.CopyTo(resStream);
	}
}
