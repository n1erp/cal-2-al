using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class GetBlobContentCacheRequest : DataCacheRequest
{
	private readonly IRecordBuffer recordBuffer;

	private readonly FieldList blobsToGet;

	public IRecordBuffer RecordBuffer
	{
		[DebuggerStepThrough]
		get
		{
			return recordBuffer;
		}
	}

	public FieldList BlobsToGet
	{
		[DebuggerStepThrough]
		get
		{
			return blobsToGet;
		}
	}

	public GetBlobContentCacheRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, bool cacheable, IRecordBuffer recordBuffer, FieldList blobsToGet)
		: base(companyToken, metaApplicationObject, cacheable, FiltersAndMarks.Empty, null, SecurityFiltering.Ignored)
	{
		if (recordBuffer == null)
		{
			throw new ArgumentNullException("recordBuffer");
		}
		if (blobsToGet == null)
		{
			throw new ArgumentNullException("blobsToGet");
		}
		this.recordBuffer = recordBuffer;
		this.blobsToGet = blobsToGet;
	}
}
