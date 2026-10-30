using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model.Authentication;
using Microsoft.Dynamics.Nav.LicensingService.Model.Exceptions;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

namespace Microsoft.Dynamics.Nav.Runtime.Encryption;

public sealed class TenantAzureKeyVaultEncryptionProvider : ISystemEncryptionProvider, IDisposable, IAzureKeyVaultSecretProvider
{
	private readonly KeyVaultClient keyVaultClient;

	private readonly AzureKeyVaultSettings azureKeyVaultSettings;

	private readonly IAuthenticationHandler authenticationHandler;

	private readonly string aadTenantId;

	public bool IsKeyCreated => true;

	public bool IsKeyPresent => true;

	public string PublicKey
	{
		get
		{
			throw new NavNCLNotSupportedOperationException();
		}
	}

	public TenantAzureKeyVaultEncryptionProvider(AzureKeyVaultSettings azureKeyVaultSettings, IAuthenticationHandler authenticationHandler, string aadTenantId)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected Obj, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected Obj, but got Unknown
		this.aadTenantId = aadTenantId;
		this.authenticationHandler = authenticationHandler;
		this.azureKeyVaultSettings = azureKeyVaultSettings;
		keyVaultClient = new KeyVaultClient((AuthenticationCallback)GetTokenFactory(this.azureKeyVaultSettings).Invoke);
	}

	public void CreateKey()
	{
		throw new NavNCLNotSupportedOperationException();
	}

	public string Decrypt(string encryptedText)
	{
		using (NavCurrentThread.Session.Diagnostics.CreateTraceScope("0000047", DiagnosticParameter.SystemMetadata("Invoking Azure KeyVault to decrypt"), MonitorType.None, Category.Runtime))
		{
			byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
			return RunCryptoProviderMethod(() => DecryptWithFallback(azureKeyVaultSettings.KeyIdentifier, azureKeyVaultSettings.EncryptionAlgorithm, encryptedBytes));
		}
	}

	private string DecryptWithFallback(string keyId, string algorithm, byte[] encryptedBytes)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		string text = DecryptInternal(keyId, algorithm, encryptedBytes);
		if (text != null)
		{
			return text;
		}
		KeyIdentifier val = new KeyIdentifier(keyId);
		ListKeysResponseMessage val2 = keyVaultClient.GetKeyVersionsAsync(((ObjectIdentifier)val).Vault, ((ObjectIdentifier)val).Name, (int?)null, default(CancellationToken)).WaitThrowingOriginalException();
		foreach (KeyItem item in val2.Value.Where((KeyItem kv) =>
		{
			KeyAttributes attributes = kv.Attributes;
			return attributes != null && attributes.Enabled == true;
		}))
		{
			if (!string.Equals(((ObjectIdentifier)item.Identifier).Identifier, ((ObjectIdentifier)val).Identifier, StringComparison.OrdinalIgnoreCase))
			{
				DiagnosticsResolver.GetMostSpecificInstance()?.SendTraceTag("00001VZ", Category.Runtime, Verbosity.Normal, "Decryption fallback. Decrypting using an older version of the encryption key: {0}", DiagnosticParameter.SystemMetadata(((ObjectIdentifier)item.Identifier).Version));
				text = DecryptInternal(((ObjectIdentifier)item.Identifier).Identifier, algorithm, encryptedBytes);
				if (text != null)
				{
					return text;
				}
			}
		}
		throw new NavEncryptionException(string.Format(CultureInfo.CurrentCulture, Lang.SystemEncryptionDecryptBadKeyError));
	}

	private string DecryptInternal(string keyId, string algorithm, byte[] encryptedBytes)
	{
		//IL_0031: Expected Obj, but got Unknown
		try
		{
			byte[] result = keyVaultClient.DecryptAsync(keyId, algorithm, encryptedBytes, default(CancellationToken)).WaitThrowingOriginalException().Result;
			return Encoding.UTF8.GetString(result);
		}
		catch (KeyVaultClientException ex)
		{
			KeyVaultClientException ex2 = ex;
			if (ex2.Error.Code == "BadParameter")
			{
				return null;
			}
			throw;
		}
	}

	public void DeleteKey()
	{
		throw new NavNCLNotSupportedOperationException();
	}

	public string Encrypt(string plaintext)
	{
		using (NavCurrentThread.Session.Diagnostics.CreateTraceScope("0000048", DiagnosticParameter.SystemMetadata("Invoking Azure KeyVault to encrypt"), MonitorType.None, Category.Runtime))
		{
			byte[] plainTextBytes = Encoding.UTF8.GetBytes(plaintext);
			return RunCryptoProviderMethod(() => Convert.ToBase64String(keyVaultClient.EncryptAsync(azureKeyVaultSettings.KeyIdentifier, azureKeyVaultSettings.EncryptionAlgorithm, plainTextBytes, default(CancellationToken)).WaitThrowingOriginalException().Result));
		}
	}

	public void ExportKey(string fileName, string keyPassword)
	{
		throw new NavNCLNotSupportedOperationException();
	}

	public void ImportKey(string fileName, string keyPassword, bool overwrite = false)
	{
		throw new NavNCLNotSupportedOperationException();
	}

	public string GetSecret(string secretName)
	{
		using (NavCurrentThread.Session.Diagnostics.CreateTraceScope("000009K", DiagnosticParameter.SystemMetadata("Invoking Azure KeyVault to get secret"), MonitorType.None, Category.Security))
		{
			return RunCryptoProviderMethod(() => keyVaultClient.GetSecretAsync(GetSecretIdentifier(secretName), default(CancellationToken)).WaitThrowingOriginalException().Value);
		}
	}

	public void Dispose()
	{
	}

	private Func<string, string, string, Task<string>> GetTokenFactory(AzureKeyVaultSettings settings)
	{
		return (string authority, string resource, string scope) =>
		{
			//IL_0075: Expected Obj, but got Unknown
			AuthenticationInfo val = null;
			using (new IfxTimingScope(NavAuthenticationHandlerFactory.AuthClientDurationMetricInstance, "TenantAzureKeyVaultEncryptionProvider_IAuthenticationHandler.AcquireToken", ServerUserSettings.Instance.LicensingServiceClientType))
			{
				try
				{
					IAuthenticationHandler val2 = authenticationHandler;
					val = val2.AcquireTokenByThumbprintAsync(aadTenantId, authority, resource, settings.ClientId.ToString(), settings.CertificateThumbprint).WaitThrowingOriginalException();
				}
				catch (LicenseServiceAdalException ex)
				{
					LicenseServiceAdalException ex2 = ex;
					throw new InvalidOperationException(((Exception)(object)ex2).Message);
				}
			}
			if (val == null)
			{
				throw new InvalidOperationException(Lang.SystemEncryptionTokenError);
			}
			return Task.FromResult(val.AccessToken);
		};
	}

	private static string RunCryptoProviderMethod(Func<string> func)
	{
		//IL_000a: Expected Obj, but got Unknown
		try
		{
			return func();
		}
		catch (KeyVaultClientException ex)
		{
			KeyVaultClientException ex2 = ex;
			throw new NavEncryptionException(string.Format(CultureInfo.CurrentCulture, Lang.SystemEncryptionGenericError, ((Exception)(object)ex2).Message), (Exception)(object)ex2);
		}
	}

	private string GetSecretIdentifier(string secretName)
	{
		UriBuilder uriBuilder = new UriBuilder(azureKeyVaultSettings.KeyIdentifier);
		uriBuilder.Scheme = uriBuilder.Scheme;
		uriBuilder.Host = uriBuilder.Host;
		uriBuilder.Path = "secrets/" + secretName;
		return uriBuilder.ToString();
	}
}
