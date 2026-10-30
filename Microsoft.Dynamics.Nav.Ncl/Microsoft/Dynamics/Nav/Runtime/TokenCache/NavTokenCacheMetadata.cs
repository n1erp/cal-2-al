using System;

namespace Microsoft.Dynamics.Nav.Runtime.TokenCache;

internal class NavTokenCacheMetadata
{
	public Guid UserSecurityId { get; set; }

	public string AadUserUniqueId { get; set; }

	public string AadTenantId { get; set; }

	public DateTime CacheWriteTime { get; set; }
}
