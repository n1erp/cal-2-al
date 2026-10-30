using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

internal class LocalCacheSymbolReferencePackage
{
	internal bool Found { get; }

	internal NavAppManifest Manifest { get; }

	internal Version Version { get; }

	internal string FilePath { get; }

	internal LocalCacheSymbolReferencePackage(bool found, NavAppManifest manifest = null, string filePath = null, Version version = null)
	{
		FilePath = filePath;
		Found = found;
		Manifest = manifest;
		Version = version ?? new Version(0, 0);
	}

	internal static LocalCacheSymbolReferencePackage CreatePackageNotFound()
	{
		return new LocalCacheSymbolReferencePackage(found: false);
	}

	internal static LocalCacheSymbolReferencePackage CreatePackageFound(NavAppManifest manifest, string filePath)
	{
		return new LocalCacheSymbolReferencePackage(found: true, manifest, filePath, manifest.AppVersion);
	}
}
