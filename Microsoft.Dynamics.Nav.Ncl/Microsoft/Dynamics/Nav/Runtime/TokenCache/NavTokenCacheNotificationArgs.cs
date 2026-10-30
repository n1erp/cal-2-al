namespace Microsoft.Dynamics.Nav.Runtime.TokenCache;

internal class NavTokenCacheNotificationArgs
{
	public string ClientId { get; }

	public string DisplayableId { get; }

	public string Resource { get; }

	public byte[] TokenCache { get; }

	public string UniqueId { get; }
}
