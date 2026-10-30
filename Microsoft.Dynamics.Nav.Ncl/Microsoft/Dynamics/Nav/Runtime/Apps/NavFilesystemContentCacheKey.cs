using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavFilesystemContentCacheKey
{
	public Sha256Hash ContentHash { get; }

	public string FileExtension { get; }

	public NavFilesystemContentCacheKey(Sha256Hash contentHash, string fileExtension)
	{
		ContentHash = contentHash ?? throw new ArgumentNullException("contentHash");
		FileExtension = fileExtension ?? throw new ArgumentNullException("fileExtension");
	}
}
