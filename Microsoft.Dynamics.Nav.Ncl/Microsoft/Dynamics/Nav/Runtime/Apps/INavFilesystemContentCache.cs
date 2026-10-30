using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface INavFilesystemContentCache
{
	byte[] GetContent(NavFilesystemContentCacheKey cacheKey);

	string GetPath(NavFilesystemContentCacheKey cacheKey);

	void PutContent(NavFilesystemContentCacheKey cacheKey, byte[] content);

	bool HasContent(NavFilesystemContentCacheKey cacheKey);

	void PurgeUnreferencedContent(IEnumerable<Sha256Hash> referencedHashes);
}
