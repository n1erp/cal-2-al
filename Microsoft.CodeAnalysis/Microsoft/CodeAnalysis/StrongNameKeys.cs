using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace Microsoft.CodeAnalysis;

internal sealed class StrongNameKeys
{
	internal readonly ImmutableArray<byte> KeyPair;

	internal readonly ImmutableArray<byte> PublicKey;

	internal readonly RSAParameters? PrivateKey;

	internal readonly Diagnostic DiagnosticOpt;

	internal readonly string KeyContainer;

	internal readonly string KeyFilePath;

	internal static readonly StrongNameKeys None = new StrongNameKeys();

	private static Tuple<ImmutableArray<byte>, ImmutableArray<byte>, RSAParameters?> s_lastSeenKeyPair;

	internal bool CanSign
	{
		get
		{
			if (KeyPair.IsDefault)
			{
				return KeyContainer != null;
			}
			return true;
		}
	}

	internal bool CanProvideStrongName
	{
		get
		{
			if (!CanSign)
			{
				return !PublicKey.IsDefault;
			}
			return true;
		}
	}

	private StrongNameKeys()
	{
	}

	internal StrongNameKeys(Diagnostic diagnostic)
	{
		DiagnosticOpt = diagnostic;
	}

	internal StrongNameKeys(ImmutableArray<byte> keyPair, ImmutableArray<byte> publicKey, RSAParameters? privateKey, string keyContainerName, string keyFilePath)
	{
		KeyPair = keyPair;
		PublicKey = publicKey;
		PrivateKey = privateKey;
		KeyContainer = keyContainerName;
		KeyFilePath = keyFilePath;
	}

	internal static StrongNameKeys Create(ImmutableArray<byte> publicKey, RSAParameters? privateKey, CommonMessageProvider messageProvider)
	{
		if (MetadataHelpers.IsValidPublicKey(publicKey))
		{
			return new StrongNameKeys(default, publicKey, privateKey, null, null);
		}
		return new StrongNameKeys(messageProvider.CreateDiagnostic(messageProvider.ERR_BadCompilationOptionValue, Location.None, "CryptoPublicKey", BitConverter.ToString(publicKey.ToArray())));
	}

	internal static StrongNameKeys Create(string keyFilePath, CommonMessageProvider messageProvider)
	{
		if (string.IsNullOrEmpty(keyFilePath))
		{
			return None;
		}
		try
		{
			return CreateHelper(ImmutableArray.Create(File.ReadAllBytes(keyFilePath)), keyFilePath);
		}
		catch (IOException ex)
		{
			return new StrongNameKeys(GetKeyFileError(messageProvider, keyFilePath, ex.Message));
		}
	}

	internal static StrongNameKeys CreateHelper(ImmutableArray<byte> keyFileContent, string keyFilePath)
	{
		RSAParameters? privateKey = null;
		Tuple<ImmutableArray<byte>, ImmutableArray<byte>, RSAParameters?> tuple = s_lastSeenKeyPair;
		ImmutableArray<byte> immutableArray;
		ImmutableArray<byte> snKey;
		if (tuple != null && keyFileContent == tuple.Item1)
		{
			immutableArray = tuple.Item1;
			snKey = tuple.Item2;
			privateKey = tuple.Item3;
		}
		else
		{
			if (MetadataHelpers.IsValidPublicKey(keyFileContent))
			{
				snKey = keyFileContent;
				immutableArray = default;
			}
			else
			{
				if (!CryptoBlobParser.TryParseKey(keyFileContent, out snKey, out privateKey))
				{
					throw new IOException(CodeAnalysisResources.InvalidPublicKey);
				}
				immutableArray = keyFileContent;
			}
			tuple = new Tuple<ImmutableArray<byte>, ImmutableArray<byte>, RSAParameters?>(immutableArray, snKey, privateKey);
			Interlocked.Exchange(ref s_lastSeenKeyPair, tuple);
		}
		return new StrongNameKeys(immutableArray, snKey, privateKey, null, keyFilePath);
	}

	internal static StrongNameKeys Create(StrongNameProvider providerOpt, string keyFilePath, string keyContainerName, CommonMessageProvider messageProvider)
	{
		if (string.IsNullOrEmpty(keyFilePath) && string.IsNullOrEmpty(keyContainerName))
		{
			return None;
		}
		if (providerOpt == null)
		{
			return new StrongNameKeys(GetError(keyFilePath, keyContainerName, new CodeAnalysisResourcesLocalizableErrorArgument("AssemblySigningNotSupported"), messageProvider));
		}
		return providerOpt.CreateKeys(keyFilePath, keyContainerName, messageProvider);
	}

	internal static Diagnostic GetError(string keyFilePath, string keyContainerName, object message, CommonMessageProvider messageProvider)
	{
		if (keyContainerName != null)
		{
			return GetContainerError(messageProvider, keyContainerName, message);
		}
		return GetKeyFileError(messageProvider, keyFilePath, message);
	}

	internal static Diagnostic GetContainerError(CommonMessageProvider messageProvider, string name, object message)
	{
		return messageProvider.CreateDiagnostic(messageProvider.ERR_PublicKeyContainerFailure, Location.None, name, message);
	}

	internal static Diagnostic GetKeyFileError(CommonMessageProvider messageProvider, string path, object message)
	{
		return messageProvider.CreateDiagnostic(messageProvider.ERR_PublicKeyFileFailure, Location.None, path, message);
	}

	internal static bool IsValidPublicKeyString(string publicKey)
	{
		if (string.IsNullOrEmpty(publicKey) || publicKey.Length % 2 != 0)
		{
			return false;
		}
		foreach (char c in publicKey)
		{
			if ((c < '0' || c > '9') && (c < 'a' || c > 'f') && (c < 'A' || c > 'F'))
			{
				return false;
			}
		}
		return true;
	}
}
