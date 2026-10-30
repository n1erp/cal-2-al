using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ExistsCacheItem : DataCacheRequestItem
{
	private bool exists;

	internal bool Exists => exists;

	internal ExistsCacheItem(ResultSetGroupCacheItem groupParent, ExistsCacheRequest request, bool existsParameter, DateTime created)
		: base(groupParent, request, created, ApproximateByteSize(request))
	{
		exists = existsParameter;
	}

	private static int ApproximateByteSize(DataCacheRequest request)
	{
		return request.ApproximateByteSize + SizableObject.GetApproximateByteSize(1) + SizableObject.GetApproximateByteSize(5) + 24;
	}
}
