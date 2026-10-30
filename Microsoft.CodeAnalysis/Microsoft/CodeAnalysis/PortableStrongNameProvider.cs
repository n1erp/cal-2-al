using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis;

internal sealed class PortableStrongNameProvider : StrongNameProvider
{
	private readonly ImmutableArray<string> _keyFileSearchPaths;

	internal StrongNameFileSystem FileSystem { get; }

	internal override SigningCapability Capability => SigningCapability.SignsPeBuilder;

	public PortableStrongNameProvider(ImmutableArray<string> keySearchPaths, StrongNameFileSystem strongNameFileSystem)
	{
		FileSystem = strongNameFileSystem ?? StrongNameFileSystem.Instance;
		_keyFileSearchPaths = keySearchPaths.NullToEmpty();
	}

	public override int GetHashCode()
	{
		return 0;
	}

	internal override StrongNameKeys CreateKeys(string keyFilePath, string keyContainerName, CommonMessageProvider messageProvider)
	{
		ImmutableArray<byte> keyPair = default;
		ImmutableArray<byte> publicKey = default;
		string keyContainerName2 = null;
		if (!string.IsNullOrEmpty(keyFilePath))
		{
			return CommonCreateKeys(FileSystem, keyFilePath, _keyFileSearchPaths, messageProvider);
		}
		return new StrongNameKeys(keyPair, publicKey, null, keyContainerName2, keyFilePath);
	}

	internal override void SignPeBuilder(ExtendedPEBuilder peBuilder, BlobBuilder peBlob, RSAParameters privkey)
	{
		peBuilder.Sign(peBlob, (IEnumerable<Blob> content) => SigningUtilities.CalculateRsaSignature(content, privkey));
	}

	internal override Stream CreateInputStream()
	{
		throw new NotSupportedException();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PortableStrongNameProvider portableStrongNameProvider))
		{
			return false;
		}
		if (FileSystem == portableStrongNameProvider.FileSystem)
		{
			return _keyFileSearchPaths.SequenceEqual(portableStrongNameProvider._keyFileSearchPaths);
		}
		return false;
	}
}
