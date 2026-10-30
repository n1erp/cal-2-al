namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class LRUCacheItem
{
	private LRUCache lruCache;

	private LRUCacheItem prev;

	private LRUCacheItem next;

	private long size;

	private readonly IReference<NavDatabase> originDatabaseReference;

	protected internal LRUCache LRUCache
	{
		get
		{
			return lruCache;
		}
		set
		{
			lruCache = value;
		}
	}

	protected internal LRUCacheItem Prev
	{
		get
		{
			return prev;
		}
		set
		{
			prev = value;
		}
	}

	protected internal LRUCacheItem Next
	{
		get
		{
			return next;
		}
		set
		{
			next = value;
		}
	}

	protected internal long Size
	{
		internal get
		{
			return size;
		}
		set
		{
			size = value;
		}
	}

	internal bool IsLinked => lruCache != null;

	internal NavDatabase OriginDatabase => originDatabaseReference.Target;

	protected LRUCacheItem(NavDatabase originDatabase, long size)
	{
		IReference<NavDatabase> reference;
		if (originDatabase != null)
		{
			reference = originDatabase.GetReference();
		}
		else
		{
			IReference<NavDatabase> reference2 = new Reference<NavDatabase>();
			reference = reference2;
		}
		originDatabaseReference = reference;
		this.size = size;
	}

	internal abstract void ItemHasBeenRemovedByScavenging();
}
