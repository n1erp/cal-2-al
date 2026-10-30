using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.DiaSymReader;

namespace Microsoft.Cci;

internal static class PeWriter
{
	private class ResourceSectionBuilderFromObj : ResourceSectionBuilder
	{
		private readonly ResourceSection _resourceSection;

		public ResourceSectionBuilderFromObj(ResourceSection resourceSection)
		{
			_resourceSection = resourceSection;
		}

		protected override void Serialize(BlobBuilder builder, SectionLocation location)
		{
			NativeResourceWriter.SerializeWin32Resources(builder, _resourceSection, location.RelativeVirtualAddress);
		}
	}

	private class ResourceSectionBuilderFromResources : ResourceSectionBuilder
	{
		private readonly IEnumerable<IWin32Resource> _resources;

		public ResourceSectionBuilderFromResources(IEnumerable<IWin32Resource> resources)
		{
			_resources = resources;
		}

		protected override void Serialize(BlobBuilder builder, SectionLocation location)
		{
			NativeResourceWriter.SerializeWin32Resources(builder, _resources, location.RelativeVirtualAddress);
		}
	}

	private static MethodInfo s_calculateChecksumMethod;

	internal static bool WritePeToStream(EmitContext context, CommonMessageProvider messageProvider, Func<Stream> getPeStream, Func<Stream> getPortablePdbStreamOpt, PdbWriter nativePdbWriterOpt, string pdbPathOpt, bool metadataOnly, bool isDeterministic, bool emitTestCoverageData, RSAParameters? privateKeyOpt, CancellationToken cancellationToken)
	{
		MetadataWriter metadataWriter = FullMetadataWriter.Create(context, messageProvider, metadataOnly, isDeterministic, emitTestCoverageData, getPortablePdbStreamOpt != null, cancellationToken);
		ModulePropertiesForSerialization serializationProperties = context.Module.SerializationProperties;
		nativePdbWriterOpt?.SetMetadataEmitter(metadataWriter);
		BlobBuilder blobBuilder = new BlobBuilder(32768);
		BlobBuilder blobBuilder2 = new BlobBuilder();
		BlobBuilder blobBuilder3 = new BlobBuilder(1024);
		metadataWriter.BuildMetadataAndIL(nativePdbWriterOpt, blobBuilder, blobBuilder2, blobBuilder3, out var mvidFixup, out var mvidStringFixup);
		metadataWriter.GetEntryPoints(out var entryPointHandle, out var debugEntryPointHandle);
		if (!debugEntryPointHandle.IsNil)
		{
			nativePdbWriterOpt?.SetEntryPoint(MetadataTokens.GetToken(debugEntryPointHandle));
		}
		if (nativePdbWriterOpt != null)
		{
			if (context.Module.SourceLinkStreamOpt != null)
			{
				nativePdbWriterOpt.EmbedSourceLink(context.Module.SourceLinkStreamOpt);
			}
			if (metadataWriter.Module.OutputKind == OutputKind.WindowsRuntimeMetadata)
			{
				nativePdbWriterOpt.WriteDefinitionLocations(metadataWriter.Module.GetSymbolToLocationMap());
			}
			nativePdbWriterOpt.WriteRemainingEmbeddedDocuments(metadataWriter.Module.DebugDocumentsBuilder.EmbeddedDocuments);
		}
		Stream stream = getPeStream();
		if (stream == null)
		{
			return false;
		}
		BlobContentId pdbContentId = nativePdbWriterOpt?.GetContentId() ?? default(BlobContentId);
		nativePdbWriterOpt = null;
		ushort portablePdbVersion = 0;
		MetadataRootBuilder rootBuilder = metadataWriter.GetRootBuilder();
		PEHeaderBuilder header = new PEHeaderBuilder(serializationProperties.Machine, serializationProperties.SectionAlignment, serializationProperties.FileAlignment, serializationProperties.BaseAddress, serializationProperties.LinkerMajorVersion, serializationProperties.LinkerMinorVersion, 4, 0, 0, 0, serializationProperties.MajorSubsystemVersion, serializationProperties.MinorSubsystemVersion, serializationProperties.Subsystem, serializationProperties.DllCharacteristics, serializationProperties.ImageCharacteristics, serializationProperties.SizeOfStackReserve, serializationProperties.SizeOfStackCommit, serializationProperties.SizeOfHeapReserve, serializationProperties.SizeOfHeapCommit);
		Func<IEnumerable<Blob>, BlobContentId> deterministicIdProvider = (isDeterministic ? ((Func<IEnumerable<Blob>, BlobContentId>)((IEnumerable<Blob> content) => BlobContentId.FromHash(CryptographicHashProvider.ComputeHash(HashAlgorithmName.SHA1, content)))) : null);
		ImmutableArray<byte> portablePdbContentHash = default;
		BlobBuilder blobBuilder4 = null;
		if (metadataWriter.EmitPortableDebugMetadata)
		{
			metadataWriter.AddRemainingEmbeddedDocuments(metadataWriter.Module.DebugDocumentsBuilder.EmbeddedDocuments);
			HashAlgorithmName pdbChecksumAlgorithm = context.Module.PdbChecksumAlgorithm;
			Func<IEnumerable<Blob>, BlobContentId> deterministicIdProviderOpt = ((pdbChecksumAlgorithm.Name != null) ? ((Func<IEnumerable<Blob>, BlobContentId>)((IEnumerable<Blob> content) => BlobContentId.FromHash(portablePdbContentHash = CryptographicHashProvider.ComputeHash(context.Module.PdbChecksumAlgorithm, content)))) : null);
			BlobBuilder blobBuilder5 = new BlobBuilder();
			PortablePdbBuilder portablePdbBuilder = metadataWriter.GetPortablePdbBuilder(rootBuilder.Sizes.RowCounts, debugEntryPointHandle, deterministicIdProviderOpt);
			pdbContentId = portablePdbBuilder.Serialize(blobBuilder5);
			portablePdbVersion = portablePdbBuilder.FormatVersion;
			if (getPortablePdbStreamOpt == null)
			{
				blobBuilder4 = blobBuilder5;
			}
			else
			{
				Stream stream2 = getPortablePdbStreamOpt();
				if (stream2 != null)
				{
					try
					{
						blobBuilder5.WriteContentTo(stream2);
					}
					catch (Exception ex) when (!(ex is OperationCanceledException))
					{
						throw new SymUnmanagedWriterException(ex.Message, ex);
					}
				}
			}
		}
		DebugDirectoryBuilder debugDirectoryBuilder;
		if (((pdbPathOpt != null) | isDeterministic) || blobBuilder4 != null)
		{
			debugDirectoryBuilder = new DebugDirectoryBuilder();
			if (pdbPathOpt != null)
			{
				string pdbPath = (isDeterministic ? pdbPathOpt : PadPdbPath(pdbPathOpt));
				debugDirectoryBuilder.AddCodeViewEntry(pdbPath, pdbContentId, portablePdbVersion);
				if (!portablePdbContentHash.IsDefault)
				{
					DebugDirectoryBuilder debugDirectoryBuilder2 = debugDirectoryBuilder;
					HashAlgorithmName pdbChecksumAlgorithm = context.Module.PdbChecksumAlgorithm;
					debugDirectoryBuilder2.AddPdbChecksumEntry(pdbChecksumAlgorithm.Name, portablePdbContentHash);
				}
			}
			if (isDeterministic)
			{
				debugDirectoryBuilder.AddReproducibleEntry();
			}
			if (blobBuilder4 != null)
			{
				debugDirectoryBuilder.AddEmbeddedPortablePdbEntry(blobBuilder4, portablePdbVersion);
			}
		}
		else
		{
			debugDirectoryBuilder = null;
		}
		StrongNameProvider strongNameProvider = context.Module.CommonCompilation.Options.StrongNameProvider;
		CorFlags corFlags = serializationProperties.CorFlags;
		ExtendedPEBuilder extendedPEBuilder = new ExtendedPEBuilder(header, rootBuilder, blobBuilder, blobBuilder2, blobBuilder3, CreateNativeResourceSectionSerializer(context.Module), debugDirectoryBuilder, SigningUtilities.CalculateStrongNameSignatureSize(context.Module, privateKeyOpt), entryPointHandle, corFlags, deterministicIdProvider, metadataOnly && !context.IncludePrivateMembers);
		BlobBuilder blobBuilder6 = new BlobBuilder();
		BlobContentId blobContentId = extendedPEBuilder.Serialize(blobBuilder6, out var mvidSectionFixup);
		PatchModuleVersionIds(mvidFixup, mvidSectionFixup, mvidStringFixup, blobContentId.Guid);
		if (privateKeyOpt.HasValue && corFlags.HasFlag(CorFlags.StrongNameSigned))
		{
			strongNameProvider.SignPeBuilder(extendedPEBuilder, blobBuilder6, privateKeyOpt.Value);
		}
		try
		{
			blobBuilder6.WriteContentTo(stream);
		}
		catch (Exception ex2) when (!(ex2 is OperationCanceledException))
		{
			throw new PeWritingException(ex2);
		}
		return true;
	}

	internal static uint CalculateChecksum(BlobBuilder peBlob, Blob checksumBlob)
	{
		if ((object)s_calculateChecksumMethod == null)
		{
			s_calculateChecksumMethod = (from m in typeof(PEBuilder).GetRuntimeMethods()
				where m.Name == "CalculateChecksum" && m.GetParameters().Length == 2
				select m).Single();
		}
		return (uint)s_calculateChecksumMethod.Invoke(null, new object[2] { peBlob, checksumBlob });
	}

	private static void PatchModuleVersionIds(Blob guidFixup, Blob guidSectionFixup, Blob stringFixup, Guid mvid)
	{
		if (!guidFixup.IsDefault)
		{
			new BlobWriter(guidFixup).WriteGuid(mvid);
		}
		if (!guidSectionFixup.IsDefault)
		{
			new BlobWriter(guidSectionFixup).WriteGuid(mvid);
		}
		if (!stringFixup.IsDefault)
		{
			new BlobWriter(stringFixup).WriteUserString(mvid.ToString());
		}
	}

	private static string PadPdbPath(string path)
	{
		return path + new string('\0', Math.Max(0, 260 - Encoding.UTF8.GetByteCount(path) - 1));
	}

	private static ResourceSectionBuilder CreateNativeResourceSectionSerializer(CommonPEModuleBuilder module)
	{
		ResourceSection win32ResourceSection = module.Win32ResourceSection;
		if (win32ResourceSection != null)
		{
			return new ResourceSectionBuilderFromObj(win32ResourceSection);
		}
		IEnumerable<IWin32Resource> win32Resources = module.Win32Resources;
		if (win32Resources != null && win32Resources.Any())
		{
			return new ResourceSectionBuilderFromResources(win32Resources);
		}
		return null;
	}
}
