namespace Microsoft.CodeAnalysis;

internal class RESOURCE
{
	internal RESOURCE_STRING pstringType;

	internal RESOURCE_STRING pstringName;

	internal uint DataSize;

	internal uint HeaderSize;

	internal uint DataVersion;

	internal ushort MemoryFlags;

	internal ushort LanguageId;

	internal uint Version;

	internal uint Characteristics;

	internal byte[] data;
}
