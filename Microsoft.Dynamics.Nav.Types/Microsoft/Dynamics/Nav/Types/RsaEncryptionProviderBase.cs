using System;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

namespace Microsoft.Dynamics.Nav.Types;

public abstract class RsaEncryptionProviderBase : ISystemEncryptionProvider, IDisposable
{
	private static readonly object SyncObject = new object();

	protected const string DiskContainerName = "Keys";

	protected const int KeySize = 2048;

	private RSACryptoServiceProvider cryptoProviderInstance;

	private string keyFileName;

	private bool isInitialized;

	public virtual bool IsKeyCreated
	{
		get
		{
			Initialize();
			return !string.IsNullOrEmpty(KeyFileName);
		}
	}

	public bool IsKeyPresent
	{
		get
		{
			if (IsKeyCreated)
			{
				return KeyDataExists;
			}
			return false;
		}
	}

	protected string KeyFileName
	{
		get
		{
			return keyFileName;
		}
		private set
		{
			ReleaseCryptoProviderInstance();
			keyFileName = value;
		}
	}

	protected string Id { get; set; }

	protected virtual string StoredEncryptionFileName { get; set; }

	protected virtual string StoredEncryptionKeyHash
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string PublicKey
	{
		get
		{
			Initialize();
			return RunCryptoProviderMethod((RSACryptoServiceProvider crypto) => cryptoProviderInstance.ToXmlString(includePrivateParameters: false));
		}
	}

	protected virtual bool KeyDataExists => KeyFileExists(KeyFileName);

	protected virtual bool IsKeyFileRequired => true;

	~RsaEncryptionProviderBase()
	{
		Dispose(isDisposing: false);
	}

	public string Encrypt(string plaintext)
	{
		if (string.IsNullOrEmpty(plaintext))
		{
			return string.Empty;
		}
		Initialize();
		lock (SyncObject)
		{
			byte[] plainTextBytes = Encoding.UTF8.GetBytes(plaintext);
			ValidateDataLength(plainTextBytes.Length);
			return RunCryptoProviderMethod((RSACryptoServiceProvider provider) => Convert.ToBase64String(provider.Encrypt(plainTextBytes, fOAEP: true)));
		}
	}

	public string Decrypt(string encryptedText)
	{
		if (string.IsNullOrEmpty(encryptedText))
		{
			return string.Empty;
		}
		Initialize();
		lock (SyncObject)
		{
			try
			{
				byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
				return RunCryptoProviderMethod((RSACryptoServiceProvider provider) => Encoding.UTF8.GetString(provider.Decrypt(encryptedBytes, fOAEP: true)));
			}
			catch (FormatException innerException)
			{
				throw new NavEncryptionException(Lang.SystemEncryptionDecryptBadDataError, innerException);
			}
			catch (CryptographicException innerException2)
			{
				throw new NavEncryptionException(Lang.SystemEncryptionDecryptBadDataError, innerException2);
			}
		}
	}

	public virtual void CreateKey()
	{
		Initialize();
		lock (SyncObject)
		{
			if (IsKeyCreated)
			{
				throw new NavEncryptionCreatedException();
			}
			KeyFileName = CreateEncryptionFileName();
			WriteToProtectedFile(GetEncryptionKeyFullname(KeyFileName), GenerateNewKeyData(), binaryData: false);
			StoredEncryptionFileName = KeyFileName;
			StoredEncryptionKeyHash = HashEncryptionKeyData(RunCryptoProviderMethod((RSACryptoServiceProvider provider) => provider.ToXmlString(includePrivateParameters: false)));
			ReleaseCryptoProviderInstance();
			ValidateKeyData();
		}
	}

	public virtual void DeleteKey()
	{
		Initialize();
		lock (SyncObject)
		{
			if (!string.IsNullOrEmpty(KeyFileName) && File.Exists(GetEncryptionKeyFullname(KeyFileName)))
			{
				File.Delete(GetEncryptionKeyFullname(KeyFileName));
			}
			KeyFileName = null;
			StoredEncryptionFileName = string.Empty;
			StoredEncryptionKeyHash = string.Empty;
			ReleaseCryptoProviderInstance();
		}
	}

	public void ExportKey(string fileName, string keyPassword)
	{
		Initialize();
		fileName = Path.GetFullPath(fileName);
		if (KeyDataExists && fileName.Equals(GetEncryptionKeyFullname(KeyFileName), StringComparison.OrdinalIgnoreCase))
		{
			throw new NavNCLArgumentException(fileName, "ExportKey", 1);
		}
		lock (SyncObject)
		{
			string data = CreateUnencryptedKeyFileContent();
			bool flag;
			if (!(flag = string.IsNullOrEmpty(keyPassword)))
			{
				data = AesCrypt(data, keyPassword, encrypt: true);
			}
			WriteToProtectedFile(fileName, data, !flag);
		}
	}

	private string CreateUnencryptedKeyFileContent()
	{
		return RunCryptoProviderMethod((RSACryptoServiceProvider provider) => provider.ToXmlString(includePrivateParameters: true));
	}

	public virtual void ImportKey(string fileName, string keyPassword, bool overwrite = false)
	{
		bool flag = !string.IsNullOrWhiteSpace(keyPassword);
		if (string.IsNullOrWhiteSpace(fileName))
		{
			throw new NavNCLFileNotFoundException(fileName);
		}
		Initialize();
		FileInfo fileInfo = new FileInfo(fileName);
		if (!fileInfo.Exists)
		{
			throw new NavNCLFileNotFoundException(fileName);
		}
		if (KeyDataExists && fileInfo.FullName.Equals(GetEncryptionKeyFullname(KeyFileName), StringComparison.OrdinalIgnoreCase))
		{
			throw new NavNCLArgumentException(fileName, "ImportKey", 1);
		}
		string text = ((!flag) ? File.ReadAllText(fileName, Encoding.UTF8) : AesCrypt(ReadAllCharsFromEncryptedKeyFile(fileInfo), keyPassword, encrypt: false));
		lock (SyncObject)
		{
			try
			{
				string text2;
				using (RSACryptoServiceProvider rSACryptoServiceProvider = CreateCryptoProviderFromBuffer(text))
				{
					text2 = HashEncryptionKeyData(rSACryptoServiceProvider.ToXmlString(includePrivateParameters: false));
					rSACryptoServiceProvider.Clear();
				}
				string storedEncryptionKeyHash = StoredEncryptionKeyHash;
				if (!string.IsNullOrEmpty(storedEncryptionKeyHash) && storedEncryptionKeyHash != text2)
				{
					throw new NavEncryptionExistingKeyImportException();
				}
				if (KeyDataExists && !overwrite)
				{
					string encryptionKeyFullname = GetEncryptionKeyFullname(KeyFileName);
					string destFileName = string.Format(CultureInfo.CurrentCulture, "{0}.{1:yyyy-MM-dd_HH-mm-ss.fffZ}", encryptionKeyFullname, DateTime.UtcNow);
					File.Move(encryptionKeyFullname, destFileName);
				}
				else if (string.IsNullOrEmpty(KeyFileName))
				{
					KeyFileName = CreateEncryptionFileName();
				}
				WriteToProtectedFile(GetEncryptionKeyFullname(KeyFileName), text, binaryData: false);
				if (string.IsNullOrEmpty(storedEncryptionKeyHash))
				{
					StoredEncryptionFileName = KeyFileName;
					StoredEncryptionKeyHash = text2;
				}
				ReleaseCryptoProviderInstance();
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

	protected static string ReadAllCharsFromEncryptedKeyFile(FileInfo fileInfo)
	{
		using FileStream fileStream = fileInfo.OpenRead();
		byte[] array = new byte[fileInfo.Length];
		fileStream.Read(array, 0, (int)fileInfo.Length);
		try
		{
			return GetString(array);
		}
		catch (ArgumentException innerException)
		{
			throw new NavEncryptionInvalidKeyFileException(innerException);
		}
	}

	protected void InitializeCryptoProviderFromBuffer(string keyData)
	{
		Initialize();
		cryptoProviderInstance = CreateCryptoProviderFromBuffer(keyData);
	}

	public void Dispose()
	{
		Dispose(isDisposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual string CreateEncryptionFileName()
	{
		return null;
	}

	protected virtual string HashEncryptionKeyData(string keyData)
	{
		byte[] bytes = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(keyData));
		return Encoding.UTF8.GetString(bytes);
	}

	protected bool KeyFileExists(string fileName)
	{
		if (!string.IsNullOrEmpty(fileName))
		{
			return File.Exists(GetEncryptionKeyFullname(fileName));
		}
		return false;
	}

	private void ValidateKeyData()
	{
		if (!KeyDataExists)
		{
			return;
		}
		using RSACryptoServiceProvider rSACryptoServiceProvider = CreateCryptoProvider(GetEncryptionKeyFullname(KeyFileName));
		rSACryptoServiceProvider.Clear();
	}

	private void RequireKeyCreatedAndPresent()
	{
		if (IsKeyFileRequired && string.IsNullOrEmpty(StoredEncryptionFileName))
		{
			throw new NavEncryptionNotCreatedException();
		}
		if (!IsKeyPresent)
		{
			throw new NavEncryptionKeyNotFoundException();
		}
	}

	protected virtual void Dispose(bool isDisposing)
	{
		lock (SyncObject)
		{
			ReleaseCryptoProviderInstance();
		}
	}

	private T RunCryptoProviderMethod<T>(Func<RSACryptoServiceProvider, T> func)
	{
		InitializeCryptoProviderInstance();
		bool flag = false;
		try
		{
			return func(cryptoProviderInstance);
		}
		catch
		{
			flag = true;
			throw;
		}
		finally
		{
			if (flag)
			{
				ReleaseCryptoProviderInstance();
			}
		}
	}

	private void Initialize()
	{
		if (isInitialized)
		{
			return;
		}
		lock (SyncObject)
		{
			if (!isInitialized)
			{
				KeyFileName = StoredEncryptionFileName;
				isInitialized = true;
			}
		}
	}

	private void InitializeCryptoProviderInstance()
	{
		Initialize();
		if (cryptoProviderInstance == null)
		{
			RequireKeyCreatedAndPresent();
			cryptoProviderInstance = CreateCryptoProvider(GetEncryptionKeyFullname(KeyFileName));
		}
	}

	private void ReleaseCryptoProviderInstance()
	{
		if (cryptoProviderInstance != null)
		{
			cryptoProviderInstance.Clear();
			cryptoProviderInstance.Dispose();
			cryptoProviderInstance = null;
		}
	}

	private static void WriteToProtectedFile(string fileName, string data, bool binaryData)
	{
		DirectoryInfo directory = new FileInfo(fileName).Directory;
		if (!directory.Exists)
		{
			try
			{
				directory.Create();
			}
			catch (IOException ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, writeExceptionToEventLog: false);
			}
			catch (UnauthorizedAccessException ex2)
			{
				NavFileHelper.MapAndRethrowClrException(ex2, writeExceptionToEventLog: false);
			}
			catch (ArgumentException ex3)
			{
				NavFileHelper.MapAndRethrowClrException(ex3, writeExceptionToEventLog: false);
			}
			catch (NotSupportedException ex4)
			{
				NavFileHelper.MapAndRethrowClrException(ex4, writeExceptionToEventLog: false);
			}
		}
		using FileStream fileStream = File.Create(fileName, 1024, FileOptions.WriteThrough);
		if (binaryData)
		{
			byte[] bytes = GetBytes(data);
			fileStream.Write(bytes, 0, bytes.Length);
			return;
		}
		using StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
		streamWriter.Write(data);
	}

	private string GetEncryptionKeyFullname(string fileName)
	{
		if (!IsKeyFileRequired)
		{
			return null;
		}
		return Path.Combine(Path.Combine(ProductApplicationData.ServerPath, "Keys"), fileName);
	}

	private static void ValidateDataLength(int dataByteLength)
	{
		if (dataByteLength > 215)
		{
			throw new NavEncryptionDataTooLongException();
		}
	}

	protected static string AesCrypt(string data, string password, bool encrypt)
	{
		byte[] array = new byte[64]
		{
			113, 170, 194, 165, 49, 66, 186, 104, 17, 233,
			99, 141, 234, 166, 130, 39, 71, 223, 247, 51,
			216, 186, 189, 236, 119, 176, 96, 87, 251, 219,
			207, 198, 144, 166, 27, 246, 71, 186, 213, 63,
			74, 155, 140, 224, 22, 178, 255, 244, 108, 137,
			13, 175, 74, 154, 138, 151, 113, 139, 55, 105,
			212, 228, 254, 114
		};
		Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, array);
		using AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider();
		aesCryptoServiceProvider.Key = rfc2898DeriveBytes.GetBytes(aesCryptoServiceProvider.BlockSize / 4);
		if (aesCryptoServiceProvider.KeySize <= 128)
		{
			throw new CryptographicException("Invalid key length.");
		}
		byte[] array2 = new byte[aesCryptoServiceProvider.BlockSize / 8];
		for (int i = 0; i < aesCryptoServiceProvider.BlockSize / 8; i++)
		{
			array2[i] = array[i];
		}
		aesCryptoServiceProvider.IV = array2;
		return encrypt ? EncryptStringAes(data, aesCryptoServiceProvider) : DecryptStringAes(data, aesCryptoServiceProvider);
	}

	private static string EncryptStringAes(string plainText, SymmetricAlgorithm aes)
	{
		ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);
		byte[] dataBuffer;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using CryptoStream stream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
			using (StreamWriter streamWriter = new StreamWriter(stream))
			{
				streamWriter.Write(plainText);
			}
			dataBuffer = memoryStream.ToArray();
		}
		return GetString(dataBuffer);
	}

	private static string DecryptStringAes(string data, SymmetricAlgorithm aes)
	{
		ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
		try
		{
			using MemoryStream stream = new MemoryStream(GetBytes(data));
			using CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
			using StreamReader streamReader = new StreamReader(stream2);
			return streamReader.ReadToEnd();
		}
		catch (CryptographicException innerException)
		{
			throw new NavEncryptionInvalidKeyFileException(Lang.SystemEncryptionInvalidKeyFileOrPassword, innerException);
		}
	}

	private static byte[] GetBytes(string dataBuffer)
	{
		byte[] array = new byte[dataBuffer.Length * 2];
		Buffer.BlockCopy(dataBuffer.ToCharArray(), 0, array, 0, array.Length);
		return array;
	}

	private static string GetString(byte[] dataBuffer)
	{
		if (dataBuffer.Length == 0)
		{
			return string.Empty;
		}
		char[] array = new char[dataBuffer.Length / 2];
		Buffer.BlockCopy(dataBuffer, 0, array, 0, dataBuffer.Length);
		return new string(array);
	}

	private static RSACryptoServiceProvider CreateCryptoProvider(string filename)
	{
		return CreateCryptoProviderFromBuffer(File.ReadAllText(filename, Encoding.UTF8));
	}

	private static RSACryptoServiceProvider CreateCryptoProviderFromBuffer(string keyData)
	{
		RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(2048);
		rSACryptoServiceProvider.FromXmlString(keyData);
		return rSACryptoServiceProvider;
	}

	protected static string GenerateNewKeyData()
	{
		using RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(2048);
		return rSACryptoServiceProvider.ToXmlString(includePrivateParameters: true);
	}
}
