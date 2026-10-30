using System;

namespace Microsoft.CodeAnalysis;

[Flags]
public enum AssemblyIdentityParts
{
	Name = 1,
	Version = 0x1E,
	VersionMajor = 2,
	VersionMinor = 4,
	VersionBuild = 8,
	VersionRevision = 0x10,
	Culture = 0x20,
	PublicKey = 0x40,
	PublicKeyToken = 0x80,
	PublicKeyOrToken = PublicKey | PublicKeyToken,
	Retargetability = 0x100,
	ContentType = 0x200,
	Unknown = 0x400
}
