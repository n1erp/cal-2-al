using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

namespace Microsoft.Dynamics.Nav.Types;

public class MemoryRsaEncryptionProvider : RsaEncryptionProviderBase
{
	private string keyData;

	public override bool IsKeyCreated => KeyDataExists;

	protected override bool KeyDataExists => !string.IsNullOrWhiteSpace(keyData);

	protected override bool IsKeyFileRequired => false;

	public MemoryRsaEncryptionProvider()
	{
	}

	public MemoryRsaEncryptionProvider(string publicEncryptionKey)
	{
		if (string.IsNullOrWhiteSpace(publicEncryptionKey))
		{
			throw new NavNCLArgumentException("publicEncryptionKey");
		}
		try
		{
			InitializeCryptoProviderFromBuffer(publicEncryptionKey);
		}
		catch (CryptographicException innerException)
		{
			throw new NavEncryptionInvalidKeyFileException(innerException);
		}
		catch (XmlSyntaxException innerException2)
		{
			throw new NavEncryptionInvalidKeyFileException(innerException2);
		}
		catch (XmlException innerException3)
		{
			throw new NavEncryptionInvalidKeyFileException(innerException3);
		}
	}

	public MemoryRsaEncryptionProvider(FileInfo encryptionKeyFileInfo, string keyPassword = null)
	{
		ReadKeyFile(encryptionKeyFileInfo.FullName, keyPassword);
	}

	public override void CreateKey()
	{
		if (IsKeyCreated)
		{
			throw new NavEncryptionCreatedException();
		}
		keyData = RsaEncryptionProviderBase.GenerateNewKeyData();
		InitializeCryptoProviderFromBuffer(keyData);
	}

	public override void DeleteKey()
	{
		try
		{
			base.DeleteKey();
		}
		finally
		{
			keyData = null;
		}
	}

	public override void ImportKey(string fileName, string keyPassword, bool overwrite = false)
	{
		throw new NotSupportedException();
	}

	private void ReadKeyFile(string fileName, string keyPassword)
	{
		FileInfo fileInfo = new FileInfo(fileName);
		if (fileInfo == null)
		{
			throw new NavNCLArgumentException("encryptionFileName");
		}
		if (!fileInfo.Exists)
		{
			throw new NavNCLFileNotFoundException(fileInfo.FullName);
		}
		try
		{
			if (!string.IsNullOrWhiteSpace(keyPassword))
			{
				string data = RsaEncryptionProviderBase.ReadAllCharsFromEncryptedKeyFile(fileInfo);
				keyData = RsaEncryptionProviderBase.AesCrypt(data, keyPassword, encrypt: false);
			}
			else
			{
				keyData = File.ReadAllText(fileInfo.FullName, Encoding.UTF8);
			}
			InitializeCryptoProviderFromBuffer(keyData);
		}
		catch (CryptographicException innerException)
		{
			throw new NavEncryptionInvalidKeyFileException(innerException);
		}
		catch (XmlSyntaxException innerException2)
		{
			throw new NavEncryptionInvalidKeyFileException(innerException2);
		}
		catch (XmlException innerException3)
		{
			throw new NavEncryptionInvalidKeyFileException(innerException3);
		}
	}
}
