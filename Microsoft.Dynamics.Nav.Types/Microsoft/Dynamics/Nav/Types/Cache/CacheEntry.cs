using System;

namespace Microsoft.Dynamics.Nav.Types.Cache;

public sealed class CacheEntry<TValue> : IExpirableItem
{
	private TValue item;

	private long version;

	private DateTime scavengeTime;

	public TValue Item
	{
		get
		{
			return item;
		}
		set
		{
			item = value;
		}
	}

	internal long Version
	{
		get
		{
			return version;
		}
		set
		{
			version = value;
		}
	}

	public DateTime ScavengeTime
	{
		get
		{
			return scavengeTime;
		}
		set
		{
			scavengeTime = value;
		}
	}

	public CacheEntry(TValue item, long version, DateTime scavengeTime)
	{
		this.item = item;
		this.version = version;
		this.scavengeTime = scavengeTime;
	}
}
