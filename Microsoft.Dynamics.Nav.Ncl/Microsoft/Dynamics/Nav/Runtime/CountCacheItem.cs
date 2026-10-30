using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CountCacheItem : DataCacheRequestItem
{
	private int count;

	internal int Count => count;

	internal CountCacheItem(ResultSetGroupCacheItem groupParent, DataCacheRequest request, int countParameter, DateTime created)
		: base(groupParent, request, created, ApproximateByteSize(request))
	{
		count = countParameter;
	}

	private static int ApproximateByteSize(DataCacheRequest request)
	{
		return request.ApproximateByteSize + SizableObject.GetApproximateByteSize(1) + SizableObject.GetApproximateByteSize(5) + 24;
	}
}
