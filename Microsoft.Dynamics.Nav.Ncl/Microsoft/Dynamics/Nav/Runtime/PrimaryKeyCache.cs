namespace Microsoft.Dynamics.Nav.Runtime;

internal class PrimaryKeyCache : DictionaryCache<NavRecordId, PrimaryKeyCacheItem>
{
	public PrimaryKeyCache(LRUCache lruCache, NavDatabase originDatabase, int transactionBumperToken)
		: base(lruCache, originDatabase, transactionBumperToken)
	{
	}
}
