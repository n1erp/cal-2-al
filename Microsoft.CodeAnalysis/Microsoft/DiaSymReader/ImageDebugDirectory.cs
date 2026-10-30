using System.Runtime.InteropServices;

namespace Microsoft.DiaSymReader;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct ImageDebugDirectory
{
	internal int Characteristics;

	internal int TimeDateStamp;

	internal short MajorVersion;

	internal short MinorVersion;

	internal int Type;

	internal int SizeOfData;

	internal int AddressOfRawData;

	internal int PointerToRawData;
}
