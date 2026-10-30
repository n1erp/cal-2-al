using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALIsolatedStorage
{
	public static bool ALSet(DataError dataError, string key, string value)
	{
		return ALSet(dataError, key, value, DataScope.Module);
	}

	public static bool ALSet(DataError dataError, string key, string value, DataScope dataScope)
	{
		return Set(dataError, key, value, dataScope, EncryptionStatus.PlainText);
	}

	public static bool ALSetEncrypted(DataError dataError, string key, string value)
	{
		return ALSetEncrypted(dataError, key, value, DataScope.Module);
	}

	public static bool ALSetEncrypted(DataError dataError, string key, string value, DataScope dataScope)
	{
		return Set(dataError, key, ALSystemEncryption.ALEncrypt(value), dataScope, EncryptionStatus.Encrypted);
	}

	private static bool Set(DataError dataError, string key, string value, DataScope dataScope, EncryptionStatus encryptionStatus)
	{
		return IsolatedStorageRepository.Set(dataError, GetCurrentApp(), dataScope, GetCompanyByScope(dataScope), GetUserByScope(dataScope), key, value, encryptionStatus);
	}

	public static bool ALGet(DataError dataError, string key, ByRef<NavText> value)
	{
		return ALGet(dataError, key, DataScope.Module, value);
	}

	public static bool ALGet(DataError dataError, string key, DataScope dataScope, ByRef<NavText> value)
	{
		if (value == null)
		{
			throw new NavNCLArgumentException("value");
		}
		(bool, EncryptionStatus) tuple = IsolatedStorageRepository.Get(dataError, GetCurrentApp(), dataScope, GetCompanyByScope(dataScope), GetUserByScope(dataScope), key, value);
		if (tuple.Item1)
		{
			if (tuple.Item2.In(EncryptionStatus.PendingForEncryption, EncryptionStatus.Encrypted) && !ALSystemEncryption.ALEncryptionEnabled())
			{
				throw new NavALException(string.Format(CultureInfo.CurrentCulture, Lang.IsolatedStorageNoEncryptionKey));
			}
			if (tuple.Item2 == EncryptionStatus.Encrypted)
			{
				value.Value = NavText.Create(ALSystemEncryption.ALDecrypt(value.Value.Value));
			}
		}
		return tuple.Item1;
	}

	public static bool ALContains(string key, DataScope dataScope = DataScope.Module)
	{
		return IsolatedStorageRepository.Contains(GetCurrentApp(), dataScope, GetCompanyByScope(dataScope), GetUserByScope(dataScope), key);
	}

	public static bool ALDelete(DataError dataError, string key)
	{
		return ALDelete(dataError, key, DataScope.Module);
	}

	public static bool ALDelete(DataError dataError, string key, DataScope dataScope)
	{
		return IsolatedStorageRepository.Delete(dataError, GetCurrentApp(), dataScope, GetCompanyByScope(dataScope), GetUserByScope(dataScope), key);
	}

	private static string GetCompanyByScope(DataScope dataScope)
	{
		if (dataScope.In(DataScope.Company, DataScope.CompanyAndUser))
		{
			string text = NavCurrentThread.Session?.Company?.CompanyName ?? string.Empty;
			if (string.IsNullOrEmpty(text))
			{
				throw new NavALException(Lang.IsolatedStorageCompanyIsNotSet);
			}
			return text;
		}
		return string.Empty;
	}

	private static NavGuid GetUserByScope(DataScope dataScope)
	{
		if (dataScope.In(DataScope.User, DataScope.CompanyAndUser))
		{
			Guid initValue = NavCurrentThread.Session?.User?.Id ?? Guid.Empty;
			return new NavGuid(initValue);
		}
		return NavGuid.Default;
	}

	private static NavGuid GetCurrentApp()
	{
		Guid initValue = ((WrappedValue<Guid>)(object)NavCurrentThread.Session?.CurrentMethodScope?.ApplicationObject?.GetOwningAppId())?.Value ?? Guid.Empty;
		return new NavGuid(initValue);
	}
}
