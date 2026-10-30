using System;
using System.Reflection.PortableExecutable;

namespace Microsoft.Cci;

internal sealed class ModulePropertiesForSerialization
{
	public readonly int FileAlignment;

	public readonly int SectionAlignment;

	public readonly string TargetRuntimeVersion;

	public readonly Machine Machine;

	public readonly Guid PersistentIdentifier;

	public readonly ulong BaseAddress;

	public readonly ulong SizeOfHeapReserve;

	public readonly ulong SizeOfHeapCommit;

	public readonly ulong SizeOfStackReserve;

	public readonly ulong SizeOfStackCommit;

	public readonly ushort MajorSubsystemVersion;

	public readonly ushort MinorSubsystemVersion;

	public readonly byte LinkerMajorVersion;

	public readonly byte LinkerMinorVersion;

	public const ulong DefaultExeBaseAddress32Bit = 4194304uL;

	public const ulong DefaultExeBaseAddress64Bit = 5368709120uL;

	public const ulong DefaultDllBaseAddress32Bit = 268435456uL;

	public const ulong DefaultDllBaseAddress64Bit = 6442450944uL;

	public const ulong DefaultSizeOfHeapReserve32Bit = 1048576uL;

	public const ulong DefaultSizeOfHeapReserve64Bit = 4194304uL;

	public const ulong DefaultSizeOfHeapCommit32Bit = 4096uL;

	public const ulong DefaultSizeOfHeapCommit64Bit = 8192uL;

	public const ulong DefaultSizeOfStackReserve32Bit = 1048576uL;

	public const ulong DefaultSizeOfStackReserve64Bit = 4194304uL;

	public const ulong DefaultSizeOfStackCommit32Bit = 4096uL;

	public const ulong DefaultSizeOfStackCommit64Bit = 16384uL;

	public const ushort DefaultFileAlignment32Bit = 512;

	public const ushort DefaultFileAlignment64Bit = 512;

	public const ushort DefaultSectionAlignment = 8192;

	public DllCharacteristics DllCharacteristics { get; }

	public Characteristics ImageCharacteristics { get; }

	public Subsystem Subsystem { get; }

	public CorFlags CorFlags { get; }

	internal ModulePropertiesForSerialization(Guid persistentIdentifier, CorFlags corFlags, int fileAlignment, int sectionAlignment, string targetRuntimeVersion, Machine machine, ulong baseAddress, ulong sizeOfHeapReserve, ulong sizeOfHeapCommit, ulong sizeOfStackReserve, ulong sizeOfStackCommit, DllCharacteristics dllCharacteristics, Characteristics imageCharacteristics, Subsystem subsystem, ushort majorSubsystemVersion, ushort minorSubsystemVersion, byte linkerMajorVersion, byte linkerMinorVersion)
	{
		PersistentIdentifier = persistentIdentifier;
		FileAlignment = fileAlignment;
		SectionAlignment = sectionAlignment;
		TargetRuntimeVersion = targetRuntimeVersion;
		Machine = machine;
		BaseAddress = baseAddress;
		SizeOfHeapReserve = sizeOfHeapReserve;
		SizeOfHeapCommit = sizeOfHeapCommit;
		SizeOfStackReserve = sizeOfStackReserve;
		SizeOfStackCommit = sizeOfStackCommit;
		LinkerMajorVersion = linkerMajorVersion;
		LinkerMinorVersion = linkerMinorVersion;
		MajorSubsystemVersion = majorSubsystemVersion;
		MinorSubsystemVersion = minorSubsystemVersion;
		ImageCharacteristics = imageCharacteristics;
		Subsystem = subsystem;
		DllCharacteristics = dllCharacteristics;
		CorFlags = corFlags;
	}
}
