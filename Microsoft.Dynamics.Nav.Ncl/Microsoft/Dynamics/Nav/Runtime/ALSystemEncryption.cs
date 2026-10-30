using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Encryption;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALSystemEncryption
{
	private static ISystemEncryptionProvider GetOrCreateTenantEncryptionProvider()
	{
		if (NavCurrentThread.Session.EncryptionProvider == null)
		{
			NavCurrentThread.Session.EncryptionProvider = TenantEncryptionProviderFactory.GetTenantEncryptionProvider();
		}
		return GetTenantEncryptionProvider();
	}

	private static ISystemEncryptionProvider GetTenantEncryptionProvider()
	{
		return NavCurrentThread.Session.EncryptionProvider;
	}

	private static void ClearCachedTenantEncryptionProviderInstance()
	{
		NavCurrentThread.Session.EncryptionProvider = null;
	}

	public static string ALEncrypt(string plaintext)
	{
		string returnValue = null;
		TryInvoke(() =>
		{
			returnValue = GetOrCreateTenantEncryptionProvider().Encrypt(plaintext);
		});
		return returnValue;
	}

	public static string ALDecrypt(string encryptedText)
	{
		string returnValue = null;
		TryInvoke(() =>
		{
			returnValue = GetOrCreateTenantEncryptionProvider().Decrypt(encryptedText);
		});
		return returnValue;
	}

	public static string ALExportKey(string password)
	{
		NavSession session = NavCurrentThread.Session;
		session.GetPermissionSet(null, new ApplicationObjectId[1]
		{
			new ApplicationObjectId(ObjectType.System, 5410)
		}).VerifyPermissions(null, PermissionMask.Execute, session.LimitedUserPermissionChecker);
		string filename = Path.Combine(NavEnvironment.Instance.TemporaryPathHelper.TempPath, Guid.NewGuid().ToString() + ".key");
		TryInvoke(() =>
		{
			GetOrCreateTenantEncryptionProvider().ExportKey(filename, password);
		});
		return filename;
	}

	public static bool ALImportKey(string keyFileName, string password)
	{
		return ALImportKey(DataError.ThrowError, keyFileName, password);
	}

	public static bool ALImportKey(DataError errorLevel, string keyFileName, string password)
	{
		try
		{
			NavSession session = NavCurrentThread.Session;
			session.GetPermissionSet(null, new ApplicationObjectId[1]
			{
				new ApplicationObjectId(ObjectType.System, 5420)
			}).VerifyPermissions(null, PermissionMask.Execute, session.LimitedUserPermissionChecker);
			TryInvoke(() =>
			{
				GetOrCreateTenantEncryptionProvider().ImportKey(keyFileName, password);
				EncryptPendingData();
			});
			return true;
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	public static bool ALCreateKey()
	{
		return ALCreateKey(DataError.ThrowError);
	}

	public static bool ALCreateKey(DataError errorLevel)
	{
		try
		{
			TryInvoke(() =>
			{
				GetOrCreateTenantEncryptionProvider().CreateKey();
			});
			EncryptPendingData();
			return true;
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	public static void ALDeleteKey()
	{
		NavSession session = NavCurrentThread.Session;
		session.GetPermissionSet(null, new ApplicationObjectId[1]
		{
			new ApplicationObjectId(ObjectType.System, 5420)
		}).VerifyPermissions(null, PermissionMask.Execute, session.LimitedUserPermissionChecker);
		DecryptTenantData();
		TryInvoke(() =>
		{
			GetOrCreateTenantEncryptionProvider().DeleteKey();
		});
	}

	private static void EncryptPendingData()
	{
		IsolatedStorageRepository.EncryptPendingData();
	}

	private static void DecryptTenantData()
	{
		IsolatedStorageRepository.DecryptData();
	}

	public static bool ALKeyExists()
	{
		bool returnValue = false;
		TryInvoke(() =>
		{
			returnValue = GetOrCreateTenantEncryptionProvider().IsKeyPresent;
		});
		return returnValue;
	}

	public static bool ALEncryptionEnabled()
	{
		bool hasKey = false;
		TryInvoke(() =>
		{
			ISystemEncryptionProvider tenantEncryptionProvider = GetTenantEncryptionProvider();
			if (tenantEncryptionProvider != null)
			{
				hasKey = tenantEncryptionProvider.IsKeyCreated;
				if (hasKey)
				{
					return;
				}
				ClearCachedTenantEncryptionProviderInstance();
			}
			hasKey = GetOrCreateTenantEncryptionProvider().IsKeyCreated;
		});
		return hasKey;
	}

	private static void TryInvoke(Action action)
	{
		try
		{
			action();
		}
		catch (CryptographicException innerException)
		{
			throw new NavUnknownEncryptionException(Lang.MSGREUNKNOWN, innerException);
		}
	}
}
