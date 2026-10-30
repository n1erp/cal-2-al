using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.TokenCache;

internal abstract class NavTokenCacheBase
{
	public static readonly byte[] EmptyData = new byte[1];

	private byte[] Data { get; set; }

	protected NavTokenCacheMetadata CurrentCacheMetadata { get; set; }

	public bool HasStateChanged { get; set; }

	protected NavTokenCacheBase()
	{
		Data = null;
	}

	public static bool IsEmptyData(byte[] data)
	{
		if (EmptyData.Length != data.Length)
		{
			return false;
		}
		for (int i = 0; i < EmptyData.Length; i++)
		{
			if (EmptyData[i] != data[i])
			{
				return false;
			}
		}
		return true;
	}

	public void Clear()
	{
		BeforeAccessNotification(null);
		CurrentCacheMetadata = null;
		Data = null;
		AfterAccessNotification(null);
	}

	public void Refresh()
	{
		BeforeAccessNotification(null);
	}

	public byte[] Serialize()
	{
		return Data;
	}

	public void Deserialize(byte[] serializedData)
	{
		Data = serializedData;
	}

	internal virtual UserTokenIdentifiers GetCurrentIdentifiers()
	{
		UserTokenIdentifiers result = null;
		if (CurrentCacheMetadata != null)
		{
			result = new UserTokenIdentifiers
			{
				UserSecurityId = ConvertGuidToString(CurrentCacheMetadata.UserSecurityId),
				AadUserUniqueId = CurrentCacheMetadata.AadUserUniqueId,
				AadTenantId = CurrentCacheMetadata.AadTenantId
			};
		}
		return result;
	}

	internal virtual void StoreAadTenantId(string aadTenantId)
	{
		if (CurrentCacheMetadata == null)
		{
			throw new NavTokenCacheException(Lang.FailedToStoreTenantIdForCache);
		}
		if (string.IsNullOrWhiteSpace(aadTenantId))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidParameterFormat, aadTenantId, "String", "aadTenantId"), "aadTenantId");
		}
		CurrentCacheMetadata.AadTenantId = aadTenantId;
	}

	internal virtual void PersistTokenCacheState(string aadTenantId, string aadUserUniqueId, byte[] tokenCacheState)
	{
	}

	protected abstract void BeforeAccessNotification(NavTokenCacheNotificationArgs args);

	protected abstract void AfterAccessNotification(NavTokenCacheNotificationArgs args);

	protected static string ConvertGuidToString(Guid guid)
	{
		if (!(guid == Guid.Empty))
		{
			return guid.ToString("D");
		}
		return null;
	}
}
