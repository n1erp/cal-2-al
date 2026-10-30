using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis;

public sealed class ResourceDescription : IFileReference
{
	private sealed class ResourceHashProvider : CryptographicHashProvider
	{
		private readonly ResourceDescription _resource;

		public ResourceHashProvider(ResourceDescription resource)
		{
			_resource = resource;
		}

		internal override ImmutableArray<byte> ComputeHash(HashAlgorithm algorithm)
		{
			try
			{
				using Stream stream = _resource.DataProvider();
				if (stream == null)
				{
					throw new InvalidOperationException(CodeAnalysisResources.ResourceDataProviderShouldReturnNonNullStream);
				}
				return ImmutableArray.CreateRange(algorithm.ComputeHash(stream));
			}
			catch (Exception inner)
			{
				throw new ResourceException(_resource.FileName, inner);
			}
		}
	}

	internal readonly string ResourceName;

	internal readonly string FileName;

	internal readonly bool IsPublic;

	internal readonly Func<Stream> DataProvider;

	private readonly CryptographicHashProvider _hashes;

	internal bool IsEmbedded => FileName == null;

	string IFileReference.FileName => FileName;

	bool IFileReference.HasMetadata => false;

	public ResourceDescription(string resourceName, Func<Stream> dataProvider, bool isPublic)
		: this(resourceName, null, dataProvider, isPublic, isEmbedded: true, checkArgs: true)
	{
	}

	public ResourceDescription(string resourceName, string fileName, Func<Stream> dataProvider, bool isPublic)
		: this(resourceName, fileName, dataProvider, isPublic, isEmbedded: false, checkArgs: true)
	{
	}

	internal ResourceDescription(string resourceName, string fileName, Func<Stream> dataProvider, bool isPublic, bool isEmbedded, bool checkArgs)
	{
		if (checkArgs)
		{
			if (dataProvider == null)
			{
				throw new ArgumentNullException("dataProvider");
			}
			if (resourceName == null)
			{
				throw new ArgumentNullException("resourceName");
			}
			if (!MetadataHelpers.IsValidMetadataIdentifier(resourceName))
			{
				throw new ArgumentException(CodeAnalysisResources.EmptyOrInvalidResourceName, "resourceName");
			}
			if (!isEmbedded)
			{
				if (fileName == null)
				{
					throw new ArgumentNullException("fileName");
				}
				if (!MetadataHelpers.IsValidMetadataFileName(fileName))
				{
					throw new ArgumentException(CodeAnalysisResources.EmptyOrInvalidFileName, "fileName");
				}
			}
		}
		ResourceName = resourceName;
		DataProvider = dataProvider;
		FileName = (isEmbedded ? null : fileName);
		IsPublic = isPublic;
		_hashes = new ResourceHashProvider(this);
	}

	internal ManagedResource ToManagedResource(CommonPEModuleBuilder moduleBeingBuilt)
	{
		return new ManagedResource(ResourceName, IsPublic, IsEmbedded ? DataProvider : null, IsEmbedded ? null : this, 0u);
	}

	ImmutableArray<byte> IFileReference.GetHashValue(AssemblyHashAlgorithm algorithmId)
	{
		return _hashes.GetHash(algorithmId);
	}
}
