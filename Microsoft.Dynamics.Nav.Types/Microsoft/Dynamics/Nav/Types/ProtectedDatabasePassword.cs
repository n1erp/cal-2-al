using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public sealed class ProtectedDatabasePassword : IDisposable
{
	[NonSerialized]
	private SecureString secureStringPassword;

	[NonSerialized]
	private readonly ISystemEncryptionProvider encryptionProvider;

	[NonSerialized]
	private string protectedPasswordValue;

	[DataMember]
	[TelemetrySuppress]
	public string Value
	{
		get
		{
			if (protectedPasswordValue == null)
			{
				protectedPasswordValue = EncryptionProviderOrDefault.Encrypt(PlaintextPassword);
			}
			return protectedPasswordValue;
		}
		private set
		{
			if (EncryptionProviderOrDefault.IsKeyPresent)
			{
				try
				{
					secureStringPassword = SecureStringExtension.CreateFromUnsecureString(EncryptionProviderOrDefault.Decrypt(value));
					protectedPasswordValue = value;
					return;
				}
				catch (NavEncryptionException)
				{
					NavDiagnostics getMostSpecificInstance = NavDiagnostics.GetMostSpecificInstance;
					getMostSpecificInstance?.SendTraceTag("00000OU", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Could not decrypt protected password.");
					try
					{
						protectedPasswordValue = EncryptionProviderOrDefault.Encrypt(value);
						return;
					}
					catch (NavEncryptionDataTooLongException)
					{
						getMostSpecificInstance?.SendTraceTag("00000OV", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Could not encrypt protected password.");
						return;
					}
				}
			}
			protectedPasswordValue = value;
		}
	}

	public SecureString SecureStringPassword
	{
		get
		{
			if (secureStringPassword == null)
			{
				secureStringPassword = SecureStringExtension.CreateFromUnsecureString(EncryptionProviderOrDefault.Decrypt(protectedPasswordValue));
			}
			return secureStringPassword;
		}
	}

	public string PlaintextPassword => SecureStringPassword.ConvertToUnsecureString();

	private ISystemEncryptionProvider EncryptionProviderOrDefault => encryptionProvider ?? ServerInstanceRsaEncryptionProvider.Instance;

	public ProtectedDatabasePassword(string protectedPassword)
	{
		Value = protectedPassword;
	}

	private ProtectedDatabasePassword(SecureString password)
	{
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		secureStringPassword = password;
	}

	public static ProtectedDatabasePassword Create(SecureString password)
	{
		if (password == null)
		{
			return null;
		}
		return new ProtectedDatabasePassword(password);
	}

	public ProtectedDatabasePassword(SecureString password, ISystemEncryptionProvider encryptionProvider)
	{
		this.encryptionProvider = encryptionProvider;
		secureStringPassword = password;
	}

	public static ProtectedDatabasePassword Parse(string settingValue, string settingName)
	{
		if (!TryParse(settingValue, out var result))
		{
			throw new NavConfigurationException(Lang.UnableToParseConfigSetting, settingName, typeof(ProtectedDatabasePassword).Name);
		}
		return result;
	}

	public static bool TryParse(string settingValue, out ProtectedDatabasePassword result)
	{
		result = null;
		try
		{
			if (string.IsNullOrEmpty(settingValue))
			{
				return true;
			}
			result = new ProtectedDatabasePassword(settingValue);
			return result.SecureStringPassword != null;
		}
		catch (NavEncryptionException)
		{
			return false;
		}
		catch (FormatException)
		{
			return false;
		}
		catch (CryptographicException)
		{
			return false;
		}
	}

	public override string ToString()
	{
		return string.Empty;
	}

	public void Dispose()
	{
		secureStringPassword.Dispose();
	}
}
