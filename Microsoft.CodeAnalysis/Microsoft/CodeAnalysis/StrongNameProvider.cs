using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis;

public abstract class StrongNameProvider
{
	internal abstract SigningCapability Capability { get; }

	public abstract override int GetHashCode();

	public abstract override bool Equals(object other);

	internal abstract Stream CreateInputStream();

	internal abstract StrongNameKeys CreateKeys(string keyFilePath, string keyContainerName, CommonMessageProvider messageProvider);

	internal StrongNameKeys CommonCreateKeys(StrongNameFileSystem fileSystem, string keyFilePath, ImmutableArray<string> keyFileSearchPaths, CommonMessageProvider messageProvider)
	{
		try
		{
			string text = fileSystem.ResolveStrongNameKeyFile(keyFilePath, keyFileSearchPaths);
			if (text == null)
			{
				return new StrongNameKeys(StrongNameKeys.GetKeyFileError(messageProvider, keyFilePath, CodeAnalysisResources.FileNotFound));
			}
			return StrongNameKeys.CreateHelper(ImmutableArray.Create(fileSystem.ReadAllBytes(text)), keyFilePath);
		}
		catch (Exception ex)
		{
			return new StrongNameKeys(StrongNameKeys.GetKeyFileError(messageProvider, keyFilePath, ex.Message));
		}
	}

	internal virtual void SignStream(StrongNameKeys keys, Stream inputStream, Stream outputStream)
	{
		throw new NotSupportedException();
	}

	internal virtual void SignPeBuilder(ExtendedPEBuilder peBuilder, BlobBuilder peBlob, RSAParameters privkey)
	{
		throw new NotSupportedException();
	}
}
