using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class PrimaryKeyCacheItem : DictionaryCacheItem<NavRecordId>
{
	private ReadOnlyRecordBuffer readOnlyRecordBuffer;

	private NavRecordId recordId;

	internal override NavRecordId Key
	{
		get
		{
			if (recordId == null)
			{
				recordId = readOnlyRecordBuffer.GetRecordId();
			}
			return recordId;
		}
	}

	internal ReadOnlyRecordBuffer ReadOnlyRecordBuffer => readOnlyRecordBuffer;

	public PrimaryKeyCacheItem(NavDatabase originDatabase, ReadOnlyRecordBuffer readOnlyRecordBuffer)
		: this(originDatabase, readOnlyRecordBuffer, DateTime.UtcNow)
	{
	}

	public PrimaryKeyCacheItem(NavDatabase originDatabase, ReadOnlyRecordBuffer readOnlyRecordBuffer, DateTime created)
		: base(originDatabase, created, readOnlyRecordBuffer.CacheSize)
	{
		if (originDatabase == null)
		{
			throw new ArgumentNullException("originDatabase");
		}
		this.readOnlyRecordBuffer = readOnlyRecordBuffer;
	}

	public PrimaryKeyCacheItem(NavDatabase originDatabase, NavRecordId recordId)
		: this(originDatabase, recordId, DateTime.UtcNow)
	{
	}

	public PrimaryKeyCacheItem(NavDatabase originDatabase, NavRecordId recordId, DateTime created)
		: base(originDatabase, created, 1)
	{
		if (originDatabase == null)
		{
			throw new ArgumentNullException("originDatabase");
		}
		this.recordId = recordId;
	}

	internal override bool ShouldItemBeKeptInCache(NavRecordId key)
	{
		if (readOnlyRecordBuffer == null)
		{
			return true;
		}
		NCLMetaApplicationObject metaApplicationObject = readOnlyRecordBuffer.MetaApplicationObject;
		if (readOnlyRecordBuffer != null && metaApplicationObject != null)
		{
			return !metaApplicationObject.Stale;
		}
		return true;
	}
}
