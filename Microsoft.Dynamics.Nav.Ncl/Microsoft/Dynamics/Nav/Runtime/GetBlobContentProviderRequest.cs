using System;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class GetBlobContentProviderRequest : DataProviderRequest
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

	public GetBlobContentProviderRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, IRecordBuffer recordBuffer, FieldList blobsToGet, DataLockState dataLockState)
		: base(companyToken, metaApplicationObject, dataLockState, FiltersAndMarks.Empty, null, SecurityFiltering.Ignored)
	{
		if (recordBuffer == null)
		{
			throw new ArgumentNullException("recordBuffer");
		}
		if (blobsToGet == null)
		{
			throw new ArgumentNullException("blobsToGet");
		}
		if (blobsToGet.AsNclMetaFields().Any((NCLMetaField field) => field.Parent != metaApplicationObject))
		{
			throw new ArgumentException("Fields must be from same applicationobject");
		}
		this.recordBuffer = recordBuffer;
		this.blobsToGet = blobsToGet;
	}

	public GetBlobContentProviderRequest(DataProviderRequest dataProviderRequest, IRecordBuffer recordBuffer, FieldList blobsToGet)
		: this(dataProviderRequest.CompanyToken, dataProviderRequest.MetaApplicationObject, recordBuffer, blobsToGet, dataProviderRequest.LockState)
	{
	}
}
