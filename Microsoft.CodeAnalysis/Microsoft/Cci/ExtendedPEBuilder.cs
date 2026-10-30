using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.Cci;

internal sealed class ExtendedPEBuilder : ManagedPEBuilder
{
	private const string MvidSectionName = ".mvid";

	public const int SizeOfGuid = 16;

	private Blob _mvidSectionFixup;

	private readonly bool _withMvidSection;

	public ExtendedPEBuilder(PEHeaderBuilder header, MetadataRootBuilder metadataRootBuilder, BlobBuilder ilStream, BlobBuilder mappedFieldData, BlobBuilder managedResources, ResourceSectionBuilder nativeResources, DebugDirectoryBuilder debugDirectoryBuilder, int strongNameSignatureSize, MethodDefinitionHandle entryPoint, CorFlags flags, Func<IEnumerable<Blob>, BlobContentId> deterministicIdProvider, bool withMvidSection)
		: base(header, metadataRootBuilder, ilStream, mappedFieldData, managedResources, nativeResources, debugDirectoryBuilder, strongNameSignatureSize, entryPoint, flags, deterministicIdProvider)
	{
		_withMvidSection = withMvidSection;
	}

	protected override ImmutableArray<Section> CreateSections()
	{
		ImmutableArray<Section> immutableArray = base.CreateSections();
		if (_withMvidSection)
		{
			ArrayBuilder<Section> instance = ArrayBuilder<Section>.GetInstance(immutableArray.Length + 1);
			instance.Add(new Section(".mvid", SectionCharacteristics.ContainsInitializedData | SectionCharacteristics.MemDiscardable | SectionCharacteristics.MemRead));
			instance.AddRange(immutableArray);
			return instance.ToImmutableAndFree();
		}
		return immutableArray;
	}

	protected override BlobBuilder SerializeSection(string name, SectionLocation location)
	{
		if (name.Equals(".mvid", StringComparison.Ordinal))
		{
			return SerializeMvidSection(location);
		}
		return base.SerializeSection(name, location);
	}

	internal BlobContentId Serialize(BlobBuilder peBlob, out Blob mvidSectionFixup)
	{
		BlobContentId result = Serialize(peBlob);
		mvidSectionFixup = _mvidSectionFixup;
		return result;
	}

	private BlobBuilder SerializeMvidSection(SectionLocation location)
	{
		BlobBuilder blobBuilder = new BlobBuilder();
		_mvidSectionFixup = blobBuilder.ReserveBytes(16);
		new BlobWriter(_mvidSectionFixup).WriteBytes(0, _mvidSectionFixup.Length);
		return blobBuilder;
	}
}
